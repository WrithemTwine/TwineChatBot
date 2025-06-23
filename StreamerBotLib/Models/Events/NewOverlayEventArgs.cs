using StreamerBotLib.Systems.Overlay.Models;

namespace StreamerBotLib.Models.Events
{
    /// <summary>
    /// EventArgs specifying the new Overlay Event to show
    /// </summary>
    public class NewOverlayEventArgs : EventArgs
    {
        /// <summary>
        /// Contains the information about the new Overlay Event to show.
        /// </summary>
        public OverlayActionType OverlayAction { get; set; }
    }
}
