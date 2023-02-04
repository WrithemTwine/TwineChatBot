
using StreamerBotLib.Data.DataSetCommonMethods;
using StreamerBotLib.Enums;
using StreamerBotLib.Interfaces;
using StreamerBotLib.Models;
using StreamerBotLib.Static;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows.Threading;
using System.Xml;

using static StreamerBotLib.Data.MultiLive.DataSource;

namespace StreamerBotLib.Data.MultiLive
{
    public class MultiDataManager : BaseDataManager, INotifyPropertyChanged, IDataManageReadOnly
    {
        private static readonly string DataFileXML = "MultiChatbotData.xml";

        private readonly DataSource _DataSource;
        private bool IsLiveStreamUpdated = false;

        public string MultiLiveStatusLog { get; set; } = "";

        public DataView Channels { get; set; }
        public DataView MsgEndPoints { get; set; }
        public DataView LiveStream { get; set; }
        public DataView SummaryLiveStream { get; set; }
        public List<ArchiveMultiStream> CleanupList { get; private set; } = new List<ArchiveMultiStream>();

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string PropName)
        {
            PropertyChanged?.Invoke(this, new(PropName));
        }

        public event EventHandler UpdatedMonitoringChannels;

        public MultiDataManager() : base(DataFileXML)
        {
            _DataSource = new();

            Channels = new(_DataSource.Channels, null, $"{_DataSource.Channels.ChannelNameColumn.ColumnName}", DataViewRowState.CurrentRows);
            MsgEndPoints = new(_DataSource.MsgEndPoints, null, $"{_DataSource.MsgEndPoints.IdColumn.ColumnName}", DataViewRowState.CurrentRows);
            LiveStream = new(_DataSource.LiveStream, null, $"{_DataSource.LiveStream.LiveDateColumn.ColumnName} DESC", DataViewRowState.CurrentRows);
            SummaryLiveStream = new(_DataSource.SummaryLiveStream, null, $"{_DataSource.SummaryLiveStream.ChannelNameColumn.ColumnName} DESC", DataViewRowState.CurrentRows);

            Channels.ListChanged += DataView_ListChanged;
            MsgEndPoints.ListChanged += DataView_ListChanged;
            LiveStream.ListChanged += DataView_ListChanged;
            SummaryLiveStream.ListChanged += DataView_ListChanged;
        }

        private void DataView_ListChanged(object sender, ListChangedEventArgs e)
        {
            DataView dataView = (DataView)sender;

            if (dataView.Table == _DataSource.Channels && _DataSource.HasChanges())
            {
                UpdatedMonitoringChannels?.Invoke(this, new());
            }
        }

        #region Load and Exit Ops
        /// <summary>
        /// Load the data source and populate with default data
        /// </summary>
        public void LoadData()
        {
            _DataSource.Clear();
            void LoadFile(string filename)
            {
                lock (_DataSource)
                {
                    if (!File.Exists(filename))
                    {
                        _DataSource.WriteXml(filename);
                    }

                    _ = _DataSource.ReadXml(new XmlTextReader(filename), XmlReadMode.DiffGram);

                }
                OptionFlags.MultiDataLoaded = true;
            }

            BeginLoadData(_DataSource.Tables);

            TryLoadFile((xmlfile) => LoadFile(xmlfile));

            try
            {
                EndLoadData(_DataSource.Tables);

                _DataSource.AcceptChanges();
            }
            catch (ConstraintException)
            {
                _DataSource.EnforceConstraints = false;

                foreach (DataTable table in _DataSource.Tables)
                {
                    List<DataRow> UniqueRows = new();
                    List<DataRow> DuplicateRows = new();

                    foreach (DataRow datarow in table.Rows)
                    {
                        if (!UniqueRows.UniqueAdd(datarow, new DataRowEquatableComparer()))
                        {
                            DuplicateRows.Add(datarow);
                        }
                    }

                    DuplicateRows.ForEach(r => r.Delete());
                }

                _DataSource.AcceptChanges();

                EndLoadData(_DataSource.Tables);

                _DataSource.EnforceConstraints = true;
            }

            NotifyPropertyChanged(nameof(Channels));
            NotifyPropertyChanged(nameof(MsgEndPoints));
            NotifyPropertyChanged(nameof(LiveStream));
            NotifyPropertyChanged(nameof(_DataSource.Channels));
            NotifyPropertyChanged(nameof(_DataSource.MsgEndPoints));
            NotifyPropertyChanged(nameof(_DataSource.LiveStream));

        }

