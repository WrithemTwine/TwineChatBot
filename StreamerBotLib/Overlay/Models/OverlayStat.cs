using System;
using System.ComponentModel;

namespace StreamerBotLib.Overlay.Models
{
    /// <summary>
    /// Manages an Overlay statistic to show how much the server performed the Overlay action.
    /// </summary>
    public class OverlayStat : IEquatable<OverlayStat>, INotifyPropertyChanged
    {
        private string _OverlayType = "";
        private int overlayCount = 0;

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
        public int OverlayCount
        {
            get => overlayCount;
            set
            {
                overlayCount = value;
                OnPropertyChanged(nameof(OverlayCount));
            }
        }
        public bool Equals(OverlayStat other)
        {
            return OverlayType == other?.OverlayType;
        }

        private void OnPropertyChanged(string Propname)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(Propname));
        }

        public event PropertyChangedEventHandler PropertyChanged;

    }
}
