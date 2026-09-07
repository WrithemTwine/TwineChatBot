using StreamerBotLib.Models.Enums;
using StreamerBotLib.Models.Interfaces;

namespace StreamerBotLib.DataSQL.TableMeta
{
    internal class UserStats : IDatabaseTableMeta
    {
        public System.TimeSpan WatchTime { get => (System.TimeSpan)Values["WatchTime"]; set => Values["WatchTime"] = value; }
        public System.Int32 ChannelChat { get => (System.Int32)Values["ChannelChat"]; set => Values["ChannelChat"] = value; }
        public System.Int32 CallCommands { get => (System.Int32)Values["CallCommands"]; set => Values["CallCommands"] = value; }
        public System.Int32 RewardRedeems { get => (System.Int32)Values["RewardRedeems"]; set => Values["RewardRedeems"] = value; }
        public System.Int32 ClipsCreated { get => (System.Int32)Values["ClipsCreated"]; set => Values["ClipsCreated"] = value; }
        public System.Int32 ShoutOutsGiven { get => (System.Int32)Values["ShoutOutsGiven"]; set => Values["ShoutOutsGiven"] = value; }
        public System.Int32 CustomWelcomeMessages { get => (System.Int32)Values["CustomWelcomeMessages"]; set => Values["CustomWelcomeMessages"] = value; }
        public System.Int32 GiveawaysEntered { get => (System.Int32)Values["GiveawaysEntered"]; set => Values["GiveawaysEntered"] = value; }
        public System.Int32 GiveawaysWon { get => (System.Int32)Values["GiveawaysWon"]; set => Values["GiveawaysWon"] = value; }
        public System.Int32 RaidsReceived { get => (System.Int32)Values["RaidsReceived"]; set => Values["RaidsReceived"] = value; }
        public System.String UserId { get => (System.String)Values["UserId"]; set => Values["UserId"] = value; }
        public Platform Platform { get => (Platform)Values["Platform"]; set => Values["Platform"] = value; }

        public Dictionary<string, object> Values { get; }

        public string TableName { get; } = "UserStats";

        public UserStats(Models.UserStats tableData)
        {
            Values = new()
            {
                 { "WatchTime", tableData.WatchTime },
                 { "ChannelChat", tableData.ChannelChat },
                 { "CallCommands", tableData.CallCommands },
                 { "RewardRedeems", tableData.RewardRedeems },
                 { "ClipsCreated", tableData.ClipsCreated },
                 { "ShoutOutsGiven", tableData.ShoutOutsGiven },
                 { "CustomWelcomeMessages", tableData.CustomWelcomeMessages },
                 { "GiveawaysEntered", tableData.GiveawaysEntered },
                 { "GiveawaysWon", tableData.GiveawaysWon },
                 { "RaidsReceived", tableData.RaidsReceived },
                 { "UserId", tableData.UserId },
                 { "Platform", tableData.Platform }
            };
        }
        public Dictionary<string, Type> Meta => new()
        {
              { "WatchTime", typeof(System.TimeSpan) },
              { "ChannelChat", typeof(System.Int32) },
              { "CallCommands", typeof(System.Int32) },
              { "RewardRedeems", typeof(System.Int32) },
              { "ClipsCreated", typeof(System.Int32) },
              { "ShoutOutsGiven", typeof(System.Int32) },
              { "CustomWelcomeMessages", typeof(System.Int32) },
              { "GiveawaysEntered", typeof(System.Int32) },
              { "GiveawaysWon", typeof(System.Int32) },
              { "RaidsReceived", typeof(System.Int32) },
              { "UserId", typeof(System.String) },
              { "Platform", typeof(Platform) }
        };
        public object GetModelEntity()
        {
            return new Models.UserStats(
            watchTime: WatchTime,
            channelChat: Convert.ToInt32(ChannelChat),
            callCommands: Convert.ToInt32(CallCommands),
            rewardRedeems: Convert.ToInt32(RewardRedeems),
            clipsCreated: Convert.ToInt32(ClipsCreated),
            userId: UserId,
            platform: Platform
        );
        }
        public void CopyUpdates(Models.UserStats modelData)
        {
            if (modelData.WatchTime != WatchTime)
            {
                modelData.WatchTime = WatchTime;
            }

            if (modelData.ChannelChat != ChannelChat)
            {
                modelData.ChannelChat = ChannelChat;
            }

            if (modelData.CallCommands != CallCommands)
            {
                modelData.CallCommands = CallCommands;
            }

            if (modelData.RewardRedeems != RewardRedeems)
            {
                modelData.RewardRedeems = RewardRedeems;
            }

            if (modelData.ClipsCreated != ClipsCreated)
            {
                modelData.ClipsCreated = ClipsCreated;
            }

            if (modelData.ShoutOutsGiven != ShoutOutsGiven)
            {
                modelData.ShoutOutsGiven = ShoutOutsGiven;
            }

            if (modelData.CustomWelcomeMessages != CustomWelcomeMessages)
            {
                modelData.CustomWelcomeMessages = CustomWelcomeMessages;
            }

            if (modelData.GiveawaysEntered != GiveawaysEntered)
            {
                modelData.GiveawaysEntered = GiveawaysEntered;
            }

            if (modelData.GiveawaysWon != GiveawaysWon)
            {
                modelData.GiveawaysWon = GiveawaysWon;
            }

            if (modelData.RaidsReceived != RaidsReceived)
            {
                modelData.RaidsReceived = RaidsReceived;
            }

            if (modelData.UserId != UserId)
            {
                modelData.UserId = UserId;
            }

            if (modelData.Platform != Platform)
            {
                modelData.Platform = Platform;
            }

        }
    }
}

