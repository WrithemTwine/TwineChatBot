using StreamerBotLib.Models.Interfaces;

namespace StreamerBotLib.DataSQL.TableMeta
{
    internal class UserStats : IDatabaseTableMeta
    {
        public System.TimeSpan WatchTime { get => (System.TimeSpan)Values["WatchTime"]; set => Values["WatchTime"] = value; }
        public System.Int32 ChannelChat { get => Convert.ToInt32(Values["ChannelChat"]); set => Values["ChannelChat"] = value; }
        public System.Int32 CallCommands { get => Convert.ToInt32(Values["CallCommands"]); set => Values["CallCommands"] = value; }
        public System.Int32 RewardRedeems { get => Convert.ToInt32(Values["RewardRedeems"]); set => Values["RewardRedeems"] = value; }
        public System.Int32 ClipsCreated { get => Convert.ToInt32(Values["ClipsCreated"]); set => Values["ClipsCreated"] = value; }
        public System.Int32 ShoutOutsGiven { get => Convert.ToInt32(Values["ShoutOutsGiven"]); set => Values["ShoutOutsGiven"] = value; }
        public System.Int32 CustomWelcomeMessages { get => Convert.ToInt32(Values["CustomWelcomeMessages"]); set => Values["CustomWelcomeMessages"] = value; }
        public System.Int32 GiveawaysEntered { get => Convert.ToInt32(Values["GiveawaysEntered"]); set => Values["GiveawaysEntered"] = value; }
        public System.Int32 GiveawaysWon { get => Convert.ToInt32(Values["GiveawaysWon"]); set => Values["GiveawaysWon"] = value; }
        public System.Int32 RaidsReceived { get => Convert.ToInt32(Values["RaidsReceived"]); set => Values["RaidsReceived"] = value; }
        public System.String UserId { get => (string)Values["UserId"]; set => Values["UserId"] = value; }
        public StreamerBotLib.Models.Enums.Platform Platform { get => (StreamerBotLib.Models.Enums.Platform)Enum.Parse(typeof(StreamerBotLib.Models.Enums.Platform), Values["Platform"]?.ToString() ?? "0"); set => Values["Platform"] = value; }

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
              { "Platform", typeof(StreamerBotLib.Models.Enums.Platform) }
        };

        public object GetModelEntity()
        {
            return new Models.UserStats(
            watchTime: WatchTime,
            channelChat: ChannelChat,
            callCommands: CallCommands,
            rewardRedeems: RewardRedeems,
            clipsCreated: ClipsCreated,
            shoutOutsGiven: ShoutOutsGiven,
            customWelcomeMessages: CustomWelcomeMessages,
            giveawaysEntered: GiveawaysEntered,
            giveawaysWon: GiveawaysWon,
            raidsReceived: RaidsReceived,
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