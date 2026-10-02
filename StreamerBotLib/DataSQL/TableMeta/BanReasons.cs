using StreamerBotLib.Models.Interfaces;

namespace StreamerBotLib.DataSQL.TableMeta
{
    internal class BanReasons : IDatabaseTableMeta
    {
        public System.Int32 Id { get => Convert.ToInt32(Values["Id"]); set => Values["Id"] = value; }
        public StreamerBotLib.Models.Enums.MsgTypes MsgType { get => (StreamerBotLib.Models.Enums.MsgTypes)Enum.Parse(typeof(StreamerBotLib.Models.Enums.MsgTypes), Values["MsgType"]?.ToString() ?? "0"); set => Values["MsgType"] = value; }
        public StreamerBotLib.Models.Enums.BanReasons BanReason { get => (StreamerBotLib.Models.Enums.BanReasons)Enum.Parse(typeof(StreamerBotLib.Models.Enums.BanReasons), Values["BanReason"]?.ToString() ?? "0"); set => Values["BanReason"] = value; }

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
              { "MsgType", typeof(StreamerBotLib.Models.Enums.MsgTypes) },
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