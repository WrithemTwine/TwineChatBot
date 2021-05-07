using System.Data;
using System.IO;
using System.Xml;

namespace MultiUserLiveBot.Data
{
    public class DataManager
    {
        private static readonly string DataFileName = "MultiChatbotData.xml";

        private DataSource _DataSource;

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



    }
}
