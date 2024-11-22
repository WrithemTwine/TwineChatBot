using StreamerBotLib.BotIOController;
using StreamerBotLib.Enums;
using StreamerBotLib.Events;
using StreamerBotLib.Interfaces;
using StreamerBotLib.Models;
using StreamerBotLib.Properties;
using StreamerBotLib.Static;
using StreamerBotLib.Systems;

using System.ComponentModel.DataAnnotations;
using System.Windows.Threading;

namespace TestStreamerBot
{
    [Collection("Sequential")]
    public class TestBotController
    {
        private bool Initialized;

        private string result = string.Empty;
        private const int Viewers = 80;

        private readonly string DatabaseName = "TestSteamerDatabase.db";

        [Required]
        private BotController botController;
        [Required]
        private IDataManagerTestMethods dataManager;

        private Random Random { get; set; } = new();

        public TestBotController()
        {
            Initialize();
        }

        private void Initialize()
        {
            lock (DatabaseName)
            {
                if (!Initialized)
                {
                    Settings.Default.EFCConnectStringSqlite = $"Data Source={DatabaseName}";
                    OptionFlags.EFCDatabaseProviderSqlite = true;
                    OptionFlags.EFCDataImportedDataGram = true;

                    OptionFlags.FirstUserChatMsg = true;
                    OptionFlags.FirstUserJoinedMsg = false;
                    OptionFlags.ManageFollowers = true;
                    OptionFlags.ManageOutRaidData = true;
                    OptionFlags.ManageRaidData = true;
                    OptionFlags.ManageStreamStats = true;
                    OptionFlags.ManageUsers = true;
                    OptionFlags.TwitchPruneNonFollowers = true;

                    OptionFlags.MediaOverlayEnabled = true;
                    OptionFlags.MediaOverlayChannelPoints = true;
                    OptionFlags.MediaOverlayShoutoutClips = true;

                    Initialized = true;

                    if (File.Exists(DatabaseName))
                    {
                        File.Delete(DatabaseName);
                    }
                    botController = new();

                    botController.OutputSentToBots += BotController_OutputSentToBots;

                    botController.SetDispatcher(Dispatcher.CurrentDispatcher);
                    dataManager = (IDataManagerTestMethods)SystemsController.DataManage;
                }
            }
        }

        private OnGetChannelGameNameEventArgs GetRandomGameIdName()
        {
            if (!Initialized)
            {
                Initialize();
            }

            List<CategoryData> output = dataManager.GetGameCategories();
            Random random = new();
            CategoryData itemfound = output[random.Next(output.Count)];

            return new() { GameId = itemfound.CategoryId, GameName = itemfound.CategoryName };
        }

        private void BotController_OutputSentToBots(object? sender, PostChannelMessageEventArgs e)
        {
            result = e.Msg;
        }

        [Theory]
        [InlineData("DarkStreamPhantom", Platform.Twitch)]
        [InlineData("WrithemTwine", Platform.Twitch)]
        [InlineData("SevenOf9", Platform.Twitch)]
        [InlineData("xFreakDuchessx", Platform.Twitch)]
        [InlineData("uegsi", Platform.Twitch)]
        public void TestUserJoined(string UserName, Platform Source)
        {
            lock (DatabaseName)
            {
                Initialize();
                botController.HandleAddChat(UserName, Source);
                botController.HandleUserJoined([new(UserName, Source)]);
            }
        }

        [Fact]
        public void TestUserTimeout()
        {
            lock (DatabaseName)
            {
                Initialize();
                // botController.HandleUserTimedOut();
            }
        }

