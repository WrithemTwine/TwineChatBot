using StreamerBotLib.Interfaces;

namespace StreamerBotLib.DataSQL.TableMeta
{
    internal class StreamStats : IDatabaseTableMeta
    {
        public System.DateTime StreamStart => (System.DateTime)Values["StreamStart"];
        public System.DateTime StreamEnd => (System.DateTime)Values["StreamEnd"];
        public System.Int32 NewFollows => (System.Int32)Values["NewFollows"];
        public System.Int32 NewSubscribers => (System.Int32)Values["NewSubscribers"];
        public System.Int32 GiftSubs => (System.Int32)Values["GiftSubs"];
        public System.Int32 Bits => (System.Int32)Values["Bits"];
        public System.Int32 Raids => (System.Int32)Values["Raids"];
        public System.Int32 Hosted => (System.Int32)Values["Hosted"];
        public System.Int32 UsersBanned => (System.Int32)Values["UsersBanned"];
        public System.Int32 UsersTimedOut => (System.Int32)Values["UsersTimedOut"];
        public System.Int32 ModeratorsPresent => (System.Int32)Values["ModeratorsPresent"];
        public System.Int32 SubsPresent => (System.Int32)Values["SubsPresent"];
        public System.Int32 VIPsPresent => (System.Int32)Values["VIPsPresent"];
        public System.Int32 TotalChats => (System.Int32)Values["TotalChats"];
        public System.Int32 CommandsMsgs => (System.Int32)Values["CommandsMsgs"];
        public System.Int32 AutomatedEvents => (System.Int32)Values["AutomatedEvents"];
        public System.Int32 AutomatedCommands => (System.Int32)Values["AutomatedCommands"];
        public System.Int32 WebhookMsgs => (System.Int32)Values["WebhookMsgs"];
        public System.Int32 ClipsMade => (System.Int32)Values["ClipsMade"];
        public System.Int32 ChannelPtCount => (System.Int32)Values["ChannelPtCount"];
        public System.Int32 ChannelChallenge => (System.Int32)Values["ChannelChallenge"];
        public System.Int32 MaxUsers => (System.Int32)Values["MaxUsers"];
        public System.String Category => (System.String)Values["Category"];

        public Dictionary<string, object> Values { get; }

        public string TableName { get; } = "StreamStats";

        public StreamStats(Models.StreamStats tableData)
        {
            Values = new()
            {
                 { "StreamStart", tableData.StreamStart },
                 { "StreamEnd", tableData.StreamEnd },
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
                 { "CommandsMsgs", tableData.CommandsMsgs },
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
              { "CommandsMsgs", typeof(System.Int32) },
              { "AutomatedEvents", typeof(System.Int32) },
              { "AutomatedCommands", typeof(System.Int32) },
              { "WebhookMsgs", typeof(System.Int32) },
              { "ClipsMade", typeof(System.Int32) },
              { "ChannelPtCount", typeof(System.Int32) },
              { "ChannelChallenge", typeof(System.Int32) },
              { "MaxUsers", typeof(System.Int32) },
              { "Category", typeof(System.String) }
        };
        public object GetModelEntity()
        {
            return new Models.StreamStats(
                                          (System.DateTime)Values["StreamStart"],
                                          (System.DateTime)Values["StreamEnd"],
                                          (System.Int32)Values["NewFollows"],
                                          (System.Int32)Values["NewSubscribers"],
                                          (System.Int32)Values["GiftSubs"],
                                          (System.Int32)Values["Bits"],
                                          (System.Int32)Values["Raids"],
                                          (System.Int32)Values["Hosted"],
                                          (System.Int32)Values["UsersBanned"],
                                          (System.Int32)Values["UsersTimedOut"],
                                          (System.Int32)Values["ModeratorsPresent"],
                                          (System.Int32)Values["SubsPresent"],
                                          (System.Int32)Values["VIPsPresent"],
                                          (System.Int32)Values["TotalChats"],
                                          (System.Int32)Values["CommandsMsgs"],
                                          (System.Int32)Values["AutomatedEvents"],
                                          (System.Int32)Values["AutomatedCommands"],
                                          (System.Int32)Values["WebhookMsgs"],
                                          (System.Int32)Values["ClipsMade"],
                                          (System.Int32)Values["ChannelPtCount"],
                                          (System.Int32)Values["ChannelChallenge"],
                                          (System.Int32)Values["MaxUsers"],
                                          (System.String)Values["Category"]
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

            if (modelData.CommandsMsgs != CommandsMsgs)
            {
                modelData.CommandsMsgs = CommandsMsgs;
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

