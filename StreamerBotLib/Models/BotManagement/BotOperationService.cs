using StreamerBotLib.BotClients;
using StreamerBotLib.Models.Enums;

namespace StreamerBotLib.Models.BotManagement
{
    public class BotOperationService(BotOverlayServer botdata, Platform platform = Platform.Service) : BotOperationBase(platform)
    {
        private readonly BotOverlayServer botOverlayServer = botdata;

        public override Task<bool> TryBuildApiAsync()
        {
            return Task.Run(() =>
            {
                ApiReady = true;
                return ApiReady;
            });
        }

        public override Task<bool> TryCheckApiReadyAsync()
        {
            return Task.Run(() =>
            {
                ApiReady = true;
                return ApiReady;
            });
        }

        public override Task<bool> TryCheckAuthorizeAsync()
        {
            return Task.Run(() =>
            {
                Authorized = true;
                return Authorized;
            });
        }

        public override Task<bool> TryCheckBotsActiveAsync()
        {
            return Task.Run(() =>
            {
                BotsActive = botOverlayServer.IsActive == true;
                return BotsActive;
            });
        }

        public override Task<bool> TryCheckBotsActiveAsync(Bots bot)
        {
            return Task.Run(() =>
            {
                if (bot == botOverlayServer.BotClientName)
                {
                    BotsActive = botOverlayServer.IsActive == true;
                }
                return BotsActive;
            });
        }

        public override Task<bool> TryCheckReadyToAuthorizeAsync()
        {
            return Task.Run(() =>
            {
                ReadyToAuthorize = true;
                return ReadyToAuthorize;
            });
        }
    }
}
