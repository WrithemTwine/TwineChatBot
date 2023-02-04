using StreamerBotLib.BotClients;
using StreamerBotLib.BotClients.Twitch;

using Xunit;

namespace TestStreamerBot
{
    public class TestBotsTwitch
    {
        private const string ChannelName = "";
        private const string BotUserName = "";
        private const string ClientId = "";
        private const string AccessToken = "";

        private BotsTwitch TwitchBot { get; set; } = new();

        private void Initialize()
        {
            TwitchBotsBase.TwitchChannelName = ChannelName;
            TwitchBotsBase.TwitchClientID = ClientId;
            TwitchBotsBase.TwitchAccessToken = AccessToken;
            TwitchBotsBase.TwitchBotUserName = BotUserName;
        }

        [Fact]
        public void TestRaid()
        {
            Initialize();

            //BotsTwitch.TwitchBotChatClient.StartBot();

            //string Category = BotsTwitch.TwitchBotUserSvc.GetUserGameCategoryName("WrithemTwine");

            //Assert.NotEqual(Category, string.Empty);

        }
    }
}
