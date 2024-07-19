using StreamerBotLib.Enums;
using StreamerBotLib.DataSQL.Models;
using StreamerBotLib.Interfaces;
using StreamerBotLib.Overlay.Enums;

namespace StreamerBotLib.DataSQL.TableMeta
{
    internal class BanRules : IDatabaseTableMeta
    {
        public System.Int32 Id => (System.Int32)Values["Id"];
        public StreamerBotLib.Enums.ViewerTypes ViewerTypes => (StreamerBotLib.Enums.ViewerTypes)Values["ViewerTypes"];
        public StreamerBotLib.Enums.MsgTypes MsgType => (StreamerBotLib.Enums.MsgTypes)Values["MsgType"];
        public StreamerBotLib.Enums.ModActions ModAction => (StreamerBotLib.Enums.ModActions)Values["ModAction"];
        public System.Int32 TimeoutSeconds => (System.Int32)Values["TimeoutSeconds"];

        public Dictionary<string, object> Values { get; }

        public string TableName { get; } = "BanRules";

        public BanRules(Models.BanRules tableData)
        {
            Values = new()
            {
                 { "Id", tableData.Id },
                 { "ViewerTypes", tableData.ViewerTypes },
                 { "MsgType", tableData.MsgType },
                 { "ModAction", tableData.ModAction },
                 { "TimeoutSeconds", tableData.TimeoutSeconds }
            };
        }
        public Dictionary<string, Type> Meta => new()
        {
              { "Id", typeof(System.Int32) },
              { "ViewerTypes", typeof(StreamerBotLib.Enums.ViewerTypes) },
              { "MsgType", typeof(StreamerBotLib.Enums.MsgTypes) },
              { "ModAction", typeof(StreamerBotLib.Enums.ModActions) },
              { "TimeoutSeconds", typeof(System.Int32) }
        };
        public object GetModelEntity()
        {
            return new Models.BanRules(
                                          Convert.ToInt32(Values["Id"]), 
                                          (StreamerBotLib.Enums.ViewerTypes)Values["ViewerTypes"], 
                                          (StreamerBotLib.Enums.MsgTypes)Values["MsgType"], 
                                          (StreamerBotLib.Enums.ModActions)Values["ModAction"], 
                                          Convert.ToInt32(Values["TimeoutSeconds"])
);
        }
        public void CopyUpdates(Models.BanRules modelData)
        {
          if (modelData.Id != Id)
            {
                modelData.Id = Id;
            }

          if (modelData.ViewerTypes != ViewerTypes)
            {
                modelData.ViewerTypes = ViewerTypes;
            }

          if (modelData.MsgType != MsgType)
            {
                modelData.MsgType = MsgType;
            }

          if (modelData.ModAction != ModAction)
            {
                modelData.ModAction = ModAction;
            }

          if (modelData.TimeoutSeconds != TimeoutSeconds)
            {
                modelData.TimeoutSeconds = TimeoutSeconds;
            }

        }
    }
}

