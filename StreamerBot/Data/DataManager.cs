using StreamerBot.Enum;

using System.Collections.Generic;
using System.Data;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using System.Xml;
using StreamerBot.Static;
using System.Reflection;

namespace StreamerBot.Data
{
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
        #endregion DataSource

        private readonly Queue<Task> SaveTasks = new();
        private bool SaveThreadStarted = false;
        private const int SaveThreadWait = 5000;

        public bool UpdatingFollowers { get; set; } = false;



        public DataManager()
        {
            _DataSource = new();
            LoadData();

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

        internal object GetRowData(DataRetrieve eventEnabled, ChannelEventActions channelEventActions)
        {
            return null;
        }
    }
}
