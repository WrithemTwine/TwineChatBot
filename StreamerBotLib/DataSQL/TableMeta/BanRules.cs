using StreamerBotLib.Models.Interfaces;

namespace StreamerBotLib.DataSQL.TableMeta
{
    internal class BanRules : IDatabaseTableMeta
    {
        public System.Int32 Id { get => Convert.ToInt32(Values["Id"]); set => Values["Id"] = value; }
        public StreamerBotLib.Models.Enums.ViewerTypes ViewerTypes { get => (StreamerBotLib.Models.Enums.ViewerTypes)Enum.Parse(typeof(StreamerBotLib.Models.Enums.ViewerTypes), Values["ViewerTypes"]?.ToString() ?? "0"); set => Values["ViewerTypes"] = value; }
        public StreamerBotLib.Models.Enums.MsgTypes MsgType { get => (StreamerBotLib.Models.Enums.MsgTypes)Enum.Parse(typeof(StreamerBotLib.Models.Enums.MsgTypes), Values["MsgType"]?.ToString() ?? "0"); set => Values["MsgType"] = value; }
        public StreamerBotLib.Models.Enums.ModActions ModAction { get => (StreamerBotLib.Models.Enums.ModActions)Enum.Parse(typeof(StreamerBotLib.Models.Enums.ModActions), Values["ModAction"]?.ToString() ?? "0"); set => Values["ModAction"] = value; }
        public System.Int32 TimeoutSeconds { get => Convert.ToInt32(Values["TimeoutSeconds"]); set => Values["TimeoutSeconds"] = value; }

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
              { "ViewerTypes", typeof(StreamerBotLib.Models.Enums.ViewerTypes) },
              { "MsgType", typeof(StreamerBotLib.Models.Enums.MsgTypes) },
              { "ModAction", typeof(StreamerBotLib.Models.Enums.ModActions) },
              { "TimeoutSeconds", typeof(System.Int32) }
        };

        public object GetModelEntity()
        {
            return new Models.BanRules(
            viewerTypes: ViewerTypes,
            msgType: MsgType,
            modAction: ModAction,
            timeoutSeconds: TimeoutSeconds
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