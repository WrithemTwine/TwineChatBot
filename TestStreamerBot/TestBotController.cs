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
        private const int Viewers = 80;

        private BotController botController;
        private DataManager dataManager;

        private Random Random { get; set; } = new();

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

        private void BotController_OutputSentToBots(object sender, PostChannelMessageEventArgs e)
        {
            result = e.Msg;
        }

        [Theory]
        [InlineData(600)]
        [InlineData(10)]
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

            botController.HandleOnStreamUpdate(newId, newCategory);

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

        [Theory]
        [InlineData("xFreakDuchessx")]
        public void TestRaid(string RaidUserName)
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

            botController.HandleIncomingRaidData(RaidUserName, DateTime.Now, Random.Next(5, Viewers).ToString(), GetRandomGameIdName().GameName, Bots.TwitchChatBot);
            Thread.Sleep(5000); // wait for category

            Assert.True(StatisticsSystem.UserChat(RaidUserName));

            botController.HandleUserJoined(new() { RaidUserName }, Bots.TwitchChatBot);
            Assert.False(StatisticsSystem.UserChat(RaidUserName));

            botController.HandleUserLeft(RaidUserName);

            Thread.Sleep(2000);

            Assert.False(StatisticsSystem.UserChat(RaidUserName)); // should be able to add the user again

        }

        [Fact]
        public void TestNewClip()
        {
            string ClipName = Path.GetRandomFileName();

            Initialize();
            OptionFlags.TwitchClipPostChat = true;

            botController.HandleBotEventPostNewClip(new()
            {
                new()
                {
                    ClipId = ClipName,
                    CreatedAt = DateTime.Now.ToString(),
                    Duration = Random.Next(30),
                    GameId = GetRandomGameIdName().GameId,
                    Language = "English",
                    Title = "My Random Test Clip",
                    Url = $"http://debug.app/{ClipName}"
                }
            });

            Thread.Sleep(2000);

            Assert.NotEmpty(result);
        }

        [Theory]
        [InlineData("BanMeUser1","Buy follows or else")]
        [InlineData("BanMeUser2", "You must buy follows or else", true)]
        [InlineData("BanMeUser3", "You're an idiot if you don't buy follows or else", false, true)]
        [InlineData("BanMeUser4", "Screw you if you don't buy follows or else", true, true)]
        public void TestUserBanned(string UserName, string Msg, bool Joined = false, bool JoinBan = false)
        {
            Initialize();

            string Title = "Let's try this stream test!";
            DateTime onlineTime = DateTime.Now.ToLocalTime();

            OnGetChannelGameNameEventArgs random = GetRandomGameIdName();

            string Id = random.GameId;
            string Category = random.GameName;

            // go online
            botController.HandleOnStreamOnline(TwitchBotsBase.TwitchChannelName, Title, onlineTime, Id, Category);

            // wait a bit
            Thread.Sleep(20000);

            // add some good/friendly users, waiting for joining
            foreach (string U in new List<string> { "DarkStreamPhantom", "Jijijava", "CuteChibiChu", "BlkbryOnline", "NewUser", "OutlawTorn14" })
            {
                botController.HandleUserJoined(new() { U }, Bots.TwitchChatBot);
                Thread.Sleep(Random.Next(10000, 80000));
                botController.HandleMessageReceived(new() { DisplayName=U, IsSubscriber = 0 == Random.Next(0, 1), Message="Hey stud!" }, Bots.TwitchChatBot);
            }

            // wait a little more
            Thread.Sleep(18000);

            // receive the hostile ban message
            botController.HandleMessageReceived(new() { DisplayName = UserName, IsSubscriber = 0 == Random.Next(0, 1), Message = Msg }, Bots.TwitchChatBot);

            // wait a moment to recognize the message
            Thread.Sleep(5000);

            // ban before or after they join, or don't join at all
            if(!JoinBan)
            {
                botController.HandleUserBanned(UserName);
                Thread.Sleep(5000);
            }

            if (Joined)
            {
                botController.HandleUserJoined(new() { UserName }, Bots.TwitchChatBot);
                Thread.Sleep(5000);
            }

            if (JoinBan)
            {
                botController.HandleUserBanned(UserName);
            }

        }
    }
}
