using StreamerBotLib.Enums;
using StreamerBotLib.DataSQL.Models;
using StreamerBotLib.Interfaces;
using StreamerBotLib.Overlay.Enums;

namespace StreamerBotLib.DataSQL.TableMeta
{
    internal class ModeratorApprove : IDatabaseTableMeta
    {
        public System.Boolean IsEnabled => (System.Boolean)Values["IsEnabled"];
        public StreamerBotLib.Enums.ModActionType ModActionType => (StreamerBotLib.Enums.ModActionType)Values["ModActionType"];
        public System.String ModActionName => (System.String)Values["ModActionName"];
        public StreamerBotLib.Enums.ModPerformType ModPerformType => (StreamerBotLib.Enums.ModPerformType)Values["ModPerformType"];
        public System.String ModPerformAction => (System.String)Values["ModPerformAction"];

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
                                          (System.Boolean)Values["IsEnabled"], 
                                          (StreamerBotLib.Enums.ModActionType)Values["ModActionType"], 
                                          (System.String)Values["ModActionName"], 
                                          (StreamerBotLib.Enums.ModPerformType)Values["ModPerformType"], 
                                          (System.String)Values["ModPerformAction"]
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

