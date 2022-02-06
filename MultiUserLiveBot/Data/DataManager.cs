using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Xml;

namespace MultiUserLiveBot.Data
{
    public class DataManager : INotifyPropertyChanged
    {
        private static readonly string DataFileXML = "MultiChatbotData.xml";

#if DEBUG
        private static readonly string DataFileName = Path.Combine(@"C:\Source\ChatBotApp\MultiUserLiveBot\bin\Debug\net5.0-windows", DataFileXML);
#else
        private static readonly string DataFileName = DataFileXML;
#endif

        private readonly DataSource _DataSource;

        public DataView Channels { get; set; }
        public DataView MsgEndPoints { get; set; }
        public DataView LiveStream { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string PropName)
        {
            PropertyChanged?.Invoke(this, new(PropName));
        }


        public DataManager()
        {
            _DataSource = new();
            LoadData();

            Channels = new DataView(_DataSource.Channels, null, "ChannelName", DataViewRowState.CurrentRows);
            MsgEndPoints = new DataView(_DataSource.MsgEndPoints, null, "Id", DataViewRowState.CurrentRows);
            LiveStream = new DataView(_DataSource.LiveStream, null, "ChannelName", DataViewRowState.CurrentRows);

            Channels.ListChanged += DataView_ListChanged;
            MsgEndPoints.ListChanged += DataView_ListChanged;
            LiveStream.ListChanged += DataView_ListChanged;
        }

        private void DataView_ListChanged(object sender, ListChangedEventArgs e)
        {
            OnPropertyChanged(nameof(sender));
        }

        #region Load and Exit Ops
        /// <summary>
        /// Load the data source and populate with default data
        /// </summary>
        private void LoadData()
        {
            if (!File.Exists(DataFileName))
            {
                _DataSource.WriteXml(DataFileName);
            }

            using XmlReader xmlreader = new XmlTextReader(DataFileName);
            _DataSource.ReadXml(xmlreader, XmlReadMode.DiffGram);
        }

        /// <summary>
        /// Save data to file upon exit
        /// </summary>
        public void SaveData()
        {
            lock (_DataSource)
            {
                _DataSource.AcceptChanges();
                _DataSource.WriteXml(DataFileName, XmlWriteMode.DiffGram);
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
            int x = 0;
            foreach (DataSource.LiveStreamRow liveStreamRow in _DataSource.LiveStream.Select())
            {
                if (liveStreamRow.ChannelName == ChannelName && liveStreamRow.LiveDate.ToShortDateString() == dateTime.ToShortDateString())
                {
                    x++;
                }
            }

            return x > 1;
        }
        /// <summary>
        /// Will post the channel and date event. Checks for not duplicating the event, i.e. same channel same date&time.
        /// </summary>
        /// <param name="ChannelName">The name of the channel for the event.</param>
        /// <param name="dateTime">The date of the event.</param>
        /// <returns>true if the event posted. false if the date & time duplicates.</returns>
        public bool PostStreamDate(string ChannelName, DateTime dateTime)
        {
            foreach (DataSource.LiveStreamRow liveStreamRow in _DataSource.LiveStream.Select())
            {
                if (liveStreamRow.ChannelName == ChannelName && liveStreamRow.LiveDate == dateTime)
                {
                    return false;
                }
            }

            _DataSource.LiveStream.AddLiveStreamRow(ChannelName, dateTime);
            SaveData();
            OnPropertyChanged(nameof(LiveStream));

            return true;
        }

        /// <summary>
        /// Retrieves the list of channel names from the Channels table.
        /// </summary>
        /// <returns>A list of strings from the Channels table.</returns>
        public List<string> GetChannelNames()
        {
            List<string> channels = new();

            foreach (DataSource.ChannelsRow c in _DataSource.Channels.Select())
            {
                channels.Add(c.ChannelName);
            }

            return channels;
        }

        /// <summary>
        /// Retrieve the Endpoints links where the user wants to post live messages.
        /// </summary>
        /// <returns>A list of URI objects for the Endpoint links.</returns>
        public List<Tuple<string, Uri>> GetWebLinks()
        {
            List<Tuple<string, Uri>> links = new();

            foreach (DataSource.MsgEndPointsRow MsgEndPointsRow in (DataSource.MsgEndPointsRow[])_DataSource.MsgEndPoints.Select())
            {
                links.Add(new(MsgEndPointsRow.Type, new(MsgEndPointsRow.URL)));
            }

            return links;
        }
    }
}
