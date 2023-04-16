using StreamerBotLib.Enums;
using StreamerBotLib.Events;
using StreamerBotLib.Models;
using StreamerBotLib.Overlay.Enums;
using StreamerBotLib.Overlay.Models;
using StreamerBotLib.Static;

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;

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
        public event EventHandler<GetChannelClipsEventArgs> GetChannelClipsEvent;
        public static event EventHandler<UpdatedTickerItemsEventArgs> UpdatedTickerItems;

        /// <summary>
        /// List of Table/Column pairs for building the overlay action selections
        /// </summary>
        private readonly Dictionary<string, string> OverlayActionColumnPairs = new()
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

            lock (GUI.GUIDataManagerLock.Lock)
            {
                foreach (string O in OverlayActionColumnPairs.Keys)
                {
                    OverlayActionPairs.Add(O, DataManage.GetRowsDataColumn(O, OverlayActionColumnPairs[O]).ConvertAll((i) => i.ToString()));
                }
            }

            // if there are no channel point rewards, the streamers credentials may need to be loaded or there aren't any channel points
            OverlayActionPairs.Add(OverlayTypes.ChannelPoints.ToString(), ChannelPointRewards.Count > 0 ? ChannelPointRewards : new() { "None or Not Loaded!" });
            OverlayActionPairs.Add(OverlayTypes.Giveaway.ToString(), new() { OverlayTypes.Giveaway.ToString() });
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
            }
        }

        private void OnNewOverlayEvent(NewOverlayEventArgs e)
        {
            NewOverlayEvent?.Invoke(this, e);
        }

        public void CheckForOverlayEvent(OverlayTypes overlayType, string Action, string UserName = null, string UserMsg = null, string ProvidedURL = null, float UrlDuration = 0)
        {
            List<OverlayActionType> overlayActionTypes = DataManage.GetOverlayActions(overlayType.ToString(), Action, UserName);
            OverlayActionType FoundAction = null;

            if (UserName != null && overlayActionTypes.Count > 0)
            {
                FoundAction = overlayActionTypes.Find(x => x.UserName == UserName) ?? overlayActionTypes.Find(x => (x.OverlayType == overlayType) && (x.ActionValue == Action));
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

#if LOG_OVERLAY
                LogWriter.OverlayLog(MethodBase.GetCurrentMethod().Name, $"OverlaySystem - found an action, {FoundAction.ActionValue} {FoundAction.OverlayType}, building a response alert.");
#endif

                CheckDiffMsg(ref FoundAction);
                if (overlayType == OverlayTypes.Commands && Action == Enums.DefaultCommand.so.ToString())
                {
                    if (OptionFlags.MediaOverlayShoutoutClips)
                    {
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

        public class ShoutOutOverlayAction
        {
            private OverlayActionType ShoutOut;
            private readonly Action<NewOverlayEventArgs> PerformShoutOut;

            public bool Finish = false;

            public ShoutOutOverlayAction(OverlayActionType ShoutOutoverlayAction, Action<NewOverlayEventArgs> ActionPostShoutOut)
            {
                ShoutOut = ShoutOutoverlayAction;
                PerformShoutOut = ActionPostShoutOut;
            }

            public void FoundChannelClips(List<Clip> clips)
            {
                if (clips.Count > 0)
                {
                    Random random = new Random();
                    int found = random.Next(clips.Count);
                    Clip resultClip = clips[found];

                    CheckURL(resultClip.Url, (int)Math.Ceiling(resultClip.Duration), ref ShoutOut);
                }

                PerformShoutOut(new() { OverlayAction = ShoutOut });

                Finish = true;
            }

        }

        public static void AddNewOverlayTickerItem(OverlayTickerItem item, string UserName)
        {
            if (OptionFlags.ManageOverlayTicker)
            {
                DataManage.UpdateOverlayTicker(item, UserName);

                UpdatedTickerItems?.Invoke(null, new() { TickerItems = DataManage.GetTickerItems() });
            }
        }

        public void SendInitialTickerItems()
        {
            UpdatedTickerItems?.Invoke(this, new() { TickerItems = DataManage.GetTickerItems() });
        }
    }
}
