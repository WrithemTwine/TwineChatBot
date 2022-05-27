using MediaOverlayServer.Enums;

using StreamerBotLib.Events;
using StreamerBotLib.Static;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreamerBotLib.Systems
{
    internal class OverlaySystem : SystemsBase
    {
        /// <summary>
        /// A stream action caused an Overlay Event to occur, and should be displayed via the Media Overlay Server.
        /// </summary>
        public event EventHandler<NewOverlayEventArgs> NewOverlayEvent;

        /// <summary>
        /// List of Table/Column pairs for building the overlay action selections
        /// </summary>
        private Dictionary<string, string> OverlayActionColumnPairs = new()
        {
            {"Commands", "CmdName"},
            {"ChannelEvents", "Name" }
        };

        private List<string> ChannelPointRewards = new();

        public void SetChannelRewardList(List<string> RewardList)
        {
            ChannelPointRewards.UniqueAddRange(RewardList);
        }

        public Dictionary<string, List<string>> GetOverlayActions()
        {
            Dictionary<string, List<string>> OverlayActionPairs = new();

            foreach (string O in OverlayActionColumnPairs.Keys)
            {
                OverlayActionPairs.Add(O, DataManage.GetRowsDataColumn(O, OverlayActionColumnPairs[O]).ConvertAll((i) => i.ToString()));
            }

            // if there are no channel point rewards, the streamers credentials may need to be loaded or there aren't any channel points
            OverlayActionPairs.Add(OverlayTypes.ChannelPoints.ToString(), ChannelPointRewards.Count > 0 ? ChannelPointRewards : new() { "None or Not Loaded!" });
            OverlayActionPairs.Add(OverlayTypes.Giveaway.ToString(), new() { OverlayTypes.Giveaway.ToString() });
            OverlayActionPairs.Add(OverlayTypes.Clip.ToString(), new() { OverlayTypes.Clip.ToString() });

            return OverlayActionPairs;
        }

        public void CheckForOverlayEvent(OverlayTypes overlayType, string Action, string UserName = "", string UserMsg="", string ProvidedURL = "")
        {

        }

    }
}
