using System;
using System.Diagnostics;

namespace StreamerBotLib.Models
{
    [DebuggerDisplay("StreamStart={StreamStart}, NewFollows={NewFollows}")]
    public class StreamStat
    {
        public DateTime StreamStart { get; set; } = DateTime.MinValue;
        public DateTime StreamEnd { get; set; } = DateTime.MinValue;
        public int NewFollows { get; set; } = 0;
        public int NewSubscribers { get; set; } = 0;
        public int GiftSubs { get; set; } = 0;
        public long Bits { get; set; } = 0;
        public int Raids { get; set; } = 0;
        public int Hosted { get; set; } = 0;
        public int UsersBanned { get; set; } = 0;
        public int UsersTimedOut { get; set; } = 0;
        public int ModeratorsPresent { get; set; } = 0;
        public int SubsPresent { get; set; } = 0;
        public int VIPsPresent { get; set; } = 0;
        public int TotalChats { get; set; } = 0;
        public int Commands { get; set; } = 0;
        public int AutomatedEvents { get; set; } = 0;
        public int AutomatedCommands { get; set; } = 0;
        public int DiscordMsgs { get; set; } = 0;
        public int ClipsMade { get; set; } = 0;
        public int ChannelPtCount { get; set; } = 0;
        public int ChannelChallenge { get; set; } = 0;
        public int MaxUsers { get; set; } = 0;

        /// <summary>
        /// Maintain current active category of the stream
        /// </summary>
        public string CurrentCategory { get; set; }

        public void Clear()
        {
            StreamStart = DateTime.MinValue;
            StreamEnd = DateTime.MinValue;
            NewFollows = 0;
            NewSubscribers = 0;
            GiftSubs = 0;
            Bits = 0;
            Raids = 0;
            Hosted = 0;
            UsersBanned = 0;
            UsersTimedOut = 0;
            ModeratorsPresent = 0;
            SubsPresent = 0;
            VIPsPresent = 0;
            TotalChats = 0;
            Commands = 0;
            AutomatedEvents = 0;
            AutomatedCommands = 0;
            DiscordMsgs = 0;
            ClipsMade = 0;
            ChannelPtCount = 0;
            ChannelChallenge = 0;
            MaxUsers = 0;
        }
    }
}
