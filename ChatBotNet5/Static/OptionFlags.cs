using ChatBot_Net5.Properties;

using System;

namespace ChatBot_Net5.Static
{
    internal static class OptionFlags
    {
        internal static bool LogBotStatus { get; set; }
        internal static bool LogExceptions { get; set; }

        internal static bool ProcessOps { get; set; }  // whether to process ops or not
        internal static bool IsStreamOnline { get; set; }

        internal static bool TwitchAddFollowersStart { get; set; }
        internal static bool TwitchPruneNonFollowers { get; set; }
        internal static bool TwitchAddFollowerNotification { get; set; }

        internal static bool FirstUserJoinedMsg { get; set; }
        internal static bool FirstUserChatMsg { get; set; }

        internal static DateTime TwitchRefreshDate { get; set; }

        internal static bool MsgAddMe { get; set; }
        internal static bool MsgNoMe { get; set; }
        internal static bool MsgPerComMe { get; set; }

        internal static bool MsgWelcomeStreamer { get; set; }

        internal static bool WelcomeCustomMsg { get; set; }

        internal static bool AutoShout { get; set; }
        internal static bool TwitchRaidShoutOut { get; set; }

        internal static bool RepeatTimer { get; set; }
        internal static bool RepeatTimerDilute { get; set; }
        internal static bool RepeatWhenLive { get; set; }

        internal static bool UserPartyStart { get; set; }
        internal static bool UserPartyStop { get; set; }

        // Enables or disables posting multiple live messages to social media on the same day, i.e. the stream crashes and restarts and another 'Live' alert is posted.
        internal static bool PostMultiLive { get; set; }
        internal static string LiveMsg { get; set; }

        internal static bool ManageUsers { get; set; }
        internal static bool ManageFollowers { get; set; }
        internal static bool ManageStreamStats { get; set; }

        //internal static bool TwitchFollowerFollowBack { get; set; }
        //internal static bool TwitchRaidFollowBack { get; set; }
        //internal static bool TwitchFollowbackBotChoice { get; set; }
        //internal static bool TwitchFollowbackStreamerChoice { get; set; }

        internal static string TwitchStreamerChannel { get; set; }
        internal static string TwitchStreamerToken { get; set; }
        internal static DateTime TwitchStreamTokenDate { get; set; }

        internal static bool TwitchChatBotConnectOnline { get; set; }
        internal static bool TwitchChatBotDisconnectOffline { get; set; }

        internal static bool TwitchClipPostChat { get; set; }
        internal static bool TwitchClipPostDiscord { get; set; }

        internal static void SetSettings()
        {
            lock (Settings.Default)
            {
                Settings.Default.Save();

                LogBotStatus = Settings.Default.LogExceptions;
                LogExceptions = Settings.Default.LogExceptions;

                TwitchAddFollowersStart = Settings.Default.TwitchAddFollowersStart;
                TwitchPruneNonFollowers = Settings.Default.TwitchPruneNonFollowers;
                TwitchAddFollowerNotification = Settings.Default.TwitchAddFollowerNotification;

                FirstUserJoinedMsg = Settings.Default.WelcomeUserJoined;
                FirstUserChatMsg = Settings.Default.WelcomeChatMsg;

                TwitchRefreshDate = Settings.Default.TwitchRefreshDate;

                MsgAddMe = Settings.Default.MsgInsertMe;
                MsgNoMe = Settings.Default.MsgNoMe;
                MsgPerComMe = Settings.Default.MsgPerComMe;

                MsgWelcomeStreamer = Settings.Default.MsgWelcomeStreamer;
                WelcomeCustomMsg = Settings.Default.WelcomeCustomMsg;

                AutoShout = Settings.Default.MsgAutoShout;

                TwitchRaidShoutOut = Settings.Default.TwitchRaidShoutOut;

                RepeatTimer = Settings.Default.RepeatTimerCommands;
                RepeatTimerDilute = Settings.Default.RepeatTimerComSlowdown;
                RepeatWhenLive = Settings.Default.RepeatWhenLive;

                UserPartyStart = Settings.Default.UserPartyStart;
                UserPartyStop = Settings.Default.UserPartyStop;

                PostMultiLive = Settings.Default.PostMultiLive;
                LiveMsg = Settings.Default.MsgLive;

                ManageUsers = Settings.Default.ManageUsers;
                ManageFollowers = Settings.Default.ManageFollowers;
                ManageStreamStats = Settings.Default.ManageStreamStats;

                //TwitchFollowerFollowBack = Settings.Default.TwitchFollowerFollowBack;
                //TwitchRaidFollowBack = Settings.Default.TwitchRaidFollowBack;

                //TwitchFollowbackBotChoice = Settings.Default.TwitchFollowbackBotChoice;
                //TwitchFollowbackStreamerChoice = Settings.Default.TwitchFollowbackStreamerChoice;
                TwitchStreamerChannel = Settings.Default.TwitchStreamerChannel;
                TwitchStreamerToken = Settings.Default.TwitchStreamerToken;
                TwitchStreamTokenDate = Settings.Default.TwitchStreamTokenDate;

                TwitchChatBotConnectOnline = Settings.Default.TwitchChatBotConnectOnline;
                TwitchChatBotDisconnectOffline = Settings.Default.TwitchChatBotDisconnectOffline;

                TwitchClipPostChat = Settings.Default.TwitchClipPostChat;
                TwitchClipPostDiscord = Settings.Default.TwitchClipPostDiscord;
            }
        }

        internal static void SetParty(bool Start = true)
        {
            Settings.Default.UserPartyStart = Start;
            Settings.Default.UserPartyStop = !Start;

            SetSettings();
        }

        internal static TimeSpan CurrentToTwitchRefreshDate(bool streamaccount = false)
        {
            return (streamaccount ? TwitchStreamTokenDate : TwitchRefreshDate) - DateTime.Now;
        }

    }
}
