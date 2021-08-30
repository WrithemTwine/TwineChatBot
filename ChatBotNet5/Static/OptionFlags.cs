﻿using ChatBot_Net5.Properties;

using System;

namespace ChatBot_Net5.Static
{
    public static class OptionFlags
    {
        public static bool LogBotStatus { get; set; }
        public static bool LogExceptions { get; set; }

        public static bool ProcessOps { get; set; }  // whether to process ops or not
        public static bool IsStreamOnline { get; set; }

        public static bool TwitchAddFollowersStart { get; set; }
        public static bool TwitchPruneNonFollowers { get; set; }
        public static bool TwitchAddFollowerNotification { get; set; }

        public static bool FirstUserJoinedMsg { get; set; }
        public static bool FirstUserChatMsg { get; set; }

        public static DateTime TwitchRefreshDate { get; set; }

        public static bool MsgAddMe { get; set; }
        public static bool MsgNoMe { get; set; }
        public static bool MsgPerComMe { get; set; }

        public static bool MsgWelcomeStreamer { get; set; }

        public static bool WelcomeCustomMsg { get; set; }

        public static bool AutoShout { get; set; }
        public static bool TwitchRaidShoutOut { get; set; }

        public static bool RepeatTimer { get; set; }
        public static bool RepeatTimerDilute { get; set; }
        public static bool RepeatWhenLive { get; set; }

        public static bool UserPartyStart { get; set; }
        public static bool UserPartyStop { get; set; }

        // Enables or disables posting multiple live messages to social media on the same day, i.e. the stream crashes and restarts and another 'Live' alert is posted.
        public static bool PostMultiLive { get; set; }
        public static string LiveMsg { get; set; }

        public static bool ManageUsers { get; set; }
        public static bool ManageFollowers { get; set; }
        public static bool ManageStreamStats { get; set; }

        //public static bool TwitchFollowerFollowBack { get; set; }
        //public static bool TwitchRaidFollowBack { get; set; }
        //public static bool TwitchFollowbackBotChoice { get; set; }
        //public static bool TwitchFollowbackStreamerChoice { get; set; }

        public static string TwitchStreamerChannel { get; set; }
        public static string TwitchStreamerToken { get; set; }
        public static DateTime TwitchStreamTokenDate { get; set; }

        public static bool TwitchChatBotConnectOnline { get; set; }
        public static bool TwitchChatBotDisconnectOffline { get; set; }

        public static bool TwitchClipPostChat { get; set; }
        public static bool TwitchClipPostDiscord { get; set; }

        public static bool TwitchCurrencyStart { get; set; }
        public static bool TwitchCurrencyOnline { get; set; }

        public static void SetSettings()
        {
            lock (Settings.Default)
            {
                Settings.Default.Save();

                LogBotStatus = Settings.Default.LogBotStatus;
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

                TwitchCurrencyStart = Settings.Default.TwitchCurrencyStart;
                TwitchCurrencyOnline = Settings.Default.TwitchCurrencyOnline;
            }
        }

        public static void SetParty(bool Start = true)
        {
            Settings.Default.UserPartyStart = Start;
            Settings.Default.UserPartyStop = !Start;

            SetSettings();
        }

        public static TimeSpan CurrentToTwitchRefreshDate(bool streamaccount = false)
        {
            return (streamaccount ? TwitchStreamTokenDate : TwitchRefreshDate) - DateTime.Now.ToLocalTime();
        }

    }
}
