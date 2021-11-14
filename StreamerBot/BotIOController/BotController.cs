using StreamerBot.BotClients;
using StreamerBot.BotClients.Twitch;
using StreamerBot.BotClients.Twitch.TwitchLib.Events.ClipService;
using StreamerBot.Enum;
using StreamerBot.Events;
using StreamerBot.Interfaces;
using StreamerBot.Static;
using StreamerBot.Systems;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Reflection;
using System.Windows.Threading;

using TwitchLib.Api.Helix.Models.Clips.GetClips;
using TwitchLib.Api.Helix.Models.Users.GetUserFollows;
using TwitchLib.Api.Services.Events.FollowerService;
using TwitchLib.Api.Services.Events.LiveStreamMonitor;
using TwitchLib.Client.Events;

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
            Twitch.BotEvent += HandleBotEvent;

            BotsList.Add(Twitch);
        }

        public void SetDispatcher(Dispatcher dispatcher)
        {
            AppDispatcher = dispatcher;
        }

        private void HandleBotEvent(object sender, BotEventArgs e)
        {
            AppDispatcher.Invoke(() =>
            {
                typeof(BotController).InvokeMember(name: e.MethodName, invokeAttr: BindingFlags.InvokeMethod, binder: null, target: this, args: new[] { e.e }, culture: CultureInfo.CurrentCulture);
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
        
        public void ClearWatchTime()
        {
            Systems.ClearWatchTime();
        }

        public void ClearAllCurrenciesValues()
        {
            Systems.ClearAllCurrenciesValues();
        }

        #region Twitch Bot Events
        public void TwitchPostNewFollowers(OnNewFollowersDetectedArgs Follower)
        {
            HandleBotEventNewFollowers(ConvertFollowers(Follower.NewFollowers));
        }

        private List<Models.Follow> ConvertFollowers(List<Follow> follows)
        {
            return follows.ConvertAll((f) =>
            {
                return new Models.Follow()
                {
                    FollowedAt = f.FollowedAt,
                    FromUserId = f.FromUserId,
                    FromUserName = f.FromUserName,
                    ToUserId = f.ToUserId,
                    ToUserName = f.ToUserName
                };
            });
        }

        public void TwitchBulkPostFollowers(OnNewFollowersDetectedArgs Follower)
        {
            HandleBotEventBulkPostFollowers(ConvertFollowers(Follower.NewFollowers));
        }

        public void TwitchPostNewClip(OnNewClipsDetectedArgs clips)
        {
            HandleBotEventPostNewClip(clips.Clips.ConvertAll((SrcClip) =>
            {
                return new Models.Clip()
                {
                    ClipId = SrcClip.Id,
                    CreatedAt = SrcClip.CreatedAt,
                    Duration = SrcClip.Duration,
                    GameId = SrcClip.GameId,
                    Language = SrcClip.Language,
                    Title = SrcClip.Title,
                    Url = SrcClip.Url
                };

            }));
        }

        public void TwitchStreamOnline(OnStreamOnlineArgs e)
        {
            HandleOnStreamOnline(e.Stream.UserName, e.Stream.Title, e.Stream.StartedAt.ToLocalTime(), e.Stream.GameName);
        }

        public void TwitchStreamupdate(OnStreamUpdateArgs e)
        {
            HandleOnStreamUpdate(e.Stream.GameId, e.Stream.GameName);
        }

        public void TwitchStreamOffline(OnStreamOfflineArgs e)
        {
            HandleOnStreamOffline();
        }

        public void TwitchNewSubscriber(OnNewSubscriberArgs e)
        {
            HandleNewSubscriber(e.Subscriber.DisplayName, e.Subscriber.MsgParamCumulativeMonths, e.Subscriber.SubscriptionPlan.ToString(), e.Subscriber.SubscriptionPlanName);
        }

        public void TwitchReSubscriber(OnReSubscriberArgs e)
        {
            HandleReSubscriber(e.ReSubscriber.DisplayName, e.ReSubscriber.Months, e.ReSubscriber.MsgParamCumulativeMonths, e.ReSubscriber.SubscriptionPlan.ToString(), e.ReSubscriber.SubscriptionPlanName, e.ReSubscriber.MsgParamShouldShareStreak, e.ReSubscriber.MsgParamStreakMonths);
        }

        public void TwitchGiftSubscription(OnGiftedSubscriptionArgs e)
        {

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

        public void HandleBotEventPostNewClip(List<Models.Clip> clips)
        {
            Systems.ClipHelper(clips);
        }

        public void HandleOnStreamOnline(string UserName, string Title, DateTime StartedAt, string Category, bool Debug = false)
        {
            try
            {
                bool Started = Systems.StreamOnline(StartedAt);
                SystemsBase.Category = Category;

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
                                Systems.UpdatedStat(StreamStatType.Discord);
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

        public void HandleOnStreamUpdate(string gameId, string gameName)
        {
            Systems.SetCategory(gameId, gameName);
        }

        public void HandleOnStreamOffline()
        {
            if (OptionFlags.IsStreamOnline)
            {
                Systems.StreamOffline(DateTime.Now.ToLocalTime());
            }
        }

        public void HandleNewSubscriber(string DisplayName, string Months, string Subscription, string SubscriptionName)
        {
            string msg = LocalizedMsgSystem.GetEventMsg(ChannelEventActions.Subscribe, out bool Enabled);
            if (Enabled)
            {
                Send(VariableParser.ParseReplace(msg, VariableParser.BuildDictionary(new Tuple<MsgVars, string>[] {
                new( MsgVars.user, DisplayName ),
                new( MsgVars.submonths, FormatData.Plurality(Months, MsgVars.Pluralmonths, Prefix: LocalizedMsgSystem.GetVar(MsgVars.Total)) ),
                new( MsgVars.subplan, Subscription ),
                new( MsgVars.subplanname, SubscriptionName )
                })));
            }

            Systems.UpdatedStat(new List<StreamStatType>() { StreamStatType.Sub, StreamStatType.AutoEvents });
        }

        public void HandleReSubscriber(string DisplayName, int Months, string TotalMonths, string Subscription, string SubscriptionName, bool ShareStreak, string StreakMonths)
        {
            string msg = LocalizedMsgSystem.GetEventMsg(ChannelEventActions.Resubscribe, out bool Enabled);
            if (Enabled)
            {
                Dictionary<string, string> dictionary = VariableParser.BuildDictionary(new Tuple<MsgVars, string>[] {
                new( MsgVars.user, DisplayName ),
                new( MsgVars.months, FormatData.Plurality(Months, MsgVars.Pluralmonths, Prefix: LocalizedMsgSystem.GetVar(MsgVars.Total)) ),
                new( MsgVars.submonths, FormatData.Plurality(TotalMonths, MsgVars.Pluralmonths, Prefix: LocalizedMsgSystem.GetVar(MsgVars.Total))),
                new( MsgVars.subplan, Subscription),
                new( MsgVars.subplanname,SubscriptionName )
                });

                // add the streak element if user wants their sub streak displayed
                if (ShareStreak)
                {
                    VariableParser.AddData(ref dictionary, new Tuple<MsgVars, string>[] { new(MsgVars.streak, StreakMonths) });
                }

                Send(VariableParser.ParseReplace(msg, dictionary));
            }

            Systems.UpdatedStat(new List<StreamStatType>() { StreamStatType.Sub, StreamStatType.AutoEvents });
        }

        #endregion

    }
}
