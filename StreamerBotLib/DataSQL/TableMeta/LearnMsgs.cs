using StreamerBotLib.Interfaces;

namespace StreamerBotLib.DataSQL.TableMeta
{
    internal class LearnMsgs : IDatabaseTableMeta
    {
        public System.Int32 Id => (System.Int32)Values["Id"];
        public StreamerBotLib.Enums.MsgTypes MsgType => (StreamerBotLib.Enums.MsgTypes)Values["MsgType"];
        public System.String TeachingMsg => (System.String)Values["TeachingMsg"];

        public Dictionary<string, object> Values { get; }

        public string TableName { get; } = "LearnMsgs";

        public LearnMsgs(Models.LearnMsgs tableData)
        {
            Values = new()
            {
                 { "Id", tableData.Id },
                 { "MsgType", tableData.MsgType },
                 { "TeachingMsg", tableData.TeachingMsg }
            };
        }
        public Dictionary<string, Type> Meta => new()
        {
              { "Id", typeof(System.Int32) },
              { "MsgType", typeof(StreamerBotLib.Enums.MsgTypes) },
              { "TeachingMsg", typeof(System.String) }
        };
        public object GetModelEntity()
        {
            return new Models.LearnMsgs(
                                          (System.Int32)Values["Id"],
                                          (StreamerBotLib.Enums.MsgTypes)Values["MsgType"],
                                          (System.String)Values["TeachingMsg"]
);
        }
        public void CopyUpdates(Models.LearnMsgs modelData)
        {
            if (modelData.Id != Id)
            {
                modelData.Id = Id;
            }

            if (modelData.MsgType != MsgType)
            {
                modelData.MsgType = MsgType;
            }

            if (modelData.TeachingMsg != TeachingMsg)
            {
                modelData.TeachingMsg = TeachingMsg;
            }

        }
    }
}

