using StreamerBotLib.Static;

namespace TestStreamerBot
{
    public class TestBotsTwitch
    {
        private const string ChannelName = "";
        private const string BotUserName = "";
        private const string ClientId = "";
        private const string AccessToken = "";

        private void Initialize()
        {
            OptionFlags.TwitchChannelName = ChannelName;
            OptionFlags.TwitchBotClientId = ClientId;
            OptionFlags.TwitchBotAccessToken = AccessToken;
            OptionFlags.TwitchBotUserName = BotUserName;
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
