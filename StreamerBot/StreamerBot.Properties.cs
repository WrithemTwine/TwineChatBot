using StreamerBotLib.BotIOController;
using StreamerBotLib.GUI;

using System.ComponentModel;

namespace StreamerBot
{
    public partial class StreamerBotWindow : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propname)
        {
            PropertyChanged?.Invoke(this, new(propname));
        }


        // class properties in different partial classes
        private readonly DateTime StartBotDate;
        private DateTime TwitchFollowRefresh;

        private DateTime ChannelPtRetrievalDate = DateTime.MinValue;
        private readonly TimeSpan ChannelPtRefresh = new(0, 15, 0);

        private int TwitchFollowerCurrRefreshHrs = 0;
        private readonly TimeSpan CheckRefreshDate = new(7, 0, 0, 0);



        private readonly GUIAppStats guiAppStats;

        internal static BotController Controller { get; private set; }

        internal static GUIBotBridge BotBridge { get; private set; }

    }
}
