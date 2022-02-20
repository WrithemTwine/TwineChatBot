using StreamerBotLib.BotClients.Twitch;
using StreamerBotLib.BotIOController;
using StreamerBotLib.Data;
using StreamerBotLib.Enums;
using StreamerBotLib.Events;
using StreamerBotLib.Models;
using StreamerBotLib.Static;
using StreamerBotLib.Systems;

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
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
                botController.OutputSentToBots += BotController_OutputSentToBots;
                       
                botController.SetDispatcher(Dispatcher.CurrentDispatcher);
                dataManager = SystemsBase.DataManage;
            }
        }

        private OnGetChannelGameNameEventArgs GetRandomGameIdName()
        {
            if (!Initialized)
            {
                Initialize();
            }

            List<Tuple<string, string>> output = dataManager.GetGameCategories();
            Random random = new();
            Tuple<string, string> itemfound = output[random.Next(output.Count)];

            return new() { GameId = itemfound.Item1, GameName = itemfound.Item2 };
        }

        private void BotController_OutputSentToBots(object sender, StreamerBotLib.Events.PostChannelMessageEventArgs e)
        {
            result = e.Msg;
        }

        [Theory]
        [InlineData(600)]
        [InlineData(100000)]
        public void TestBulkFollowers(int PickFollowers)
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

            int bulkfollowercount = (int)(new Random().NextDouble() * PickFollowers);
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

            Thread.Sleep(5000); // wait enough time for the regular followers to add into the database
            Assert.False(dataManager.AddFollower(regularfollower[0].FromUserName, regularfollower[0].FollowedAt));
        }

        [Fact]
        public void TestAddClips()
        {
            Initialize();

            List<Clip> clips = new() {
                new()
                {
                    ClipId = "3489249",
                    CreatedAt = DateTime.Now.ToString(),
                    Duration = 20.5F,
                    GameId = "7193",
                    Url = "http://www.flightsimulator.com",
                    Title = "Ready for TakeOff",
                    Language = "en"
                }
            };

            botController.HandleBotEventPostNewClip(clips);

            Assert.False(dataManager.AddClip(clips[0].ClipId, clips[0].CreatedAt, clips[0].Duration, clips[0].GameId, clips[0].Language, clips[0].Title, clips[0].Url));
        }

        [Fact]
        public void TestStreamOnUpdateOff()
        {
            Initialize();

            string Title = "Let's try this stream test!";
            DateTime onlineTime = DateTime.Now.ToLocalTime();
            string Id = "7193";
            string Category = "Microsoft Flight Simulator";

            botController.HandleOnStreamOnline(TwitchBotsBase.TwitchChannelName, Title, onlineTime, Id, Category);

            Thread.Sleep(1000);
            Assert.False(dataManager.AddStream(onlineTime));
            Assert.True(dataManager.AddCategory(Id, Category));

            string newId = "981578";
            string newCategory = "DebugStreamCategory";

            BotController.HandleOnStreamUpdate(newId, newCategory);

            Assert.True(dataManager.AddCategory(newId, newCategory));

            BotController.HandleOnStreamOffline();
        }

        [Theory]
        [InlineData("DarkStreamPhantom", "25", "Tier 1", "Tier1 Subscription")]
        [InlineData("SevenOf9", "6", "Tier 2", "Tier2 Subscription")]
        public void TestNewSubscriber(string DisplayName, string Months, string Subscription, string SubscriptionName)
        {
            Initialize();

            botController.HandleNewSubscriber(DisplayName, Months, Subscription, SubscriptionName);

            Thread.Sleep(800);

            string subeventmsg = VariableParser.ParseReplace(LocalizedMsgSystem.GetEventMsg(ChannelEventActions.Subscribe, out bool Enabled, out _), VariableParser.BuildDictionary(new Tuple<MsgVars, string>[] { new(MsgVars.user, DisplayName), new(MsgVars.submonths, FormatData.Plurality(Months, MsgVars.Pluralmonth, Prefix: LocalizedMsgSystem.GetVar(MsgVars.Total))), new(MsgVars.subplan, Subscription), new(MsgVars.subplanname, SubscriptionName) }));

            Assert.Equal(Enabled ? subeventmsg : "", result);
        }

        [Fact]
        public void TestResubscribe()
        {
            Initialize();



        }

        [Fact]
        public void TestManyFunctions()
        {
            Initialize();

            string Title = "Let's try this stream test!";
            DateTime onlineTime = DateTime.Now.ToLocalTime();
            string Id = "7193";
            string Category = "Microsoft Flight Simulator";

            botController.HandleOnStreamOnline(TwitchBotsBase.TwitchChannelName, Title, onlineTime, Id, Category);

            botController.HandleIncomingRaidData("Pitcy", DateTime.Now.ToLocalTime(), "13", "Fortnite", Bots.TwitchChatBot);
            botController.HandleUserJoined(new() { "Pitcy", "DarkStreamPhantom", "OutlawTorn14", "MrTopiczz", "pitcyissmelly" }, Bots.TwitchChatBot);
            botController.HandleChatCommandReceived(new() { UserType = ViewerTypes.Mod, DisplayName="Pitcy", IsModerator = true, Message = "!followage" }, Bots.TwitchChatBot);

        }

        [Fact]
        public void TestGiveaway()
        {
            Initialize();

            OnGetChannelGameNameEventArgs randomGame = GetRandomGameIdName();
            botController.HandleOnStreamOnline("writhemtwine", "Test Giveaway", DateTime.Now.ToLocalTime(), randomGame.GameId, randomGame.GameName);

            OptionFlags.GiveawayBegMsg = "Test Project Begin the Giveaway";
            OptionFlags.GiveawayEndMsg = "Test Project End the Giveaway";
            OptionFlags.GiveawayWinMsg = "Test Project the winner is #winner!";
            OptionFlags.GiveawayMultiEntries = 5;
            OptionFlags.GiveawayMultiUser = true;
            OptionFlags.GiveawayCount = 2;
            OptionFlags.ManageGiveawayUsers = true;

            List<string> entrylist = new() { "CuteChibiChu", "WrithemTwine", "DarkStreamPhantom", "OutlawTorn14" };

            botController.HandleGiveawayBegin(GiveawayTypes.Command, "giveaway");

            foreach(string s in entrylist)
            {
                int y = 0;
                while (y < OptionFlags.GiveawayMultiEntries)
                {
                    botController.HandleGiveawayPostName(s);
                    y++;
                }
            }

            botController.HandleGiveawayEnd();
            botController.HandleGiveawayWinner();

        }
    }
}
