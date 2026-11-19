using StreamerBotLib.BotClients;
using StreamerBotLib.Models.Enums;

namespace StreamerBotLib.BotIOController
{
    public partial class BotController
    {
        public void Services_OnBotStarted(object sender, EventArgs e)
        { 
            HandleBotStatusChanged(new()
            {
                Platform = Platform.Service,
                Bots = ((IOModule)sender).BotClientName,
                Started = true,
                Stopped = false
            });
        }

        public void Services_OnBotStopped(object sender, EventArgs e)
        {
            HandleBotStatusChanged(new()
            {
                Platform = Platform.Service,
                Bots = ((IOModule)sender).BotClientName,
                Started = false,
                Stopped = true
            });
        }

        public void Services_OnBotFailedStart(object sender, EventArgs e)
        {
            HandleBotStatusChanged(new()
            {
                Platform = Platform.Service,
                Bots = ((IOModule)sender).BotClientName,
                Started = false,
                Stopped = true,
                Error = true
            });
        }
    }
}
