using StreamerBotLib.Enums;
using StreamerBotLib.Events;
using StreamerBotLib.Models;
using StreamerBotLib.Overlay.Enums;
using StreamerBotLib.Overlay.Models;
using StreamerBotLib.Static;

using System.Reflection;

/*
 * For clips to appear in any overlay action, Twitch requires for their embed player a domain name and the domain must utilize SSL. 
 * https://dev.twitch.tv/docs/embed
 * 
 * For this reason, currently disabling the Clips features - either showing the clip someone made of the current channel and shoutout random clip for a user.
 * Only hid the features by comment as they might be used in the future.
*/

namespace StreamerBotLib.Systems
{
    internal partial class ActionSystem
    {
        /// <summary>
        /// A stream action caused an Overlay Event to occur, and should be displayed via the Media Overlay Server.
        /// </summary>
        public event EventHandler<NewOverlayEventArgs> NewOverlayEvent;
        /// <summary>
        /// Event to get the channel clips for a specific user name - regarding the overlay can include one of the user's stream clips
        /// </summary>
        public event EventHandler<GetChannelClipsEventArgs> GetChannelClipsEvent;
        /// <summary>
        /// Event when a new ticker item occurs.
        /// </summary>
        public static event EventHandler<UpdatedTickerItemsEventArgs> UpdatedTickerItems;

        /// <summary>
        /// List of Table/Column pairs for building the overlay action selections
        /// </summary>
        private readonly Dictionary<string, string> OverlayActionColumnPairs = new()
        {
            {"Commands", "CmdName"},
            {"ChannelEvents", "Name" }
        };
        private readonly List<string> ChannelPointRewards = [];

        /// <summary>
        /// Setup the channel points reward list, update the new information.
        /// </summary>
        /// <param name="RewardList">The list of rewards. Internal list updates for any new data.</param>
        public void SetChannelRewardList(List<string> RewardList)
        {
            ChannelPointRewards.UniqueAddRange(RewardList);
        }

        public Dictionary<string, List<string>> GetOverlayActions()
        {
            Dictionary<string, List<string>> OverlayActionPairs = [];

            lock (GUI.GUIDataManagerLock.Lock)
            {
                foreach (string O in OverlayActionColumnPairs.Keys)
                {
                    OverlayActionPairs.Add(O, DataManage.GetRowsDataColumn(O, OverlayActionColumnPairs[O]).ConvertAll((i) => i.ToString()));
                }
            }

            // if there are no channel point rewards, the streamers credentials may need to be loaded or there aren't any channel points
            OverlayActionPairs.Add(OverlayTypes.ChannelPoints.ToString(), ChannelPointRewards.Count > 0 ? ChannelPointRewards : ["None or Not Loaded!"]);
            OverlayActionPairs.Add(OverlayTypes.Giveaway.ToString(), [OverlayTypes.Giveaway.ToString()]);
            //OverlayActionPairs.Add(OverlayTypes.Clip.ToString(), new() { OverlayTypes.Clip.ToString() });

            foreach (string K in OverlayActionPairs.Keys)
            {
                OverlayActionPairs[K].Sort();
            }

            return OverlayActionPairs;
        }

        private static void CheckURL(string ProvidedURL, float UrlDuration, ref OverlayActionType data)
        {
            if (ProvidedURL != null && UrlDuration != 0)
            {
                data.MediaFile = ProvidedURL;
                data.Duration = Math.Min((int)Math.Ceiling(UrlDuration), data.Duration);

                LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.OverlayBot, $"The provided URL {ProvidedURL} and Duration {data.Duration} check out, and are added to data.");

            }
        }

