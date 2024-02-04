using Microsoft.EntityFrameworkCore;

namespace StreamerBotLib.DataSQL.Models
{
    [PrimaryKey(nameof(StreamStart))]
    [Index(nameof(StreamStart), IsDescending = [true])]
    public class StreamStats(DateTime streamStart = default,
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
                             int commandsMsgs = 0,
                             int automatedEvents = 0,
                             int automatedCommands = 0,
                             int webhookMsgs = 0,
                             int clipsMade = 0,
                             int channelPtCount = 0,
                             int channelChallenge = 0,
                             int maxUsers = 0,
                             string category = default) : EntityBase
    {
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
        public int CommandsMsgs { get; set; } = commandsMsgs;
        public int AutomatedEvents { get; set; } = automatedEvents;
        public int AutomatedCommands { get; set; } = automatedCommands;
        public int WebhookMsgs { get; set; } = webhookMsgs;
        public int ClipsMade { get; set; } = clipsMade;
        public int ChannelPtCount { get; set; } = channelPtCount;
        public int ChannelChallenge { get; set; } = channelChallenge;
        public int MaxUsers { get; set; } = maxUsers;
        public string Category { get; set; } = category;

        public void Update(StreamerBotLib.Models.StreamStat streamStat)
        {
            StreamStart = streamStat.StreamStart;
            StreamEnd = streamStat.StreamEnd;
            NewFollows = streamStat.NewFollows;
            NewSubscribers = streamStat.NewSubscribers;
            GiftSubs = streamStat.GiftSubs;
            Bits = streamStat.Bits;
            Raids = streamStat.Raids;
            Hosted = streamStat.Hosted;
            UsersBanned = streamStat.UsersBanned;
            UsersTimedOut = streamStat.UsersTimedOut;
            ModeratorsPresent = streamStat.ModeratorsPresent;
            SubsPresent = streamStat.SubsPresent;
            VIPsPresent = streamStat.VIPsPresent;
            TotalChats = streamStat.TotalChats;
            CommandsMsgs = streamStat.CommandsMsgs;
            AutomatedEvents = streamStat.AutomatedEvents;
            AutomatedCommands = streamStat.AutomatedCommands;
            WebhookMsgs = streamStat.WebhookMsgs;
            ClipsMade = streamStat.ClipsMade;
            ChannelPtCount = streamStat.ChannelPtCount;
            ChannelChallenge = streamStat.ChannelChallenge;
            MaxUsers = streamStat.MaxUsers;
            Category = streamStat.Category;
        }
    }
}