        /// <summary>
        /// Save data to file upon exit
        /// </summary>
        public void SaveData()
        {
            if (OptionFlags.MultiDataLoaded)
            {
                SaveData(
                    (stream, xmlwrite) => _DataSource.WriteXml(stream, xmlwrite),
                    (filename, xmlwrite) => _DataSource.WriteXml(filename, xmlwrite),
                    _DataSource,
                    (SaveDataMemoryStream) =>
                    {
                        DataSource testinput = new();   // start a new database
                        SaveDataMemoryStream.Position = 0;          // reset the reader
                        testinput.ReadXml(SaveDataMemoryStream);    // try to read the database, when in valid state this doesn't cause an exception (try/catch)
                    }
                    );
            }
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
                return (from LiveStreamRow liveStreamRow in _DataSource.LiveStream.Select()
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

                if ((from LiveStreamRow liveStreamRow in _DataSource.LiveStream.Select($"{_DataSource.Channels.ChannelNameColumn.ColumnName}='{ChannelName}'")
                     where liveStreamRow.LiveDate == dateTime
                     select new { }).Any())
                {
                    result = false;
                }
                else
                {
                    // since we know this addition is only from a source based on the Channels, we forego null checking
                    _DataSource.LiveStream.AddLiveStreamRow((ChannelsRow)_DataSource.Channels.Select($"{_DataSource.Channels.ChannelNameColumn.ColumnName}='{ChannelName}'").FirstOrDefault(), dateTime);
                    _DataSource.LiveStream.AcceptChanges();
                    NotifyPropertyChanged(nameof(_DataSource.LiveStream));
                    SaveData();

                    result = true;
                    IsLiveStreamUpdated = true; // flag to update the summary due to new entry
                }
                return result;
            }
        }

        public void SummarizeStreamData()
        {
            if (IsLiveStreamUpdated || CleanupList.Count == 0) // only perform if flag for update occurs
            {
                CleanupList.Clear();

                List<DateTime> AllDates = new(DataSetStatic.GetRows(_DataSource.LiveStream).Select(dataRow => ((LiveStreamRow)dataRow).LiveDate.Date).OrderByDescending((k) => k.Date));
                List<DateTime> UniqueDates = new(AllDates.Intersect(AllDates));
                CleanupList.AddRange(UniqueDates.Select(uniqueDate => new ArchiveMultiStream()
                {
                    ThroughDate = uniqueDate,
                    StreamCount = (from DateTime dates in AllDates
                                   where dates.Date <= uniqueDate
                                   select dates).Count()
                }));

                IsLiveStreamUpdated = false; // reset update flag indicator
                NotifyPropertyChanged(nameof(CleanupList));
            }
        }

