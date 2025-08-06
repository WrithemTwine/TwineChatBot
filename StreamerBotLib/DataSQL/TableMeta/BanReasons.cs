using StreamerBotLib.Models.Enums;
using StreamerBotLib.Models.Interfaces;

namespace StreamerBotLib.DataSQL.TableMeta
{
    internal class BanReasons : IDatabaseTableMeta
    {
        public System.Int32 Id { get => (System.Int32)Values["Id"]; set => Values["Id"] = value; }
        public MsgTypes MsgType { get => (MsgTypes)Values["MsgType"]; set => Values["MsgType"] = value; }
        public StreamerBotLib.Models.Enums.BanReasons BanReason { get => (StreamerBotLib.Models.Enums.BanReasons)Values["BanReason"]; set => Values["BanReason"] = value; }

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
              { "MsgType", typeof(MsgTypes) },
              { "BanReason", typeof(StreamerBotLib.Models.Enums.BanReasons) }
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

