using StreamerBotLib.Models;
using StreamerBotLib.Models.Enums;
using StreamerBotLib.Models.Events;
using StreamerBotLib.Static;
using StreamerBotLib.Systems.Overlay.Enums;
using StreamerBotLib.Systems.Overlay.Models;

/*
 * For clips to appear in any overlay action, Twitch requires for their embed player a domain name and the domain must utilize SSL. 
 * https://dev.twitch.tv/docs/embed
 * 
 * For this reason, currently disabling the Clips features - either showing the clip someone made of the current channel and shoutout random clip for a user.
 * Only hid the features by comment as they might be used in the future.
*/

namespace StreamerBotLib.Systems
{
    public partial class ActionSystem
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

        private readonly List<string> ChannelPointRewards = [];

        public void SetNewOverlayEventHandler(EventHandler<NewOverlayEventArgs> NewOverlayeventHandler, EventHandler<UpdatedTickerItemsEventArgs> UpdatedTickerEventHandler)
        {
            LogWriter.DebugLog("SetNewOverlayEventHandler", DebugLogTypes.SystemController, "Setting new overlay event handlers.");
            NewOverlayEvent += NewOverlayeventHandler;
            UpdatedTickerItems += UpdatedTickerEventHandler;
        }

        /// <summary>
        /// Setup the channel points reward list, update the new information.
        /// </summary>
        /// <param name="RewardList">The list of rewards. Internal list updates for any new data.</param>
        public void SetChannelRewardList(List<string> RewardList)
        {
            LogWriter.DebugLog("SetChannelRewardList", DebugLogTypes.OverlayBot, $"Setting the Channel Reward List with {RewardList.Count} rewards.");
            ChannelPointRewards.UniqueAddRange(RewardList);
        }

        public Dictionary<string, List<string>> GetOverlayActions()
        {
            LogWriter.DebugLog("GetOverlayActions", DebugLogTypes.OverlayBot, "Getting the Overlay Actions for the Overlay Server.");
            Dictionary<string, List<string>> OverlayActionPairs = new()
            {
                // if there are no channel point rewards, the streamers credentials
                // may need to be loaded or there aren't any channel points
                { OverlayTypes.ChannelPoints.ToString(), ChannelPointRewards.Count > 0 ? ChannelPointRewards : ["None or Not Loaded!"] },
                { OverlayTypes.Giveaway.ToString(), [OverlayTypes.Giveaway.ToString()] },
                { OverlayTypes.Commands.ToString(), new(DataManage.GetCommandList(false)) },
                { OverlayTypes.ChannelEvents.ToString(), new(Enum.GetNames<ChannelEventActions>()) }
            };

            //OverlayActionPairs.Add(OverlayTypes.Clip.ToString(), new() { OverlayTypes.Clip.ToString() });

            foreach (string K in OverlayActionPairs.Keys)
            {
                OverlayActionPairs[K].Sort();
            }

            return OverlayActionPairs;
        }

        private static void CheckURL(string ProvidedURL, float UrlDuration, ref OverlayActionType data)
        {
            LogWriter.DebugLog("CheckURL", DebugLogTypes.OverlayBot, $"Checking the provided URL {ProvidedURL} and Duration {UrlDuration}.");
            if (ProvidedURL != null && UrlDuration != 0)
            {
                data.MediaFile = ProvidedURL;
                data.Duration = Math.Min((int)Math.Ceiling(UrlDuration), data.Duration);

                LogWriter.DebugLog("CheckURL", DebugLogTypes.OverlayBot, $"The provided URL {ProvidedURL} and Duration {data.Duration} check out, and are added to data.");

            }
        }

        private void OnNewOverlayEvent(NewOverlayEventArgs e)
        {
            LogWriter.DebugLog("OnNewOverlayEvent", DebugLogTypes.OverlayBot, $"Building Overlay Event with action data, {e.OverlayAction.OverlayType} and {e.OverlayAction.ActionValue}.");

            NewOverlayEvent?.Invoke(this, e);
        }

