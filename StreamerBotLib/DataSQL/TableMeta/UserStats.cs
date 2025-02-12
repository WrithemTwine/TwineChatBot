using StreamerBotLib.Interfaces;

namespace StreamerBotLib.DataSQL.TableMeta
{
    internal class UserStats : IDatabaseTableMeta
    {
        public System.TimeSpan WatchTime { get => (System.TimeSpan)Values["WatchTime"]; set => Values["WatchTime"] = value; }
        public System.Int32 ChannelChat { get => (System.Int32)Values["ChannelChat"]; set => Values["ChannelChat"] = value; }
        public System.Int32 CallCommands { get => (System.Int32)Values["CallCommands"]; set => Values["CallCommands"] = value; }
        public System.Int32 RewardRedeems { get => (System.Int32)Values["RewardRedeems"]; set => Values["RewardRedeems"] = value; }
        public System.Int32 ClipsCreated { get => (System.Int32)Values["ClipsCreated"]; set => Values["ClipsCreated"] = value; }
        public System.String UserId { get => (System.String)Values["UserId"]; set => Values["UserId"] = value; }
        public StreamerBotLib.Enums.Platform Platform { get => (StreamerBotLib.Enums.Platform)Values["Platform"]; set => Values["Platform"] = value; }

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
              { "UserId", typeof(System.String) },
              { "Platform", typeof(StreamerBotLib.Enums.Platform) }
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

