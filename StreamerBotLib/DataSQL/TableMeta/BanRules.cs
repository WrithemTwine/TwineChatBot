using StreamerBotLib.Models.Enums;
using StreamerBotLib.Models.Interfaces;

namespace StreamerBotLib.DataSQL.TableMeta
{
    internal class BanRules : IDatabaseTableMeta
    {
        public System.Int32 Id { get => (System.Int32)Values["Id"]; set => Values["Id"] = value; }
        public ViewerTypes ViewerTypes { get => (ViewerTypes)Values["ViewerTypes"]; set => Values["ViewerTypes"] = value; }
        public MsgTypes MsgType { get => (MsgTypes)Values["MsgType"]; set => Values["MsgType"] = value; }
        public ModActions ModAction { get => (ModActions)Values["ModAction"]; set => Values["ModAction"] = value; }
        public System.Int32 TimeoutSeconds { get => (System.Int32)Values["TimeoutSeconds"]; set => Values["TimeoutSeconds"] = value; }

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
              { "ViewerTypes", typeof(ViewerTypes) },
              { "MsgType", typeof(MsgTypes) },
              { "ModAction", typeof(ModActions) },
              { "TimeoutSeconds", typeof(System.Int32) }
        };
        public object GetModelEntity()
        {
            return new Models.BanRules(
            viewerTypes: ViewerTypes,
            msgType: MsgType,
            modAction: ModAction,
            timeoutSeconds: Convert.ToInt32(TimeoutSeconds)
        );
        }
        public void CopyUpdates(Models.BanRules modelData)
        {
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

