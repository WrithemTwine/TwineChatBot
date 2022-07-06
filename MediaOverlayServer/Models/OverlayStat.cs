using MediaOverlayServer.Enums;

using System;

namespace MediaOverlayServer.Models
{
    /// <summary>
    /// Manages an Overlay statistic to show how much the server performed the Overlay action.
    /// </summary>
    public class OverlayStat : IEquatable<OverlayStat>
    {
        private string _OverlayType = "";

        /// <summary>
        /// The type of the Overlay for the stat.
        /// </summary>
        public string OverlayType 
        {
            get { return _OverlayType + " : "; }
            set { _OverlayType = value; }        
        }

        /// <summary>
        /// The number of actions performed for this Overlay.
        /// </summary>
        public int OverlayCount { get; set; } = 0;

        public bool Equals(OverlayStat? other)
        {
            return OverlayType == other?.OverlayType;
        }
    }
}
