﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

using StreamerBotLib.DataSQL.Models;
using StreamerBotLib.Enums;
using StreamerBotLib.Events;
using StreamerBotLib.GUI;
using StreamerBotLib.Interfaces;
using StreamerBotLib.MLearning;
using StreamerBotLib.Models;
using StreamerBotLib.Overlay.Enums;
using StreamerBotLib.Overlay.Models;
using StreamerBotLib.Static;
using StreamerBotLib.Systems;

using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Data;
using System.Globalization;
using System.Reflection;

namespace StreamerBotLib.DataSQL
{
    /*

!command: <switches-optional> <message>

switches:
-t:<table>   (requires -f)
-f:<field>    (requires -t)
-c:<currency> (requires -f, optional switch)
-unit:<field units>   (optional with -f, but recommended)

-p:<permission>
-top:<number>
-s:<sort>
-a:<action>
-e:<true|false> // IsEnabled
-param:<allow params to command>
-timer:<seconds>
-use:<usage message>
-category:<All-defaul>

-m:<message> -> The message to display, may include parameters (e.g. #user, #field).
 */

    public class DataManagerSQL : IDataManager, IDataManagerReadOnly
    {
        private readonly DataManagerFactory dbContextFactory = new();
        private SQLDBContext context;

        private bool constructingModel_Context;
        private bool BulkFollowerUpdate;

        /// <summary>
        /// always true to begin one learning cycle
        /// </summary>
        private bool LearnMsgChanged = true;

        private readonly ConcurrentQueue<IEnumerable<Follow>> followsQueue = new();

        private readonly string DefaulSocialMsg = LocalizedMsgSystem.GetVar(Msg.MsgDefaultSocialMsg);
        private DateTime CurrStreamStart { get; set; } = default;

        public event EventHandler<OnBulkFollowersAddFinishedEventArgs> OnBulkFollowersAddFinished;
        public event EventHandler<OnDataCollectionUpdatedEventArgs> OnDataCollectionUpdated;

        public DataManagerSQL()
        {
            context = dbContextFactory.CreateDbContext();
        }

        private void BuildDataContext()
        {
            if (context == default)
            {
                context = dbContextFactory.CreateDbContext();
            }
        }

        private void ClearDataContext()
        {
            //if (!constructingModel_Context && !BulkFollowerUpdate)
            //{
            //    context.Dispose();
            //    context = default;
            //}
        }

        public void NotifyDataCollectionUpdated(string TableName)
        {
            OnDataCollectionUpdated?.Invoke(this, new(TableName));
        }

        #region Construct default items
        /// <summary>
        /// Perform table setup procedures
        /// </summary>
        public void Initialize()
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.DataManager, $"Initializing the database.");

            constructingModel_Context = true;

            BuildDataContext();

            SetDefaultChannelEventsTable();  // check all default ChannelEvents names
            SetDefaultCommandsTable(); // check all default Commands
            SetLearnedMessages();

            constructingModel_Context = false;

