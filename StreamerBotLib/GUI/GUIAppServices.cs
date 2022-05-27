using StreamerBotLib.BotClients;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreamerBotLib.GUI
{
    public class GUIAppServices : GUIBotBase, INotifyPropertyChanged
    {
        public BotOverlayServer MediaOverlayServer { get; private set; }

        public int MediaItems { get
            {
                int x = MediaOverlayServer.MediaItems;
                return x;
            } }

        public GUIAppServices()
        {
            MediaOverlayServer = BotIOController.BotController.OverlayServerBot;
            MediaOverlayServer.OnBotStarted += Service_Started;
            MediaOverlayServer.OnBotStopped += Service_Stopped;
            MediaOverlayServer.ActionQueueChanged += MediaOverlayServer_ActionQueueChanged;
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
