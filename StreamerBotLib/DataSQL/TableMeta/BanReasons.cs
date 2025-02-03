using StreamerBotLib.Enums;
using StreamerBotLib.DataSQL.Models;
using StreamerBotLib.Interfaces;
using StreamerBotLib.Overlay.Enums;

namespace StreamerBotLib.DataSQL.TableMeta
{
    internal class BanReasons : IDatabaseTableMeta
    {
        public System.Int32 Id { get => (System.Int32)Values["Id"]; set => Values["Id"] = value; }
        public StreamerBotLib.Enums.MsgTypes MsgType { get => (StreamerBotLib.Enums.MsgTypes)Values["MsgType"]; set => Values["MsgType"] = value; }
        public StreamerBotLib.Enums.BanReasons BanReason { get => (StreamerBotLib.Enums.BanReasons)Values["BanReason"]; set => Values["BanReason"] = value; }

        public Dictionary<string, object> Values { get; }

        public string TableName { get; } = "BanReasons";

        public BanReasons(Models.BanReasons tableData)
        {
            Values = new()
            {
                 { "Id", tableData.Id },
                 { "MsgType", tableData.MsgType },
                 { "BanReason", tableData.BanReason }
            };
        }
        public Dictionary<string, Type> Meta => new()
        {
              { "Id", typeof(System.Int32) },
              { "MsgType", typeof(StreamerBotLib.Enums.MsgTypes) },
              { "BanReason", typeof(StreamerBotLib.Enums.BanReasons) }
        };
        public object GetModelEntity()
        {
            return new Models.BanReasons(
            msgType: MsgType, 
            banReason: BanReason
        );
        }
        public void CopyUpdates(Models.BanReasons modelData)
        {
          if (modelData.MsgType != MsgType)
            {
                modelData.MsgType = MsgType;
            }

          if (modelData.BanReason != BanReason)
            {
                modelData.BanReason = BanReason;
            }

        }
    }
}

