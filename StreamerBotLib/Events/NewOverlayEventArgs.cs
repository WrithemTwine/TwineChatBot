using StreamerBotLib.Overlay.Models;

namespace StreamerBotLib.Events
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