        public void CheckForOverlayEvent(OverlayTypes overlayType, Enum enumvalue, LiveUser User, string UserMsg = null, string ProvidedURL = null, float UrlDuration = 0)
        {
            LogWriter.DebugLog("CheckForOverlayEvent", DebugLogTypes.OverlaySystem, "Checking for overlay event.");
            CheckForOverlayEvent(overlayType, enumvalue.ToString(), User, UserMsg, ProvidedURL, UrlDuration);
        }

        public void CheckForOverlayEvent(OverlayTypes overlayType, string Action, LiveUser User, string UserMsg = null, string ProvidedURL = null, float UrlDuration = 0)
        {
            LogWriter.DebugLog("CheckForOverlayEvent", DebugLogTypes.OverlayBot, $"Checking for an Overlay Event with action data, {overlayType} and {Action}.");
            List<OverlayActionType> overlayActionTypes = DataManage.GetOverlayActions(overlayType, Action, User?.UserName);
            OverlayActionType FoundAction = null;

            if (overlayType == OverlayTypes.ChannelPoints && User.UserId != null)
            {
                DataManage.UpdateStats(DBUserStats.ChannelRewards, User.UserId, User.Platform);
            }

            if (User?.UserName != null && overlayActionTypes.Count > 0)
            {
                FoundAction = overlayActionTypes.Find(x => x.UserName == User.UserName) ?? overlayActionTypes.Find(x => (x.OverlayType == overlayType) && (x.ActionValue == Action));

                LogWriter.DebugLog("CheckForOverlayEvent", DebugLogTypes.OverlayBot, $"Determined {FoundAction?.OverlayType} {FoundAction?.ActionValue} as the matching Overlay action.");
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
                LogWriter.DebugLog("CheckForOverlayEvent", DebugLogTypes.OverlayBot, $"OverlaySystem - found an action, {FoundAction.ActionValue} {FoundAction.OverlayType}, building a response alert.");

                CheckDiffMsg(ref FoundAction);
                if (overlayType == OverlayTypes.Commands && Action == DefaultCommand.so.ToString())
                {
                    if (OptionFlags.MediaOverlayShoutoutClips)
                    {
                        LogWriter.DebugLog("CheckForOverlayEvent", DebugLogTypes.OverlayBot, $"OverlaySystem - action, {FoundAction.ActionValue} {FoundAction.OverlayType}, is a shoutout.");

                        ThreadManager.CreateThreadStart("CheckForOverlayEvent", () =>
                        {
                            ShoutOutOverlayAction UserShout = new(FoundAction, OnNewOverlayEvent);
                            OnGetChannelClipsEvent(new() { ChannelName = User.UserName, CallBackResult = UserShout.FoundChannelClips });

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
            LogWriter.DebugLog("OnGetChannelClipsEvent", DebugLogTypes.OverlayBot, $"Requesting Channel Clips for {e.ChannelName}.");
            GetChannelClipsEvent?.Invoke(this, e);
        }

        public class ShoutOutOverlayAction(OverlayActionType ShoutOutoverlayAction, Action<NewOverlayEventArgs> ActionPostShoutOut)
        {
            private OverlayActionType ShoutOut = ShoutOutoverlayAction;
            private readonly Action<NewOverlayEventArgs> PerformShoutOut = ActionPostShoutOut;

            public bool Finish;

            public void FoundChannelClips(List<Clip> clips)
            {
                LogWriter.DebugLog("FoundChannelClips", DebugLogTypes.OverlayBot, $"Found {clips.Count} clips for the shoutout.");
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
            LogWriter.DebugLog("AddNewOverlayTickerItem", DebugLogTypes.OverlayBot, $"Adding a new Overlay Ticker Item for {UserName}.");
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
            LogWriter.DebugLog("SendInitialTickerItems", DebugLogTypes.OverlayBot, "Sending the initial Ticker Items to the Overlay Server.");
            UpdatedTickerItems?.Invoke(this, new() { TickerItems = DataManage.GetTickerItems() });
        }
    }
}
