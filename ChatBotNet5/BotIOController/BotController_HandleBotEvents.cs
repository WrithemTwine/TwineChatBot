using ChatBot_Net5.BotClients;
using ChatBot_Net5.Enum;
using ChatBot_Net5.Static;
using ChatBot_Net5.Systems;

using System;
using System.Collections.Generic;
using System.Reflection;

using TwitchLib.Api.Services.Events.LiveStreamMonitor;

namespace ChatBot_Net5.BotIOController
{
    public partial class BotController
    {
        private void HandleOnStreamOnline(OnStreamOnlineArgs e)
        {
            if (e.Channel != TwitchBots.TwitchChannelName)
            {
                SendMultiLiveMsg(e);
            }
            else
            {
                HandleOnStreamOnline(e.Stream.UserName, e.Stream.Title, e.Stream.StartedAt.ToLocalTime(), e.Stream.GameName);
            }
        }

        public void HandleOnStreamOnline(string UserName, string Title, DateTime StartedAt, string Category, bool Debug = false)
        {
            try
            {
                if (OptionFlags.TwitchChatBotConnectOnline && TwitchIO.IsStopped)
                {
                    TwitchIO.StartBot();
                }

                bool Started = Stats.StreamOnline(StartedAt);
                SystemsController.Category = Category;

                if (Started)
                {
                    bool MultiLive = StatisticsSystem.CheckStreamTime(StartedAt);

                    if ((OptionFlags.PostMultiLive && MultiLive) || !MultiLive)
                    {
                        // get message, set a default if otherwise deleted/unavailable
                        string msg = LocalizedMsgSystem.GetEventMsg(ChannelEventActions.Live, out bool Enabled);

                        // keys for exchanging codes for representative names
                        Dictionary<string, string> dictionary = VariableParser.BuildDictionary(new Tuple<MsgVars, string>[]
                        {
                                new(MsgVars.user, UserName),
                                new(MsgVars.category, Category),
                                new(MsgVars.title, Title),
                                new(MsgVars.url, UserName)
                        });

                        string TempMsg = VariableParser.ParseReplace(msg, dictionary);

                        if (Enabled && !Debug)
                        {
                            foreach (Tuple<bool, Uri> u in StatisticsSystem.GetDiscordWebhooks(WebhooksKind.Live))
                            {
                                DiscordWebhook.SendMessage(u.Item2, VariableParser.ParseReplace(TempMsg, VariableParser.BuildDictionary(new Tuple<MsgVars, string>[]
                                                                {
                                                                        new(MsgVars.everyone, u.Item1 ? "@everyone" : "")
                                                                }
                                                            )
                                                        )
                                                    );
                                Stats.AddDiscord();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex, MethodBase.GetCurrentMethod().Name);
            }
        }

        public void HandleOnStreamOffline()
        {
            if (OptionFlags.IsStreamOnline)
            {
                Stats.StreamOffline(DateTime.Now.ToLocalTime());
            }

            if (OptionFlags.TwitchChatBotDisconnectOffline && TwitchIO.IsStarted)
            {
                TwitchIO.StopBot();
            }
        }
    }
}
