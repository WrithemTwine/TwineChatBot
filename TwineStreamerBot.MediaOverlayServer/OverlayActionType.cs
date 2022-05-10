using System;

namespace TwineStreamerBot.MediaOverlayServer
{
    public enum OverlayTypes { ChannelPoints, ChannelEvents, Commands }

    /// <summary>
    /// Defines message payload to either load data or specify active data via the named pipe from the main bot process.
    /// </summary>
    [Serializable]
    public class OverlayActionType : EventArgs
    {
        /// <summary>
        /// Identify the category for the overlay type
        /// </summary>
        public OverlayTypes OverlayType { get; set; }

        /// <summary>
        /// The value of the item to trigger, such as a Channel Point Redemption
        /// </summary>
        public string ActionValue { get; set; } = string.Empty;

        /// <summary>
        /// Specifies if this is part of loading data into the server. 
        /// Specific info would require connecting to sources
        /// </summary>
        public bool DataLoad { get; set; } = false;

        public int HashCode { get; set; }

        private int GetHashcode()
        {
            return string.GetHashCode(OverlayType.ToString() + ActionValue + DataLoad.ToString());
        }
    }
}
