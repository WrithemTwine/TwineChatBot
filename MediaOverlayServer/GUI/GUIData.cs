using MediaOverlayServer.Enums;
using MediaOverlayServer.Models;
using MediaOverlayServer.Server;

using System.Collections.Generic;
using System.ComponentModel;

namespace MediaOverlayServer.GUI
{
    /// <summary>
    /// Class to report any back end to GUI data.
    /// </summary>
    public class GUIData : INotifyPropertyChanged
    {
        /// <summary>
        /// Holds the Status Bar statistics per Overlay type how many times the server performed an action.
        /// </summary>
        public List<OverlayStat> OverlayStats { get; private set; }

        public static List<OverlayPage> OverlayLinks => PrefixGenerator.GetLinks();

        public List<OverlayStyle> OverlayEditStyles { get; private set; } = new();

        public GUIData()
        {
            OverlayStats = new List<OverlayStat>() 
            {
#if DEBUG_
                new() { OverlayType = OverlayTypes.ChannelPoints.ToString(), OverlayCount = 5 }, 
                new() { OverlayCount=10, OverlayType=OverlayTypes.ChannelEvents.ToString() }
#endif
            };

#if DEBUG_
            OverlayEditStyles.Add(new OverlayStyle(OverlayTypes.None.ToString()));
#endif

            foreach(string T in System.Enum.GetNames(typeof(OverlayTypes)))
            {
                if (T != OverlayTypes.None.ToString()) {
                    OverlayStat item = new() { OverlayCount = 0, OverlayType = T };
                    if (!OverlayStats.Contains(item))
                    {
                        OverlayStats.Add(item);
                    } 
                }
            }
            OnPropertyChanged(nameof(OverlayStats));
        }

        public void UpdateStat(string OverType, int newCount)
        {
            UpdateStat(new() { OverlayCount = newCount, OverlayType = OverType });
        }

        public void UpdateStat(OverlayStat overlayStatData)
        {
            if (OverlayStats.Contains(overlayStatData))
            {
                OverlayStats[OverlayStats.IndexOf(overlayStatData)].OverlayCount = overlayStatData.OverlayCount;
                OnPropertyChanged(nameof(OverlayStats));
            }
        }

        public void UpdateLinks()
        {
            OnPropertyChanged(nameof(OverlayLinks));
        }

        public void ClearEditPages()
        {
            OverlayEditStyles.Clear();
        }

        public void AddEditPage(string overlayType)
        {
            OverlayEditStyles.Add(new OverlayStyle(overlayType));
            OnPropertyChanged(nameof(OverlayEditStyles));
        }

        public void AddEditPage(string[] overlayTypes)
        {
            OverlayEditStyles.AddRange(new List<string>(overlayTypes).ConvertAll((t) => new OverlayStyle(t)));

        }

        public void SaveEditPage()
        {
            foreach(OverlayStyle overlayEditStyle in OverlayEditStyles)
            {
                overlayEditStyle.SaveFile();
            }
        }

        private void OnPropertyChanged(string Propname)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(Propname));
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
