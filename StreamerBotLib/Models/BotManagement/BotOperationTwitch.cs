using StreamerBotLib.BotClients;
using StreamerBotLib.Models.Enums;
using StreamerBotLib.Static;

namespace StreamerBotLib.Models.BotManagement
{
    public class BotOperationTwitch(BotsTwitch botdata, Platform platform = Platform.Twitch) : BotOperationBase(platform)
    {
        private readonly BotsTwitch twitchBots = botdata;

        public override async Task<bool> TryBuildApiAsync()
        {
            return await Task.Run(async () =>
            {
                if (Authorized)
                {
                    await twitchBots.InitializeHelix();
                    ApiReady = BotsTwitch.ConfirmHelixApiObjects();
                }
                return ApiReady;
            }).ConfigureAwait(false);

        }

        public override Task<bool> TryCheckApiReadyAsync()
        {
            return ApiReady ? Task.FromResult(true) : Task.FromResult(false);
        }

        public override Task<bool> TryCheckAuthorizeAsync()
        {
            return Task.Run(() =>
            {
                List<string> fieldstrings = [];

                if (OptionFlags.TwitchTokenUseAuth)
                {
                    fieldstrings.Add(OptionFlags.TwitchAuthBotAuthCode);
                    if (OptionFlags.TwitchStreamerUseToken)
                    {
                        fieldstrings.Add(OptionFlags.TwitchAuthStreamerAuthCode);
                    }
                }
                else
                {
                    fieldstrings.Add(OptionFlags.TwitchBotAccessToken);
                    if (OptionFlags.TwitchStreamerUseToken)
                    {
                        fieldstrings.Add(OptionFlags.TwitchStreamerAccessToken);
                    }
                }

                Authorized = ReadyToAuthorize && !CalculateEmptyString(fieldstrings);

                return Authorized;
            });
        }

        public override Task<bool> TryCheckBotsActiveAsync()
        {
            return Task.Run(() =>
            {
                BotsActive = twitchBots.BotsList.Cast<IOModule>().Any(b => b.IsActive == true);
                return BotsActive;
            });
        }

        public override Task<bool> TryCheckBotsActiveAsync(Bots bot)
        {
            return Task.Run(() =>
            {
                var foundBot = (IOModule)twitchBots.BotsList.Where(b => b.BotClientName == bot).Select(c => c).FirstOrDefault();

                return foundBot == default ? false : foundBot.IsActive == true;
            });
        }

        public override Task<bool> TryCheckReadyToAuthorizeAsync()
        {
            return Task.Run(() =>
            {
                List<string> fieldchecks = [string.Empty];

                if (!OptionFlags.CheckSettingIsDefault(nameof(OptionFlags.TwitchChannelName))
                    && !OptionFlags.CheckSettingIsDefault(nameof(OptionFlags.TwitchBotUserName)))
                {
                    fieldchecks.Clear();
                    if (OptionFlags.TwitchTokenUseAuth)
                    {
                        // authcode access token mode
                        fieldchecks.AddRange([ OptionFlags.TwitchBotUserName,
                            OptionFlags.TwitchAuthBotClientId,
                            OptionFlags.TwitchAuthBotClientSecret,
                            OptionFlags.TwitchChannelName]);
                        if (OptionFlags.TwitchStreamerUseToken)
                        {
                            fieldchecks.AddRange([OptionFlags.TwitchAuthStreamerClientId,
                                OptionFlags.TwitchAuthStreamerClientSecret]);
                        }
                    }
                    else
                    {
                        // manual access token mode
                        fieldchecks.AddRange([OptionFlags.TwitchBotUserName,
                            OptionFlags.TwitchBotClientId,
                            OptionFlags.TwitchBotAccessToken,
                            OptionFlags.TwitchChannelName]);

                        if (OptionFlags.TwitchStreamerUseToken)
                        {
                            fieldchecks.AddRange([OptionFlags.TwitchStreamerClientId,
                                OptionFlags.TwitchStreamerAccessToken,
                                OptionFlags.TwitchStreamerNoScopesAccessToken]);
                        }
                    }
                }

                UseSecondAccount = OptionFlags.TwitchStreamerUseToken;
                ReadyToAuthorize = !CalculateEmptyString(fieldchecks);

                return ReadyToAuthorize;
            });
        }
    }
}
