using StreamerBotLib.Models;
using StreamerBotLib.Models.Enums;
using StreamerBotLib.Static;

namespace TestStreamerBot
{
    public class TestAPIShoutOut
    {
        private bool TestActive = false;

        private bool ShoutOutTaskActive = false;
        private readonly List<ShoutOutLiveUser> ShoutOutUsers = [];

        private bool shoutTest = false, firstTest = false, repeatTest = false;
        private string TestUserName = "";

        private void SendShoutOut(LiveUser user)
        {
            if (!ShoutOutTaskActive)
            {
                ShoutOutTaskActive = true;
                Task.Run(EvaluateShoutOutUsers);
            }

            lock (ShoutOutUsers)
            {
                ShoutOutLiveUser shoutOutLiveUser = new(user);
                if (!ShoutOutUsers.UniqueAdd(shoutOutLiveUser))
                {
                    var shoutuser = ShoutOutUsers.Find(
                            s => s.Equals(shoutOutLiveUser)
                                && (s.LastShoutOut != null
                                && s.LastShoutOut.Value.AddSeconds(2) < DateTime.Now)
                        );
                    if (shoutuser != null)
                    {
                        shoutuser.NextShoutOut = shoutuser.LastShoutOut?.AddSeconds(10);
                    }
                }
            }
        }

        private Task EvaluateShoutOutUsers()
        {
            // NewUserEntry: Different users can only be shoutout once every 2 minutes
            // -LastShoutOut = null, NextShoutOut = null => first shoutout occurs asap
            // 
            // ExistingUserEntry: Same user can only be shoutout after at least every 60 minutes
            // -LastShoutOut = value, NextShoutOut = null => no shoutout scheduled
            // -LastShoutOUt = value, NextShoutOut = value => computed next shoutout to perform

            return Task.Run(async () =>
            {
                try
                {
                    DateTime lastShoutOut = DateTime.MinValue;

                    while (TestActive)
                    {
                        ShoutOutLiveUser? nextShoutOut = null;
                        lock (ShoutOutUsers)
                        {
                            foreach (var S in ShoutOutUsers)
                            {
                                if (S.LastShoutOut == null && S.NextShoutOut == null)
                                {
                                    nextShoutOut ??= S;
                                    break;
                                }
                                else if (S.NextShoutOut != null)
                                {
                                    nextShoutOut ??= S;
                                    break;
                                }
                            }
                        }

                        DateTime Curr = DateTime.Now;
                        if (nextShoutOut != null)
                        {
                            if (nextShoutOut.NextShoutOut == null && lastShoutOut.AddSeconds(5) <= Curr)
                            { // new shoutout user, allowed per Twitch API every 2 minutes
                                EmulateShoutOut(nextShoutOut.User.UserName);

                                lock (ShoutOutUsers)
                                {
                                    nextShoutOut.LastShoutOut = Curr;
                                }

                                lastShoutOut = Curr;
                                firstTest = true;
                            }
                            else if (lastShoutOut.AddSeconds(10) <= Curr && nextShoutOut.NextShoutOut < Curr)
                            { // existing shoutout user, allowed per Twitch API every 60 minutes
                                EmulateShoutOut(nextShoutOut.User.UserName);

                                lock (ShoutOutUsers)
                                {
                                    nextShoutOut.LastShoutOut = Curr;
                                    nextShoutOut.NextShoutOut = null;
                                }

                                lastShoutOut = Curr;
                                repeatTest = true;
                            }
                        }
                        await Task.Delay(2000);
                    }
                }
                catch (Exception ex)
                {
                    LogWriter.LogException(ex, "EvaluateShoutOutUsers");
                    ShoutOutTaskActive = false;
                }
            });
        }

        private bool EmulateShoutOut(string UserName)
        {
            shoutTest = true;
            TestUserName = UserName;
            // we assume sending shoutout request to Twitch will succeed.
            return shoutTest;
        }

        [Fact]
        public void TestSendShoutOut()
        {
            void ResetTest()
            {
                firstTest = false; repeatTest = false; shoutTest = false;
            }

            LiveUser liveUser1 = new("userName1", Platform.Twitch);
            LiveUser liveUser2 = new("userName2", Platform.Twitch);
            LiveUser liveUser3 = new("userName3", Platform.Twitch);
            LiveUser liveUser4 = new("userName4", Platform.Twitch);
            LiveUser liveUser5 = new("userName5", Platform.Twitch);

            SendShoutOut(liveUser1);
            //Assert.True(firstTest);
            //Assert.True(shoutTest);
            //Assert.False(repeatTest);
            //Assert.Equal(TestUserName, liveUser1.UserName);
            //ResetTest();

            SendShoutOut(liveUser1);
            //Assert.False(firstTest || shoutTest || repeatTest);

            SendShoutOut(liveUser2);
            //Assert.True(firstTest);
            //Assert.True(shoutTest);
            //Assert.False(repeatTest);
            //Assert.Equal(TestUserName, liveUser2.UserName);
            //ResetTest();

            SendShoutOut(liveUser3);
            //Assert.True(firstTest);
            //Assert.True(shoutTest);
            //Assert.False(repeatTest);
            //Assert.Equal(TestUserName, liveUser3.UserName);
            //ResetTest();

            Assert.Equal(3, ShoutOutUsers.Count);

            SendShoutOut(liveUser4);
            //Assert.True(firstTest);
            //Assert.True(shoutTest);
            //Assert.False(repeatTest);
            //Assert.Equal(TestUserName, liveUser4.UserName);
            //ResetTest();

            SendShoutOut(liveUser5);
            //Assert.True(firstTest);
            //Assert.True(shoutTest);
            //Assert.False(repeatTest);
            //Assert.Equal(TestUserName, liveUser5.UserName);
            //ResetTest();

            SendShoutOut(liveUser3);
            //Assert.True(firstTest);
            //Assert.True(shoutTest);
            //Assert.False(repeatTest);
            //Assert.Equal(TestUserName, liveUser3.UserName);
            ResetTest();

            Assert.Equal(5, ShoutOutUsers.Count);

        }

    }
}
