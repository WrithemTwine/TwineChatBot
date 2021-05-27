//#if DEBUG
//#define LOGGING
//#endif

using ChatBot_Net5.Properties;

namespace ChatBot_Net5.Data
{
    internal static class OptionFlags
    {
        internal static bool ProcessOps { get; set; } = false;  // whether to process ops or not

        internal static bool FirstFollowerProcess { get; set; }
        internal static bool FirstUserJoinedMsg { get; set; }
        internal static bool FirstUserChatMsg { get; set; }
        
        internal static bool AddMeMsg { get; set; }
        internal static bool NoMeMsg { get; set; }
        internal static bool PerComMeMsg { get; set; }

        internal static bool AutoShout { get; set; }
        internal static bool RepeatTimer { get; set; }
        internal static bool RepeatWhenLive { get; set; }

        internal static bool UserPartyStart { get; set; }
        internal static bool UserPartyStop { get; set; }

        // Enables or disables posting multiple live messages to social media on the same day, i.e. the stream crashes and restarts and another 'Live' alert is posted.
        internal static bool PostMultiLive { get; set; }
        internal static string LiveMsg { get; set; }

        internal static bool ManageUsers { get; set; }
        internal static bool ManageFollowers { get; set; }
        internal static bool ManageStreamStats { get; set; }

        internal static void SetSettings()
        {
            string s = "";

            lock (s)
            {
                Settings.Default.Save();

                FirstFollowerProcess = Settings.Default.AddFollowersStart;
                FirstUserJoinedMsg = Settings.Default.WelcomeUserJoined;
                FirstUserChatMsg = Settings.Default.WelcomeChatMsg;

                AddMeMsg = Settings.Default.InsertMeToMsg;
                NoMeMsg = Settings.Default.NoMeMsg;
                PerComMeMsg = Settings.Default.PerComMeMsg;

                AutoShout = Settings.Default.AutoShout;
                RepeatTimer = Settings.Default.RepeatTimerCommands;
                RepeatWhenLive = Settings.Default.RepeatWhenLive;

                UserPartyStart = Settings.Default.UserPartyStart;
                UserPartyStop = Settings.Default.UserPartyStop;

                PostMultiLive = Settings.Default.PostMultiLive;
                LiveMsg = Settings.Default.LiveMsg;

                ManageUsers = Settings.Default.ManageUsers;
                ManageFollowers = Settings.Default.ManageFollowers;
                ManageStreamStats = Settings.Default.ManageStreamStats;
            }
        }

        internal static void SetParty(bool Start = true)
        {
            Settings.Default.UserPartyStart = Start;
            Settings.Default.UserPartyStop = !Start;

            SetSettings();
        }

    }
}
