using StreamerBotLib.Enums;
using StreamerBotLib.Models;
using StreamerBotLib.Overlay.Communication;
using StreamerBotLib.Overlay.Interfaces;
using StreamerBotLib.Overlay.Models;
using StreamerBotLib.Static;

using System;
using System.Collections.Generic;
using System.Linq;

namespace StreamerBotLib.Overlay.Control
{
    internal class TickerFormatter
    {
        public static readonly List<SelectedTickerItem> selectedTickerItems = new();
        private readonly List<TickerItem> overlayTickerItemsData;

        public TickerFormatter()
        {
            overlayTickerItemsData = new();
        }

        /// <summary>
        /// Sets the user selected Ticker Items.
        /// </summary>
        /// <param name="tickerItems">Contains only the selected Ticker Item.</param>
        public void SetTickersSelected(IEnumerable<SelectedTickerItem> tickerItems)
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

        public IEnumerable<IOverlayPageReadOnly> GetTickerPages(IEnumerable<OverlayStyle> overlayStyles)
        {
            List<IOverlayPageReadOnly> pages = new();
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
