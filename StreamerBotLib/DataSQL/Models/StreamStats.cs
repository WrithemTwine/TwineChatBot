using Microsoft.EntityFrameworkCore;

using System.ComponentModel.DataAnnotations.Schema;

namespace StreamerBotLib.DataSQL.Models
{
    [Index(nameof(StreamStart))]
    public class StreamStats(uint id,
                             DateTime streamStart = default,
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
                             string category = default)
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public uint Id { get; set; } = id;
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
        public string Category { get; set; } = category;

    }
}
