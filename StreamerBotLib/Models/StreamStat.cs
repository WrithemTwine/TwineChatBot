﻿using StreamerBotLib.DataSQL.Models;

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
                            int newFollows = 0,
                            int newSubscribers = 0,
                            int giftSubs = 0,
                            int bits = 0,
                            int raids = 0,
                            int hosted = 0,
                            int usersBanned = 0,
                            int usersTimedOut = 0,
                            int moderatorsPresent = 0,
                            int subsPresent = 0,
                            int vIPsPresent = 0,
                            int totalChats = 0,
                            int commands = 0,
                            int automatedEvents = 0,
                            int automatedCommands = 0,
                            int webhooks = 0,
                            int clipsMade = 0,
                            int channelPtCount = 0,
                            int channelChallenge = 0,
                            int maxUsers = 0,
                            string currentCategory = null)
    {
        public StreamStat(StreamStats streamStats) : this()
        {
            foreach (PropertyInfo property in streamStats.GetType().GetProperties())
            {
                GetType().GetProperty(property.Name).SetValue(this, property.GetValue(streamStats));
            }
        }

        public DateTime StreamStart { get; set; } = streamStart;
        public DateTime StreamEnd { get; set; } = streamEnd;
        public int NewFollows { get; set; } = newFollows;
        public int NewSubscribers { get; set; } = newSubscribers;
        public int GiftSubs { get; set; } = giftSubs;
        public int Bits { get; set; } = bits;
        public int Raids { get; set; } = raids;
        public int Hosted { get; set; } = hosted;
        public int UsersBanned { get; set; } = usersBanned;
        public int UsersTimedOut { get; set; } = usersTimedOut;
        public int ModeratorsPresent { get; set; } = moderatorsPresent;
        public int SubsPresent { get; set; } = subsPresent;
        public int VIPsPresent { get; set; } = vIPsPresent;
        public int TotalChats { get; set; } = totalChats;
        public int Commands { get; set; } = commands;
        public int AutomatedEvents { get; set; } = automatedEvents;
        public int AutomatedCommands { get; set; } = automatedCommands;
        public int Webhooks { get; set; } = webhooks;
        public int ClipsMade { get; set; } = clipsMade;
        public int ChannelPtCount { get; set; } = channelPtCount;
        public int ChannelChallenge { get; set; } = channelChallenge;
        public int MaxUsers { get; set; } = maxUsers;

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
            Webhooks = 0;
            ClipsMade = 0;
            ChannelPtCount = 0;
            ChannelChallenge = 0;
            MaxUsers = 0;
        }
    }
}
