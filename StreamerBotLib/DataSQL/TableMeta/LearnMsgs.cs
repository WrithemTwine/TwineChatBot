
namespace StreamerBotLib.DataSQL.TableMeta
{
    using StreamerBotLib.Models.Enums;
    using StreamerBotLib.Models.Interfaces;
    internal class LearnMsgs : IDatabaseTableMeta
    {
        public System.Int32 Id { get => (System.Int32)Values["Id"]; set => Values["Id"] = value; }
        public MsgTypes MsgType { get => (MsgTypes)Values["MsgType"]; set => Values["MsgType"] = value; }
        public System.String TeachingMsg { get => (System.String)Values["TeachingMsg"]; set => Values["TeachingMsg"] = value; }

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
              { "MsgType", typeof(MsgTypes) },
              { "TeachingMsg", typeof(System.String) }
        };
        public object GetModelEntity()
        {
            return new Models.LearnMsgs(
            msgType: MsgType,
            teachingMsg: TeachingMsg
        );
        }
        public void CopyUpdates(Models.LearnMsgs modelData)
        {
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

