using StreamerBotLib.Models.BotManagement;
using StreamerBotLib.Models.Enums;

namespace StreamerBotLib.BotIOController
{
    /// <summary>
    /// Defines the Bot status for the GUI, and build the API objects
    /// </summary>
    public partial class BotController
    {
        private List<BotOperationBase> BotOperationStatus = [];

        public bool GetBotOperationStatus(Platform platform, BotOperationTypes botOperationType)
        {
            var botOperation = BotOperationStatus.FirstOrDefault(x => x.Platform == platform);
            if (botOperation == null)
            {
                return false;
            }
            return botOperation[botOperationType];
        }

        public void PerformBotOperation(BotOperationTypes botOperationTypes, Platform p = Platform.Default, Bots bots = Bots.Default)
        {
            List<BotOperationBase> performTask = (p == Platform.Default) ?  BotOperationStatus : [BotOperationStatus.FirstOrDefault(b => b.Platform == p)];

            foreach(var bot in performTask)
            {
                _ = bot.PerformOperation(botOperationTypes, bots: bots);
            }
        }

        public void StartBot(Platform p = Platform.Default, BotIdKey key = null)
        {
            switch (p)
            {
                case Platform.Service:
                    OverlayServerBot.StartBot();
                    break;
                case Platform.Twitch:
                    TwitchBots.StartBots(key?.Bots);
                    break;
                case Platform.Default:
                    OverlayServerBot.StartBot();
                    TwitchBots.StartBots(null);
                    break;
                default:
                    break;
            }
        }

        public void StartAutoBots()
        {
            OverlayServerBot.StartAutoBot();
            TwitchBots.StartAutoBots();
        }

        public void StopBot(Platform p = Platform.Default, BotIdKey key = null)
        {
            switch (p)
            {
                case Platform.Service:
                    OverlayServerBot.StopBot();
                    break;
                case Platform.Twitch:
                    TwitchBots.StopBots(key?.Bots);
                    break;
                case Platform.Default:
                    OverlayServerBot.StopBot();
                    TwitchBots.StopBots(null);
                    break;
                default:
                    break;
            }
        }
    }
}
