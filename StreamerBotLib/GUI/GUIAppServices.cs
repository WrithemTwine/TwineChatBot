using StreamerBotLib.BotClients;

using System.ComponentModel;

namespace StreamerBotLib.GUI
{
    public class GUIAppServices : GUIBotBase, INotifyPropertyChanged
    {
        private string _ADataDir = "";
        public string AppDataDirectory
        {
            get { return _ADataDir; }
            set { _ADataDir = value; NotifyPropertyChanged(nameof(AppDataDirectory)); }
        }

        public BotOverlayServer MediaOverlayServer { get; private set; } = BotIOController.BotController.OverlayServerBot;

        public int MediaItems
        {
            get
            {
                int x = MediaOverlayServer.MediaItems;
                return x;
            }
        }

        public GUIAppServices()
        {
            MediaOverlayServer = BotIOController.BotController.OverlayServerBot;
            MediaOverlayServer.OnBotStarted += Service_Started;
            MediaOverlayServer.OnBotStopped += Service_Stopped;
            MediaOverlayServer.ActionQueueChanged += MediaOverlayServer_ActionQueueChanged;

            AppDataDirectory = "";
        }

        private void MediaOverlayServer_ActionQueueChanged(object sender, EventArgs e)
        {
            NotifyPropertyChanged(nameof(MediaItems));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propname)
        {
            PropertyChanged?.Invoke(this, new(propname));
        }

        private void Service_Started(object sender, EventArgs e)
        {
            IOModule currbot = sender as IOModule;
            BotStarted(new() { BotName = currbot.BotClientName, Started = currbot.IsStarted, Stopped = currbot.IsStopped });
        }

        private void Service_Stopped(object sender, EventArgs e)
        {
            IOModule currbot = sender as IOModule;
            BotStopped(new() { BotName = currbot.BotClientName, Started = currbot.IsStarted, Stopped = currbot.IsStopped });
        }
    }
}
