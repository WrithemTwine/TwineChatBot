using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

using TwitchLib.Client.Events;

namespace MultiUserLiveBot.Clients
{
    public partial class TwitchLiveBot : INotifyPropertyChanged
    {
        private const int maxlength = 8000;

        public string MultiLiveStatusLog { get; set; } = "";

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Event to handle when the Twitch client sends and event. Updates the StatusLog property with the logged activity.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The payload of the event.</param>
        private void TwitchChat_OnLog(object sender, OnLogArgs e)
        {
            void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }

            if (MultiLiveStatusLog.Length + e.DateTime.ToLocalTime().ToString().Length + e.Data.Length + 2 >= maxlength)
            {
                MultiLiveStatusLog = MultiLiveStatusLog[MultiLiveStatusLog.IndexOf('\n')..];
            }

            MultiLiveStatusLog += e.DateTime.ToLocalTime().ToString() + " " + e.Data + "\n";

            NotifyPropertyChanged(nameof(MultiLiveStatusLog));
        }

        public void LogEntry(string data, DateTime dateTime)
        {
            TwitchChat_OnLog(this, new OnLogArgs() { Data = data, DateTime = dateTime });
        }
    }
}
