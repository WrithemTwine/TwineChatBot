using StreamerBotLib.Enums;
using StreamerBotLib.DataSQL.Models;
using StreamerBotLib.Interfaces;
using StreamerBotLib.Overlay.Enums;

namespace StreamerBotLib.DataSQL.TableMeta
{
    internal class StreamStats : IDatabaseTableMeta
    {
        public System.DateTime StreamStart { get => (System.DateTime)Values["StreamStart"]; set => Values["StreamStart"] = value; }
        public System.DateTime StreamEnd { get => (System.DateTime)Values["StreamEnd"]; set => Values["StreamEnd"] = value; }
        public System.TimeSpan Duration { get => (System.TimeSpan)Values["Duration"]; set => Values["Duration"] = value; }
        public System.Int32 NewFollows { get => (System.Int32)Values["NewFollows"]; set => Values["NewFollows"] = value; }
        public System.Int32 NewSubscribers { get => (System.Int32)Values["NewSubscribers"]; set => Values["NewSubscribers"] = value; }
        public System.Int32 GiftSubs { get => (System.Int32)Values["GiftSubs"]; set => Values["GiftSubs"] = value; }
        public System.Int32 Bits { get => (System.Int32)Values["Bits"]; set => Values["Bits"] = value; }
        public System.Int32 Raids { get => (System.Int32)Values["Raids"]; set => Values["Raids"] = value; }
        public System.Int32 Hosted { get => (System.Int32)Values["Hosted"]; set => Values["Hosted"] = value; }
        public System.Int32 UsersBanned { get => (System.Int32)Values["UsersBanned"]; set => Values["UsersBanned"] = value; }
        public System.Int32 UsersTimedOut { get => (System.Int32)Values["UsersTimedOut"]; set => Values["UsersTimedOut"] = value; }
        public System.Int32 ModeratorsPresent { get => (System.Int32)Values["ModeratorsPresent"]; set => Values["ModeratorsPresent"] = value; }
        public System.Int32 SubsPresent { get => (System.Int32)Values["SubsPresent"]; set => Values["SubsPresent"] = value; }
        public System.Int32 VIPsPresent { get => (System.Int32)Values["VIPsPresent"]; set => Values["VIPsPresent"] = value; }
        public System.Int32 TotalChats { get => (System.Int32)Values["TotalChats"]; set => Values["TotalChats"] = value; }
        public System.Int32 CommandMsgs { get => (System.Int32)Values["CommandMsgs"]; set => Values["CommandMsgs"] = value; }
        public System.Int32 AutomatedEvents { get => (System.Int32)Values["AutomatedEvents"]; set => Values["AutomatedEvents"] = value; }
        public System.Int32 AutomatedCommands { get => (System.Int32)Values["AutomatedCommands"]; set => Values["AutomatedCommands"] = value; }
        public System.Int32 WebhookMsgs { get => (System.Int32)Values["WebhookMsgs"]; set => Values["WebhookMsgs"] = value; }
        public System.Int32 ClipsMade { get => (System.Int32)Values["ClipsMade"]; set => Values["ClipsMade"] = value; }
        public System.Int32 ChannelPtCount { get => (System.Int32)Values["ChannelPtCount"]; set => Values["ChannelPtCount"] = value; }
        public System.Int32 ChannelChallenge { get => (System.Int32)Values["ChannelChallenge"]; set => Values["ChannelChallenge"] = value; }
        public System.Int32 MaxUsers { get => (System.Int32)Values["MaxUsers"]; set => Values["MaxUsers"] = value; }
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
            newFollows: Convert.ToInt32(NewFollows), 
            newSubscribers: Convert.ToInt32(NewSubscribers), 
            giftSubs: Convert.ToInt32(GiftSubs), 
            bits: Convert.ToInt32(Bits), 
            raids: Convert.ToInt32(Raids), 
            hosted: Convert.ToInt32(Hosted), 
            usersBanned: Convert.ToInt32(UsersBanned), 
            usersTimedOut: Convert.ToInt32(UsersTimedOut), 
            moderatorsPresent: Convert.ToInt32(ModeratorsPresent), 
            subsPresent: Convert.ToInt32(SubsPresent), 
            vIPsPresent: Convert.ToInt32(VIPsPresent), 
            totalChats: Convert.ToInt32(TotalChats), 
            commandMsgs: Convert.ToInt32(CommandMsgs), 
            automatedEvents: Convert.ToInt32(AutomatedEvents), 
            automatedCommands: Convert.ToInt32(AutomatedCommands), 
            webhookMsgs: Convert.ToInt32(WebhookMsgs), 
            clipsMade: Convert.ToInt32(ClipsMade), 
            channelPtCount: Convert.ToInt32(ChannelPtCount), 
            channelChallenge: Convert.ToInt32(ChannelChallenge), 
            maxUsers: Convert.ToInt32(MaxUsers), 
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