        private void OnNewOverlayEvent(NewOverlayEventArgs e)
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.OverlayBot, $"Building Overlay Event with action data, {e.OverlayAction.OverlayType} and {e.OverlayAction.ActionValue}.");

            NewOverlayEvent?.Invoke(this, e);
        }

        public void CheckForOverlayEvent(OverlayTypes overlayType, string Action, string UserName = null, string UserMsg = null, string ProvidedURL = null, float UrlDuration = 0)
        {
            List<OverlayActionType> overlayActionTypes = DataManage.GetOverlayActions(overlayType.ToString(), Action, UserName);
            OverlayActionType FoundAction = null;

            if (UserName != null && overlayActionTypes.Count > 0)
            {
                FoundAction = overlayActionTypes.Find(x => x.UserName == UserName) ?? overlayActionTypes.Find(x => (x.OverlayType == overlayType) && (x.ActionValue == Action));

                LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.OverlayBot, $"Determined {FoundAction?.OverlayType} {FoundAction?.ActionValue} as the matching Overlay action.");
            }

            void CheckDiffMsg(ref OverlayActionType data)
            {
                if (data.UseChatMsg)
                {
                    data.Message = UserMsg;
                }
            }

            if (FoundAction != null)
            {
                LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.OverlayBot, $"OverlaySystem - found an action, {FoundAction.ActionValue} {FoundAction.OverlayType}, building a response alert.");

                CheckDiffMsg(ref FoundAction);
                if (overlayType == OverlayTypes.Commands && Action == DefaultCommand.so.ToString())
                {
                    if (OptionFlags.MediaOverlayShoutoutClips)
                    {
                        LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.OverlayBot, $"OverlaySystem - action, {FoundAction.ActionValue} {FoundAction.OverlayType}, is a shoutout.");

                        ThreadManager.CreateThreadStart(() =>
                        {
                            ShoutOutOverlayAction UserShout = new(FoundAction, OnNewOverlayEvent);
                            OnGetChannelClipsEvent(new() { ChannelName = UserName, CallBackResult = UserShout.FoundChannelClips });

                            while (!UserShout.Finish) // keep thread open until Clips bot gives a response
                            {
                                Thread.Sleep(1000);
                            }
                        });
                    }
                    else
                    {
                        OnNewOverlayEvent(new() { OverlayAction = FoundAction });
                    }
                }
                else
                {
                    CheckURL(ProvidedURL, UrlDuration, ref FoundAction);
                    OnNewOverlayEvent(new() { OverlayAction = FoundAction });
                }
            }
        }

        private void OnGetChannelClipsEvent(GetChannelClipsEventArgs e)
        {
            GetChannelClipsEvent?.Invoke(this, e);
        }

        public class ShoutOutOverlayAction(OverlayActionType ShoutOutoverlayAction, Action<NewOverlayEventArgs> ActionPostShoutOut)
        {
            private OverlayActionType ShoutOut = ShoutOutoverlayAction;
            private readonly Action<NewOverlayEventArgs> PerformShoutOut = ActionPostShoutOut;

            public bool Finish;

            public void FoundChannelClips(List<Clip> clips)
            {
                if (clips.Count > 0)
                {
                    Random random = new();
                    int found = random.Next(clips.Count);
                    Clip resultClip = clips[found];

                    CheckURL(resultClip.Url, (int)Math.Ceiling(resultClip.Duration), ref ShoutOut);
                }

                PerformShoutOut(new() { OverlayAction = ShoutOut });

                Finish = true;
            }

        }

        /// <summary>
        /// Add the new Ticker Item to the database, then send it to the Overlay server
        /// </summary>
        /// <param name="item">An object containing the overlay ticker item details for updating.</param>
        /// <param name="UserName">The Username specific to the ticker item.</param>
        public static void AddNewOverlayTickerItem(OverlayTickerItem item, string UserName)
        {
            if (OptionFlags.ManageOverlayTicker)
            {
                DataManage.UpdateOverlayTicker(item, UserName);

                UpdatedTickerItems?.Invoke(null, new() { TickerItems = DataManage.GetTickerItems() });
            }
        }

        /// <summary>
        /// Initialize the Overlay Ticker Items when the server first starts, because the data is empty at the start (not persistent)
        /// </summary>
        public void SendInitialTickerItems()
        {
            UpdatedTickerItems?.Invoke(this, new() { TickerItems = DataManage.GetTickerItems() });
        }
    }
}