        [Theory]
        [InlineData("12345", "BanMeUser1", "Buy follows or else")]
        [InlineData("124856", "BanMeUser2", "You must buy follows or else", true)]
        [InlineData("9381984", "BanMeUser3", "You're an idiot if you don't buy follows or else", false, true)]
        [InlineData("4985973", "BanMeUser4", "Screw you if you don't buy follows or else", true, true)]
        public void TestUserBanned(string UserId, string UserName, string Msg, bool Joined = false, bool JoinBan = false)
        {
            lock (DatabaseName)
            {
                Initialize();


                string Title = "Let's try this stream test!";
                DateTime onlineTime = DateTime.Now.ToLocalTime();

                OnGetChannelGameNameEventArgs random = GetRandomGameIdName();

                string Id = random.GameId;
                string Category = random.GameName;

                // go online
                botController.HandleOnStreamOnline(OptionFlags.TwitchChannelName, Title, onlineTime, new(Id, Category));

                // wait a bit
                Thread.Sleep(20000);

                // add some good/friendly users, waiting for joining
                foreach (string U in new List<string> { "DarkStreamPhantom", "Jijijava", "CuteChibiChu", "BlkbryOnline", "NewUser", "OutlawTorn14" })
                {
                    botController.HandleUserJoined([new(U, Platform.Twitch)]);
                    Thread.Sleep(Random.Next(10000, 80000));
                    botController.HandleMessageReceived(new() { DisplayName = U, IsSubscriber = 0 == Random.Next(0, 1), Message = "Hey stud!" }, Platform.Twitch);
                }

                // wait a little more
                Thread.Sleep(18000);

                // receive the hostile ban message
                botController.HandleMessageReceived(new() { DisplayName = UserName, IsSubscriber = 0 == Random.Next(0, 1), Message = Msg }, Platform.Twitch);

                // wait a moment to recognize the message
                Thread.Sleep(5000);

                // ban before or after they join, or don't join at all
                if (!JoinBan)
                {
                    botController.HandleUserBanned(new(userId: UserId, userName: UserName, botSource: Platform.Twitch));
                    Thread.Sleep(5000);
                }

                if (Joined)
                {
                    botController.HandleUserJoined([new(UserName, Platform.Twitch)]);
                    Thread.Sleep(5000);
                }

                if (JoinBan)
                {
                    botController.HandleUserBanned(new(UserName, Platform.Twitch));
                }
            }
        }

        [Fact]
        public void TestUserLeft()
        {
            Initialize();
            // botController.HandleUserLeft();
        }

        [Theory]
        [InlineData("12345", "DarkStreamPhantom", false, 4)]
        public void TestBeingHost(string UserId, string ChannelHost, bool AutoHosted, int Viewers)
        {
            Initialize();
            lock (DatabaseName)
            {
                botController.HandleBeingHosted(new(userId: UserId, userName: ChannelHost, botSource: Platform.Twitch), ChannelHost, AutoHosted, Viewers);
            }
        }

        [Theory]
        [InlineData(600)]
        [InlineData(10)]
        public void TestBulkFollowers(int PickFollowers)
        {
            Initialize();
            lock (DatabaseName)
            {
                DateTime followed = DateTime.Now;
                int x;

                DateTime getFollowedAt()
                {
                    return followed.AddSeconds(new Random().NextDouble() * 20);
                }

                Follow GenerateFollower(string prefix)
                {
                    return new(
                        getFollowedAt(),
                        "00112233",
                        $"{prefix}Follower{x}",
                        Platform.Default,
                        GetRandomGameIdName().GameName
                    );
                }

                List<Follow> bulkfollows = [];

                int bulkfollowercount = (int)(new Random().NextDouble() * PickFollowers);
                for (x = 0; x < bulkfollowercount; x++)
                {
                    bulkfollows.Add(GenerateFollower("Bulk"));
                }

                List<Follow> regularfollower = [GenerateFollower("Reg")];

                BotController.HandleBotEventStartBulkFollowers();

                botController.HandleBotEventNewFollowers(regularfollower[0]);

                Assert.True(dataManager.PostFollower(
                    new(regularfollower[0].FollowedAt,
                    regularfollower[0].FromUserId,
                    regularfollower[0].
                    FromUserName,
                    Platform.Twitch,
                    GetRandomGameIdName().GameName)));

                BotController.HandleBotEventBulkPostFollowers(bulkfollows);

                foreach (Follow f in bulkfollows)
                {
                    Assert.False(dataManager.PostFollower(new(f.FollowedAt, f.FromUserId, f.FromUserName, Platform.Twitch, GetRandomGameIdName().GameName)));
                }

                Thread.Sleep(5000); // wait enough time for the regular followers to add into the database
                Assert.False(dataManager.PostFollower(
                    new(regularfollower[0].FollowedAt,
                    regularfollower[0].FromUserId,
                    regularfollower[0].FromUserName,
                    Platform.Twitch,
                    GetRandomGameIdName().GameName)));
            }
        }

        [Fact]
        public void TestNewFollowers()
        {
            Initialize();
            lock (DatabaseName)
            {
                string datestring = DateTime.Now.ToString("MMddhhmmss");

                List<Follow> follows = [];

                for (int x = 0; x < Random.Next(1, 20); x++)
                {
                    botController.HandleBotEventNewFollowers(new(
                            followedAt: DateTime.Now,
                            fromUserId: "00112233",
                            fromUserName: $"IFollow{datestring}{x}",
                            platform: Platform.Default,
                            GetRandomGameIdName().GameName)
                            );
                }

            }
        }

