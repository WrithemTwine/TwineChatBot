using StreamerBotLib.Models;
using StreamerBotLib.Overlay.Communication;
using StreamerBotLib.Overlay.Enums;
using StreamerBotLib.Overlay.Interfaces;
using StreamerBotLib.Overlay.Models;
using StreamerBotLib.Static;

namespace StreamerBotLib.Overlay.Control
{
    /// <summary>
    /// Class to manage which Ticker Items the user wants to include in overlay
    /// display, and the data from primary bot to maintain in ticker activity. Responds to 
    /// changes in the ticker style and rebuild current ticker.
    /// </summary>
    internal class TickerFormatter
    {
        /// <summary>
        /// the current selected items the user wants to view for the ticker
        /// </summary>
        public static readonly List<SelectedTickerItem> selectedTickerItems = [];
        private readonly List<TickerItem> overlayTickerItemsData;

        public TickerFormatter()
        {
            overlayTickerItemsData = [];
        }

        /// <summary>
        /// Sets the user selected Ticker Items.
        /// </summary>
        /// <param name="tickerItems">Contains only the selected Ticker Item.</param>
        public static void SetTickersSelected(IEnumerable<SelectedTickerItem> tickerItems)
        {
            selectedTickerItems.Clear();
            selectedTickerItems.AddRange(tickerItems);
        }

        /// <summary>
        /// Set the Ticker Items data used to generate each ticker item.
        /// </summary>
        /// <param name="tickerItems">IEnumberable collection of the ticker data.</param>
        public void SetTickerData(IEnumerable<TickerItem> tickerItems)
        {
            overlayTickerItemsData.Clear();
            overlayTickerItemsData.AddRange(tickerItems);
        }

        /// <summary>
        /// Builds the ticker pages to show based on user selections and the provided user configured
        /// styles.
        /// </summary>
        /// <param name="overlayStyles">The current ticker styles based on the user's option selection and
        /// adjusted styles.</param>
        /// <returns></returns>
        public IEnumerable<IOverlayPageReadOnly> GetTickerPages(IEnumerable<OverlayStyle> overlayStyles)
        {
            List<IOverlayPageReadOnly> pages = [];
            if (OptionFlags.MediaOverlayTickerSingle)
            {
                // convert a data collection in a collection of pages
                foreach (TickerItem ticker in overlayTickerItemsData)
                {
                    pages.Add(ProcessHyperText.ProcessTicker(ticker, overlayStyles));
                }
            }
            else
            {
                // fill up ticker data with blanks if not available

                // convert a data collection into a single page
                pages.Add(ProcessHyperText.ProcessTicker(new List<TickerItem>(
                    from SelectedTickerItem S in selectedTickerItems
                    let newitem = new TickerItem()
                    {
                        OverlayTickerItem = (OverlayTickerItem)Enum.Parse(typeof(OverlayTickerItem), S.OverlayTickerItem),
                        UserName = overlayTickerItemsData.Find(
                            (f) => f.OverlayTickerItem == (OverlayTickerItem)Enum.Parse(typeof(OverlayTickerItem), S.OverlayTickerItem)
                            )?.UserName
                        ?? "-"
                    }
                    select newitem
                    ), overlayStyles));
            }

            return pages;
        }
    }
}
