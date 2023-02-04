using StreamerBotLib.Enums;
using StreamerBotLib.Static;
using StreamerBotLib.Systems;

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

        public TestSystemsController()
        {
            systemsController = new();
            Initialize();
        }

        private void Initialize()
        {
            if (!Initialized)
            {
                if (File.Exists(DataFileXML))
                {
                    File.Delete(DataFileXML);
                }
                systemsController.PostChannelMessage += SystemsController_PostChannelMessage;

                OptionFlags.FirstUserChatMsg = true;
                OptionFlags.FirstUserJoinedMsg = false;

                Initialized = true;
            }
        }

        private void SystemsController_PostChannelMessage(object? sender, StreamerBotLib.Events.PostChannelMessageEventArgs e)
        {
            result = e.Msg;
        }

        [Fact]
        public void UserJoined()
        {
            Initialize();

            Assert.True(OptionFlags.FirstUserChatMsg);
            Assert.False(OptionFlags.FirstUserJoinedMsg);

            systemsController.UserJoined(new() { new("DarkStreamPhantom" , Platform.Twitch)});
            Assert.Empty(result);
        }

        [Fact]
        public void UserChat()
        {
            Initialize();

            Assert.True(OptionFlags.FirstUserChatMsg);
            Assert.False(OptionFlags.FirstUserJoinedMsg);

            systemsController.UserJoined(new() { new( "DarkStreamPhantom", Platform.Twitch) });

            result = string.Empty;
            
            systemsController.UserJoined(new() { new("DarkStreamPhantom", Platform.Twitch) });
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

            systemsController.PostIncomingRaid(new(RaidName, Platform.Twitch), RaidTime, viewers, Category);
            SystemsController.PostOutgoingRaid(RaidName, RaidTime);

            Assert.True(SystemsController.DataManage.TestInRaidData(RaidName, RaidTime, viewers, Category));
            Assert.True(SystemsController.DataManage.TestOutRaidData(RaidName, RaidTime));
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

            systemsController.UserJoined(new() { new(UserName, Platform.Twitch) });
            Assert.True(SystemsController.DataManage.CheckUser(new(UserName, Platform.Twitch)));
            systemsController.UserLeft(new(UserName, Platform.Twitch));
        }
    }
}