        [Fact]
        public void TestAddClips()
        {
            Initialize();
            lock (DatabaseName)
            {
                List<Clip> clips = [
                new()
                {
                    ClipId = "3489249",
                    CreatedAt = DateTime.Now.ToString(),
                    Duration = 20.5F,
                    GameId = "7193",
                    Url = "http://www.flightsimulator.com",
                    Title = "Ready for TakeOff",
                    Language = "en",
                    FromUserId = "91938475",
                    FromUserName = "CuriousUserName"
                }
            ];

                botController.HandleBotEventPostNewClip(clips);

                Assert.False(dataManager.PostClip(clips[0].ClipId, DateTime.Parse(clips[0].CreatedAt), Convert.ToDecimal(clips[0].Duration), clips[0].GameId, clips[0].Language, clips[0].Title, clips[0].Url, clips[0].FromUserId, clips[0].FromUserName));
            }
        }

        [Fact]
        public void TestStreamOnUpdateOff()
        {
            Initialize();
            lock (DatabaseName)
            {
                string Title = "Let's try this stream test!";
                DateTime onlineTime = DateTime.Now.ToLocalTime();
                string Id = "7193";
                string Category = "Microsoft Flight Simulator";

                botController.HandleOnStreamOnline(OptionFlags.TwitchChannelName, Title, onlineTime, new(Id, Category));

                Thread.Sleep(1000);
                Assert.False(dataManager.PostStream(onlineTime, Category));
                Assert.True(dataManager.PostCategory(new(Id, Category)));

                string newId = "981578";
                string newCategory = "DebugStreamCategory";

                botController.HandleOnStreamUpdate(new(newId, newCategory));

                Assert.True(dataManager.PostCategory(new(newId, newCategory)));

                BotController.HandleOnStreamOffline();
            }
        }

        [Theory]
        [InlineData("1234", "DarkStreamPhantom", "25", "Tier 1", "Tier1 Subscription")]
        [InlineData("7890", "SevenOf9", "6", "Tier 2", "Tier2 Subscription")]
        public void TestNewSubscriber(string userId, string DisplayName, string Months, string Subscription, string SubscriptionName)
        {
            Initialize();
            lock (DatabaseName)
            {
                botController.HandleNewSubscriber(new(DisplayName, Platform.Twitch, userId), Months, Subscription, SubscriptionName);

                Thread.Sleep(800);

                string subeventmsg = VariableParser.ParseReplace(LocalizedMsgSystem.GetEventMsg(ChannelEventActions.Subscribe, out bool Enabled, out _), VariableParser.BuildDictionary(new Tuple<MsgVars, string>[] { new(MsgVars.user, DisplayName), new(MsgVars.submonths, FormatData.Plurality(Months, MsgVars.Pluralmonth, Prefix: LocalizedMsgSystem.GetVar(MsgVars.Total))), new(MsgVars.subplan, Subscription), new(MsgVars.subplanname, SubscriptionName) }));

                Assert.Equal(Enabled ? subeventmsg : "", result);
            }
        }

        [Fact]
        public void TestResubscribe()
        {
            Initialize();

            // botController.HandleReSubscriber();
        }

        [Fact]
        public void TestCommunitySub()
        {
            Initialize();

            // botController.HandleCommunitySubscription();
        }

        [Fact]
        public void TestGiftSubs()
        {
            Initialize();

            // botController.HandleGiftSubscription();
        }

        [Fact]
        public void TestCustomReward()
        {
            Initialize();

            // botController.HandleCustomReward();
        }

        [Fact]
        public void TestMsgReceived()
        {
            Initialize();

            // botController.HandleMessageReceived();
        }

        [Fact]
        public void TestManyFunctions()
        {
            Initialize();
            lock (DatabaseName)
            {
                string Title = "Let's try this stream test!";
                DateTime onlineTime = DateTime.Now.ToLocalTime();
                string Id = "7193";
                string Category = "Microsoft Flight Simulator";

                botController.HandleOnStreamOnline(OptionFlags.TwitchChannelName, Title, onlineTime, new(Id, Category));

                botController.HandleIncomingRaidData(new("Pitcy", Platform.Twitch), DateTime.Now.ToLocalTime(), 13, new("284757", "Fortnite"));
                botController.HandleUserJoined([new("Pitcy", Platform.Twitch), new("DarkStreamPhantom", Platform.Twitch), new("OutlawTorn14", Platform.Twitch), new("MrTopiczz", Platform.Twitch), new("pitcyissmelly", Platform.Twitch)]);
                botController.HandleChatCommandReceived(new() { UserType = ViewerTypes.Mod, DisplayName = "Pitcy", IsModerator = true, Message = "!followage" }, Platform.Twitch);
            }
        }

