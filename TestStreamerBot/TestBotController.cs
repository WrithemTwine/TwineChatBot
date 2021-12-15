using StreamerBot.BotClients.Twitch;
using StreamerBot.BotIOController;
using StreamerBot.Data;
using StreamerBot.Models;
using StreamerBot.Static;
using StreamerBot.Systems;

using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Threading;

using Xunit;

namespace TestStreamerBot
{
    public class TestBotController
    {
        private bool Initialized;
        private static readonly string DataFileXML = "ChatDataStore.xml";

        private string result = string.Empty;

        private BotController botController;
        private DataManager dataManager;

        private void Initialize()
        {
            if (!Initialized)
            {
                if (File.Exists(DataFileXML))
                {
                    File.Delete(DataFileXML);
                }

                OptionFlags.FirstUserChatMsg = true;
                OptionFlags.FirstUserJoinedMsg = false;
                OptionFlags.ManageFollowers = true;
                OptionFlags.ManageOutRaidData = true;
                OptionFlags.ManageRaidData = true;
                OptionFlags.ManageStreamStats = true;
                OptionFlags.ManageUsers = true;
                OptionFlags.TwitchPruneNonFollowers = true;

                Initialized = true;

                botController = new();
                botController.Systems.PostChannelMessage += SystemsController_PostChannelMessage;
                botController.SetDispatcher(Dispatcher.CurrentDispatcher);
                dataManager = SystemsBase.DataManage;
            }
        }

        private void SystemsController_PostChannelMessage(object sender, StreamerBot.Events.PostChannelMessageEventArgs e)
        {
            result = e.Msg;
        }

        [Fact]
        public void TestBulkFollowers()
        {
            Initialize();

            DateTime followed = DateTime.Now;
            int x;

            DateTime getFollowedAt()
            {
                return followed.AddSeconds(new Random().NextDouble() * 20);
            }

            Follow GenerateFollower(string prefix)
            {
                return new()
                {
                    FromUserName = $"{prefix}Follower{x}",
                    FollowedAt = getFollowedAt(),
                    ToUserName = TwitchBotsBase.TwitchBotUserName
                };
            }

            List<Follow> bulkfollows = new();


            int bulkfollowercount = (int)(new Random().NextDouble() * 200);
            for (x = 0; x < bulkfollowercount; x++)
            {
                bulkfollows.Add(GenerateFollower("Bulk"));
            }

            List<Follow> regularfollower = new() { GenerateFollower("Reg") };

            BotController.HandleBotEventStartBulkFollowers();
            botController.HandleBotEventNewFollowers(regularfollower);
            Assert.True(dataManager.AddFollower(regularfollower[0].FromUserName, regularfollower[0].FollowedAt));

            BotController.HandleBotEventBulkPostFollowers(bulkfollows);
            BotController.HandleBotEventStopBulkFollowers();

            foreach (Follow f in bulkfollows)
            {
                Assert.False(dataManager.AddFollower(f.FromUserName, f.FollowedAt));
            }

            regularfollower.Add(GenerateFollower("Reg"));
            botController.HandleBotEventNewFollowers(regularfollower);

            Assert.False(dataManager.AddFollower(regularfollower[1].FromUserName, regularfollower[1].FollowedAt));

        }

    }
}
