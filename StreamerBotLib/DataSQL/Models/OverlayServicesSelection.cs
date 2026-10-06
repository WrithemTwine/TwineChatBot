using Microsoft.EntityFrameworkCore;

using StreamerBotLib.Models.Overlay;
using StreamerBotLib.Systems.Overlay.Enums;

using System.Diagnostics;

namespace StreamerBotLib.DataSQL.Models
{
    [PrimaryKey(nameof(OverlayType), nameof(OverlayAction))]
    [Index(nameof(OverlayType), nameof(OverlayAction))]
    [DebuggerDisplay("OverlayType={OverlayType}, OverlayAction={OverlayAction}, SelectionType={SelectionType}")]
    public class OverlayServicesSelection(
        OverlayTypes overlayType = OverlayTypes.None,
        string overlayAction = "",
        OverlayMultiSelectionTypes selectionType = OverlayMultiSelectionTypes.EliminationSelection
        ) : EntityBase
    {
        public OverlayTypes OverlayType { get; set; } = overlayType;
        public string OverlayAction { get; set; } = overlayAction;

        public OverlayMultiSelectionTypes SelectionType { get; set; } = selectionType;
    }
}
