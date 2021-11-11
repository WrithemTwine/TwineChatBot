using StreamerBot.BotClients;
using StreamerBot.BotClients.Twitch;
using StreamerBot.Events;
using StreamerBot.Interfaces;
using StreamerBot.Static;
using StreamerBot.Systems;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows.Threading;

using TwitchLib.Api.Helix.Models.Users.GetUserFollows;
using TwitchLib.Api.Services.Events.FollowerService;
using TwitchLib.Api.Services.Events.LiveStreamMonitor;

namespace StreamerBot.BotIOController
{
    public class BotController
    {
        private Dispatcher AppDispatcher { get; set; }
        public SystemsController Systems { get; private set; }
        internal Collection<IBotTypes> BotsList { get; private set; } = new();

        public BotsTwitch TwitchBots { get; private set; }



        public BotController()
        {
            Systems = new();
            Systems.PostChannelMessage += Systems_PostChannelMessage;


            BotsTwitch Twitch = new BotsTwitch();
            TwitchBots = Twitch;
            Twitch.BotEvent += Twitch_BotEvent;


            BotsList.Add(Twitch);

        }

        public void SetDispatcher(Dispatcher dispatcher)
        {
            AppDispatcher = dispatcher;
        }

        private void Twitch_BotEvent(object sender, BotEventArgs e)
        {
            AppDispatcher.Invoke(() =>
            {
                typeof(BotController).InvokeMember(name: e.MethodName, invokeAttr: BindingFlags.InvokeMethod, binder: null, target: this, args: new[] { e.e });
            });
        }

        private void Systems_PostChannelMessage(object sender, Events.PostChannelMessageEventArgs e)
        {
            Send(e.Msg);
        }

        public void Send(string s)
        {
            foreach (IBotTypes bot in BotsList)
            {
                bot.Send(s);
            }
        }

        public void ExitBots()
        {
            foreach (IBotTypes bot in BotsList)
            {
                bot.StopBots();
            }
        }

        public void ClearWatchTime()
        {
            Systems.ClearWatchTime();
        }

        #region Twitch Bot Events
        public void TwitchPostNewFollowers(OnNewFollowersDetectedArgs Follower)
        {
            HandleBotEventNewFollowers(ConvertFollowers(Follower.NewFollowers));
        }

        private List<Models.Follow> ConvertFollowers(List<Follow> follows)
        {
            List<Models.Follow> followsList = new();

            foreach (Follow f in follows)
            {
                followsList.Add(new() { FollowedAt = f.FollowedAt, FromUserId = f.FromUserId, FromUserName = f.FromUserName, ToUserId = f.ToUserId, ToUserName = f.ToUserName });
            }

            return followsList;
        }

        public void TwitchBulkPostFollowers(OnNewFollowersDetectedArgs Follower)
        {
            HandleBotEventBulkPostFollowers(ConvertFollowers(Follower.NewFollowers));
        }

        #endregion

        #region Handle Bot Events

        public void HandleBotEventNewFollowers(List<Models.Follow> follows)
        {
            Systems.AddNewFollowers(follows);
        }

        public void HandleBotEventBulkPostFollowers(List<Models.Follow> follows)
        {
            Dispatcher.CurrentDispatcher.Invoke(() => Systems.UpdateFollowers(follows));
        }



        private void HandleOnStreamOnline(OnStreamOnlineArgs e)
        {
            if (e.Channel != TwitchBotsBase.TwitchChannelName)
            {
                TwitchBots.GetLiveMonitorSvc().SendMultiLiveMsg(e);
            }
            else
            {
                HandleOnStreamOnline(e.Stream.UserName, e.Stream.Title, e.Stream.StartedAt.ToLocalTime(), e.Stream.GameName);
            }
        }

        public void HandleOnStreamOnline(string UserName, string Title, DateTime StartedAt, string Category, bool Debug = false)
        {
            //try
            //{
            //    if (OptionFlags.TwitchChatBotConnectOnline && TwitchIO.IsStopped)
            //    {
            //        TwitchIO.StartBot();
            //    }

            //    bool Started = SystemsController.StreamOnline(StartedAt);
            //    SystemsController.Category = Category;

            //    if (Started)
            //    {
            //        bool MultiLive = StatisticsSystem.CheckStreamTime(StartedAt);

            //        if ((OptionFlags.PostMultiLive && MultiLive) || !MultiLive)
            //        {
            //            // get message, set a default if otherwise deleted/unavailable
            //            string msg = LocalizedMsgSystem.GetEventMsg(ChannelEventActions.Live, out bool Enabled);

            //            // keys for exchanging codes for representative names
            //            Dictionary<string, string> dictionary = VariableParser.BuildDictionary(new Tuple<MsgVars, string>[]
            //            {
            //                    new(MsgVars.user, UserName),
            //                    new(MsgVars.category, Category),
            //                    new(MsgVars.title, Title),
            //                    new(MsgVars.url, UserName)
            //            });

            //            string TempMsg = VariableParser.ParseReplace(msg, dictionary);

            //            if (Enabled && !Debug)
            //            {
            //                foreach (Tuple<bool, Uri> u in StatisticsSystem.GetDiscordWebhooks(WebhooksKind.Live))
            //                {
            //                    DiscordWebhook.SendMessage(u.Item2, VariableParser.ParseReplace(TempMsg, VariableParser.BuildDictionary(new Tuple<MsgVars, string>[]
            //                                                    {
            //                                                            new(MsgVars.everyone, u.Item1 ? "@everyone" : "")
            //                                                    }
            //                                                )
            //                                            )
            //                                        );
            //                    SystemsController.UpdatedStat(StreamStatType.Discord);
            //                }
            //            }
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    LogWriter.LogException(ex, MethodBase.GetCurrentMethod().Name);
            //}
        }

        public void HandleOnStreamOffline()
        {
            if (OptionFlags.IsStreamOnline)
            {
                Systems.StreamOffline(DateTime.Now.ToLocalTime());
            }

            //if (OptionFlags.TwitchChatBotDisconnectOffline && TwitchIO.IsStarted)
            //{
            //    TwitchIO.StopBot();
            //}
        }


        /// <summary>
        /// This method checks the user settings and will delete any DB data if the user unchecks the setting. 
        /// Other methods to manage users & followers will adapt to if the user adjusted the setting
        /// </summary>
        public void ManageDatabase()
        {
            Systems.ManageDatabase();
            // TODO: add fixes if user re-enables 'managing { users || followers || stats }' to restart functions without restarting the bot

            // if ManageFollowers is False, then remove followers!, upstream code stops the follow bot
            if (OptionFlags.ManageFollowers)
            {
                TwitchBots.GetAllFollowers();
            }
            // when management resumes, code upstream enables the startbot process 
        }

        #endregion

    }
}