        public void SummarizeStreamData(ArchiveMultiStream archiveRecord)
        {
            lock (_DataSource)
            {
                List<LiveStreamRow> toDeleteRows = new(from LiveStreamRow dataRow in DataSetStatic.GetRows(_DataSource.LiveStream)
                                                       where dataRow.LiveDate.Date <= archiveRecord.ThroughDate.Date
                                                       select dataRow);

                foreach ((ArchiveMultiStream SumNameRows, SummaryLiveStreamRow summaryrow) in from ArchiveMultiStream SumNameRows in toDeleteRows.GroupBy(
                     (key) => key.ChannelName,
                     (date) => date.LiveDate,
                     (name, dates) => new ArchiveMultiStream
                     {
                         Name = name,
                         StreamCount = dates.Count(),
                         ThroughDate = dates.Max()
                     }
                     )
                                                          let summaryrow = (SummaryLiveStreamRow)DataSetStatic.GetRow(_DataSource.SummaryLiveStream, $"{_DataSource.SummaryLiveStream.ChannelNameColumn.ColumnName}='{SumNameRows.Name}'")
                                                          select (SumNameRows, summaryrow)
                 )
                {
                    if (summaryrow == null)
                    {
                        ChannelsRow UserRow = (ChannelsRow)DataSetStatic.GetRow(_DataSource.Channels, $"{_DataSource.Channels.ChannelNameColumn}='{SumNameRows.Name}'");
                        _DataSource.SummaryLiveStream.AddSummaryLiveStreamRow(UserRow.Id, UserRow.ChannelName, SumNameRows.StreamCount, SumNameRows.ThroughDate);
                    }
                    else
                    {
                        summaryrow.StreamCount += SumNameRows.StreamCount;
                        summaryrow.ThroughDate = summaryrow.ThroughDate < SumNameRows.ThroughDate ? SumNameRows.ThroughDate : summaryrow.ThroughDate;

                    }
                }

                toDeleteRows.ForEach((r) => r.Delete());
                _DataSource.LiveStream.AcceptChanges();
                _DataSource.SummaryLiveStream.AcceptChanges();
                SaveData();
                IsLiveStreamUpdated = true;

                NotifyPropertyChanged(nameof(_DataSource.LiveStream));
                NotifyPropertyChanged(nameof(_DataSource.SummaryLiveStream));
                CleanupList.Clear();
                NotifyPropertyChanged(nameof(CleanupList));

                SummarizeStreamData();
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
                return new(from ChannelsRow c in _DataSource.Channels.Select()
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
                return new(from MsgEndPointsRow MsgEndPointsRow in (MsgEndPointsRow[])_DataSource.MsgEndPoints.Select("IsEnabled='True'")
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
                SaveData();
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

        public int GetMonitorChannelCount()
        {
            lock (_DataSource)
            {
                return _DataSource.Channels.Count;
            }
        }

        public void PostMonitorChannel(string UserName)
        {
            lock (_DataSource)
            {
                _DataSource.Channels.AddChannelsRow(UserName);
                _DataSource.Channels.AcceptChanges();
                SaveData();
                NotifyPropertyChanged(nameof(_DataSource.Channels));
            }
        }

        /// <summary>
        /// Event to handle when the Twitch client sends and event. Updates the StatusLog property with the logged activity.
        /// </summary>
        /// <param name="data">The string of the message.</param>
        /// <param name="dateTime">The time of the event.</param>
        public void LogEntry(string data, DateTime dateTime)
        {
            if (MultiLiveStatusLog.Length + dateTime.ToString().Length + data.Length + 2 >= MaxLogLength)
            {
                MultiLiveStatusLog = MultiLiveStatusLog[MultiLiveStatusLog.IndexOf('\n')..];
            }

            MultiLiveStatusLog += dateTime.ToString() + " " + data + "\n";

            NotifyPropertyChanged(nameof(MultiLiveStatusLog));
        }

        #region interface members

        public string GetKey(string Table)
        {
            return DataSetStatic.GetKey(_DataSource.Tables[Table], Table);
        }

        public List<string> GetTableFields(string TableName)
        {
            return DataSetStatic.GetTableFields(_DataSource.Tables[TableName]);
        }
        public List<string> GetTableNames()
        {
            lock (_DataSource)
            {
                return new(from DataTable table in _DataSource.Tables
                           select table.TableName);
            }
        }

        public List<object> GetRowsDataColumn(string dataTable, string dataColumn)
        {
            lock (_DataSource)
            {
                return GetTableNames().Contains(dataTable) && CheckField(dataTable, dataColumn)
                    ? (from DataRow row in _DataSource.Tables[dataTable].Rows
                            select row[dataColumn]).ToList()
                    : (new());
            }
        }

        public bool CheckPermission(string cmd, ViewerTypes permission)
        {
            throw new NotImplementedException();
        }

        public bool CheckShoutName(string UserName)
        {
            throw new NotImplementedException();
        }

        public string GetSocials()
        {
            throw new NotImplementedException();
        }

        public string GetUsage(string command)
        {
            throw new NotImplementedException();
        }

        public CommandData GetCommand(string cmd)
        {
            throw new NotImplementedException();
        }

        public List<Tuple<string, int, string[]>> GetTimerCommands()
        {
            throw new NotImplementedException();
        }

        public Tuple<string, int, string[]> GetTimerCommand(string Cmd)
        {
            throw new NotImplementedException();
        }

        public string GetEventRowData(ChannelEventActions rowcriteria, out bool Enabled, out short Multi)
        {
            throw new NotImplementedException();
        }

        public List<Tuple<bool, Uri>> GetWebhooks(WebhooksKind webhooks)
        {
            throw new NotImplementedException();
        }

        public bool TestInRaidData(string user, DateTime time, string viewers, string gamename)
        {
            throw new NotImplementedException();
        }

        public bool TestOutRaidData(string HostedChannel, DateTime dateTime)
        {
            throw new NotImplementedException();
        }

        public List<LearnMsgRecord> UpdateLearnedMsgs()
        {
            throw new NotImplementedException();
        }

        public List<Tuple<string, string>> GetGameCategories()
        {
            throw new NotImplementedException();
        }

        public List<string> GetCurrencyNames()
        {
            throw new NotImplementedException();
        }

        public bool CheckFollower(string User)
        {
            throw new NotImplementedException();
        }

        public bool CheckUser(LiveUser User)
        {
            throw new NotImplementedException();
        }

        public bool CheckFollower(string User, DateTime ToDateTime)
        {
            throw new NotImplementedException();
        }

        public bool CheckUser(LiveUser User, DateTime ToDateTime)
        {
            throw new NotImplementedException();
        }


        public string GetUserId(LiveUser User)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> GetKeys(string Table)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> GetCommandList()
        {
            throw new NotImplementedException();
        }

        public string GetCommands()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
