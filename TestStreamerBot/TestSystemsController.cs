using StreamerBot.Static;
using StreamerBot.Systems;

using System;
using System.IO;

using Xunit;

namespace TestStreamerBot
{
    public class TestSystemsController
    {
        private bool Initialized;
        private static readonly string DataFileXML = "ChatDataStore.xml";

        private string result = string.Empty;

        private SystemsController systemsController;

        private void Initialize()
        {
            if (!Initialized)
            {
                if (File.Exists(DataFileXML))
                {
                    File.Delete(DataFileXML);
                }
                systemsController = new();
                systemsController.PostChannelMessage += SystemsController_PostChannelMessage;
                OptionFlags.SetSettings();

                OptionFlags.FirstUserChatMsg = true;
                OptionFlags.FirstUserJoinedMsg = false;

                Initialized = true;
            }
        }

        private void SystemsController_PostChannelMessage(object sender, StreamerBot.Events.PostChannelMessageEventArgs e)
        {
            result = e.Msg;
        }

        [Fact]
        public void UserJoined()
        {
            Initialize();

            Assert.True(OptionFlags.FirstUserChatMsg);
            Assert.False(OptionFlags.FirstUserJoinedMsg);

            systemsController.UserJoined(new() { "DarkStreamPhantom" }, StreamerBot.Enum.Bots.TwitchChatBot);
            Assert.Empty(result);
        }

        [Fact]
        public void UserChat()
        {
            Initialize();

            Assert.True(OptionFlags.FirstUserChatMsg);
            Assert.False(OptionFlags.FirstUserJoinedMsg);

            systemsController.UserJoined(new() { "DarkStreamPhantom" }, StreamerBot.Enum.Bots.TwitchChatBot);

            result = string.Empty;

            systemsController.AddChat("DarkStreamPhantom", StreamerBot.Enum.Bots.TwitchChatBot);
            Assert.Empty(result);
        }

        [Fact]
        public void RaidData()
        {
            Initialize();

            OptionFlags.ManageRaidData = true;
            OptionFlags.ManageOutRaidData = true;

            string RaidName = "CuteChibiChu";
            string viewers = "5000";
            string Category = "New World";
            DateTime RaidTime = DateTime.Now;

            systemsController.PostIncomingRaid(RaidName, RaidTime, viewers, Category, StreamerBot.Enum.Bots.TwitchChatBot);
            SystemsController.PostOutgoingRaid(RaidName, RaidTime);

            Assert.True(SystemsBase.DataManage.TestInRaidData(RaidName, RaidTime, viewers, Category));
            Assert.True(SystemsBase.DataManage.TestOutRaidData(RaidName, RaidTime));
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

            systemsController.UserJoined(new() { UserName }, StreamerBot.Enum.Bots.TwitchChatBot);
            Assert.True(SystemsController.DataManage.CheckUser(UserName));
            SystemsController.UserLeft(UserName);
        }
    }
}
