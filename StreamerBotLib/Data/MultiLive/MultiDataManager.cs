
using StreamerBotLib.Enums;
using StreamerBotLib.GUI;
using StreamerBotLib.Static;

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

using static StreamerBotLib.Data.MultiLive.DataSource;

namespace StreamerBotLib.Data.MultiLive
{
    public class MultiDataManager : INotifyPropertyChanged
    {
        private static readonly string DataFileXML = "MultiChatbotData.xml";

#if DEBUG
        private static readonly string DataFileName = Path.Combine(@"C:\Source\ChatBotApp\MultiUserLiveBot\bin\Debug\net6.0-windows", DataFileXML);
#else
        private static readonly string DataFileName = DataFileXML;
#endif

        private readonly DataSource _DataSource;

        private readonly Queue<Task> SaveTasks = new();
        private bool SaveThreadStarted = false;
        private const int SaveThreadWait = 10000;

        private int BackupSaveToken = 0;
        private const int BackupSaveIntervalMins = 15;
        private const int BackupHrInterval = 60 / BackupSaveIntervalMins;
        private readonly string BackupDataFileXML = $"Backup_{DataFileXML}";

        public DataView Channels { get; set; }
        public DataView MsgEndPoints { get; set; }
        public DataView LiveStream { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string PropName)
        {
            PropertyChanged?.Invoke(this, new(PropName));
        }

        public MultiDataManager()
        {
            _DataSource = new();

            Channels = new DataView(_DataSource.Channels, null, $"{_DataSource.Channels.ChannelNameColumn.ColumnName}", DataViewRowState.CurrentRows);
            MsgEndPoints = new DataView(_DataSource.MsgEndPoints, null, $"{_DataSource.MsgEndPoints.IdColumn.ColumnName}", DataViewRowState.CurrentRows);
            LiveStream = new DataView(_DataSource.LiveStream, null, $"{_DataSource.LiveStream.LiveDateColumn.ColumnName} DESC", DataViewRowState.CurrentRows);

            Channels.ListChanged += DataView_ListChanged;
            MsgEndPoints.ListChanged += DataView_ListChanged;
            LiveStream.ListChanged += DataView_ListChanged;
        }

        private void DataView_ListChanged(object sender, ListChangedEventArgs e)
        {
            lock (_DataSource)
            {
                DataView dataView = (DataView)sender;
                OnPropertyChanged(nameof(dataView.Table));
            }
        }

        #region Load and Exit Ops
        /// <summary>
        /// Load the data source and populate with default data
        /// </summary>
        public void LoadData()
        {
            _DataSource.Clear();
            if (!File.Exists(DataFileName))
            {
                _DataSource.WriteXml(DataFileName);
            }

            using XmlReader xmlreader = new XmlTextReader(DataFileName);
            _DataSource.ReadXml(xmlreader, XmlReadMode.DiffGram);

            foreach(MsgEndPointsRow dataRow in _DataSource.MsgEndPoints.Rows)
            {
                if (DBNull.Value.Equals(dataRow["IsEnabled"]))
                {
                    dataRow.IsEnabled = true;
                }
            }
        }

        /// <summary>
        /// Save data to file upon exit
        /// </summary>
        public void SaveData()
        {
            int CurrMins = DateTime.Now.Minute;
            bool IsBackup = CurrMins >= BackupSaveToken * BackupSaveIntervalMins && CurrMins < (BackupSaveToken + 1) % BackupHrInterval * BackupSaveIntervalMins;

            if (!SaveThreadStarted) // only start the thread once per save cycle, flag is an object lock
            {
                SaveThreadStarted = true;
                ThreadManager.CreateThreadStart(PerformSaveOp, ThreadWaitStates.Wait, ThreadExitPriority.Low); // need to wait, else could corrupt datafile
            }

            if (_DataSource.HasChanges())
            {
                lock (SaveTasks) // lock the Queue, block thread if currently save task has started
                {
                    SaveTasks.Enqueue(new(() =>
                    {
                        lock (_DataSource)
                        {
                            _DataSource.AcceptChanges();
                            try
                            {
                                MemoryStream SaveData = new();  // new memory stream

                                _DataSource.WriteXml(SaveData, XmlWriteMode.DiffGram); // save the database to the memory stream

                                DataSource testinput = new();   // start a new database
                                SaveData.Position = 0;          // reset the reader
                                testinput.ReadXml(SaveData);    // try to read the database, when in valid state this doesn't cause an exception (try/catch)

                                _DataSource.WriteXml(DataFileName, XmlWriteMode.DiffGram); // write the valid data to file

                                // determine if current time is within a certain time frame, and perform the save
                                if (IsBackup && OptionFlags.IsStreamOnline)
                                {
                                    // write backup file
                                    _DataSource.WriteXml(BackupDataFileXML, XmlWriteMode.DiffGram); // write the valid data to file
                                }
                            }
                            catch (Exception ex)
                            {
                                LogWriter.LogException(ex, MethodBase.GetCurrentMethod().Name);
                            }
                        }
                    }));
                }
            }
        }