            ClearDataContext();
            OptionFlags.DataLoaded = true;
        }

        /// <summary>
        /// Add default data to Channel Events table, to ensure the data is available to use in event messages.
        /// </summary>
        private void SetDefaultChannelEventsTable()
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.DataManager, $"Setting default channel events, adding any missing events.");

            lock (GUIDataManagerLock.Lock)
            {
                Dictionary<ChannelEventActions, Tuple<string, string>> dictionary = new()
                {
                    {
                        ChannelEventActions.BeingHosted,
                        new(LocalizedMsgSystem.GetEventMsg(ChannelEventActions.BeingHosted, out _, out _), VariableParser.ConvertVars([MsgVars.user, MsgVars.autohost, MsgVars.viewers]))
                    },
                    {
                        ChannelEventActions.Bits,
                        new(LocalizedMsgSystem.GetEventMsg(ChannelEventActions.Bits, out _, out _), VariableParser.ConvertVars([MsgVars.user, MsgVars.bits]))
                    },
                    {
                        ChannelEventActions.CommunitySubs,
                        new(LocalizedMsgSystem.GetEventMsg(ChannelEventActions.CommunitySubs, out _, out _), VariableParser.ConvertVars([MsgVars.user, MsgVars.count, MsgVars.subplan]))
                    },
                    {
                        ChannelEventActions.NewFollow,
                        new(LocalizedMsgSystem.GetEventMsg(ChannelEventActions.NewFollow, out _, out _), VariableParser.ConvertVars([MsgVars.user]))
                    },
                    {
                        ChannelEventActions.GiftSub,
                        new(LocalizedMsgSystem.GetEventMsg(ChannelEventActions.GiftSub, out _, out _), VariableParser.ConvertVars([MsgVars.user, MsgVars.months, MsgVars.receiveuser, MsgVars.subplan, MsgVars.subplanname]))
                    },
                    {
                        ChannelEventActions.Live,
                        new(LocalizedMsgSystem.GetEventMsg(ChannelEventActions.Live, out _, out _), VariableParser.ConvertVars([MsgVars.user, MsgVars.category, MsgVars.title, MsgVars.url, MsgVars.everyone]))
                    },
                    {
                        ChannelEventActions.Raid,
                        new(LocalizedMsgSystem.GetEventMsg(ChannelEventActions.Raid, out _, out _), VariableParser.ConvertVars([MsgVars.user, MsgVars.viewers]))
                    },
                    {
                        ChannelEventActions.Resubscribe,
                        new(LocalizedMsgSystem.GetEventMsg(ChannelEventActions.Resubscribe, out _, out _), VariableParser.ConvertVars([MsgVars.user, MsgVars.months, MsgVars.submonths, MsgVars.subplan, MsgVars.subplanname, MsgVars.streak]))
                    },
                    {
                        ChannelEventActions.Subscribe,
                        new(LocalizedMsgSystem.GetEventMsg(ChannelEventActions.Subscribe, out _, out _), VariableParser.ConvertVars([MsgVars.user, MsgVars.submonths, MsgVars.subplan, MsgVars.subplanname]))
                    },
                    {
                        ChannelEventActions.UserJoined,
                        new(LocalizedMsgSystem.GetEventMsg(ChannelEventActions.UserJoined, out _, out _), VariableParser.ConvertVars([MsgVars.user]))
                    },
                    {
                        ChannelEventActions.ReturnUserJoined,
                        new(LocalizedMsgSystem.GetEventMsg(ChannelEventActions.ReturnUserJoined, out _, out _), VariableParser.ConvertVars([MsgVars.user]))
                    },
                    {
                        ChannelEventActions.SupporterJoined,
                        new(LocalizedMsgSystem.GetEventMsg(ChannelEventActions.SupporterJoined, out _, out _), VariableParser.ConvertVars([MsgVars.user]))
                    },
                    {
                        ChannelEventActions.BannedUser,
                        new(LocalizedMsgSystem.GetEventMsg(ChannelEventActions.BannedUser, out _, out _), VariableParser.ConvertVars([MsgVars.user]))
                    }
                };

                context.ChannelEvents.AddRange(from CE in from E in dictionary.ExceptBy(context.ChannelEvents.Select(C => C.Name), E => E.Key)
                                                          let values = dictionary[E.Key]
                                                          select (E.Key, values)
                                               select new ChannelEvents(name: CE.Key, repeatMsg: 0, addMe: false, isEnabled: true, message: CE.values.Item1, commands: CE.values.Item2));
                context.SaveChanges(true);

            }
        }

        /// <summary>
        /// Add all of the default commands to the table, ensure they are available
        /// </summary>
        private void SetDefaultCommandsTable()
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.DataManager, $"Setting up and checking default commands, adding missing commands.");

            lock (GUIDataManagerLock.Lock)
            {
                if (!(from C in context.CategoryList where C.Category == LocalizedMsgSystem.GetVar(Msg.MsgAllCategory) select C).Any())
                {
                    context.CategoryList.Add(new(categoryId: "0", category: LocalizedMsgSystem.GetVar(Msg.MsgAllCategory), streamCount: 0));
                }

                // dictionary with commands, messages, and parameters
                // command name     // msg   // params
                Dictionary<string, Tuple<string, string>> DefCommandsDictionary = [];

                // add each of the default commands with localized strings
                foreach (DefaultCommand com in Enum.GetValues(typeof(DefaultCommand)))
                {
                    DefCommandsDictionary.Add(com.ToString(), new(LocalizedMsgSystem.GetDefaultComMsg(com), LocalizedMsgSystem.GetDefaultComParam(com)));
                }

                // add each of the social commands
                foreach (DefaultSocials social in Enum.GetValues(typeof(DefaultSocials)))
                {
                    DefCommandsDictionary.Add(social.ToString(), new(DefaulSocialMsg, LocalizedMsgSystem.GetVar("Parameachsocial")));
                }

                context.Commands.AddRange(from C in (from key in DefCommandsDictionary.ExceptBy(context.Commands.Select((C) => C.CmdName), c => c.Key)
                                                     let param = CommandParams.Parse(DefCommandsDictionary[key.Key].Item2)
                                                     select (key.Key, param))
                                          select new Commands(cmdName: C.Key,
                                                     addMe: false,
                                                     permission: C.param.Permission,
                                                     isEnabled: C.param.IsEnabled,
                                                     message: DefCommandsDictionary[C.Key].Item1,
                                                     repeatTimer: C.param.Timer,
                                                     sendMsgCount: C.param.RepeatMsg,
                                                     category: [string.IsNullOrEmpty(C.param.Category) ?
                                                                 LocalizedMsgSystem.GetVar(Msg.MsgAllCategory) :
                                                                 C.param.Category],
                                                     allowParam: C.param.AllowParam,
                                                     usage: C.param.Usage,
                                                     lookupData: C.param.LookupData,
                                                     table: C.param.Table,
                                                     keyField: !string.IsNullOrEmpty(C.param.Table) ? GetKey(C.param.Table) : "",
                                                     dataField: C.param.Field,
                                                     currencyField: C.param.Currency,
                                                     unit: C.param.Unit,
                                                     action: C.param.Action,
                                                     top: C.param.Top,
                                                     sort: C.param.Sort)
                 );

                context.SaveChanges(true);
            }
        }

        private void SetLearnedMessages()
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.DataManager, $"Machine learning, setting learned messages.");

            lock (GUIDataManagerLock.Lock)
            {
                if (!context.LearnMsgs.Any())
                {
                    context.LearnMsgs.AddRange(from M in LearnedMessagesPrimer.PrimerList
                                               select new LearnMsgs(msgType: M.MsgType, teachingMsg: M.Message));
                }

                if (!context.BanReasons.Any())
                {
                    context.BanReasons.AddRange(from B in LearnedMessagesPrimer.BanReasonList
                                                select new Models.BanReasons(msgType: B.MsgType, banReason: B.Reason));
                }

                if (!context.BanRules.Any())
                {
                    context.BanRules.AddRange(from R in LearnedMessagesPrimer.BanViewerRulesList
                                              select new BanRules(0, R.ViewerType, R.MsgType, R.ModAction, R.TimeoutSeconds));
                }

                context.SaveChanges(true);
            }
        }
        #endregion

        #region LocalView ObservableCollection

        public ObservableCollection<Models.BanReasons> GetBanReasonsLocalObservable()
        {
            context.BanReasons.Load(); return context.BanReasons.Local.ToObservableCollection();
        }

        public ObservableCollection<BanRules> GetBanRulesLocalObservable()
        {
            context.BanRules.Load(); return context.BanRules.Local.ToObservableCollection();
        }

        public ObservableCollection<CategoryList> GetCategoryListLocalObservable()
        {
            context.CategoryList.Load(); return context.CategoryList.Local.ToObservableCollection();
        }

        public ObservableCollection<ChannelEvents> GetChannelEventsLocalObservable()
        {
            context.ChannelEvents.Load(); return context.ChannelEvents.Local.ToObservableCollection();
        }

        public ObservableCollection<Clips> GetClipsLocalObservable()
        {
            context.Clips.Load(); return context.Clips.Local.ToObservableCollection();
        }

        public ObservableCollection<Commands> GetCommandsLocalObservable()
        {
            context.Commands.Load(); return context.Commands.Local.ToObservableCollection();
        }

        public ObservableCollection<CommandsUser> GetCommandsUserLocalObservable()
        {
            context.CommandsUser.Load(); return context.CommandsUser.Local.ToObservableCollection();
        }

        public ObservableCollection<Currency> GetCurrencyLocalObservable()
        {
            context.Currency.Load(); return context.Currency.Local.ToObservableCollection();
        }

        public ObservableCollection<Models.CurrencyType> GetCurrencyTypeLocalObservable()
        {
            context.CurrencyType.Load(); return context.CurrencyType.Local.ToObservableCollection();
        }

        public ObservableCollection<CustomWelcome> GetCustomWelcomeLocalObservable()
        {
            context.CustomWelcome.Load(); return context.CustomWelcome.Local.ToObservableCollection();
        }

        public ObservableCollection<Followers> GetFollowersLocalObservable()
        {
            context.Followers.Load(); return context.Followers.Local.ToObservableCollection();
        }

        public ObservableCollection<GameDeadCounter> GetGameDeadCounterLocalObservable()
        {
            context.GameDeadCounter.Load(); return context.GameDeadCounter.Local.ToObservableCollection();
        }

        public ObservableCollection<GiveawayUserData> GetGiveawayUserDataLocalObservable()
        {
            context.GiveawayUserData.Load(); return context.GiveawayUserData.Local.ToObservableCollection();
        }

        public ObservableCollection<InRaidData> GetInRaidDataLocalObservable()
        {
            context.InRaidData.Load(); return context.InRaidData.Local.ToObservableCollection();
        }

        public ObservableCollection<LearnMsgs> GetLearnMsgsLocalObservable()
        {
            context.LearnMsgs.Load(); return context.LearnMsgs.Local.ToObservableCollection();
        }

        public ObservableCollection<ModeratorApprove> GetModeratorApproveLocalObservable()
        {
            context.ModeratorApprove.Load(); return context.ModeratorApprove.Local.ToObservableCollection();
        }

        public ObservableCollection<MultiChannels> GetMultiChannelsLocalObservable()
        {
            context.MultiChannels.Load(); return context.MultiChannels.Local.ToObservableCollection();
        }

        public ObservableCollection<MultiLiveStreams> GetMultiLiveStreamsLocalObservable()
        {
            context.MultiLiveStreams.Load(); return context.MultiLiveStreams.Local.ToObservableCollection();
        }

        public ObservableCollection<MultiMsgEndPoints> GetMultiMsgEndPointsLocalObservable()
        {
            context.MultiMsgEndPoints.Load(); return context.MultiMsgEndPoints.Local.ToObservableCollection();
        }

        public ObservableCollection<MultiSummaryLiveStreams> GetMultiSummaryLiveStreamsLocalObservable()
        {
            context.MultiSummaryLiveStreams.Load(); return context.MultiSummaryLiveStreams.Local.ToObservableCollection();
        }

        public ObservableCollection<OutRaidData> GetOutRaidDataLocalObservable()
        {
            context.OutRaidData.Load(); return context.OutRaidData.Local.ToObservableCollection();
        }

        public ObservableCollection<OverlayServices> GetOverlayServicesLocalObservable()
        {
            context.OverlayServices.Load(); return context.OverlayServices.Local.ToObservableCollection();
        }

        public ObservableCollection<OverlayTicker> GetOverlayTickerLocalObservable()
        {
            context.OverlayTicker.Load(); return context.OverlayTicker.Local.ToObservableCollection();
        }

        public ObservableCollection<Quotes> GetQuotesLocalObservable()
        {
            context.Quotes.Load(); return context.Quotes.Local.ToObservableCollection();
        }

        public ObservableCollection<ShoutOuts> GetShoutOutsLocalObservable()
        {
            context.ShoutOuts.Load(); return context.ShoutOuts.Local.ToObservableCollection();
        }

        public ObservableCollection<StreamStats> GetStreamStatsLocalObservable()
        {
            context.StreamStats.Load(); return context.StreamStats.Local.ToObservableCollection();
        }

        public ObservableCollection<Users> GetUsersLocalObservable()
        {
            context.Users.Load();
            return context.Users.Local.ToObservableCollection();
        }

        public ObservableCollection<UserStats> GetUserStatsLocalObservable()
        {
            context.UserStats.Load(); return context.UserStats.Local.ToObservableCollection();
        }

        public ObservableCollection<Webhooks> GetWebhooksLocalObservable()
        {
            context.Webhooks.Load(); return context.Webhooks.Local.ToObservableCollection();
        }

        #endregion

        #region Check_Methods
        public bool CheckCurrency(LiveUser User, double value, string CurrencyName)
        {
            lock (GUIDataManagerLock.Lock)
            {
                BuildDataContext();
                var result = (from C in context.Currency
                              where (C.UserName == User.UserName && C.CurrencyName == CurrencyName)
                              select C.Value).FirstOrDefault() >= value;
                ClearDataContext();
                return result;
            }
        }

        public bool CheckField(string table, string field)
        {
            lock (GUIDataManagerLock.Lock)
            {
                BuildDataContext();
                var result = (context.Model.FindEntityType($"StreamerBotLib.DataSQL.Models.{table}").FindProperty(field) != null);
                ClearDataContext();
                return result;
            }
        }

        public bool CheckFollower(string User)
        {
            lock (GUIDataManagerLock.Lock)
            {
                BuildDataContext();
                var result = CheckFollower(User, default);
                ClearDataContext();
                return result;
            }
        }

        public bool CheckFollower(string User, DateTime ToDateTime)
        {
            lock (GUIDataManagerLock.Lock)
            {
                BuildDataContext();
                var result = (from f in context.Followers
                              where (f.IsFollower && f.UserName == User && (ToDateTime == default || f.FollowedDate < ToDateTime))
                              select f).Any();
                ClearDataContext();
                return result;
            }
        }

        public Tuple<string, string> CheckModApprovalRule(ModActionType modActionType, string ModAction)
        {
            lock (GUIDataManagerLock.Lock)
            {
                BuildDataContext();
                var result = (from M in context.ModeratorApprove
                              where (M.ModActionType == modActionType && M.ModActionName == ModAction)
                              select new Tuple<string, string>(
                                  !string.IsNullOrEmpty(M.ModPerformType.ToString()) ? M.ModPerformType.ToString() : M.ModActionType.ToString(),
                                  !string.IsNullOrEmpty(M.ModPerformAction) ? M.ModPerformAction : M.ModActionName
                                  )).FirstOrDefault();
                ClearDataContext();
                return result;
            }
        }

        /// <summary>
        /// Determines if there are multiple streams based on the same start date.
        /// </summary>
        /// <param name="streamStart">The stream start date and time to check.</param>
        /// <returns><code>true</code> if there are multiple streams
        /// <code>false</code> if there is no more than one stream.</returns>
        public bool CheckMultiStreams(DateTime streamStart)
        {
            lock (GUIDataManagerLock.Lock)
            {
                BuildDataContext();

                bool result = (from s in context.StreamStats
                               where (s.StreamStart == streamStart)
                               select s).Count() > 1;
                ClearDataContext();
                return result;
            }
        }

        /// <summary>
        /// Determine if the user invoking the command has permission to access the command.
        /// </summary>
        /// <param name="cmd">The command to verify the permission.</param>
        /// <param name="permission">The supplied permission to check.</param>
        /// <returns><code>true</code> - the permission is allowed to the command. <code>false</code> - the command permission is not allowed.</returns>
        public bool CheckPermission(string cmd, ViewerTypes permission)
        {
            lock (GUIDataManagerLock.Lock)
            {
                BuildDataContext();

                bool result = (from c in context.Commands
                               where c.CmdName == cmd
                               select c).FirstOrDefault().Permission > permission;
                ClearDataContext();
                return result;
            }
        }

        /// <summary>
        /// Verify if the provided UserName is within the ShoutOut table.
        /// </summary>
        /// <param name="UserName">The UserName to shoutout.</param>
        /// <returns>true if in the ShoutOut table.</returns>
        public bool CheckShoutName(string UserId)
        {
            lock (GUIDataManagerLock.Lock)
            {
                BuildDataContext();

                bool result = (from s in context.ShoutOuts
                               where s.UserId == UserId
                               select s).Any();
                ClearDataContext();
                return result;
            }
        }

        /// <summary>
        /// Find if stream data already exists for the current stream
        /// </summary>
        /// <param name="CurrTime">The time to check</param>
        /// <returns><code>true</code>: the stream already has a data entry; <code>false</code>: the stream has no data entry</returns>
        public bool CheckStreamTime(DateTime CurrTime)
        {
            lock (GUIDataManagerLock.Lock)
            {
                BuildDataContext();

                bool result = (from s in context.StreamStats
                               where s.StreamStart == CurrTime
                               select s).Any();
                ClearDataContext();
                return result;
            }
        }

        /// <summary>
        /// Check to see if the <paramref name="User"/> has been in the channel prior to DateTime.MaxValue.
        /// </summary>
        /// <param name="User">The user to check in the database.</param>
        /// <returns><code>true</code> if the <paramref name="User"/> has arrived anytime, <code>false</code> otherwise.</returns>
        public bool CheckUser(LiveUser User)
        {
            lock (GUIDataManagerLock.Lock)
            {
                return CheckUser(User, default);
            }
        }

        /// <summary>
        /// Check if the <paramref name="User"/> has visited the channel prior to <paramref name="ToDateTime"/>, identified as either DateTime.Now.ToLocalTime() or the current start of the stream.
        /// </summary>
        /// <param name="User">The user to verify.</param>
        /// <param name="ToDateTime">Specify the date to check if the user arrived to the channel prior to this date and time.</param>
        /// <returns><c>True</c> if the <paramref name="User"/> has been in channel before <paramref name="ToDateTime"/>, <c>false</c> otherwise.</returns>
        public bool CheckUser(LiveUser User, DateTime ToDateTime)
        {
            lock (GUIDataManagerLock.Lock)
            {
                BuildDataContext();

                bool result = (from s in context.Users
                               where (ToDateTime == default || s.FirstDateSeen < ToDateTime) && s.UserName == User.UserName && s.Platform == User.Platform
                               select s).Any();
                ClearDataContext();
                return result;
            }
        }

        /// <summary>
        /// Check the CustomWelcome table for the user and provide the message.
        /// </summary>
        /// <param name="User">The user to check for a welcome message.</param>
        /// <returns>The welcome message if user is available, or empty string if not found.</returns>
        public string CheckWelcomeUser(string User)
        {
            lock (GUIDataManagerLock.Lock)
            {
                BuildDataContext();

                string result = (from s in context.CustomWelcome
                                 where s.UserName == User
                                 select s.Message).FirstOrDefault() ?? "";
                ClearDataContext();
                return result;
            }
        }

        public void ClearAllCurrencyValues()
        {
            lock (GUIDataManagerLock.Lock)
            {

                BuildDataContext();
                foreach (Currency c in from u in context.Currency
                                       select u)
                {
                    c.Value = 0;
                }
                context.SaveChanges(true);
                ClearDataContext();
            }
        }

        /// <summary>
        /// Clear all User rows for users not included in the Followers table.
        /// </summary>
        public void ClearUsersNotFollowers()
        {
            lock (GUIDataManagerLock.Lock)
            {
                BuildDataContext();


                context.Users.RemoveRange((IEnumerable<Users>)(from user in context.Users
                                                               join follower in context.Followers on user.UserId equals follower.UserId into UserFollow
                                                               from subuser in UserFollow.DefaultIfEmpty()
                                                               where !subuser.IsFollower
                                                               select subuser));
                context.SaveChanges(true);
                ClearDataContext();
            }
        }

        /// <summary>
        /// Clear all user watchtimes
        /// </summary>
        public void ClearWatchTime()
        {
            lock (GUIDataManagerLock.Lock)
            {
                BuildDataContext();

                foreach (var userstat in from US in context.UserStats
                                         select US)
                {
                    userstat.WatchTime = new(0);
                }
                context.SaveChanges(true);
                ClearDataContext();
            }
        }
        #endregion Check_Methods

        public void DeleteDataRows(IEnumerable<DataRow> dataRows)
        {
            throw new NotImplementedException();
        }

        public string EditCommand(string cmd, List<string> Arglist)
        {
            string result = "";

            lock (GUIDataManagerLock.Lock)
            {
                BuildDataContext();
                Dictionary<string, string> EditParamsDict = CommandParams.ParseEditCommandParams(Arglist);
                Commands EditCom = (from C in context.Commands
                                    where C.CmdName == cmd
                                    select C).FirstOrDefault();

                if (EditCom != default)
                {
                    foreach (string k in EditParamsDict.Keys)
                    {
                        EditCom.GetType().GetProperty(k).SetValue(EditCom, EditParamsDict[k]);
                    }
                    result = string.Format(CultureInfo.CurrentCulture, LocalizedMsgSystem.GetDefaultComMsg(DefaultCommand.editcommand), cmd);
                    context.SaveChanges(true);
                }
                else
                {
                    result = string.Format(CultureInfo.CurrentCulture, LocalizedMsgSystem.GetVar("Msgcommandnotfound"), cmd);

                }
                ClearDataContext();
            }
            return result;
        }

        public Tuple<ModActions, Enums.BanReasons, int> FindRemedy(ViewerTypes viewerTypes, MsgTypes msgTypes)
        {
            lock (GUIDataManagerLock.Lock)
            {
                BuildDataContext();
                Enums.BanReasons banReasons = (from Br in context.BanReasons
                                               where Br.MsgType == msgTypes
                                               select Br.BanReason).FirstOrDefault();
                BanRules banRules = (from B in context.BanRules
                                     where (B.ViewerTypes == ViewerTypes.Viewer && B.MsgType == msgTypes)
                                     select B).FirstOrDefault();

                ClearDataContext();
                return new(banRules?.ModAction ?? ModActions.Allow, banReasons, banRules.TimeoutSeconds);
            }
        }

        #region Get_Methods
        public CommandData GetCommand(string cmd)
        {
            lock (GUIDataManagerLock.Lock)
            {
                BuildDataContext();
                CommandData result = new((from Com in (context.Commands.Union(context.CommandsUser))
                                          where Com.CmdName == cmd
                                          select Com).FirstOrDefault());
                ClearDataContext();
                return result;
            }
        }

        public IEnumerable<string> GetCommandList()
        {
            BuildDataContext();
            IEnumerable<string> result = GetCommands().Split(", ");
            ClearDataContext();
            return result;
        }

        public string GetCommands()
        {
            lock (GUIDataManagerLock.Lock)
            {
                BuildDataContext();
                string result = string.Join(", ", (from Com in (context.Commands.Union(context.CommandsUser))
                                                   where (Com.Message != DefaulSocialMsg && Com.IsEnabled)
                                                   orderby Com.CmdName
                                                   select $"!{Com.CmdName}"));
                ClearDataContext();
                return result;
            }
        }

        public List<string> GetCurrencyNames()
        {
            lock (GUIDataManagerLock.Lock)
            {
                BuildDataContext();
                List<string> result = new(from C in context.CurrencyType
                                          select C.CurrencyName);
                ClearDataContext();
                return result;
            }
        }

        public int GetDeathCounter(string currCategory)
        {
            lock (GUIDataManagerLock.Lock)
            {
                BuildDataContext();
                int result = (from D in context.GameDeadCounter
                              where D.Category == currCategory
                              select D.Counter).FirstOrDefault();
                ClearDataContext();
                return result;
            }
        }

        public string GetEventRowData(ChannelEventActions rowcriteria, out bool Enabled, out short Multi)
        {
            lock (GUIDataManagerLock.Lock)
            {
                BuildDataContext();
                ChannelEvents found = (from Event in context.ChannelEvents
                                       where Event.Name == rowcriteria
                                       select Event).FirstOrDefault();
                Enabled = found?.IsEnabled ?? false;
                Multi = found?.RepeatMsg ?? 0;
                ClearDataContext();

                return found?.Message;
            }
        }

        public int GetFollowerCount()
        {
            lock (GUIDataManagerLock.Lock)
            {
                BuildDataContext();
                var result = (from F in context.Followers
                              select F).Count();
                ClearDataContext();
                return result;
            }
        }

        public List<Tuple<string, string>> GetGameCategories()
        {
            lock (GUIDataManagerLock.Lock)
            {
                BuildDataContext();
                List<Tuple<string, string>> result = new(from G in context.CategoryList
                                                         let game = new Tuple<string, string>(G.CategoryId, G.Category)
                                                         select game);
                ClearDataContext();
                return result;
            }
        }

        public string GetKey(string Table)
        {
            lock (GUIDataManagerLock.Lock)
            {
                BuildDataContext();
                string result = context.Model.FindEntityType($"StreamerBotLib.DataSQL.Models.{Table}").FindPrimaryKey().GetName();
                ClearDataContext();
                return result;
            }
        }

        public IEnumerable<string> GetKeys(string Table)
        {
            lock (GUIDataManagerLock.Lock)
            {
                BuildDataContext();
                IEnumerable<string> result = new List<string>(from P in context.Model.FindEntityType($"StreamerBotLib.DataSQL.Models.{Table}").FindPrimaryKey().Properties select P.Name);
                ClearDataContext();
                return result;
            }
        }

        public string GetNewestFollower()
        {
            lock (GUIDataManagerLock.Lock)
            {
                BuildDataContext();
                string result = (from F in context.Followers orderby F.FollowedDate descending select F).FirstOrDefault().UserName;
                ClearDataContext();
                return result;
            }
        }

        public List<OverlayActionType> GetOverlayActions(OverlayTypes overlayType, string overlayAction, string username)
        {
            lock (GUIDataManagerLock.Lock)
            {
                BuildDataContext();
                List<OverlayActionType> result = new(from O in context.OverlayServices
                                                     where (O.IsEnabled
                                                     && O.OverlayType == overlayType
                                                     && (string.IsNullOrEmpty(O.UserName) || O.UserName == username)
                                                     && O.OverlayAction == overlayAction)
                                                     select new OverlayActionType()
                                                     {
                                                         ActionValue = O.OverlayAction,
                                                         Duration = O.Duration,
                                                         MediaFile = O.MediaFile,
                                                         ImageFile = O.ImageFile,
                                                         Message = O.Message,
                                                         OverlayType = O.OverlayType,
                                                         UserName = O.UserName,
                                                         UseChatMsg = O.UseChatMsg
                                                     });
                ClearDataContext();
                return result;
            }
        }

        public string GetQuote(int QuoteNum)
        {
            lock (GUIDataManagerLock.Lock)
            {
                BuildDataContext();
                var result = (from Q in context.Quotes
                              where Q.Number == QuoteNum
                              select $"{Q.Number}: {Q.Quote}").FirstOrDefault();
                ClearDataContext();
                return result;
            }
        }

        public int GetQuoteCount()
        {
            lock (GUIDataManagerLock.Lock)
            {
                BuildDataContext();
                int result = context.Quotes.MaxBy((q) => q.Number)?.Number ?? 0;
                ClearDataContext();
                return result;
            }
        }

        public Dictionary<string, List<string>> GetOverlayActions()
        {
            lock (GUIDataManagerLock.Lock)
            {
                BuildDataContext();
                Dictionary<string, List<string>> result = new()
                {
                    { nameof(Commands), new(from C in context.Commands select C.CmdName) },
                    { nameof(ChannelEvents), new(from E in context.ChannelEvents select E.Name.ToString()) }
                };
                ClearDataContext();
                return result;
            }
        }

        public List<string> GetSocialComs()
        {
            lock (GUIDataManagerLock.Lock)
            {
                BuildDataContext();
                List<string> result = new(from SC in context.Commands.IntersectBy((string[])Enum.GetValues(typeof(DefaultSocials)), (c) => c.CmdName)
                                          select SC.CmdName);
                ClearDataContext();
                return result;
            }
        }

        public string GetSocials()
        {
            lock (GUIDataManagerLock.Lock)
            {
                BuildDataContext();
                var result = string.Join(" ", (from SC in
                                             context.Commands.IntersectBy((string[])Enum.GetValues(typeof(DefaultSocials)),
                                                (c) => c.CmdName)
                                               where SC.Message != DefaulSocialMsg
                                               select SC));
                ClearDataContext();
                return result;
            }
        }

        public StreamStat GetStreamData(DateTime dateTime)
        {
            lock (GUIDataManagerLock.Lock)
            {
                BuildDataContext();

                var result = (from SD in context.StreamStats
                              where SD.StreamStart == dateTime
                              select StreamStat.Create(SD)).FirstOrDefault();
                ClearDataContext();
                return result;
            }
        }

        public List<string> GetTableFields(string TableName)
        {
            lock (GUIDataManagerLock.Lock)
            {
                BuildDataContext();
                List<string> result = new(from T in context.Model.FindEntityType($"StreamerBotLib.DataSQL.Models.{TableName}").GetMembers()
                                          select T.Name);
                ClearDataContext();
                return result;
            }
        }

        public List<string> GetTableNames()
        {
            lock (GUIDataManagerLock.Lock)
            {
                BuildDataContext();
                var list = new List<string>()
                {
                    nameof(Currency),
                    nameof(UserStats),
                    nameof(Commands),
                    nameof(CustomWelcome),
                    nameof(Followers)
                };
                list.Sort();
                ClearDataContext();
                return list;
            }
        }

        public List<TickerItem> GetTickerItems()
        {
            lock (GUIDataManagerLock.Lock)
            {
                BuildDataContext();
                List<TickerItem> result = new(from F in context.OverlayTicker
                                              select new TickerItem(F.TickerName, F.UserName));
                ClearDataContext();
                return result;
            }
        }

        public Tuple<string, int, List<string>> GetTimerCommand(string Cmd)
        {
            lock (GUIDataManagerLock.Lock)
            {
                BuildDataContext();
                Tuple<string, int, List<string>> result = (from R in context.Commands
                                                           where R.RepeatTimer > 0
                                                           select new Tuple<string, int, List<string>>(R.CmdName, R.RepeatTimer, new(R.Category))).FirstOrDefault();
                ClearDataContext();
                return result;
            }
        }

        public List<Tuple<string, int, List<string>>> GetTimerCommands()
        {
            lock (GUIDataManagerLock.Lock)
            {
                BuildDataContext();
                List<Tuple<string, int, List<string>>> result = new(from R in context.Commands
                                                                    where R.RepeatTimer > 0
                                                                    select new Tuple<string, int, List<string>>(R.CmdName, R.RepeatTimer, new(R.Category)));
                ClearDataContext();
                return result;
            }
        }

        public int GetTimerCommandTime(string Cmd)
        {
            lock (GUIDataManagerLock.Lock)
            {
                BuildDataContext();
                int result = (from R in context.Commands
                              where R.CmdName == Cmd
                              select R.RepeatTimer).FirstOrDefault();
                ClearDataContext();
                return result;
            }
        }

        public string GetUsage(string command)
        {
            lock (GUIDataManagerLock.Lock)
            {
                BuildDataContext();
                string result = (from C in context.Commands
                                 where C.CmdName == command
                                 select C.Usage).FirstOrDefault();
                ClearDataContext();
                return result;
            }
        }

        public LiveUser GetUser(string UserName)
        {
            lock (GUIDataManagerLock.Lock)
            {
                BuildDataContext();
                LiveUser result = (from U in context.Users
                                   where U.UserName == UserName
                                   select new LiveUser(U.UserName, U.Platform, U.UserId)).FirstOrDefault();
                ClearDataContext();
                return result;
            }
        }

        public string GetUserId(LiveUser User)
        {
            lock (GUIDataManagerLock.Lock)
            {
                BuildDataContext();
                var result = (from U in context.Users
                              where U.UserName == User.UserName
                              select U.UserId).FirstOrDefault();
                ClearDataContext();
                return result;
            }
        }

        public List<Tuple<bool, Uri>> GetWebhooks(WebhooksSource webhooksSource, WebhooksKind webhooks)
        {
            lock (GUIDataManagerLock.Lock)
            {
                BuildDataContext();
                List<Tuple<bool, Uri>> result = new(from W in context.Webhooks
                                                    where (W.WebhooksSource == webhooksSource && W.Kind == webhooks)
                                                    select new Tuple<bool, Uri>(W.AddEveryone, W.Webhook));
                ClearDataContext();
                return result;
            }
        }
        #endregion Get_Methods

        public object[] PerformQuery(Commands row, int Top = 0)
        {
            lock (GUIDataManagerLock.Lock)
            {
                BuildDataContext();

                IEnumerable<object> output = row.Table switch
                {
                    nameof(Currency) => (from C in context.Currency where C.CurrencyName == row.CurrencyField orderby C.UserName select new Tuple<object, object>(C[row.KeyField], C[row.DataField])),
                    nameof(Followers) => (from F in context.Followers orderby F.UserName select F),
                    nameof(UserStats) => (from US in context.UserStats orderby US.UserName select new Tuple<object, object>(US[row.KeyField], US[row.DataField])),
                    _ => [""]
                };

                if (row.Sort == CommandSort.DESC)
                {
                    output = output.OrderByDescending((o) => (o as Tuple<object, object>).Item1);
                }

                if (row.Top > 0)
                {
                    output = output.Take(row.Top);
                }

                ClearDataContext();
                return output.ToArray();
            }
        }

        public object PerformQuery(Commands row, string ParamValue)
        {
            lock (GUIDataManagerLock.Lock)
            {
                BuildDataContext();

                object output = row.Table switch
                {
                    nameof(Currency) => (from C in context.Currency where (C.UserName == ParamValue && C.CurrencyName == row.CurrencyField) select C[row.DataField ?? "Value"]).FirstOrDefault(),
                    nameof(CustomWelcome) => (from W in context.CustomWelcome where W.UserName == ParamValue select W[row.DataField]).FirstOrDefault(),
                    nameof(Followers) => (from F in context.Followers where F.UserName == ParamValue select F).FirstOrDefault(),
                    nameof(UserStats) => (from US in context.UserStats where US.UserName == ParamValue select US[row.DataField]).FirstOrDefault(),
                    nameof(Commands) => (from C in context.Commands where C.CmdName == ParamValue select C[row.DataField]).FirstOrDefault(),
                    _ => ""
                };

                if (row.Table == nameof(Followers))
                {
                    output = ((Followers)output).IsFollower ? ((Followers)output).FollowedDate : LocalizedMsgSystem.GetVar(Msg.MsgNotFollower);
                }

                ClearDataContext();
                return output;
            }
        }

        #region Post_Methods
        public bool PostCategory(string CategoryId, string newCategory)
        {
            bool found = false;
            if (string.IsNullOrEmpty(CategoryId) && string.IsNullOrEmpty(newCategory))
            {
                found = false;
            }
            else
            {
                lock (GUIDataManagerLock.Lock)
                {
                    BuildDataContext();
                    CategoryList categoryList = (from CL in context.CategoryList
                                                 where (CL.Category == FormatData.AddEscapeFormat(newCategory)) || CL.CategoryId == CategoryId
                                                 select CL).FirstOrDefault();
                    if (categoryList == default)
                    {
                        context.CategoryList.Add(new(categoryId: CategoryId, category: newCategory));
                        found = true;
                    }
                    else
                    {
                        categoryList.CategoryId ??= CategoryId;
                        categoryList.Category ??= newCategory;
                        if (OptionFlags.IsStreamOnline)
                        {
                            categoryList.StreamCount++;
                        }
                        found = true;
                    }
                    context.SaveChanges(true);
                }
            }
            ClearDataContext();
            return found;
        }

        public bool PostClip(int ClipId, DateTime CreatedAt, decimal Duration, string GameId, string Language, string Title, string Url)
        {
            lock (GUIDataManagerLock.Lock)
            {
                bool result;
                BuildDataContext();
                if (!(from C in context.Clips
                      where (C.ClipId == ClipId)
                      select C).Any())
                {
                    context.Clips.Add(new(clipId: ClipId, createdAt: CreatedAt, title: Title, categoryId: GameId, language: Language, duration: (float)Duration, url: Url));
                    context.SaveChanges(true);
                    result = true;
                }
                result = false;
                ClearDataContext();
                return result;
            }
        }

        public string PostCommand(string cmd, CommandParams Params)
        {
            lock (GUIDataManagerLock.Lock)
            {
                string result;
                BuildDataContext();
                if (!(from Com in (context.Commands.Union(context.CommandsUser))
                      where Com.CmdName == cmd
                      select Com).Any())
                {
                    context.CommandsUser.Add(new(cmdName: cmd, addMe: Params.AddMe, permission: Params.Permission,
                    isEnabled: Params.IsEnabled, message: Params.Message, repeatTimer: Params.Timer, sendMsgCount: Params.RepeatMsg, category: [Params.Category],
                    allowParam: Params.AllowParam, usage: Params.Usage, lookupData: Params.LookupData, table: Params.Table, keyField: GetKey(Params.Table),
                    dataField: Params.Field, currencyField: Params.Currency, unit: Params.Unit, action: Params.Action, top: Params.Top, sort: Params.Sort));

                    context.SaveChanges(true);
                    result = string.Format(CultureInfo.CurrentCulture, LocalizedMsgSystem.GetDefaultComMsg(DefaultCommand.addcommand), cmd);
                }
                result = string.Format(CultureInfo.CurrentCulture, LocalizedMsgSystem.GetVar(Msg.MsgAddCommandFailed), cmd);
                ClearDataContext();
                return result;
            }
        }

        /// <summary>
        /// Post a new currency type to the database. Will add new currency records for existing users.
        /// </summary>
        /// <param name="currencyType"></param>
        public void PostCurrencyType(Models.CurrencyType currencyType)
        {
            lock (GUIDataManagerLock.Lock)
            {
                BuildDataContext();

                if (!(from CT in context.CurrencyType where CT.CurrencyName == currencyType.CurrencyName select CT).Any())
                {
                    context.CurrencyType.Add(currencyType);

                    List<Models.CurrencyType> types = new(context.CurrencyType);

                    foreach (Users U in context.Users)
                    {
                        foreach (Models.CurrencyType t in types)
                        {
                            if (!(from C in context.Currency where (C.User == U && C.CurrencyName == t.CurrencyName) select C).Any())
                            {
                                context.Currency.Add(new(userName: U.UserName, value: 0, currencyName: t.CurrencyName));
                            }
                        }
                    }
                }

                context.SaveChanges(true);
                ClearDataContext();
            }
        }

        public void PostCurrencyUpdate(LiveUser User, double value, string CurrencyName)
        {
            lock (GUIDataManagerLock.Lock)
            {
                BuildDataContext();
                Currency currency = (from C in context.Currency
                                     where (C.CurrencyName == CurrencyName && C.UserName == User.UserName)
                                     select C).FirstOrDefault();
                if (currency != default)
                {
                    currency.Value = Math.Min(Math.Round(currency.Value + value, 2), currency.CurrencyType.MaxValue);
                }
                context.SaveChanges(true);
                ClearDataContext();
            }
        }

        public int PostDeathCounterUpdate(string currCategory, bool Reset = false, int updateValue = 1)
        {
            lock (GUIDataManagerLock.Lock)
            {
                BuildDataContext();
                GameDeadCounter update = (from G in context.GameDeadCounter where G.Category == currCategory select G).FirstOrDefault();
                if (update != default)
                {
                    if (Reset)
                    {
                        update.Counter = updateValue;
                    }
                    else
                    {
                        update.Counter += updateValue;
                    }
                }
                else
                {
                    update = context.GameDeadCounter.Add(new(category: currCategory)).Entity;
                }
                context.SaveChanges(true);
                ClearDataContext();
                return update?.Counter ?? 0;
            }
        }

        /// <summary>
        /// Add a new follower to the database.
        /// </summary>
        /// <param name="follow">The follower information to add to the database.</param>
        /// <returns>
        ///     <code>true</code>: first time follower; 
        ///     <code>false</code>: user previously followed.
        /// </returns>
        public bool PostFollower(Follow follow)
        {
            lock (GUIDataManagerLock.Lock)
            {
                BuildDataContext();
                followsQueue.Enqueue([follow]);
                PostFollowsQueue();

                var result = !(from F in context.Followers
                               where F.UserId == follow.FromUser.UserId && F.Platform == follow.FromUser.Platform
                               select F).Any();

                ClearDataContext();
                return result;
            }
        }

        private static bool ProcessFollowQueuestarted = false;

        /// <summary>
        /// Threaded database update to add followers.
        /// </summary>
        private void PostFollowsQueue()
        {
            if (!ProcessFollowQueuestarted)
            {
                ProcessFollowQueuestarted = true;

                ThreadManager.CreateThreadStart(() =>
                {
                    SystemsController.AppDispatcher.BeginInvoke(() =>
                    {
                        DateTime currTime = DateTime.Now.ToLocalTime();
                        //Thread.Sleep(1000); // wait some to stay inside while loop for lots of followers at one time
                        BuildDataContext();

                        while (followsQueue.TryDequeue(out IEnumerable<Follow> currUser))
                        {
                            lock (GUIDataManagerLock.Lock)
                            {
                                List<Followers> tempfollow = [];
                                foreach (Follow f in currUser)
                                {
                                    var user = PostNewUser(f.FromUser, f.FollowedAt);

                                    if (user.Followers != null)
                                    {
                                        user.Followers.IsFollower = true;
                                        user.Followers.FollowedDate = f.FollowedAt;
                                        user.Followers.Category ??= f.Category;
                                    }
                                    else
                                    {
                                        tempfollow.Add(new Followers(userId: f.FromUser.UserId,
                                                                            userName: f.FromUser.UserName, platform: f.FromUser.Platform,
                                                                            isFollower: true, followedDate: f.FollowedAt,
                                                                            statusChangeDate: f.FollowedAt, addDate: currTime,
                                                                            category: f.Category));
                                    }
                                }
                                context.Followers.AddRange(tempfollow);
                            }
                        }

                        NotifyDataCollectionUpdated(nameof(Followers));

                        lock (GUIDataManagerLock.Lock)
                        {
                            context.SaveChanges(true);
                        }
                        ClearDataContext();
                        ProcessFollowQueuestarted = false;

                        if (BulkFollowerUpdate)
                        {
                            StopBulkFollows();
                        }
                    });
                });
            }
        }

        public void PostGiveawayData(string DisplayName, DateTime dateTime)
        {
            lock (GUIDataManagerLock.Lock)
            {
                BuildDataContext();
                context.GiveawayUserData.Add(new(dateTime: dateTime, userName: DisplayName));
                context.SaveChanges(true);
                ClearDataContext();
            }
        }

        public void PostInRaidData(string user, DateTime time, int viewers, string gamename, Platform platform)
        {
            lock (GUIDataManagerLock.Lock)
            {
                BuildDataContext();
                context.InRaidData.Add(new(userName: user, raidDate: time, viewerCount: viewers, category: gamename, platform: platform));
                context.SaveChanges(true);
                ClearDataContext();
            }
        }

        public void PostLearnMsgsRow(string Message, MsgTypes MsgType)
        {
            lock (GUIDataManagerLock.Lock)
            {
                BuildDataContext();
                if (!(from M in context.LearnMsgs
                      where M.TeachingMsg == Message
                      select M).Any())
                {
                    context.LearnMsgs.Add(new(msgType: MsgType, teachingMsg: Message));
                    LearnMsgChanged = true;
                }
                context.SaveChanges(true);
                ClearDataContext();
            }
        }

        public bool PostMergeUserStats(string CurrUser, string SourceUser, Platform platform)
        {
            lock (GUIDataManagerLock.Lock)
            {
                BuildDataContext();
                IEnumerable<Currency> userCurrency = from uCu in context.Currency where uCu.UserName == CurrUser select uCu;
                IEnumerable<Currency> srcCurrency = from sCu in context.Currency where sCu.UserName == SourceUser select sCu;

                foreach ((Currency UC, Currency SC) in (from U in userCurrency
                                                        from S in srcCurrency
                                                        where U.CurrencyName == S.CurrencyName
                                                        select (U, S)))
                {
                    UC.Add(SC);
                }
                context.Currency.RemoveRange(srcCurrency);

                UserStats currUserstat = (from Cu in context.UserStats where (Cu.UserName == CurrUser && Cu.Platform == platform) select Cu).FirstOrDefault();
                UserStats sourceUser = (from Su in context.UserStats where (Su.UserName == SourceUser && Su.Platform == platform) select Su).FirstOrDefault();

                bool result;
                if (currUserstat != default && sourceUser != default)
                {
                    currUserstat += sourceUser;

                    context.UserStats.Remove(sourceUser);
                    context.SaveChanges(true);

                    result = true;
                }
                else
                {
                    result = false;
                }

                ClearDataContext();
                return result;
            }
        }

        public void PostNewAutoShoutUser(string UserName, string UserId, Platform platform)
        {
            lock (GUIDataManagerLock.Lock)
            {
                BuildDataContext();
                if (!(from SO in context.ShoutOuts where (SO.UserId == UserId && SO.UserName == UserName && SO.Platform == platform) select SO).Any())
                {
                    context.ShoutOuts.Add(new(userId: UserId, userName: UserName, platform: platform));
                }
                context.SaveChanges(true);
                ClearDataContext();
            }
        }

        private Users PostNewUser(LiveUser User, DateTime FirstSeen)
        {
            lock (GUIDataManagerLock.Lock)
            {
                Users newuser = (from U in context.Users where (U.UserName == User.UserName && U.Platform == User.Platform) select U).FirstOrDefault();
                if (newuser == default)
                {
                    newuser = context.Users.Add(new(userId: User.UserId, userName: User.UserName, platform: User.Platform, firstDateSeen: FirstSeen, currLoginDate: FirstSeen, lastDateSeen: FirstSeen)).Entity;
                }
                else
                {
                    newuser.UserId ??= User.UserId;
                    if (newuser.Platform == default) { newuser.Platform = User.Platform; }
                    if (newuser.UserName != User.UserName && newuser.UserId == User.UserId) { newuser.UserName = User.UserName; }
                }

                List<Models.CurrencyType> types = new(context.CurrencyType);

                foreach (Models.CurrencyType t in types)
                {
                    if (!(from UC in context.Currency where (UC.UserName == newuser.UserName && UC.CurrencyName == t.CurrencyName) select UC).Any())
                    {
                        context.Currency.Add(new(userName: newuser.UserName, value: 0, currencyName: t.CurrencyName));
                    }
                }

                if (!(from US in context.UserStats
                      where (US.UserId == User.UserId && US.UserName == User.UserName && US.Platform == User.Platform)
                      select US).Any())
                {
                    context.UserStats.Add(new(userId: User.UserId, userName: User.UserName, platform: User.Platform));
                }

                return newuser;
            }
        }

        public void PostOutgoingRaid(string HostedChannel, DateTime dateTime)
        {
            lock (GUIDataManagerLock.Lock)
            {
                BuildDataContext();
                context.OutRaidData.Add(new(channelRaided: HostedChannel, raidDate: dateTime));
                context.SaveChanges(true);
                ClearDataContext();
            }
        }

        public int PostQuote(string Text)
        {
            lock (GUIDataManagerLock.Lock)
            {
                BuildDataContext();
                List<Quotes> quotes = new(from Q in context.Quotes select Q);
                short opennum = (from Q in context.Quotes select Q.Number)
                    .IntersectBy(Enumerable.Range(1, quotes.Count > 0 ? quotes.Max((f) => f.Number) : 1), q => q).Min();

                context.Quotes.Add(new(number: opennum, quote: Text));
                context.SaveChanges(true);
                ClearDataContext();
                return opennum;
            }
        }

        /// <summary>
        /// Starts a new Stream record, if it doesn't currently exist.
        /// </summary>
        /// <param name="StreamStart">The time of stream start.</param>
        /// <returns><code>true: for posting a new stream start;</code> <code>false: when a stream start date row already exists</code></returns>
        public bool PostStream(DateTime StreamStart)
        {
            lock (GUIDataManagerLock.Lock)
            {
                BuildDataContext();
                bool addstream = !(from S in context.StreamStats where S.StreamStart == StreamStart select S).Any();
                if (addstream)
                {
                    context.StreamStats.Add(new(streamStart: StreamStart, streamEnd: StreamStart));
                    context.SaveChanges(true);
                }

                ClearDataContext();
                return addstream;
            }
        }

        public void PostStreamStat(StreamStat streamStat)
        {
            lock (GUIDataManagerLock.Lock)
            {
                BuildDataContext();
                StreamStats currStream = (from S in context.StreamStats where S.StreamStart == streamStat.StreamStart select S).FirstOrDefault();
                if (currStream != default)
                {
                    currStream.Update(streamStat);
                    context.SaveChanges(true);
                    ClearDataContext();
                }
            }
        }

        /// <summary>
        /// Add a custom welcome message for a specific user. Does not edit existing welcome message.
        /// </summary>
        /// <param name="User">The user to add the custom welcome message.</param>
        /// <param name="WelcomeMsg">The message for the user.</param>
        public void PostUserCustomWelcome(LiveUser User, string WelcomeMsg)
        {
            lock (GUIDataManagerLock.Lock)
            {
                BuildDataContext();
                if (!(from W in context.CustomWelcome where W.UserId == User.UserId select W).Any())
                {
                    context.CustomWelcome.Add(new(userId: User.UserId, userName: User.UserName, platform: User.Platform, message: WelcomeMsg));
                    context.SaveChanges(true);
                    ClearDataContext();
                }
            }
        }

        #endregion Post_Methods

        #region Clear DataBase Records 
        /// <summary>
        /// Clear all 'Followers' table records.
        /// </summary>
        public void RemoveAllFollowers()
        {
            lock (GUIDataManagerLock.Lock)
            {
                BuildDataContext();
                context.Followers.RemoveRange(context.Followers);
                context.SaveChanges(true);
                ClearDataContext();
            }
        }

        /// <summary>
        /// Clear all 'GiveawayUserData' table records.
        /// </summary>
        public void RemoveAllGiveawayData()
        {
            lock (GUIDataManagerLock.Lock)
            {
                BuildDataContext();
                context.GiveawayUserData.RemoveRange(context.GiveawayUserData);
                context.SaveChanges(true);
                ClearDataContext();
            }
        }

        /// <summary>
        /// Clear all 'InRaidData' table records.
        /// </summary>
        public void RemoveAllInRaidData()
        {
            lock (GUIDataManagerLock.Lock)
            {
                BuildDataContext();
                context.InRaidData.RemoveRange(context.InRaidData);
                context.SaveChanges(true);
                ClearDataContext();
            }
        }

        /// <summary>
        /// Clear all 'OutRaidData' table records.
        /// </summary>
        public void RemoveAllOutRaidData()
        {
            lock (GUIDataManagerLock.Lock)
            {
                BuildDataContext();
                context.OutRaidData.RemoveRange(context.OutRaidData);
                context.SaveChanges(true);
                ClearDataContext();
            }
        }

        /// <summary>
        /// Clear all 'OverlayTicker' table records.
        /// </summary>
        public void RemoveAllOverlayTickerData()
        {
            lock (GUIDataManagerLock.Lock)
            {
                BuildDataContext();
                context.OverlayTicker.RemoveRange(context.OverlayTicker);
                context.SaveChanges(true);
                ClearDataContext();
            }
        }

        /// <summary>
        /// Clear all 'StreamStats' table records.
        /// </summary>
        public void RemoveAllStreamStats()
        {
            lock (GUIDataManagerLock.Lock)
            {
                BuildDataContext();
                context.StreamStats.RemoveRange(context.StreamStats);
                context.SaveChanges(true);
                ClearDataContext();
            }
        }

        /// <summary>
        /// Clear all 'Users' table records.
        /// </summary>
        public void RemoveAllUsers()
        {
            lock (GUIDataManagerLock.Lock)
            {
                BuildDataContext();
                context.Users.RemoveRange(context.Users);
                context.SaveChanges(true);
                ClearDataContext();
            }
        }
        #endregion

        public bool RemoveCommand(string command)
        {
            lock (GUIDataManagerLock.Lock)
            {
                BuildDataContext();
                bool found = false;

                Commands cmd = (from C in context.Commands where C.CmdName == command select C).FirstOrDefault();
                if (cmd != default)
                {
                    context.Commands.Remove(cmd);
                    found = true;
                }
                context.SaveChanges(true);
                ClearDataContext();
                return found;
            }
        }

        public bool RemoveQuote(int QuoteNum)
        {
            lock (GUIDataManagerLock.Lock)
            {
                BuildDataContext();
                bool found = false;

                Quotes quotes = (from Q in context.Quotes where Q.Number == QuoteNum select Q).FirstOrDefault();
                if (quotes != default)
                {
                    context.Quotes.Remove(quotes);
                    found = true;
                }
                context.SaveChanges(true);
                ClearDataContext();
                return found;
            }
        }

        #region Set_IsEnabled Methods
        public void SetBuiltInCommandsEnabled(bool Enabled)
        {
            lock (GUIDataManagerLock.Lock)
            {
                BuildDataContext();
                foreach (var Command in (from C in context.Commands
                                         join D in
                                             (from def in Enum.GetNames<DefaultCommand>().Union(Enum.GetNames<DefaultSocials>().ToList())
                                              select def) on C.CmdName equals D into DefCmds
                                         from DC in DefCmds.DefaultIfEmpty()
                                         where DC != null
                                         orderby C.CmdName
                                         select C))
                {
                    Command.IsEnabled = Enabled;
                }
                NotifyDataCollectionUpdated(nameof(Commands));
                context.SaveChanges(true);
                ClearDataContext();
            }
        }

        public void SetWebhooksEnabled(bool Enabled)
        {
            lock (GUIDataManagerLock.Lock)
            {
                BuildDataContext();

                foreach (var webhook in context.Webhooks)
                {
                    webhook.IsEnabled = Enabled;
                }
                NotifyDataCollectionUpdated(nameof(Webhooks));
                context.SaveChanges(true);
                ClearDataContext();
            }
        }

        public void SetIsEnabled(IEnumerable<DataRow> dataRows, bool IsEnabled = false)
        {
            throw new NotImplementedException();
        }

        public void SetSystemEventsEnabled(bool Enabled)
        {
            lock (GUIDataManagerLock.Lock)
            {
                BuildDataContext();
                foreach (var Sys in context.ChannelEvents)
                {
                    Sys.IsEnabled = Enabled;
                }
                NotifyDataCollectionUpdated(nameof(ChannelEvents));
                context.SaveChanges(true);
                ClearDataContext();
            }
        }

        /// <summary>
        /// Sets the 'IsEnabled' column for all records of the Commands table, specifically the user created commands (not the default commands).
        /// </summary>
        /// <param name="Enabled">The value to set for 'IsEnabled'.</param>
        public void SetUserDefinedCommandsEnabled(bool Enabled)
        {
            lock (GUIDataManagerLock.Lock)
            {
                BuildDataContext();
                foreach (var Command in (from C in context.Commands
                                         join D in (from def in Enum.GetNames<DefaultCommand>().Union(Enum.GetNames<DefaultSocials>().ToList())
                                                    select def) on C.CmdName equals D into UsrCmds
                                         from UC in UsrCmds.DefaultIfEmpty()
                                         where UC == null
                                         orderby C.CmdName
                                         select C))
                {
                    Command.IsEnabled = Enabled;
                }
                NotifyDataCollectionUpdated(nameof(CommandsUser));
                context.SaveChanges(true);
                ClearDataContext();
            }
        }

        #endregion     

        public void StartBulkFollowers()
        {
            lock (GUIDataManagerLock.Lock)
            {
                BulkFollowerUpdate = true;
                BuildDataContext();
                foreach (Followers F in context.Followers)
                {
                    F.IsFollower = false; // reset all followers to not following, add existing followers back as followers
                }
                context.SaveChanges(true);
                ClearDataContext();
            }
        }

        private void StopBulkFollows()
        {
            lock (GUIDataManagerLock.Lock)
            {
                DateTime currtime = DateTime.Now.ToLocalTime();
                BuildDataContext();

                if (OptionFlags.TwitchPruneNonFollowers)
                {
                    context.Followers.RemoveRange(from R in context.Followers where !R.IsFollower select R);
                }
                else // if pruning followers, there won't be multiple 'UserId' records
                {
                    List<string> FollowUserIds = [];
                    FollowUserIds.UniqueAddRange(from f in context.Followers select f.UserId);

                    foreach (string Id in FollowUserIds)
                    {
                        var UF = (from F in context.Followers where F.UserId == Id orderby F.AddDate descending select F);

                        if (UF.Count() == 1 && UF.First().StatusChangeDate != UF.First().FollowedDate)
                        {
                            UF.First().StatusChangeDate = UF.First().FollowedDate;
                        }
                        else if (UF.Count() > 1)
                        { // adjust the status change date to the current time
                            List<Followers> currUser = new(UF.Take(2));

                            if (currUser[0].StatusChangeDate != currUser[1].StatusChangeDate)
                            { // only change when the dates are different, show user which records change
                                currUser[0].StatusChangeDate = currtime;
                                currUser[1].StatusChangeDate = currtime;
                            }
                        }
                    }
                }
                OnBulkFollowersAddFinished?.Invoke(this, new(GetNewestFollower()));
                BulkFollowerUpdate = false;
                context.SaveChanges(true);
                ClearDataContext();
            }
        }


        public void UpdateCurrency(List<string> Users, DateTime dateTime)
        {
            lock (GUIDataManagerLock.Lock)
            {
                BuildDataContext();
                foreach (string U in Users)
                {
                    UpdateCurrency((from user in context.Users where user.UserName == U select user).FirstOrDefault(), dateTime);
                }
                context.SaveChanges(true);
                ClearDataContext();
            }
        }

        private void UpdateCurrency(Users User, DateTime CurrTime)
        {
            lock (GUIDataManagerLock.Lock)
            {
                TimeSpan clock = CurrTime - User.LastDateSeen;
                foreach (Currency currency in User.Currency)
                {
                    currency.Value =
                        Math.Min(
                            currency.CurrencyType.MaxValue,
                            Math.Round((currency.Value + currency.CurrencyType.AccrueAmt) * (clock.TotalSeconds / currency.CurrencyType.Seconds), 2)
                        );
                }
                User.LastDateSeen = CurrTime;
            }
        }

        public void UpdateFollowers(IEnumerable<Follow> follows)
        {
            if (follows.Any())
            {
                lock (followsQueue)
                {
                    followsQueue.Enqueue(follows);
                    PostFollowsQueue();
                }
            }
        }

        public List<LearnMsgRecord> UpdateLearnedMsgs()
        {
            lock (GUIDataManagerLock.Lock)
            {
                List<LearnMsgRecord> result;
                BuildDataContext();
                if (LearnMsgChanged)
                {
                    LearnMsgChanged = false;
                    result = new(from L in context.LearnMsgs
                                 select new LearnMsgRecord(L.Id, L.MsgType.ToString(), L.TeachingMsg));
                }
                else
                {
                    result = null;
                }
                ClearDataContext();
                return result;
            }
        }

        public void UpdateOverlayTicker(OverlayTickerItem item, string name)
        {
            lock (GUIDataManagerLock.Lock)
            {
                BuildDataContext();
                OverlayTicker ticker = (from T in context.OverlayTicker where T.TickerName == item select T).FirstOrDefault();
                if (ticker == default)
                {
                    context.OverlayTicker.Add(new(tickerName: item, userName: name));
                }
                else
                {
                    ticker.UserName = name;
                }
                context.SaveChanges(true);
                ClearDataContext();
            }
        }

        public void UpdateWatchTime(List<LiveUser> Users, DateTime CurrTime)
        {
            lock (GUIDataManagerLock.Lock)
            {
                BuildDataContext();

                foreach (LiveUser L in Users)
                {
                    UserStats stats = (from S in context.UserStats
                                       where (S.UserId == L.UserId && S.UserName == L.UserName && S.Platform == L.Platform)
                                       select S).FirstOrDefault();

                    if (stats == default)
                    {
                        stats = context.UserStats.Add(new(userId: L.UserId, userName: L.UserName, platform: L.Platform)).Entity;
                    }

                    if (stats.Users.LastDateSeen < CurrStreamStart)
                    {
                        stats.Users.LastDateSeen = CurrStreamStart;
                    }

                    if (CurrTime > stats.Users.LastDateSeen && CurrTime > CurrStreamStart)
                    {
                        stats.WatchTime = stats.WatchTime.Add(CurrTime - stats.Users.LastDateSeen);
                    }
                }
                NotifyDataCollectionUpdated(nameof(Models.UserStats));
                context.SaveChanges(true);
                ClearDataContext();
            }
        }

        public void UpdateWatchTime(LiveUser User, DateTime CurrTime)
        {
            UpdateWatchTime([User], CurrTime);
        }

        /// <summary>
        /// Adds a new user to the Users table; updates active dates to <paramref name="NowSeen"/>.
        /// </summary>
        /// <param name="User">The user to add in database and update.</param>
        /// <param name="NowSeen">The reported date & time of the user.</param>
        public void UserJoined(LiveUser User, DateTime NowSeen)
        {
            lock (GUIDataManagerLock.Lock)
            {
                BuildDataContext();
                Users user = PostNewUser(User, NowSeen);
                user.CurrLoginDate = NowSeen;
                user.LastDateSeen = NowSeen;
                NotifyDataCollectionUpdated(nameof(Models.Users));
                context.SaveChanges(true);
                ClearDataContext();
            }
        }

        /// <summary>
        /// Adds a collection of new users to the Users table; updates active dates to <paramref name="NowSeen"/>.
        /// </summary>
        /// <param name="Users">The user to add in database and update.</param>
        /// <param name="NowSeen">The reported date & time of the user.</param>
        public void UserJoined(IEnumerable<LiveUser> Users, DateTime NowSeen)
        {
            lock (GUIDataManagerLock.Lock)
            {
                BuildDataContext();
                foreach (var L in Users)
                {
                    Users user = PostNewUser(L, NowSeen);
                    user.CurrLoginDate = NowSeen;
                    user.LastDateSeen = NowSeen;
                }
                NotifyDataCollectionUpdated(nameof(Models.Users));
                context.SaveChanges(true);
                ClearDataContext();
            }
        }

        public void UserLeft(LiveUser User, DateTime LastSeen)
        {
            lock (GUIDataManagerLock.Lock)
            {
                BuildDataContext();
                Users user = (from U in context.Users where (U.UserName == User.UserName && U.Platform == User.Platform) select U).FirstOrDefault();
                if (user != default)
                {
                    UpdateWatchTime(User, LastSeen);
                    if (OptionFlags.CurrencyStart && (OptionFlags.CurrencyOnline && OptionFlags.IsStreamOnline))
                    {
                        UpdateCurrency(user, LastSeen);
                    }
                    context.SaveChanges(true);
                }
                ClearDataContext();
            }
        }

        #region MultiLive data
        public event EventHandler UpdatedMonitoringChannels;
        public ObservableCollection<ArchiveMultiStream> CleanupList { get; } = [];
        private bool IsLiveStreamUpdated = false;
        public string MultiLiveStatusLog { get; private set; } = "";

        public bool CheckMultiStreamDate(string ChannelName, Platform platform, DateTime dateTime)
        {
            lock (GUIDataManagerLock.Lock)
            {
                BuildDataContext();
                var result = (from P in context.MultiLiveStreams where (P.UserName == ChannelName && P.Platform == platform && P.LiveDate == dateTime) select P).Count() > 1;
                ClearDataContext();
                return result;
            }
        }

        public bool GetMultiChannelName(string UserName, Platform platform)
        {
            lock (GUIDataManagerLock.Lock)
            {
                BuildDataContext();
                var result = (from M in context.MultiChannels where (M.UserName == UserName && M.Platform == platform) select M).Any();
                ClearDataContext();
                return result;
            }
        }

        /// <summary>
        /// The user selects channels to monitor, get all of the channel Ids for the selected channels.
        /// </summary>
        /// <param name="platform">The platform to retrieve</param>
        /// <returns>A list of monitored UserIds for the provided platform.</returns>
        public List<string> GetMultiChannelNames(Platform platform)
        {
            lock (GUIDataManagerLock.Lock)
            {
                BuildDataContext();
                List<string> result = new(from M in context.MultiChannels where M.Platform == platform select M.UserId);
                ClearDataContext();
                return result;
            }
        }

        public List<Tuple<WebhooksSource, Uri>> GetMultiWebHooks()
        {
            lock (GUIDataManagerLock.Lock)
            {
                BuildDataContext();
                List<Tuple<WebhooksSource, Uri>> result = new(from W in context.MultiMsgEndPoints where W.IsEnabled select new Tuple<WebhooksSource, Uri>(W.WebhooksSource, W.Webhook));
                ClearDataContext();
                return result;
            }
        }

        public void PostMonitorChannel(IEnumerable<LiveUser> liveUsers)
        {
            lock (GUIDataManagerLock.Lock)
            {
                BuildDataContext();
                foreach (LiveUser U in liveUsers)
                {
                    if ((from L in context.MultiChannels
                         where (L.UserName == U.UserName && L.UserId == U.UserId && L.Platform == U.Platform)
                         select L).Any())
                    {
                        context.MultiChannels.Add(new(userId: U.UserId, userName: U.UserName, platform: U.Platform));
                    }
                }

                context.SaveChanges(true);
                UpdatedMonitoringChannels?.Invoke(this, new());
                ClearDataContext();
            }
        }

        public bool PostMultiStreamDate(string userid, string username, Platform platform, DateTime onDate)
        {
            lock (GUIDataManagerLock.Lock)
            {
                BuildDataContext();
                bool result = (from P in context.MultiLiveStreams where (P.UserId == userid && P.UserName == username && P.LiveDate == onDate) select P).Any();
                if (!result)
                {
                    context.MultiLiveStreams.Add(new(userId: userid, userName: username, platform: platform, liveDate: onDate));
                    context.SaveChanges(true);
                }
                ClearDataContext();

                return !result;
            }
        }

        public void SummarizeStreamData()
        {
            if (IsLiveStreamUpdated || CleanupList.Count == 0) // only perform if flag for update occurs
            {
                lock (GUIDataManagerLock.Lock)
                {
                    BuildDataContext();
                    CleanupList.Clear();

                    List<DateTime> AllDates = new(from ML in context.MultiLiveStreams select ML.LiveDate);
                    List<DateTime> UniqueDates = new(AllDates.Intersect(AllDates));

                    foreach (var A in (from M in UniqueDates.Select(uniqueDate => new ArchiveMultiStream()
                    {
                        ThroughDate = uniqueDate,
                        StreamCount = (int)(from DateTime dates in AllDates
                                            where dates.Date <= uniqueDate
                                            select dates).Count()
                    })
                                       select M))
                    {
                        CleanupList.Add(A);
                    }

                    IsLiveStreamUpdated = false; // reset update flag indicator
                    ClearDataContext();
                }
            }
        }

        public void SummarizeStreamData(ArchiveMultiStream archiveRecord)
        {
            lock (GUIDataManagerLock.Lock)
            {
                BuildDataContext();
                var multiLiveStreams = (from MLS in context.MultiLiveStreams
                                        where MLS.LiveDate <= archiveRecord.ThroughDate.Date
                                        group MLS by
                                        new
                                        {
                                            name = MLS.UserName,
                                            date = MLS.LiveDate,
                                            platform = MLS.Platform,
                                            userid = MLS.UserId
                                        } into GroupedMLS
                                        select new ArchiveMultiStream()
                                        {
                                            Name = new(GroupedMLS.Key.name, GroupedMLS.Key.platform, GroupedMLS.Key.userid),
                                            ThroughDate = GroupedMLS.MaxBy(G => GroupedMLS.Key.date).LiveDate,
                                            StreamCount = (int)GroupedMLS.Count()
                                        });

                foreach (var GroupedStreams in
                    (from archive in multiLiveStreams
                     join sumlive in context.MultiSummaryLiveStreams on archive.Name.UserName equals sumlive.UserName into GroupedSum
                     from G in GroupedSum.DefaultIfEmpty()
                     select new { username = archive.Name, livestreamrow = archive, sumrow = G }))
                {
                    if (GroupedStreams != default)
                    {
                        GroupedStreams.sumrow.ThroughDate = GroupedStreams.livestreamrow.ThroughDate;
                        GroupedStreams.sumrow.StreamCount += GroupedStreams.livestreamrow.StreamCount;
                    }
                    else
                    {
                        context.MultiSummaryLiveStreams.Add(new(
                            userId: GroupedStreams.sumrow.UserId, userName: GroupedStreams.sumrow.UserName,
                            streamCount: GroupedStreams.livestreamrow.StreamCount, throughDate: GroupedStreams.livestreamrow.ThroughDate));
                    }
                }

                context.MultiLiveStreams.RemoveRange((from MLS in context.MultiLiveStreams
                                                      where MLS.LiveDate <= archiveRecord.ThroughDate.Date
                                                      select MLS));

                IsLiveStreamUpdated = true;

                CleanupList.Clear();
                context.SaveChanges(true);
                SummarizeStreamData();

                ClearDataContext();
            }
        }

        #endregion

        private void LearnMsgs_LearnMsgsRowDeleted(object sender, EventArgs e)
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.DataManager, $"Machine learning, whether learned message rows are deleted.");

            LearnMsgChanged = true;
        }

        private void LearnMsgs_TableNewRow(object sender, DataTableNewRowEventArgs e)
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.DataManager, $"Machine learning, whether adding a new learned message.");

            LearnMsgChanged = true;
        }

        #region Test Method Verification

        /// <summary>
        /// Provides check for test code; checks if there's a record for the provided channel, date, viewer count, and game name.
        /// </summary>
        /// <param name="user">The user bringing in the raid.</param>
        /// <param name="time">The time the user raided.</param>
        /// <param name="viewers">The viewers they brought.</param>
        /// <param name="gamename">The category the raiding stream had at the raid time.</param>
        /// <returns><code>true: when record is found.</code>
        /// <code>false: when record is not found.</code></returns>
        public bool TestInRaidData(string user, DateTime time, string viewers, string gamename)
        {
            lock (GUIDataManagerLock.Lock)
            {
                BuildDataContext();
                var result = (from I in context.InRaidData
                              where (I.UserName == user && I.RaidDate == time && I.ViewerCount.ToString() == viewers && I.Category == gamename)
                              select I).Any();
                ClearDataContext();
                return result;
            }
        }

        /// <summary>
        /// Provides check for test code; checks if there's a record for the provided channel and date.
        /// </summary>
        /// <param name="HostedChannel">The channel raided.</param>
        /// <param name="dateTime">The date & time of the raid.</param>
        /// <returns><code>true: when record is found.</code>
        /// <code>false: when record is not found.</code></returns>
        public bool TestOutRaidData(string HostedChannel, DateTime dateTime)
        {
            lock (GUIDataManagerLock.Lock)
            {
                BuildDataContext();
                var result = (from O in context.OutRaidData
                              where (O.ChannelRaided == HostedChannel && O.RaidDate == dateTime)
                              select O).Any();
                ClearDataContext();
                return result;
            }
        }

        #endregion
    }
}
