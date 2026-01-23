using StreamerBotLib.Models.Enums;
using StreamerBotLib.Models.Events;
using StreamerBotLib.Static;
using StreamerBotLib.Systems;

namespace TestStreamerBot
{
    public class TestSystemsController
    {
        private bool Initialized;

        private string result = string.Empty;

        private readonly DataBot dataBot;
        //private readonly DataManagerSQL DataManage;

        public TestSystemsController()
        {
            dataBot = new();
            Initialize();
            //DataManage = dataBot.GetDataManager();
        }

        private void Initialize()
        {
            if (!Initialized)
            {
                dataBot.SetPostChannelMessageHandler(SystemsController_PostChannelMessage);

                OptionFlags.FirstUserChatMsg = true;
                OptionFlags.FirstUserJoinedMsg = false;

                Initialized = true;
            }
        }

        private void SystemsController_PostChannelMessage(object? sender, PostChannelMessageEventArgs e)
        {
            result = e.Msg;
        }

        [Fact]
        public void UserJoined()
        {
            Initialize();

            Assert.True(OptionFlags.FirstUserChatMsg);
            Assert.False(OptionFlags.FirstUserJoinedMsg);

            dataBot.UserJoined([new("DarkStreamPhantom", Platform.Twitch)]);
            Assert.Empty(result);
        }

        [Fact]
        public void UserChat()
        {
            Initialize();

            Assert.True(OptionFlags.FirstUserChatMsg);
            Assert.False(OptionFlags.FirstUserJoinedMsg);

            dataBot.UserJoined([new("DarkStreamPhantom", Platform.Twitch)]);

            result = string.Empty;

            dataBot.UserJoined([new("DarkStreamPhantom", Platform.Twitch)]);
            Assert.Empty(result);
        }

        [Fact]
        public void RaidData()
        {
            Initialize();

            OptionFlags.ManageRaidData = true;
            OptionFlags.ManageOutRaidData = true;

            string RaidName = "CuteChibiChu";
            int viewers = 5000;
            string Category = "New World";
            DateTime RaidTime = DateTime.Now;

            dataBot.PostIncomingRaid(new(RaidName, Platform.Twitch), RaidTime, viewers, new("3813210654", Category));
            dataBot.PostOutgoingRaid(RaidName, RaidTime);
        }

        [Theory]
        [InlineData("CuteChibiChu")]
        [InlineData("DarkStreamPhantom")]
        [InlineData("SevenOf9")]
        public void UserJoinLeave(string UserName)
        {
            Initialize();
            OptionFlags.ManageUsers = true;
            OptionFlags.IsStreamOnline = true;

            dataBot.UserJoined([new(UserName, Platform.Twitch)]);
            dataBot.UserLeft(new(UserName, Platform.Twitch));
        }
    }
}
