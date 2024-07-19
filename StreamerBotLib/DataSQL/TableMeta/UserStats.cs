using StreamerBotLib.Enums;
using StreamerBotLib.DataSQL.Models;
using StreamerBotLib.Interfaces;
using StreamerBotLib.Overlay.Enums;

namespace StreamerBotLib.DataSQL.TableMeta
{
    internal class UserStats : IDatabaseTableMeta
    {
        public System.TimeSpan WatchTime => (System.TimeSpan)Values["WatchTime"];
        public System.Int32 ChannelChat => (System.Int32)Values["ChannelChat"];
        public System.Int32 CallCommands => (System.Int32)Values["CallCommands"];
        public System.Int32 RewardRedeems => (System.Int32)Values["RewardRedeems"];
        public System.Int32 ClipsCreated => (System.Int32)Values["ClipsCreated"];
        public System.String UserId => (System.String)Values["UserId"];
        public System.String UserName => (System.String)Values["UserName"];
        public StreamerBotLib.Enums.Platform Platform => (StreamerBotLib.Enums.Platform)Values["Platform"];

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
                 { "UserName", tableData.UserName },
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
              { "UserName", typeof(System.String) },
              { "Platform", typeof(StreamerBotLib.Enums.Platform) }
        };
        public object GetModelEntity()
        {
            return new Models.UserStats(
                                          (System.TimeSpan)Values["WatchTime"], 
                                          Convert.ToInt32(Values["ChannelChat"]), 
                                          Convert.ToInt32(Values["CallCommands"]), 
                                          Convert.ToInt32(Values["RewardRedeems"]), 
                                          Convert.ToInt32(Values["ClipsCreated"]), 
                                          (System.String)Values["UserId"], 
                                          (System.String)Values["UserName"], 
                                          (StreamerBotLib.Enums.Platform)Values["Platform"]
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

          if (modelData.UserName != UserName)
            {
                modelData.UserName = UserName;
            }

          if (modelData.Platform != Platform)
            {
                modelData.Platform = Platform;
            }

        }
    }
}

