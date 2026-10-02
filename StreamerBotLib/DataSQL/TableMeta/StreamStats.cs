using StreamerBotLib.Models.Interfaces;

namespace StreamerBotLib.DataSQL.TableMeta
{
    internal class StreamStats : IDatabaseTableMeta
    {
        public System.DateTime StreamStart { get => Convert.ToDateTime(Values["StreamStart"]); set => Values["StreamStart"] = value; }
        public System.DateTime StreamEnd { get => Convert.ToDateTime(Values["StreamEnd"]); set => Values["StreamEnd"] = value; }
        public System.TimeSpan Duration { get => (System.TimeSpan)Values["Duration"]; set => Values["Duration"] = value; }
        public System.Int32 NewFollows { get => Convert.ToInt32(Values["NewFollows"]); set => Values["NewFollows"] = value; }
        public System.Int32 NewSubscribers { get => Convert.ToInt32(Values["NewSubscribers"]); set => Values["NewSubscribers"] = value; }
        public System.Int32 GiftSubs { get => Convert.ToInt32(Values["GiftSubs"]); set => Values["GiftSubs"] = value; }
        public System.Int32 Bits { get => Convert.ToInt32(Values["Bits"]); set => Values["Bits"] = value; }
        public System.Int32 Raids { get => Convert.ToInt32(Values["Raids"]); set => Values["Raids"] = value; }
        public System.Int32 Hosted { get => Convert.ToInt32(Values["Hosted"]); set => Values["Hosted"] = value; }
        public System.Int32 UsersBanned { get => Convert.ToInt32(Values["UsersBanned"]); set => Values["UsersBanned"] = value; }
        public System.Int32 UsersTimedOut { get => Convert.ToInt32(Values["UsersTimedOut"]); set => Values["UsersTimedOut"] = value; }
        public System.Int32 ModeratorsPresent { get => Convert.ToInt32(Values["ModeratorsPresent"]); set => Values["ModeratorsPresent"] = value; }
        public System.Int32 SubsPresent { get => Convert.ToInt32(Values["SubsPresent"]); set => Values["SubsPresent"] = value; }
        public System.Int32 VIPsPresent { get => Convert.ToInt32(Values["VIPsPresent"]); set => Values["VIPsPresent"] = value; }
        public System.Int32 TotalChats { get => Convert.ToInt32(Values["TotalChats"]); set => Values["TotalChats"] = value; }
        public System.Int32 CommandMsgs { get => Convert.ToInt32(Values["CommandMsgs"]); set => Values["CommandMsgs"] = value; }
        public System.Int32 AutomatedEvents { get => Convert.ToInt32(Values["AutomatedEvents"]); set => Values["AutomatedEvents"] = value; }
        public System.Int32 AutomatedCommands { get => Convert.ToInt32(Values["AutomatedCommands"]); set => Values["AutomatedCommands"] = value; }
        public System.Int32 WebhookMsgs { get => Convert.ToInt32(Values["WebhookMsgs"]); set => Values["WebhookMsgs"] = value; }
        public System.Int32 ClipsMade { get => Convert.ToInt32(Values["ClipsMade"]); set => Values["ClipsMade"] = value; }
        public System.Int32 ChannelPtCount { get => Convert.ToInt32(Values["ChannelPtCount"]); set => Values["ChannelPtCount"] = value; }
        public System.Int32 ChannelChallenge { get => Convert.ToInt32(Values["ChannelChallenge"]); set => Values["ChannelChallenge"] = value; }
        public System.Int32 MaxUsers { get => Convert.ToInt32(Values["MaxUsers"]); set => Values["MaxUsers"] = value; }
        public ICollection<System.String> Category { get => (ICollection<System.String>)Values["Category"]; set => Values["Category"] = value; }

        public Dictionary<string, object> Values { get; }

        public string TableName { get; } = "StreamStats";

        public StreamStats(Models.StreamStats tableData)
        {
            Values = new()
            {
                 { "StreamStart", tableData.StreamStart },
                 { "StreamEnd", tableData.StreamEnd },
                 { "Duration", tableData.Duration },
                 { "NewFollows", tableData.NewFollows },
                 { "NewSubscribers", tableData.NewSubscribers },
                 { "GiftSubs", tableData.GiftSubs },
                 { "Bits", tableData.Bits },
                 { "Raids", tableData.Raids },
                 { "Hosted", tableData.Hosted },
                 { "UsersBanned", tableData.UsersBanned },
                 { "UsersTimedOut", tableData.UsersTimedOut },
                 { "ModeratorsPresent", tableData.ModeratorsPresent },
                 { "SubsPresent", tableData.SubsPresent },
                 { "VIPsPresent", tableData.VIPsPresent },
                 { "TotalChats", tableData.TotalChats },
                 { "CommandMsgs", tableData.CommandMsgs },
                 { "AutomatedEvents", tableData.AutomatedEvents },
                 { "AutomatedCommands", tableData.AutomatedCommands },
                 { "WebhookMsgs", tableData.WebhookMsgs },
                 { "ClipsMade", tableData.ClipsMade },
                 { "ChannelPtCount", tableData.ChannelPtCount },
                 { "ChannelChallenge", tableData.ChannelChallenge },
                 { "MaxUsers", tableData.MaxUsers },
                 { "Category", tableData.Category }
            };
        }

