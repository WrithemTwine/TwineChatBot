using ChatBot_Net5.Enum;
using ChatBot_Net5.Static;

using System;
using System.Collections.Generic;
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

    public partial class DataManager
    {

        #region DataSource

        private static readonly string DataFileXML = "ChatDataStore.xml";

#if DEBUG
        private static readonly string DataFileName = Path.Combine(@"C:\Source\ChatBotApp\ChatBotNet5\bin\Debug\net5.0-windows", DataFileXML);
#else
        private static readonly string DataFileName = DataFileXML;
#endif

        internal readonly DataSource _DataSource;

        private readonly Queue<Task> SaveTasks = new();
        private bool SaveThreadStarted = false;
        private const int SaveThreadWait = 5000;

        public bool UpdatingFollowers { get; set; } = false;

        #endregion DataSource

        public DataManager()
        {
            _DataSource = new();            
            LoadData();
            CurrStreamStart = DateTime.MinValue;

            OnSaveData += SaveData;
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
            }

            SaveData(this, new());
        }

        public void Initialize()
        {
            SetDefaultChannelEventsTable();  // check all default ChannelEvents names
            SetDefaultCommandsTable(); // check all default Commands
        }

        /// <summary>
        /// Provide an internal notification event to save the data outside of any multi-threading mechanisms.
        /// </summary>
        public event EventHandler OnSaveData;
        private void NotifySaveData()
        {
            OnSaveData?.Invoke(this, new());
        }

        /// <summary>
        /// Save data to file upon exit and after data changes. Pauses for 15 seconds (unless exiting) to slow down multiple saves in a short time.
        /// </summary>
        public void SaveData(object sender, EventArgs e)
        {
            if (!UpdatingFollowers) // block saving data until the follower updating is completed
            {

                if (!SaveThreadStarted) // only start the thread once per save cycle, flag is an object lock
                {
                    SaveThreadStarted = true;
                    new Thread(new ThreadStart(PerformSaveOp)).Start();
                }

                if (_DataSource.HasChanges())
                {
                    _DataSource.AcceptChanges();

                    lock (SaveTasks) // lock the Queue, block thread if currently save task has started
                    {
                        SaveTasks.Enqueue(new(() =>
                        {
                            lock (_DataSource)
                            {
                                string result = Path.GetRandomFileName();
                                try
                                {
                                    _DataSource.WriteXml(result, XmlWriteMode.DiffGram);

                                    DataSource testinput = new();

                                    XmlReader xmlReader = new XmlTextReader(result);
                                // test load
                                _ = testinput.ReadXml(xmlReader, XmlReadMode.DiffGram);
                                    xmlReader.Close();

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
        }

        private void PerformSaveOp()
        {
            if (OptionFlags.BotStarted) // don't sleep if exiting app
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
            }
            SaveThreadStarted = false; // indicate start another thread to save data
        }

        #endregion

        #region Helpers
        /// <summary>
        /// Access the DataSource to retrieve the first row matching the search criteria.
        /// </summary>
        /// <param name="dataRetrieve">The name of the table and column to retrieve.</param>
        /// <param name="rowcriteria">The search string for a particular row.</param>
        /// <returns>Null for no value or the first row found using the <i>rowcriteria</i></returns>
        public object GetRowData(DataRetrieve dataRetrieve, ChannelEventActions rowcriteria)
        {
            return GetAllRowData(dataRetrieve, rowcriteria).FirstOrDefault();
        }

        /// <summary>
        /// Access the DataSource to retrieve the first row matching the search criteria.
        /// </summary>
        /// <param name="dataRetrieve">The name of the table and column to retrieve.</param>
        /// <param name="rowcriteria">The search string for a particular row.</param>
        /// <returns>All data found using the <i>rowcriteria</i></returns>
        public object[] GetAllRowData(DataRetrieve dataRetrieve, ChannelEventActions rowcriteria)
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
        public List<Tuple<bool, Uri>> GetWebhooks(WebhooksKind webhooks)
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
        public void UpdateCategory(string CategoryId, string newCategory)
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

            NotifySaveData();
        }
        #endregion

        #region Clips

        public bool AddClip(string ClipId, string CreatedAt, float Duration, string GameId, string Language, string Title, string Url)
        {
            lock (_DataSource)
            {
                DataSource.ClipsRow[] clipsRows = (DataSource.ClipsRow[])_DataSource.Clips.Select("Id='" + ClipId + "'");

                if (clipsRows.Length == 0)
                {
                    _ = _DataSource.Clips.AddClipsRow(ClipId, DateTime.Parse(CreatedAt).ToLocalTime().ToString(), Title, GameId, Language, (decimal)Duration, Url);
                    NotifySaveData();
                    return true;
                }
                return false;
            }
        }

        #endregion
    }
}
