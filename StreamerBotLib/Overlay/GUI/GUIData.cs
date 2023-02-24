using StreamerBotLib.Enums;
using StreamerBotLib.Overlay.Enums;
using StreamerBotLib.Overlay.Models;
using StreamerBotLib.Overlay.Server;

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace StreamerBotLib.Overlay.GUI
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

        public ObservableCollection<OverlayPage> OverlayLinks { get; private set; } = new();

        public List<OverlayStyle> OverlayEditStyles { get; private set; } = new();

        public GUIData()
        {
            OverlayStats = new()
            {
#if DEBUG
                new() { OverlayType = OverlayTypes.ChannelPoints.ToString(), OverlayCount = 5 },
                new() { OverlayCount=10, OverlayType=OverlayTypes.ChannelEvents.ToString() }
#endif
            };

#if DEBUG_
            OverlayEditStyles.Add(new OverlayStyle(OverlayTypes.None.ToString()));
#endif

            foreach (string T in System.Enum.GetNames(typeof(OverlayTypes)))
            {
                if (T != OverlayTypes.None.ToString())
                {
                    OverlayStat item = new() { OverlayCount = 0, OverlayType = T };
                    if (!OverlayStats.Contains(item))
                    {
                        OverlayStats.Add(item);
                    }
                }
            }
        }

        public void UpdateStat(string OverType)
        {
            UpdateStat(new OverlayStat() { OverlayType = OverType });
            //RefreshStats();
        }

        public void UpdateStat(OverlayStat overlayStatData)
        {
            if (OverlayStats.Contains(overlayStatData))
            {
                int idx = OverlayStats.IndexOf(overlayStatData);

                OverlayStats[idx].OverlayCount++;
            }
        }

        public void RefreshStats()
        {
            foreach (OverlayStat overlayStat in OverlayStats)
            {
                OnPropertyChanged(nameof(overlayStat));
            }
        }

        public void UpdateLinks()
        {
            OverlayLinks.Clear();

            foreach (OverlayPage O in PrefixGenerator.GetLinks())
            {
                OverlayLinks.Add(O);
            }
        }

        public void ClearEditPages()
        {
            OverlayEditStyles.Clear();
        }

        public void ClearLinks()
        {
            OverlayLinks.Clear();
        }

        public void AddEditPage(string overlayType)
        {
            OverlayEditStyles.Add(new OverlayStyle(overlayType));
            OnPropertyChanged(nameof(OverlayEditStyles));
        }

        public void AddEditPage(string[] overlayTypes)
        {
            foreach (string s in overlayTypes)
            {
                AddEditPage(s);
            }
        }

        public void AddEditPage(OverlayTickerItem overlayTickerItem)
        {
            OverlayEditStyles.Add(new OverlayStyle(overlayTickerItem));
            OnPropertyChanged(nameof(OverlayEditStyles));
        }

        public void AddEditPage(OverlayTickerItem[] overlayTickerItems)
        {
            foreach (OverlayTickerItem s in overlayTickerItems)
            {
                AddEditPage(s);
            }
        }

        internal void AddEditPage(TickerStyle tickerStyle)
        {
            OverlayEditStyles.Add(new OverlayStyle(tickerStyle));
            OnPropertyChanged(nameof(OverlayEditStyles));
        }

        internal void UpdateEditPage(TickerStyle tickerStyle)
        {
            OverlayEditStyles.Find((t) => t.OverlayType == tickerStyle.ToString()).OverlayStyleText = new OverlayStyle(tickerStyle, true).OverlayStyleText;
            OnPropertyChanged(nameof(OverlayEditStyles));
        }

        public void SaveEditPage()
        {
            foreach (OverlayStyle overlayEditStyle in OverlayEditStyles)
            {
                overlayEditStyle.SaveFile();
            }
        }

        private void OnPropertyChanged(string Propname)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(Propname));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
