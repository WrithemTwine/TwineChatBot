using StreamerBotLib.Interfaces;

namespace StreamerBotLib.DataSQL.TableMeta
{
    internal class BanReasons : IDatabaseTableMeta
    {
        public System.Int32 Id => (System.Int32)Values["Id"];
        public StreamerBotLib.Enums.MsgTypes MsgType => (StreamerBotLib.Enums.MsgTypes)Values["MsgType"];
        public StreamerBotLib.Enums.BanReasons BanReason => (StreamerBotLib.Enums.BanReasons)Values["BanReason"];

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
                                          Convert.ToInt32(Values["Id"]),
                                          (StreamerBotLib.Enums.MsgTypes)Values["MsgType"],
                                          (StreamerBotLib.Enums.BanReasons)Values["BanReason"]
);
        }
        public void CopyUpdates(Models.BanReasons modelData)
        {
            if (modelData.Id != Id)
            {
                modelData.Id = Id;
            }

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

