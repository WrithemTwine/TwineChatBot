using MediaOverlayServer.Enums;
using MediaOverlayServer.Models;

using StreamerBotLib.Events;
using StreamerBotLib.Models;
using StreamerBotLib.Static;

using System;
using System.Collections.Generic;
using System.Threading;

namespace StreamerBotLib.Systems
{
    internal class OverlaySystem : SystemsBase
    {
        /// <summary>
        /// A stream action caused an Overlay Event to occur, and should be displayed via the Media Overlay Server.
        /// </summary>
        public event EventHandler<NewOverlayEventArgs> NewOverlayEvent;
        public event EventHandler<GetChannelClipsEventArgs> GetChannelClipsEvent;

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
            private Action<NewOverlayEventArgs> PerformShoutOut;

            public bool Finish = false;

            public ShoutOutOverlayAction(OverlayActionType ShoutOutoverlayAction, Action<NewOverlayEventArgs> ActionPostShoutOut)
            {
                ShoutOut = ShoutOutoverlayAction;
                PerformShoutOut = ActionPostShoutOut;
            }

            public void FoundChannelClips(List<Clip> clips)
            {
                if(clips.Count > 0)
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
    }
}
