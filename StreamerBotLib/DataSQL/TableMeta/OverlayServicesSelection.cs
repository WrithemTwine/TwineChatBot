using StreamerBotLib.Models.Interfaces;

namespace StreamerBotLib.DataSQL.TableMeta
{
    internal class OverlayServicesSelection : IDatabaseTableMeta
    {
        public StreamerBotLib.Systems.Overlay.Enums.OverlayTypes OverlayType { get => (StreamerBotLib.Systems.Overlay.Enums.OverlayTypes)Enum.Parse(typeof(StreamerBotLib.Systems.Overlay.Enums.OverlayTypes), Values["OverlayType"]?.ToString() ?? "0"); set => Values["OverlayType"] = value; }
        public System.String OverlayAction { get => (string)Values["OverlayAction"]; set => Values["OverlayAction"] = value; }
        public StreamerBotLib.Models.Overlay.OverlayMultiSelectionTypes SelectionType { get => (StreamerBotLib.Models.Overlay.OverlayMultiSelectionTypes)Enum.Parse(typeof(StreamerBotLib.Models.Overlay.OverlayMultiSelectionTypes), Values["SelectionType"]?.ToString() ?? "0"); set => Values["SelectionType"] = value; }

        public Dictionary<string, object> Values { get; }

        public string TableName { get; } = "OverlayServicesSelection";

        public OverlayServicesSelection(Models.OverlayServicesSelection tableData)
        {
            Values = new()
            {
                 { "OverlayType", tableData.OverlayType },
                 { "OverlayAction", tableData.OverlayAction },
                 { "SelectionType", tableData.SelectionType }
            };
        }

        public Dictionary<string, Type> Meta => new()
        {
              { "OverlayType", typeof(StreamerBotLib.Systems.Overlay.Enums.OverlayTypes) },
              { "OverlayAction", typeof(System.String) },
              { "SelectionType", typeof(StreamerBotLib.Models.Overlay.OverlayMultiSelectionTypes) }
        };

        public object GetModelEntity()
        {
            return new Models.OverlayServicesSelection(
            overlayType: OverlayType,
            overlayAction: OverlayAction,
            selectionType: SelectionType
            );
        }

        public void CopyUpdates(Models.OverlayServicesSelection modelData)
        {
            if (modelData.OverlayType != OverlayType)
            {
                modelData.OverlayType = OverlayType;
            }
            if (modelData.OverlayAction != OverlayAction)
            {
                modelData.OverlayAction = OverlayAction;
            }
            if (modelData.SelectionType != SelectionType)
            {
                modelData.SelectionType = SelectionType;
            }
        }
    }
}