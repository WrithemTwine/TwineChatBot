using StreamerBotLib.Models.Interfaces;

namespace StreamerBotLib.DataSQL.TableMeta
{
    internal class ModeratorApprove : IDatabaseTableMeta
    {
        public System.Boolean IsEnabled { get => Convert.ToBoolean(Values["IsEnabled"]); set => Values["IsEnabled"] = value; }
        public StreamerBotLib.Models.Enums.ModActionType ModActionType { get => (StreamerBotLib.Models.Enums.ModActionType)Enum.Parse(typeof(StreamerBotLib.Models.Enums.ModActionType), Values["ModActionType"]?.ToString() ?? "0"); set => Values["ModActionType"] = value; }
        public System.String ModActionName { get => (string)Values["ModActionName"]; set => Values["ModActionName"] = value; }
        public StreamerBotLib.Models.Enums.ModPerformType ModPerformType { get => (StreamerBotLib.Models.Enums.ModPerformType)Enum.Parse(typeof(StreamerBotLib.Models.Enums.ModPerformType), Values["ModPerformType"]?.ToString() ?? "0"); set => Values["ModPerformType"] = value; }
        public System.String ModPerformAction { get => (string)Values["ModPerformAction"]; set => Values["ModPerformAction"] = value; }

        public Dictionary<string, object> Values { get; }

        public string TableName { get; } = "ModeratorApprove";

        public ModeratorApprove(Models.ModeratorApprove tableData)
        {
            Values = new()
            {
                 { "IsEnabled", tableData.IsEnabled },
                 { "ModActionType", tableData.ModActionType },
                 { "ModActionName", tableData.ModActionName },
                 { "ModPerformType", tableData.ModPerformType },
                 { "ModPerformAction", tableData.ModPerformAction }
            };
        }

        public Dictionary<string, Type> Meta => new()
        {
              { "IsEnabled", typeof(System.Boolean) },
              { "ModActionType", typeof(StreamerBotLib.Models.Enums.ModActionType) },
              { "ModActionName", typeof(System.String) },
              { "ModPerformType", typeof(StreamerBotLib.Models.Enums.ModPerformType) },
              { "ModPerformAction", typeof(System.String) }
        };

        public object GetModelEntity()
        {
            return new Models.ModeratorApprove(
            isEnabled: IsEnabled,
            modActionType: ModActionType,
            modActionName: ModActionName,
            modPerformType: ModPerformType,
            modPerformAction: ModPerformAction
            );
        }

        public void CopyUpdates(Models.ModeratorApprove modelData)
        {
            if (modelData.IsEnabled != IsEnabled)
            {
                modelData.IsEnabled = IsEnabled;
            }
            if (modelData.ModActionType != ModActionType)
            {
                modelData.ModActionType = ModActionType;
            }
            if (modelData.ModActionName != ModActionName)
            {
                modelData.ModActionName = ModActionName;
            }
            if (modelData.ModPerformType != ModPerformType)
            {
                modelData.ModPerformType = ModPerformType;
            }
            if (modelData.ModPerformAction != ModPerformAction)
            {
                modelData.ModPerformAction = ModPerformAction;
            }
        }
    }
}