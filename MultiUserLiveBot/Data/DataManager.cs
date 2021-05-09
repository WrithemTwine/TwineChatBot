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
        private static readonly string DataFileName = Path.Combine(Directory.GetCurrentDirectory(), "MultiChatbotData.xml");

        private DataSource _DataSource;

        public event PropertyChangedEventHandler PropertyChanged;

        public DataView Channels { get; private set; }
        public DataView Discord { get; private set; }
        public DataView LiveStream { get; private set; }
        
        public DataManager()
        {
            _DataSource = new();
            LoadData();
            
            Channels = new(_DataSource.Channels, null, "ChannelName", DataViewRowState.CurrentRows);
            Discord = new(_DataSource.Discord, null, "Id", DataViewRowState.CurrentRows);
            LiveStream = new(_DataSource.LiveStream, null, "ChannelName", DataViewRowState.CurrentRows);
        }

        private void NotifyPropertyChanged(string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
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

            using (XmlReader xmlreader = new XmlTextReader(DataFileName))
            {
                _DataSource.ReadXml(xmlreader, XmlReadMode.DiffGram);
            }

            SaveData();
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

        private DataSource.LiveStreamRow[] GetLiveStreamRows(string ChannelName)
        {
            return (DataSource.LiveStreamRow[])_DataSource.LiveStream.Select();
        }

        /// <summary>
        /// Checks if the channel has already posted a stream for today.
        /// </summary>
        /// <param name="ChannelName">Name of channel.</param>
        /// <param name="dateTime">The time of the stream.</param>
        /// <returns>true if the channel and date have already posted 2+ events for the same day. false if there is no match or just 1 event post for the current date.</returns>
        public bool CheckStreamDate(string ChannelName, DateTime dateTime)
        {
            int x = 0;
            foreach(DataSource.LiveStreamRow liveStreamRow in GetLiveStreamRows(ChannelName))
            {
                if (liveStreamRow.ChannelName == ChannelName && liveStreamRow.LiveDate.ToShortDateString()==dateTime.ToShortDateString())
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
            foreach (DataSource.LiveStreamRow liveStreamRow in GetLiveStreamRows(ChannelName))
            {
                if (liveStreamRow.ChannelName == ChannelName && liveStreamRow.LiveDate == dateTime)
                {
                    return false;
                }
            }

            _DataSource.LiveStream.AddLiveStreamRow(ChannelName, dateTime);
            _DataSource.AcceptChanges();
            SaveData();
            NotifyPropertyChanged("LiveStream");
            return true;
        }

        public List<string> GetChannelNames()
        {
            DataSource.ChannelsRow[] channels = (DataSource.ChannelsRow[])_DataSource.Channels.Select();
            List<string> list = new();

            foreach(DataSource.ChannelsRow row in channels)
            {
                list.Add(row.ChannelName);
            }

            return list;
        }

        public List<Uri> GetDiscordLinks()
        {
            List<Uri> links = new();

            foreach( DataSource.DiscordRow discordRow in (DataSource.DiscordRow[]) _DataSource.Discord.Select())
            {
                links.Add(new(discordRow.URL));
            }

            return links;
        }
    }
}
