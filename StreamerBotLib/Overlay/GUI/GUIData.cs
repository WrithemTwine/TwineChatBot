using StreamerBotLib.Overlay.Enums;
using StreamerBotLib.Overlay.Models;
using StreamerBotLib.Overlay.Server;
using StreamerBotLib.Static;

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

        /// <summary>
        /// Collection of the links implemented in the http server
        /// </summary>
        public ObservableCollection<OverlayPage> OverlayLinks { get; private set; } = new();

        /// <summary>
        /// The collection of styles presented to the user in the GUI, saved to specific files, and 
        /// used to serve the Content pages with these styles.
        /// </summary>
        public List<OverlayStyle> OverlayEditStyles { get; private set; } = new();

        /// <summary>
        /// Initialize, and add debug data to the stats for testing.
        /// </summary>
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
                    OverlayStats.UniqueAdd(item);
                }
            }
        }

        /// <summary>
        /// Update a stat based on the string name.
        /// </summary>
        /// <param name="OverlayType">Name of the stat</param>
        public void UpdateStat(string OverlayType)
        {
            UpdateStat(new OverlayStat() { OverlayType = OverlayType });
            //RefreshStats();
        }

        /// <summary>
        /// Increment a stat per the provided OverlayType
        /// </summary>
        /// <param name="overlayStatData">Contains the OverlayType name of the stat to increment.</param>
        public void UpdateStat(OverlayStat overlayStatData)
        {
            if (OverlayStats.Contains(overlayStatData))
            {
                int idx = OverlayStats.IndexOf(overlayStatData);

                OverlayStats[idx].OverlayCount++;
            }
        }

        /// <summary>
        /// Cycle refresh all of the stats.
        /// </summary>
        public void RefreshStats()
        {
            foreach (OverlayStat overlayStat in OverlayStats)
            {
                OnPropertyChanged(nameof(overlayStat));
            }
        }

        /// <summary>
        /// Refreshes the links shown to the user for accessing html Content.
        /// </summary>
        public void UpdateLinks()
        {
            OverlayLinks.Clear();

            foreach (OverlayPage O in PrefixGenerator.GetLinks())
            {
                OverlayLinks.Add(O);
            }
        }

        /// <summary>
        /// Clears all of the saved styles.
        /// </summary>
        public void ClearEditPages()
        {
            OverlayEditStyles.Clear();
        }

        /// <summary>
        /// Clears all of the saved links.
        /// </summary>
        public void ClearLinks()
        {
            OverlayLinks.Clear();
        }

        /// <summary>
        /// Add a new action style page based on the provided name.
        /// </summary>
        /// <param name="overlayType">The type of the overlay to define its style.</param>
        public void AddEditPage(string overlayType)
        {
            OverlayEditStyles.Add(new OverlayStyle(overlayType));
            OnPropertyChanged(nameof(OverlayEditStyles));
        }

        /// <summary>
        /// Adds a collection of action style pages based on the provided names.
        /// <br />
        /// See also <seealso cref="AddEditPage(string)"/>
        /// </summary>
        /// <param name="overlayTypes">A collection of the overlay type names.</param>
        public void AddEditPage(string[] overlayTypes)
        {
            foreach (string s in overlayTypes)
            {
                AddEditPage(s);
            }
        }

        /// <summary>
        /// Adds a new ticker style page based on the provided ticker type.
        /// </summary>
        /// <param name="overlayTickerItem">The type of ticker item to add to the style lists.</param>
        public void AddEditPage(OverlayTickerItem overlayTickerItem)
        {
            OverlayEditStyles.Add(new OverlayStyle(overlayTickerItem));
            OnPropertyChanged(nameof(OverlayEditStyles));
        }

        /// <summary>
        /// Adds a collection of ticker style pages.
        /// </summary>
        /// <param name="overlayTickerItems">A collection of ticker tyle names to add to the list.</param>
        public void AddEditPage(OverlayTickerItem[] overlayTickerItems)
        {
            foreach (OverlayTickerItem s in overlayTickerItems)
            {
                AddEditPage(s);
            }
        }

        /// <summary>
        /// Adds a ticker style, distinguished by presentation method - static, rotating, or scrolling.
        /// </summary>
        /// <param name="tickerStyle">The ticker style to add.</param>
        internal void AddEditPage(TickerStyle tickerStyle)
        {
            OverlayEditStyles.Add(new OverlayStyle(tickerStyle));
            OnPropertyChanged(nameof(OverlayEditStyles));
        }

        /// <summary>
        /// Update an existing ticker presentation style, when the user updates duration parameters (seconds for whole animation).
        /// </summary>
        /// <param name="tickerStyle">The ticker presentation style to update.</param>
        internal void UpdateEditPage(TickerStyle tickerStyle)
        {
            // find existing ticker style, rebuild the style data, and update the text for the new ticker style duration parameter.
            OverlayEditStyles.Find((t) => t.OverlayType == tickerStyle.ToString()).OverlayStyleText = new OverlayStyle(tickerStyle, true).OverlayStyleText;
            OnPropertyChanged(nameof(OverlayEditStyles));
        }

        /// <summary>
        /// Save each style file.
        /// </summary>
        public void SaveEditPage()
        {
            foreach (OverlayStyle overlayEditStyle in OverlayEditStyles)
            {
                overlayEditStyle.SaveFile();
            }
        }

        /// <summary>
        /// Implement INotifyPropertyChanged interface
        /// </summary>
        /// <param name="Propname">Name of the changed property</param>
        private void OnPropertyChanged(string Propname)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(Propname));
        }

        /// <summary>
        /// Event for the changing property
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
