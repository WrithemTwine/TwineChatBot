using System;
using System.Diagnostics;

namespace ChatBot_Net5.Models
{
    [DebuggerDisplay("StreamStart={StreamStart}, NewFollows={NewFollows}")]
    internal class StreamStat
    {
        internal DateTime StreamStart { get; set; } = DateTime.MinValue;
        internal DateTime StreamEnd { get; set; } = DateTime.MinValue;
        internal int NewFollows { get; set; } = 0;
        internal int NewSubscribers { get; set; } = 0;
        internal int GiftSubs { get; set; } = 0;
        internal int Bits { get; set; } = 0;
        internal int Raids { get; set; } = 0;
        internal int Hosted { get; set; } = 0;
        internal int UsersBanned { get; set; } = 0;
        internal int UsersTimedOut { get; set; } = 0;
        internal int ModeratorsPresent { get; set; } = 0;
        internal int SubsPresent { get; set; } = 0;
        internal int VIPsPresent { get; set; } = 0;
        internal int TotalChats { get; set; } = 0;
        internal int Commands { get; set; } = 0;
        internal int AutomatedEvents { get; set; } = 0;
        internal int AutomatedCommands { get; set; } = 0;
        internal int DiscordMsgs { get; set; } = 0;
        internal int ClipsMade { get; set; } = 0;
        internal int ChannelPtCount { get; set; } = 0;
        internal int ChannelChallenge { get; set; } = 0;
        internal int MaxUsers { get; set; } = 0;

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
