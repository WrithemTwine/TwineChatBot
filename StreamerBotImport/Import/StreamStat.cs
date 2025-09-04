using static StreamerBotImport.Import.DataSource;

namespace StreamerBotImport.Import
{
    internal class StreamStat : StreamerBotLib.Models.StreamStat
    {
        public void Update(StreamStatsRow streamStatsRow)
        {
            StreamStart = streamStatsRow.StreamStart;
            StreamEnd = streamStatsRow.StreamEnd;
            NewFollows = streamStatsRow.NewFollows;
            NewSubscribers = streamStatsRow.NewSubscribers;
            GiftSubs = streamStatsRow.GiftSubs;
            try
            {
                Bits = Convert.ToInt32(streamStatsRow.Bits);
            }
            catch (OverflowException)
            {
                Bits = int.MaxValue;
            }
            Raids = streamStatsRow.Raids;
            Hosted = streamStatsRow.Hosted;
            UsersBanned = streamStatsRow.UsersBanned;
            UsersTimedOut = streamStatsRow.UsersTimedOut;
            ModeratorsPresent = streamStatsRow.ModeratorsPresent;
            SubsPresent = streamStatsRow.SubsPresent;
            VIPsPresent = streamStatsRow.VIPsPresent;
            TotalChats = streamStatsRow.TotalChats;
            CommandMsgs = streamStatsRow.Commands;
            AutomatedCommands = streamStatsRow.AutomatedCommands;
            WebhookMsgs = streamStatsRow.DiscordMsgs;
            ClipsMade = streamStatsRow.ClipsMade;
            ChannelPtCount = streamStatsRow.ChannelPtCount;
            ChannelChallenge = streamStatsRow.ChannelChallenge;
            MaxUsers = streamStatsRow.MaxUsers;
        }
    }
}
