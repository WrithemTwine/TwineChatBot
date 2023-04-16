

using StreamerBotLib.Enums;
using StreamerBotLib.Static;

using System;
using System.Collections.Generic;
using System.Linq;

namespace StreamerBotLib.Overlay.Models
{
    public class SelectedTickerItem
    {
        public bool IsSelected { get; set; } = false;
        public string OverlayTickerItem { get; set; }
    }

    public class SelectedTickerItems
    {
        public List<SelectedTickerItem> TickerItems { get; private set; }

        public SelectedTickerItems()
        {
            TickerItems = new(from string ticker in Enum.GetNames(typeof(OverlayTickerItem))
                                 select new SelectedTickerItem() { 
                                     IsSelected =  new List<string>(OptionFlags.MediaOverlayTickerSelected).Contains(ticker), 
                                     OverlayTickerItem = ticker });
        }

        public void SaveSelections()
        {
            OptionFlags.MediaOverlayTickerSelected =
             (from SelectedTickerItem ticker in TickerItems
             where ticker.IsSelected
             select ticker.OverlayTickerItem).ToArray();
            
        }

        public IEnumerable<SelectedTickerItem> GetSelectedTickers()
        {
            return (from ticker in TickerItems
                    where ticker.IsSelected
                    select ticker);
        }
    }
}
