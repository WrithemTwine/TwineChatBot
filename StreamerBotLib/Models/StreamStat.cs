using StreamerBotLib.DataSQL.Models;

using System.Diagnostics;
using System.Reflection;

namespace StreamerBotLib.Models
{
    /// <summary>
    /// Properties of a live stream
    /// </summary>
    [DebuggerDisplay("StreamStart={StreamStart}, NewFollows={NewFollows}")]
    public class StreamStat(DateTime streamStart = default,
                            DateTime streamEnd = default,
                            uint newFollows = 0,
                            uint newSubscribers = 0,
                            uint giftSubs = 0,
                            uint bits = 0,
                            uint raids = 0,
                            uint hosted = 0,
                            uint usersBanned = 0,
                            uint usersTimedOut = 0,
                            uint moderatorsPresent = 0,
                            uint subsPresent = 0,
                            uint vIPsPresent = 0,
                            uint totalChats = 0,
                            uint commands = 0,
                            uint automatedEvents = 0,
                            uint automatedCommands = 0,
                            uint discordMsgs = 0,
                            uint clipsMade = 0,
                            uint channelPtCount = 0,
                            uint channelChallenge = 0,
                            uint maxUsers = 0,
                            string currentCategory = null)
    {
        public StreamStat(StreamStats streamStats) : this()
        {
            foreach(PropertyInfo property in streamStats.GetType().GetProperties())
            {
                GetType().GetProperty(property.Name).SetValue(this,property.GetValue(streamStats));
            }
        }

        public DateTime StreamStart { get; set; } = streamStart;
        public DateTime StreamEnd { get; set; } = streamEnd;
        public uint NewFollows { get; set; } = newFollows;
        public uint NewSubscribers { get; set; } = newSubscribers;
        public uint GiftSubs { get; set; } = giftSubs;
        public uint Bits { get; set; } = bits;
        public uint Raids { get; set; } = raids;
        public uint Hosted { get; set; } = hosted;
        public uint UsersBanned { get; set; } = usersBanned;
        public uint UsersTimedOut { get; set; } = usersTimedOut;
        public uint ModeratorsPresent { get; set; } = moderatorsPresent;
        public uint SubsPresent { get; set; } = subsPresent;
        public uint VIPsPresent { get; set; } = vIPsPresent;
        public uint TotalChats { get; set; } = totalChats;
        public uint Commands { get; set; } = commands;
        public uint AutomatedEvents { get; set; } = automatedEvents;
        public uint AutomatedCommands { get; set; } = automatedCommands;
        public uint DiscordMsgs { get; set; } = discordMsgs;
        public uint ClipsMade { get; set; } = clipsMade;
        public uint ChannelPtCount { get; set; } = channelPtCount;
        public uint ChannelChallenge { get; set; } = channelChallenge;
        public uint MaxUsers { get; set; } = maxUsers;

        /// <summary>
        /// Maintain current active category of the stream
        /// </summary>
        public string Category { get; set; } = currentCategory;

        public void Clear()
        {
            StreamStart = default;
            StreamEnd = default;
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
