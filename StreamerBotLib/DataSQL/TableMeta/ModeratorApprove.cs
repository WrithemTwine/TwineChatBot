using StreamerBotLib.Enums;
using StreamerBotLib.DataSQL.Models;
using StreamerBotLib.Interfaces;
using StreamerBotLib.Overlay.Enums;

namespace StreamerBotLib.DataSQL.TableMeta
{
    internal class ModeratorApprove : IDatabaseTableMeta
    {
        public System.Boolean IsEnabled { get => (System.Boolean)Values["IsEnabled"]; set => Values["IsEnabled"] = value; }
        public StreamerBotLib.Enums.ModActionType ModActionType { get => (StreamerBotLib.Enums.ModActionType)Values["ModActionType"]; set => Values["ModActionType"] = value; }
        public System.String ModActionName { get => (System.String)Values["ModActionName"]; set => Values["ModActionName"] = value; }
        public StreamerBotLib.Enums.ModPerformType ModPerformType { get => (StreamerBotLib.Enums.ModPerformType)Values["ModPerformType"]; set => Values["ModPerformType"] = value; }
        public System.String ModPerformAction { get => (System.String)Values["ModPerformAction"]; set => Values["ModPerformAction"] = value; }

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
              { "ModActionType", typeof(StreamerBotLib.Enums.ModActionType) },
              { "ModActionName", typeof(System.String) },
              { "ModPerformType", typeof(StreamerBotLib.Enums.ModPerformType) },
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

