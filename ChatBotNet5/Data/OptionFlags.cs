//#if DEBUG
//#define LOGGING
//#endif

using ChatBot_Net5.Properties;

namespace ChatBot_Net5.Data
{
    internal static class OptionFlags
    {
        internal static bool ProcessOps { get; set; } = false;  // whether to process ops or not
        internal static bool IsStreamOnline { get; set; } = false;

        internal static bool TwitchAddFollowersStart { get; set; }
        internal static bool TwitchPruneNonFollowers { get; set; }
        internal static bool TwitchFollowerFollowBack { get; set; }
        internal static bool TwitchRaidFollowBack { get; set; }
        internal static bool TwitchAddFollowerNotification { get; set; }

        internal static bool FirstUserJoinedMsg { get; set; }
        internal static bool FirstUserChatMsg { get; set; }
        
        internal static bool MsgAddMe { get; set; }
        internal static bool MsgNoMe { get; set; }
        internal static bool MsgPerComMe { get; set; }

        internal static bool MsgWelcomeStreamer { get; set; }

        internal static bool AutoShout { get; set; }
        internal static bool TwitchRaidShoutOut { get; set; }

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
            lock (Settings.Default)
            {
                Settings.Default.Save();

                TwitchAddFollowersStart = Settings.Default.TwitchAddFollowersStart;
                TwitchPruneNonFollowers = Settings.Default.TwitchPruneNonFollowers;
                TwitchFollowerFollowBack = Settings.Default.TwitchFollowerFollowBack;
                TwitchRaidFollowBack = Settings.Default.TwitchRaidFollowBack;
                TwitchAddFollowerNotification = Settings.Default.TwitchAddFollowerNotification;

                FirstUserJoinedMsg = Settings.Default.WelcomeUserJoined;
                FirstUserChatMsg = Settings.Default.WelcomeChatMsg;

                MsgAddMe = Settings.Default.MsgInsertMe;
                MsgNoMe = Settings.Default.MsgNoMe;
                MsgPerComMe = Settings.Default.MsgPerComMe;

                MsgWelcomeStreamer = Settings.Default.MsgWelcomeStreamer;

                AutoShout = Settings.Default.MsgAutoShout;

                TwitchRaidShoutOut = Settings.Default.TwitchRaidShoutOut;

                RepeatTimer = Settings.Default.RepeatTimerCommands;
                RepeatWhenLive = Settings.Default.RepeatWhenLive;

                UserPartyStart = Settings.Default.UserPartyStart;
                UserPartyStop = Settings.Default.UserPartyStop;

                PostMultiLive = Settings.Default.PostMultiLive;
                LiveMsg = Settings.Default.MsgLive;

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
