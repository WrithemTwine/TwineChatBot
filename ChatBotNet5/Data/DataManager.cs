using ChatBot_Net5.Enum;
using ChatBot_Net5.Static;
using ChatBot_Net5.Systems;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace ChatBot_Net5.Data
{
    /*
        - DataManager should not be converted to "local culture" strings, because the data file would have problems if the system language changed
        - Having mixed identifiers within the data file would create problems
        - Can convert to localized GUI identifiers to aid users not comfortable with English (and their language is provided to the app)
        - Also, the 'add command' parameters should remain
        - A GUI to add new commands would provide more localized help, apart from names of data tables <= unless there's somehow a converter between the name they choose and the database name => could be a dictionary with keys of the localized language and the values to the data manager data table values
    */

    public partial class DataManager : INotifyPropertyChanged
    {

        #region DataSource
#if DEBUG
        private static readonly string DataFileName = Path.Combine(@"C:\Source\ChatBotApp\ChatBotNet5\bin\Debug\net5.0-windows", "ChatDataStore.xml");
#else
        private static readonly string DataFileName = Path.Combine(Directory.GetCurrentDirectory(), "ChatDataStore.xml");
#endif

        private readonly DataSource _DataSource;

        private readonly Queue<Task> SaveTasks = new();
        private bool SaveThreadStarted = false;
        private const int SaveThreadWait = 1500;

        public bool UpdatingFollowers { get; set; } = false;

        public List<string> KindsWebhooks { get; private set; } = new(System.Enum.GetNames(typeof(WebhooksKind)));
        public DataView ChannelEvents { get; private set; } // DataSource.ChannelEventsDataTable
        public DataView Users { get; private set; }  // DataSource.UsersDataTable
        public DataView Followers { get; private set; } // DataSource.FollowersDataTable
        public DataView Discord { get; private set; } // DataSource.DiscordDataTable
        public DataView Currency { get; private set; }  // DataSource.CurrencyDataTable
        public DataView CurrencyType { get; private set; }  // DataSource.CurrencyTypeDataTable
        public DataView BuiltInCommands { get; private set; } // DataSource.CommandsDataTable
        public DataView Commands { get; private set; }  // DataSource.CommandsDataTable
        public DataView StreamStats { get; private set; } // DataSource.StreamStatsTable
        public DataView ShoutOuts { get; private set; } // DataSource.ShoutOutsTable
        public DataView Category { get; private set; } // DataSource.CategoryTable
        public DataView Clips { get; private set; }  // DataSource.ClipsDataTable

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string PropName)
        {
            PropertyChanged?.Invoke(this, new(PropName));
        }

        #endregion DataSource

        public DataManager()
        {
            static string ComFilter()
            {
                string filter = string.Empty;

                foreach (DefaultCommand d in System.Enum.GetValues(typeof(DefaultCommand)))
                {
                    filter += "'" + d.ToString() + "',";
                }

                foreach (DefaultSocials s in System.Enum.GetValues(typeof(DefaultSocials)))
                {
                    filter += "'" + s.ToString() + "',";
                }

                return filter == string.Empty ? "" : filter[0..^1];
            }

            _DataSource = new();
            
            LocalizedMsgSystem.SetDataManager(this);
            LoadData();
            
            ChannelEvents = _DataSource.ChannelEvents.DefaultView;
            Users = new(_DataSource.Users, null, "UserName", DataViewRowState.CurrentRows);
            Followers = new(_DataSource.Followers, null, "FollowedDate", DataViewRowState.CurrentRows);
            Discord = _DataSource.Discord.DefaultView;
            CurrencyType = new(_DataSource.CurrencyType, null, "CurrencyName", DataViewRowState.CurrentRows);
            Currency = new(_DataSource.Currency, null, "UserName", DataViewRowState.CurrentRows);
            BuiltInCommands = new(_DataSource.Commands, "CmdName IN (" + ComFilter() + ")", "CmdName", DataViewRowState.CurrentRows);
            Commands = new(_DataSource.Commands, "CmdName NOT IN (" + ComFilter() + ")", "CmdName", DataViewRowState.CurrentRows);
            StreamStats = new(_DataSource.StreamStats, null, "StreamStart", DataViewRowState.CurrentRows);
            ShoutOuts = new(_DataSource.ShoutOuts, null, "UserName", DataViewRowState.CurrentRows);
            Category = new(_DataSource.CategoryList, null, "Id", DataViewRowState.CurrentRows);
            Clips = new(_DataSource.Clips, null, "Id", DataViewRowState.CurrentRows);

            CurrStreamStart = DateTime.MinValue;
        }

        #region Load and Exit Ops
        /// <summary>
        /// Load the data source and populate with default data
        /// </summary>
        private void LoadData()
        {
            lock (_DataSource)
            {
                if (!File.Exists(DataFileName))
                {
                    _DataSource.WriteXml(DataFileName);
                }

                using (XmlReader xmlreader = new XmlTextReader(DataFileName))
                {
                    _ = _DataSource.ReadXml(xmlreader, XmlReadMode.DiffGram);
                }

                SetDefaultChannelEventsTable();  // check all default ChannelEvents names
                SetDefaultCommandsTable(); // check all default Commands
            }

            SaveData();
        }

        /// <summary>
        /// Save data to file upon exit and after data changes. Pauses for 15 seconds (unless exiting) to slow down multiple saves in a short time.
        /// </summary>
        public void SaveData()
        {
            if (!UpdatingFollowers) // block saving data until the follower updating is completed
            {
                lock (SaveTasks) // lock the Queue, block thread if currently save task has started
                {
                    if (!SaveThreadStarted) // only start the thread once per save cycle, flag is an object lock
                    {
                        SaveThreadStarted = true;
                        new Thread(new ThreadStart(PerformSaveOp)).Start();
                    }

                    SaveTasks.Enqueue(new(() =>
                    {
                        lock (_DataSource)
                        {
                            string result = Path.GetRandomFileName();

                            _DataSource.AcceptChanges();

                            try
                            {
                                _DataSource.WriteXml(result, XmlWriteMode.DiffGram);

                                DataSource testinput = new();
                                using (XmlReader xmlReader = new XmlTextReader(result))
                                {
                                    // test load
                                    _ = testinput.ReadXml(xmlReader, XmlReadMode.DiffGram);
                                }

                                File.Move(result, DataFileName, true);
                                File.Delete(result);
                            }
                            catch (Exception ex)
                            {
                                LogWriter.LogException(ex, MethodBase.GetCurrentMethod().Name);
                                File.Delete(result);
                            }
                        }
                    }));
                }
            }
        }

        private void PerformSaveOp()
        {
            if (OptionFlags.ProcessOps) // don't sleep if exiting app
            {
                Thread.Sleep(SaveThreadWait);
            }

            lock (SaveTasks) // in case save actions arrive during save try
            {
                if (SaveTasks.Count >= 1)
                {
                    SaveTasks.Dequeue().Start(); // only run 1 of the save tasks
                }

                SaveTasks.Clear();
                SaveThreadStarted = false; // indicate start another thread to save data
            }
        }


        #endregion

        #region Helpers
        /// <summary>
        /// Access the DataSource to retrieve the first row matching the search criteria.
        /// </summary>
        /// <param name="dataRetrieve">The name of the table and column to retrieve.</param>
        /// <param name="rowcriteria">The search string for a particular row.</param>
        /// <returns>Null for no value or the first row found using the <i>rowcriteria</i></returns>
        internal object GetRowData(DataRetrieve dataRetrieve, ChannelEventActions rowcriteria)
        {
            return GetAllRowData(dataRetrieve, rowcriteria).FirstOrDefault();
        }

        /// <summary>
        /// Access the DataSource to retrieve the first row matching the search criteria.
        /// </summary>
        /// <param name="dataRetrieve">The name of the table and column to retrieve.</param>
        /// <param name="rowcriteria">The search string for a particular row.</param>
        /// <returns>All data found using the <i>rowcriteria</i></returns>
        internal object[] GetAllRowData(DataRetrieve dataRetrieve, ChannelEventActions rowcriteria)
        {
            string criteriacolumn = "";
            string datacolumn = "";
            string table = "";

            switch (dataRetrieve)
            {
                case DataRetrieve.EventMessage:
                    table = DataSourceTableName.ChannelEvents.ToString();
                    criteriacolumn = "Name";
                    datacolumn = "MsgStr";
                    break;
                case DataRetrieve.EventEnabled:
                    table = DataSourceTableName.ChannelEvents.ToString();
                    criteriacolumn = "Name";
                    datacolumn = "IsEnabled";
                    break;
            }

            DataRow[] row = null;

            lock (_DataSource)
            {
                row = _DataSource.Tables[table].Select(criteriacolumn + "='" + rowcriteria.ToString() + "'");
            }

            List<object> list = new();
            foreach (DataRow d in row)
            {
                list.Add(d.Field<object>(datacolumn));
            }

            return list.ToArray();
        }
        #endregion Helpers

        #region Discord and Webhooks
        /// <summary>
        /// Retrieve all the webhooks from the Discord table
        /// </summary>
        /// <returns></returns>
        internal List<Tuple<bool, Uri>> GetWebhooks(WebhooksKind webhooks)
        {
            lock (_DataSource.Discord)
            {
                DataRow[] dataRows = _DataSource.Discord.Select();

                List<Tuple<bool, Uri>> uris = new();

                foreach (DataRow d in dataRows)
                {
                    DataSource.DiscordRow row = d as DataSource.DiscordRow;

                    if (row.Kind == webhooks.ToString())
                    {
                        uris.Add(new Tuple<bool, Uri>(row.AddEveryone, new Uri(row.Webhook)));
                    }
                }
                return uris;
            }
        }
        #endregion Discord and Webhooks

        #region Category

        /// <summary>
        /// Checks for the supplied category in the category list, adds if it isn't already saved.
        /// </summary>
        /// <param name="newCategory">The category to add to the list if it's not available.</param>
        internal void UpdateCategory(string CategoryId, string newCategory)
        {
            lock (_DataSource)
            {
                DataSource.CategoryListRow[] categoryList = (DataSource.CategoryListRow[])_DataSource.CategoryList.Select("Category='" + newCategory.Replace("'", "''") + "'");

                if (categoryList.Length == 0)
                {
                    _DataSource.CategoryList.AddCategoryListRow(CategoryId, newCategory);
                }
                else if (categoryList[0].CategoryId == null)
                {
                    categoryList[0].CategoryId = CategoryId;
                }
            }

            SaveData();
            OnPropertyChanged(nameof(Category));
        }
        #endregion

        #region Clips

        internal bool AddClip(string ClipId, string CreatedAt, float Duration, string GameId, string Language, string Title, string Url)
        {
            lock (_DataSource)
            {
                DataSource.ClipsRow[] clipsRows = (DataSource.ClipsRow[])_DataSource.Clips.Select("Id='" + ClipId + "'");

                if (clipsRows.Length == 0)
                {
                    _ = _DataSource.Clips.AddClipsRow(ClipId, DateTime.Parse(CreatedAt).ToLocalTime().ToString(), Title, GameId, Language, (decimal)Duration, Url);
                    SaveData();
                    OnPropertyChanged(nameof(Clips));
                    return true;
                }
                return false;
            }
        }

        #endregion
    }
}