        public Dictionary<string, Type> Meta => new()
        {
              { "StreamStart", typeof(System.DateTime) },
              { "StreamEnd", typeof(System.DateTime) },
              { "Duration", typeof(System.TimeSpan) },
              { "NewFollows", typeof(System.Int32) },
              { "NewSubscribers", typeof(System.Int32) },
              { "GiftSubs", typeof(System.Int32) },
              { "Bits", typeof(System.Int32) },
              { "Raids", typeof(System.Int32) },
              { "Hosted", typeof(System.Int32) },
              { "UsersBanned", typeof(System.Int32) },
              { "UsersTimedOut", typeof(System.Int32) },
              { "ModeratorsPresent", typeof(System.Int32) },
              { "SubsPresent", typeof(System.Int32) },
              { "VIPsPresent", typeof(System.Int32) },
              { "TotalChats", typeof(System.Int32) },
              { "CommandMsgs", typeof(System.Int32) },
              { "AutomatedEvents", typeof(System.Int32) },
              { "AutomatedCommands", typeof(System.Int32) },
              { "WebhookMsgs", typeof(System.Int32) },
              { "ClipsMade", typeof(System.Int32) },
              { "ChannelPtCount", typeof(System.Int32) },
              { "ChannelChallenge", typeof(System.Int32) },
              { "MaxUsers", typeof(System.Int32) },
              { "Category", typeof(ICollection<System.String>) }
        };

        public object GetModelEntity()
        {
            return new Models.StreamStats(
            streamStart: StreamStart,
            streamEnd: StreamEnd,
            newFollows: NewFollows,
            newSubscribers: NewSubscribers,
            giftSubs: GiftSubs,
            bits: Bits,
            raids: Raids,
            hosted: Hosted,
            usersBanned: UsersBanned,
            usersTimedOut: UsersTimedOut,
            moderatorsPresent: ModeratorsPresent,
            subsPresent: SubsPresent,
            vIPsPresent: VIPsPresent,
            totalChats: TotalChats,
            commandMsgs: CommandMsgs,
            automatedEvents: AutomatedEvents,
            automatedCommands: AutomatedCommands,
            webhookMsgs: WebhookMsgs,
            clipsMade: ClipsMade,
            channelPtCount: ChannelPtCount,
            channelChallenge: ChannelChallenge,
            maxUsers: MaxUsers,
            category: Category
            );
        }

        public void CopyUpdates(Models.StreamStats modelData)
        {
            if (modelData.StreamStart != StreamStart)
            {
                modelData.StreamStart = StreamStart;
            }
            if (modelData.StreamEnd != StreamEnd)
            {
                modelData.StreamEnd = StreamEnd;
            }
            if (modelData.NewFollows != NewFollows)
            {
                modelData.NewFollows = NewFollows;
            }
            if (modelData.NewSubscribers != NewSubscribers)
            {
                modelData.NewSubscribers = NewSubscribers;
            }
            if (modelData.GiftSubs != GiftSubs)
            {
                modelData.GiftSubs = GiftSubs;
            }
            if (modelData.Bits != Bits)
            {
                modelData.Bits = Bits;
            }
            if (modelData.Raids != Raids)
            {
                modelData.Raids = Raids;
            }
            if (modelData.Hosted != Hosted)
            {
                modelData.Hosted = Hosted;
            }
            if (modelData.UsersBanned != UsersBanned)
            {
                modelData.UsersBanned = UsersBanned;
            }
            if (modelData.UsersTimedOut != UsersTimedOut)
            {
                modelData.UsersTimedOut = UsersTimedOut;
            }
            if (modelData.ModeratorsPresent != ModeratorsPresent)
            {
                modelData.ModeratorsPresent = ModeratorsPresent;
            }
            if (modelData.SubsPresent != SubsPresent)
            {
                modelData.SubsPresent = SubsPresent;
            }
            if (modelData.VIPsPresent != VIPsPresent)
            {
                modelData.VIPsPresent = VIPsPresent;
            }
            if (modelData.TotalChats != TotalChats)
            {
                modelData.TotalChats = TotalChats;
            }
            if (modelData.CommandMsgs != CommandMsgs)
            {
                modelData.CommandMsgs = CommandMsgs;
            }
            if (modelData.AutomatedEvents != AutomatedEvents)
            {
                modelData.AutomatedEvents = AutomatedEvents;
            }
            if (modelData.AutomatedCommands != AutomatedCommands)
            {
                modelData.AutomatedCommands = AutomatedCommands;
            }
            if (modelData.WebhookMsgs != WebhookMsgs)
            {
                modelData.WebhookMsgs = WebhookMsgs;
            }
            if (modelData.ClipsMade != ClipsMade)
            {
                modelData.ClipsMade = ClipsMade;
            }
            if (modelData.ChannelPtCount != ChannelPtCount)
            {
                modelData.ChannelPtCount = ChannelPtCount;
            }
            if (modelData.ChannelChallenge != ChannelChallenge)
            {
                modelData.ChannelChallenge = ChannelChallenge;
            }
            if (modelData.MaxUsers != MaxUsers)
            {
                modelData.MaxUsers = MaxUsers;
            }
            if (modelData.Category != Category)
            {
                modelData.Category = Category;
            }
        }
    }
}