        [Fact]
        public void TestChatCommandReceived()
        {
            Initialize();
        }

        [Fact]
        public void TestGiveaway()
        {
            Initialize();
            lock (DatabaseName)
            {
                OnGetChannelGameNameEventArgs randomGame = GetRandomGameIdName();
                botController.HandleOnStreamOnline("writhemtwine", "Test Giveaway", DateTime.Now.ToLocalTime(), new(randomGame.GameId, randomGame.GameName));

                OptionFlags.GiveawayBegMsg = "Test Project Begin the Giveaway";
                OptionFlags.GiveawayEndMsg = "Test Project End the Giveaway";
                OptionFlags.GiveawayWinMsg = "Test Project the winner is #winner!";
                OptionFlags.GiveawayMaxEntries = 5;
                OptionFlags.GiveawayMultiUser = true;
                OptionFlags.GiveawayCount = 2;
                OptionFlags.ManageGiveawayUsers = true;

                List<LiveUser> entrylist = [new("CuteChibiChu",Platform.Twitch,"382849"),
                new("WrithemTwine",Platform.Twitch,"3948573"),
                new("DarkStreamPhantom",Platform.Twitch,"82848573"),
                new("OutlawTorn14",Platform.Twitch,"9293757")];

                botController.HandleGiveawayBegin(GiveawayTypes.Command, "giveaway");

                foreach (LiveUser s in entrylist)
                {
                    int y = 0;
                    while (y < OptionFlags.GiveawayMaxEntries)
                    {
                        botController.HandleGiveawayPostName(s);
                        y++;
                    }
                }

                botController.HandleGiveawayEnd();
                botController.HandleGiveawayWinner();
            }
        }

        [Theory]
        [InlineData("289384787", "xFreakDuchessx")]
        public void TestRaid(string userId, string RaidUserName)
        {
            Initialize();
            lock (DatabaseName)
            {
                string Title = "Let's try this stream test!";
                DateTime onlineTime = DateTime.Now.ToLocalTime();
                string Id = "7193";
                string Category = "Microsoft Flight Simulator";

                botController.HandleOnStreamOnline(OptionFlags.TwitchChannelName, Title, onlineTime, new(Id, Category));

                Thread.Sleep(1000);
                Assert.False(dataManager.PostStream(onlineTime, Category));
                Assert.False(dataManager.PostCategory(new(Id, Category)));

                OnGetChannelGameNameEventArgs randomGame = GetRandomGameIdName();

                botController.HandleIncomingRaidData(new(RaidUserName, Platform.Twitch, userId), DateTime.Now, Random.Next(5, Viewers), new(randomGame.GameId, randomGame.GameName));
                Thread.Sleep(5000); // wait for category

                // Assert.True(ActionSystem.UserChat(new(RaidUserName, Platform.Twitch)));

                botController.HandleUserJoined([new(RaidUserName, Platform.Twitch)]);
                //Assert.False(StatisticsSystem.UserChat(new(RaidUserName, Platform.Twitch)));

                botController.HandleUserLeft(new(RaidUserName, Platform.Twitch));

                Thread.Sleep(2000);

                // Assert.False(StatisticsSystem.UserChat(new(RaidUserName, Platform.Twitch))); // should be able to add the user again
            }
        }

        [Fact]
        public void TestNewClip()
        {
            lock (DatabaseName)
            {
                string ClipName = Path.GetRandomFileName();

                Initialize();
                OptionFlags.TwitchClipPostChat = true;

                Clip TestClip = new()
                {
                    ClipId = ClipName,
                    CreatedAt = DateTime.Now.ToString(),
                    Duration = Random.Next(30),
                    GameId = GetRandomGameIdName().GameId,
                    Language = "English",
                    Title = "My Random Test Clip",
                    Url = $"http://debug.app/{ClipName}",
                    FromUserId = "8475630",
                    FromUserName = "Queenmeteor"
                };

                botController.HandleBotEventPostNewClip([TestClip]);

                Thread.Sleep(2000);

                Assert.False(
                    dataManager.PostClip(TestClip.ClipId,
                                         DateTime.Parse(TestClip.CreatedAt),
                                         Convert.ToDecimal(TestClip.Duration),
                                         TestClip.GameId,
                                         TestClip.Language,
                                         TestClip.Title,
                                         TestClip.Url,
                                         TestClip.FromUserId,
                                         TestClip.FromUserName)
                    );
            }
        }

        [Fact]
        public void TestOverlayData()
        {
            Initialize();
        }
    }
}
