using System;

namespace ChatBot_Net5.Data
{
    internal class StreamStat
    {
        internal static readonly string DefaultTime = "1/1/1990";

        internal DateTime StreamStart { get; set; } = DateTime.Parse(DefaultTime);
        internal DateTime StreamEnd { get; set; } = DateTime.Parse(DefaultTime);
        internal int NewFollows { get; set; } = 0;
        internal int NewSubs { get; set; } = 0;
        internal int GiftSubs { get; set; } = 0;
        internal int Bits { get; set; } = 0;
        internal int Raids { get; set; } = 0;
        internal int Hosted { get; set; } = 0;
        internal int UsersBanned { get; set; } = 0;
        internal int UsersTimedOut { get; set; } = 0;
        internal int ModsPresent { get; set; } = 0;
        internal int SubsPresent { get; set; } = 0;
        internal int VIPsPresent { get; set; } = 0;
        internal int TotalChats { get; set; } = 0;
        internal int Commands { get; set; } = 0;
        internal int AutoEvents { get; set; } = 0;
        internal int AutoCommands { get; set; } = 0;
        internal int DiscordMsgs { get; set; } = 0;
        internal int ClipsMade { get; set; } = 0;
        internal int ChannelPtCount { get; set; } = 0;
        internal int ChannelChallenge { get; set; } = 0;
        internal int MaxUsers { get; set; } = 0;

        public void Clear()
        {
            StreamStart = DateTime.Parse(DefaultTime);
            StreamEnd = DateTime.Parse(DefaultTime);
            NewFollows = 0;
            NewSubs = 0;
            GiftSubs = 0;
            Bits = 0;
            Raids = 0;
            Hosted = 0;
            UsersBanned = 0;
            UsersTimedOut = 0;
            ModsPresent = 0;
            SubsPresent = 0;
            VIPsPresent = 0;
            TotalChats = 0;
            Commands = 0;
            AutoEvents = 0;
            AutoCommands = 0;
            DiscordMsgs = 0;
            ClipsMade = 0;
            ChannelPtCount = 0;
            ChannelChallenge = 0;
            MaxUsers = 0;
        }
    }
}
