using StreamerBotLib.Models.Interfaces;

namespace StreamerBotLib.DataSQL.TableMeta
{
    internal class LearnMsgs : IDatabaseTableMeta
    {
        public System.Int32 Id { get => Convert.ToInt32(Values["Id"]); set => Values["Id"] = value; }
        public StreamerBotLib.Models.Enums.MsgTypes MsgType { get => (StreamerBotLib.Models.Enums.MsgTypes)Enum.Parse(typeof(StreamerBotLib.Models.Enums.MsgTypes), Values["MsgType"]?.ToString() ?? "0"); set => Values["MsgType"] = value; }
        public System.String TeachingMsg { get => (string)Values["TeachingMsg"]; set => Values["TeachingMsg"] = value; }

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
              { "MsgType", typeof(StreamerBotLib.Models.Enums.MsgTypes) },
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