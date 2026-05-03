using StreamerBotLib.Models.Enums;
using StreamerBotLib.Static;
using StreamerBotLib.Systems.Overlay.Enums;

namespace StreamerBotLib.Systems
{
    public partial class ActionSystem
    {
        internal void NotifyAdSoon(int secondsUntilAd, TimeSpan AdDuration)
        {
            // don't need OptionFlags.TwitchAdsNotify because this event chain won't start unless this option is enabled

            lock (ProcMsgQueue)
            {
                ProcMsgQueue.Enqueue(new(() =>
                {
                    LogWriter.DebugLog("NotifyAdSoon", DebugLogTypes.SystemController, "Notifying ad soon message.");
                    string msg = LocalizedMsgSystem.GetEventMsg(ChannelEventActions.AdSoon, out bool Enabled, out short Multi);

                    if (Enabled)
                    {
                        LogWriter.DebugLog("NotifyAdSoon", DebugLogTypes.SystemController, "Sending message for the ad starting soon.");
                        Dictionary<string, string> dictionary = VariableParser.BuildDictionary(new Tuple<MsgVars, string>[]
                        {
                            new(MsgVars.adduration, FormatData.FormatTimes(AdDuration)),
                            new(MsgVars.adtime, FormatData.FormatTimes(TimeSpan.FromSeconds(secondsUntilAd)))
                        });
                        SendMessage(VariableParser.ParseReplace(msg, dictionary), DataManage.GetEventAnnounce(ChannelEventActions.AdSoon), Multi);
                    }

                    CheckForOverlayEvent(OverlayTypes.ChannelEvents, ChannelEventActions.AdSoon.ToString(), null);
                })
                );
            }
        }

        internal void NotifyAdStart(TimeSpan AdDuration)
        {
            // don't need OptionFlags.TwitchAdsNotify because this event chain won't start unless this option is enabled

            lock (ProcMsgQueue)
            {
                ProcMsgQueue.Enqueue(new(() =>
                {
                    LogWriter.DebugLog("NotifyAdStart", DebugLogTypes.SystemController, "Notifying ad start message.");
                    string msg = LocalizedMsgSystem.GetEventMsg(ChannelEventActions.AdStart, out bool Enabled, out short Multi);

                    if (Enabled)
                    {
                        LogWriter.DebugLog("NotifyAdStart", DebugLogTypes.SystemController, "Sending message for the ad starting now.");
                        Dictionary<string, string> dictionary = VariableParser.BuildDictionary(new Tuple<MsgVars, string>[]
                        {
                            new(MsgVars.adduration, FormatData.FormatTimes(AdDuration))
                        });
                        SendMessage(VariableParser.ParseReplace(msg, dictionary), DataManage.GetEventAnnounce(ChannelEventActions.AdStart), Multi);
                    }

                    CheckForOverlayEvent(OverlayTypes.ChannelEvents, ChannelEventActions.AdStart.ToString(), null);
                })
                );
            }
        }

        internal void NotifyAdEnd()
        {
            // don't need OptionFlags.TwitchAdsNotify because this event chain won't start unless this option is enabled

            lock (ProcMsgQueue)
            {
                ProcMsgQueue.Enqueue(new(() =>
                {
                    LogWriter.DebugLog("NotifyAdEnd", DebugLogTypes.SystemController, "Notifying ad end message.");
                    string msg = LocalizedMsgSystem.GetEventMsg(ChannelEventActions.AdEnd, out bool Enabled, out short Multi);

                    if (Enabled)
                    {
                        LogWriter.DebugLog("NotifyAdEnd", DebugLogTypes.SystemController, "Sending message for the ad ending now.");
                        SendMessage(msg, DataManage.GetEventAnnounce(ChannelEventActions.AdEnd), Multi);
                    }

                    CheckForOverlayEvent(OverlayTypes.ChannelEvents, ChannelEventActions.AdEnd.ToString(), null);
                })
                );
            }
        }
    }
}
