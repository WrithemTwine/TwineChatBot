//#if DEBUG
//#define LOGGING
//#endif

using ChatBot_Net5.Properties;

namespace ChatBot_Net5.BotIOController
{
    internal static class OptionFlags
    {
        internal static bool ProcessOps { get; set; } = false;  // whether to process ops or not

        internal static bool FirstFollowerProcess { get; set; }
        internal static bool FirstUserJoinedMsg { get; set; }
        internal static bool FirstUserChatMsg { get; set; }
        internal static bool AddMeMsg { get; set; }
        internal static bool AutoShout { get; set; }
        internal static bool RepeatTimer { get; set; }


        // Enables or disables posting multiple live messages to social media on the same day, i.e. the stream crashes and restarts and another 'Live' alert is posted.
        internal static bool PostMultiLive { get; set; }

        internal static void SetSettings()
        {
            FirstFollowerProcess = Settings.Default.AddFollowersStart;
            FirstUserJoinedMsg = Settings.Default.WelcomeUserJoined;
            FirstUserChatMsg = Settings.Default.WelcomeChatMsg;
            AddMeMsg = Settings.Default.InsertMeToMsg;
            AutoShout = Settings.Default.AutoShout;
            RepeatTimer = Settings.Default.RepeatTimerCommands;
            PostMultiLive = Settings.Default.PostMultiLive;
        }
    }
}