        private void PerformSaveOp()
        {
#if LogDataManager_Actions
            LogWriter.DataActionLog(MethodBase.GetCurrentMethod().Name, $"Managed database save data.");
#endif

            if (OptionFlags.ActiveToken) // don't sleep if exiting app
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

        /// <summary>
        /// Checks if the channel has already posted a stream for today.
        /// </summary>
        /// <param name="ChannelName">Name of channel.</param>
        /// <param name="dateTime">The time of the stream.</param>
        /// <returns>true if the channel and date have already posted 2+ events for the same day. false if there is no match or just 1 event post for the current date.</returns>
        public bool CheckStreamDate(string ChannelName, DateTime dateTime)
        {
            lock (_DataSource)
            {
                return (from DataSource.LiveStreamRow liveStreamRow in _DataSource.LiveStream.Select()
                        where liveStreamRow.ChannelName == ChannelName && liveStreamRow.LiveDate.ToShortDateString() == dateTime.ToShortDateString()
                        select liveStreamRow).Count() > 1;
            }
        }

        /// <summary>
        /// Will post the channel and date event. Checks for not duplicating the event, i.e. same channel same date&time.
        /// </summary>
        /// <param name="ChannelName">The name of the channel for the event.</param>
        /// <param name="dateTime">The date of the event.</param>
        /// <returns>true if the event posted. false if the date & time duplicates.</returns>
        public bool PostStreamDate(string ChannelName, DateTime dateTime)
        {
            lock (_DataSource)
            {
                bool result = false;

                if ((from DataSource.LiveStreamRow liveStreamRow in _DataSource.LiveStream.Select()
                     where liveStreamRow.ChannelName == ChannelName && liveStreamRow.LiveDate == dateTime
                     select new { }).Any())
                {
                    result = false;
                }
                else
                {
                    _DataSource.LiveStream.AddLiveStreamRow(ChannelName, dateTime);
                    SaveData();
                    OnPropertyChanged(nameof(LiveStream));

                    result = true;
                }

                return result;
            }
        }

        /// <summary>
        /// Retrieves the list of channel names from the Channels table.
        /// </summary>
        /// <returns>A list of strings from the Channels table.</returns>
        public List<string> GetChannelNames()
        {
            lock (_DataSource)
            {
                return new(from DataSource.ChannelsRow c in _DataSource.Channels.Select()
                           select c.ChannelName);
            }
        }

        /// <summary>
        /// Retrieve the Endpoints links where the user wants to post live messages.
        /// </summary>
        /// <returns>A list of URI objects for the Endpoint links.</returns>
        public List<Tuple<string, Uri>> GetWebLinks()
        {
            lock (_DataSource)
            {
                return new(from DataSource.MsgEndPointsRow MsgEndPointsRow in (DataSource.MsgEndPointsRow[])_DataSource.MsgEndPoints.Select()
                           select new Tuple<string, Uri>(MsgEndPointsRow.Type, new(MsgEndPointsRow.URL)));
            }
        }


        public void UpdateIsEnabledRows(IEnumerable<DataRow> dataRows, bool IsEnabled)
        {
            lock (_DataSource)
            {
                List<DataTable> updated = new();
                foreach (DataRow dr in dataRows)
                {
                    if (CheckField(dr.Table.TableName, "IsEnabled"))
                    {
                        updated.UniqueAdd(dr.Table);
                        SetDataRowFieldRow(dr, "IsEnabled", IsEnabled);
                    }
                }
                updated.ForEach((T) => T.AcceptChanges());
            }
        }


        /// <summary>
        /// Check if the provided field is part of the supplied table.
        /// </summary>
        /// <param name="table">The table to check.</param>
        /// <param name="field">The field within the table to see if it exists.</param>
        /// <returns><i>true</i> - if table contains the supplied field, <i>false</i> - if table doesn't contain the supplied field.</returns>
        public bool CheckField(string table, string field)
        {
#if LogDataManager_Actions
            LogWriter.DataActionLog(MethodBase.GetCurrentMethod().Name, $"Check if field {field} is in table {table}.");
#endif

            lock (_DataSource)
            {
                return _DataSource.Tables[table].Columns.Contains(field);
            }
        }

        public void SetDataRowFieldRow(DataRow dataRow, string dataColumn, object value)
        {
            lock (_DataSource)
            {
                dataRow[dataColumn] = value;
            }
        }

        public void PostMonitorChannel(string UserName)
        {
            lock (_DataSource)
            {
                if(_DataSource.Channels.Count < 100)
                {
                    _DataSource.Channels.AddChannelsRow(UserName);
                }
            }
        }
    }
}
