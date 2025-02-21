using Microsoft.EntityFrameworkCore;

using StreamerBotLib.DataSQL.Models;
using StreamerBotLib.Enums;
using StreamerBotLib.Interfaces;
using StreamerBotLib.Models;
using StreamerBotLib.Static;

using System.Collections.ObjectModel;

namespace StreamerBotLib.DataSQL
{
    internal partial class DataManagerSQLAsync
    {
        #region DataManager TableViews
        private List<Users> UsersList { get; set; }
        private List<Followers> FollowersList { get; set; }
        private List<CommandsUser> CommandsUserList { get; set; }
        private List<Commands> CommandsList { get; set; }
        private List<BanRules> BanRulesList { get; set; }
        private List<Models.BanReasons> BanReasonsList { get; set; }
        private static List<CategoryList> CategoryListList { get; set; }
        private List<ChannelEvents> ChannelEventsList { get; set; }
        private List<Clips> ClipsList { get; set; }
        private List<Currency> CurrencyList { get; set; }
        private List<Models.CurrencyType> CurrencyTypeList { get; set; }
        private List<CustomWelcome> CustomWelcomeList { get; set; }
        private List<GameDeadCounter> GameDeadCounterList { get; set; }
        private List<GiveawayUserData> GiveawayUserDataList { get; set; }
        private List<InRaidData> InRaidDataList { get; set; }
        private List<LearnMsgs> LearnMsgsList { get; set; }
        private List<ModeratorApprove> ModeratorApproveList { get; set; }
        private List<OldFollowUsers> OldFollowUsersList { get; set; }
        private List<OutRaidData> OutRaidDataList { get; set; }
        private List<OverlayServices> OverlayServicesList { get; set; }
        private List<OverlayTicker> OverlayTickerList { get; set; }
        private List<Quotes> QuotesList { get; set; }
        private List<ShoutOuts> ShoutOutsList { get; set; }
        private List<StreamStats> StreamStatsList { get; set; }
        private List<UserStats> UserStatsList { get; set; }
        private List<Webhooks> WebhooksList { get; set; }
        #region MultiLive Collections
        private List<MultiWebhooks> MultiWebhooksList { get; set; }
        private List<MultiChannels> MultiChannelsList { get; set; }
        private List<MultiLiveStreams> MultiLiveStreamsList { get; set; }
        private List<MultiSummaryLiveStreams> MultiSummaryLiveStreamsList { get; set; }
        #endregion
        #endregion


        #region LocalView List

        private readonly SQLDBContext GUIContext;

        internal object GetICollection(DataTables dataTable)
        {
            LogWriter.DebugLog("GetList", DebugLogTypes.DataManager,
                $"Getting the observable collection for {dataTable}.");

            return dataTable switch
            {
                DataTables.BanReasons => GetBanReasonsLocalCollectionAsync().Result,
                DataTables.BanRules => GetBanRulesLocalCollectionAsync().Result,
                DataTables.CategoryList => GetCategoryListLocalCollectionAsync().Result,
                DataTables.ChannelEvents => GetChannelEventsLocalCollectionAsync().Result,
                DataTables.Clips => GetClipsLocalCollectionAsync().Result,
                DataTables.Commands => GetCommandsLocalCollectionAsync().Result,
                DataTables.CommandsBase => throw new NotImplementedException(),
                DataTables.CommandsUser => GetCommandsUserLocalCollectionAsync().Result,
                DataTables.Currency => GetCurrencyLocalCollectionAsync().Result,
                DataTables.CurrencyType => GetCurrencyTypeLocalCollectionAsync().Result,
                DataTables.CustomWelcome => GetCustomWelcomeLocalCollectionAsync().Result,
                DataTables.Followers => GetFollowersLocalCollectionAsync().Result,
                DataTables.GameDeadCounter => GetGameDeadCounterLocalCollectionAsync().Result,
                DataTables.GiveawayUserData => GetGiveawayUserDataLocalCollectionAsync().Result,
                DataTables.InRaidData => GetInRaidDataLocalCollectionAsync().Result,
                DataTables.LearnMsgs => GetLearnMsgsLocalCollectionAsync().Result,
                DataTables.ModeratorApprove => GetModeratorApproveLocalCollectionAsync().Result,
                DataTables.MultiChannels => GetMultiChannelsLocalCollectionAsync().Result,
                DataTables.MultiLiveStreams => GetMultiLiveStreamsLocalCollectionAsync().Result,
                DataTables.MultiSummaryLiveStreams => GetMultiSummaryLiveStreamsLocalCollectionAsync().Result,
                DataTables.MultiWebhooks => GetMultiWebhooksLocalCollectionAsync().Result,
                DataTables.OldFollowUsers => GetOldFollowUsersLocalCollectionAsync().Result,
                DataTables.OutRaidData => GetOutRaidDataLocalCollectionAsync().Result,
                DataTables.OverlayServices => GetOverlayServicesLocalCollectionAsync().Result,
                DataTables.OverlayTicker => GetOverlayTickerLocalCollectionAsync().Result,
                DataTables.Quotes => GetQuotesLocalCollectionAsync().Result,
                DataTables.ShoutOuts => GetShoutOutsLocalCollectionAsync().Result,
                DataTables.StreamStats => GetStreamStatsLocalCollectionAsync().Result,
                DataTables.UserBase => throw new NotImplementedException(),
                DataTables.Users => GetUsersLocalCollectionAsync().Result,
                DataTables.UserStats => GetUserStatsLocalCollectionAsync().Result,
                DataTables.Webhooks => GetWebhooksLocalCollectionAsync().Result,
                DataTables.WebhooksBase => throw new NotImplementedException(),
                _ => throw new NotImplementedException()
            };
        }

        private Task<List<Models.BanReasons>> GetBanReasonsLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetBanReasonsLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading BanReasons data into the database context.");

                await GUIContext.BanReasons.LoadAsync();
                BanReasonsList = GUIContext.BanReasons.Local.ToList();
                return BanReasonsList;
            });
        }

        private Task<List<BanRules>> GetBanRulesLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetBanRulesLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading BanRules data into the database context.");
                await GUIContext.BanRules.LoadAsync();
                BanRulesList = GUIContext.BanRules.Local.ToList();
                return BanRulesList;
            });
        }

        private Task<List<CategoryList>> GetCategoryListLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetCategoryListLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading CategoryList data into the database context.");
                await GUIContext.CategoryList.LoadAsync();
                ThreadManager.AddTaskToGUIDispatcher(() =>
                {
                    CategoryListList = GUIContext.CategoryList.Local.ToList();
                });
                return CategoryListList;
            });
        }

        private Task<List<ChannelEvents>> GetChannelEventsLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetChannelEventsLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading ChannelEvents data into the database context.");
                await GUIContext.ChannelEvents.LoadAsync();
                ThreadManager.AddTaskToGUIDispatcher(() =>
                {
                    ChannelEventsList = GUIContext.ChannelEvents.Local.ToList();
                });
                return ChannelEventsList;
            });
        }

        private Task<List<Clips>> GetClipsLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetClipsLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading Clips data into the database context.");
                await GUIContext.Clips.LoadAsync();
                ClipsList = GUIContext.Clips.Local.ToList();
                return ClipsList;
            });
        }

        private Task<List<Commands>> GetCommandsLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetCommandsLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading Commands data into the database context.");
                await GUIContext.Commands.LoadAsync();
                CommandsList = GUIContext.Commands.Local.ToList();
                return CommandsList;
            });
        }

        private Task<List<CommandsUser>> GetCommandsUserLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetCommandsUserLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading CommandsUser data into the database context.");
                await GUIContext.CommandsUser.LoadAsync();
                CommandsUserList = GUIContext.CommandsUser.Local.ToList();
                return CommandsUserList;
            });
        }

        private Task<List<Currency>> GetCurrencyLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetCurrencyLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading Currency data into the database context.");
                await GUIContext.Currency.LoadAsync();
                CurrencyList = GUIContext.Currency.Local.ToList();
                return CurrencyList;
            });
        }

        private Task<List<Models.CurrencyType>> GetCurrencyTypeLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetCurrencyTypeLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading CurrencyType data into the database context.");
                await GUIContext.CurrencyType.LoadAsync();
                CurrencyTypeList = GUIContext.CurrencyType.Local.ToList();
                return CurrencyTypeList;
            });
        }

        private Task<List<CustomWelcome>> GetCustomWelcomeLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetCustomWelcomeLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading CustomWelcome data into the database context.");
                await GUIContext.CustomWelcome.LoadAsync();
                CustomWelcomeList = GUIContext.CustomWelcome.Local.ToList();
                return CustomWelcomeList;
            });
        }

        private Task<List<Followers>> GetFollowersLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetFollowersLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading Followers data into the database context.");
                await GUIContext.Followers.LoadAsync();
                FollowersList = GUIContext.Followers.Local.ToList();
                return FollowersList;
            });
        }

        private Task<List<GameDeadCounter>> GetGameDeadCounterLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetGameDeadCounterLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading GameDeadCounter data into the database context.");

                await GUIContext.GameDeadCounter.LoadAsync();
                GameDeadCounterList = GUIContext.GameDeadCounter.Local.ToList();
                return GameDeadCounterList;
            });
        }

        private Task<List<GiveawayUserData>> GetGiveawayUserDataLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetGiveawayUserDataLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading GiveawayUserData data into the database context.");

                await GUIContext.GiveawayUserData.LoadAsync();
                GiveawayUserDataList = GUIContext.GiveawayUserData.Local.ToList();
                return GiveawayUserDataList;
            });
        }

        private Task<List<InRaidData>> GetInRaidDataLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetInRaidDataLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading InRaidData data into the database context.");
                await GUIContext.InRaidData.LoadAsync();
                InRaidDataList = GUIContext.InRaidData.Local.ToList();
                return InRaidDataList;
            });
        }

        private Task<List<LearnMsgs>> GetLearnMsgsLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetLearnMsgsLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading LearnMsgs data into the database context.");
                await GUIContext.LearnMsgs.LoadAsync();
                LearnMsgsList = GUIContext.LearnMsgs.Local.ToList();
                return LearnMsgsList;
            });
        }

        private Task<List<ModeratorApprove>> GetModeratorApproveLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetModeratorApproveLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading ModeratorApprove data into the database context.");
                await GUIContext.ModeratorApprove.LoadAsync();
                ModeratorApproveList = GUIContext.ModeratorApprove.Local.ToList();
                return ModeratorApproveList;
            });
        }

        private Task<List<MultiChannels>> GetMultiChannelsLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetMultiChannelsLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading MultiChannels data into the database context.");
                await GUIContext.MultiChannels.LoadAsync();
                MultiChannelsList = GUIContext.MultiChannels.Local.ToList();
                return MultiChannelsList;
            });
        }

        private Task<List<MultiLiveStreams>> GetMultiLiveStreamsLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetMultiLiveStreamsLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading MultiLiveStreams data into the database context.");
                await GUIContext.MultiLiveStreams.LoadAsync();
                MultiLiveStreamsList = GUIContext.MultiLiveStreams.Local.ToList();
                return MultiLiveStreamsList;
            });
        }

        private Task<List<MultiWebhooks>> GetMultiWebhooksLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetMultiWebhooksLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading MultiWebhooks data into the database context.");
                await GUIContext.MultiWebhooks.LoadAsync();
                MultiWebhooksList = GUIContext.MultiWebhooks.Local.ToList();
                return MultiWebhooksList;
            });
        }

        private Task<List<MultiSummaryLiveStreams>> GetMultiSummaryLiveStreamsLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetMultiSummaryLiveStreamsLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading MultiSummaryLiveStreams data into the database context.");
                await GUIContext.MultiSummaryLiveStreams.LoadAsync();
                MultiSummaryLiveStreamsList = GUIContext.MultiSummaryLiveStreams.Local.ToList();
                return MultiSummaryLiveStreamsList;
            });
        }

        private Task<List<OldFollowUsers>> GetOldFollowUsersLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetOldFollowUsersLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading OldFollowUsers data into the database context.");
                await GUIContext.OldFollowUsers.LoadAsync();
                OldFollowUsersList = GUIContext.OldFollowUsers.Local.ToList();
                return OldFollowUsersList;
            });
        }

        private Task<List<OutRaidData>> GetOutRaidDataLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetOutRaidDataLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading OutRaidData data into the database context.");
                await GUIContext.OutRaidData.LoadAsync();
                OutRaidDataList = GUIContext.OutRaidData.Local.ToList();
                return OutRaidDataList;
            });
        }

        private Task<List<OverlayServices>> GetOverlayServicesLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetOverlayServicesLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading OverlayServices data into the database context.");
                await GUIContext.OverlayServices.LoadAsync();
                OverlayServicesList = GUIContext.OverlayServices.Local.ToList();
                return OverlayServicesList;
            });
        }

        private Task<List<OverlayTicker>> GetOverlayTickerLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetOverlayTickerLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading OverlayTicker data into the database context.");
                await GUIContext.OverlayTicker.LoadAsync();
                OverlayTickerList = GUIContext.OverlayTicker.Local.ToList();
                return OverlayTickerList;
            });
        }

        private Task<List<Quotes>> GetQuotesLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetQuotesLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading Quotes data into the database context.");
                await GUIContext.Quotes.LoadAsync();
                QuotesList = GUIContext.Quotes.Local.ToList();
                return QuotesList;
            });
        }

        private Task<List<ShoutOuts>> GetShoutOutsLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetShoutOutsLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading ShoutOuts data into the database context.");
                await GUIContext.ShoutOuts.LoadAsync();
                ShoutOutsList = GUIContext.ShoutOuts.Local.ToList();
                return ShoutOutsList;
            });
        }

        private Task<List<StreamStats>> GetStreamStatsLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetStreamStatsLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading StreamStats data into the database context.");
                await GUIContext.StreamStats.LoadAsync();
                StreamStatsList = GUIContext.StreamStats.Local.ToList();
                return StreamStatsList;
            });
        }

        private Task<List<Users>> GetUsersLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetUsersLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading Users data into the database context.");
                await GUIContext.Users.LoadAsync();
                UsersList = GUIContext.Users.Local.ToList();
                return UsersList;
            });
        }

        private Task<List<UserStats>> GetUserStatsLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetUserStatsLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading UserStats data into the database context.");
                await GUIContext.UserStats.LoadAsync();
                UserStatsList = GUIContext.UserStats.Local.ToList();
                return UserStatsList;
            });
        }

        private Task<List<Webhooks>> GetWebhooksLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetWebhooksLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading Webhooks data into the database context.");
                await GUIContext.Webhooks.LoadAsync();
                WebhooksList = GUIContext.Webhooks.Local.ToList();
                return WebhooksList;
            });
        }

        #endregion

        internal void NotifyDataCollectionUpdated(string TableName)
        {
            LogWriter.DebugLog("NotifyDataCollectionUpdated", DebugLogTypes.DataManager,
                $"Notifying the DataCollection is Updated for {TableName}.");
            OnDataCollectionUpdated?.Invoke(this, new(TableName));
        }

        #region Refresh Collections
        private void RefreshBanReasonsList()
        {
            LogWriter.DebugLog("RefreshBanReasonsList", DebugLogTypes.DataManager,
                $"Reloading BanReasons data into the database context.");
            PostActionQueue(new Task(async () =>
            {
                await GUIContext.BanReasons.LoadAsync();
                ThreadManager.AddTaskToGUIDispatcher(() => { BanReasonsList = GUIContext.BanReasons.Local.ToList(); });
                NotifyDataCollectionUpdated(nameof(GUIContext.BanReasons));
            }), "RefreshBanReasonsList");
        }

        private void RefreshBanRulesList()
        {
            LogWriter.DebugLog("RefreshBanRulesList", DebugLogTypes.DataManager,
                $"Reloading BanRules data into the database context.");
            PostActionQueue(new Task(async () =>
            {
                await GUIContext.BanRules.LoadAsync();
                ThreadManager.AddTaskToGUIDispatcher(() => { BanRulesList = GUIContext.BanRules.Local.ToList(); });
                NotifyDataCollectionUpdated(nameof(GUIContext.BanRules));
            }), "RefreshBanRulesList");
        }

        private void RefreshCategoryListList()
        {
            LogWriter.DebugLog("RefreshCategoryListList", DebugLogTypes.DataManager,
                $"Reloading CategoryList data into the database context.");
            PostActionQueue(new Task(async () =>
            {
                await GUIContext.CategoryList.LoadAsync();
                ThreadManager.AddTaskToGUIDispatcher(() => { CategoryListList = GUIContext.CategoryList.Local.ToList(); });
                NotifyDataCollectionUpdated(nameof(GUIContext.CategoryList));
            }), "RefreshCategoryList");
        }

        private void RefreshChannelEventsList()
        {
            LogWriter.DebugLog("RefreshChannelEventsList", DebugLogTypes.DataManager,
                $"Reloading ChannelEvents data into the database context.");
            PostActionQueue(new Task(async () =>
            {
                await GUIContext.ChannelEvents.LoadAsync();
                ThreadManager.AddTaskToGUIDispatcher(() => { ChannelEventsList = GUIContext.ChannelEvents.Local.ToList(); });
                NotifyDataCollectionUpdated(nameof(GUIContext.ChannelEvents));
            }), "RefreshChannelEventsList");
        }

        private void RefreshClipsList()
        {
            LogWriter.DebugLog("RefreshClipsList", DebugLogTypes.DataManager,
                $"Reloading Clips data into the database context.");
            PostActionQueue(new Task(async () =>
            {
                await GUIContext.Clips.LoadAsync();
                ThreadManager.AddTaskToGUIDispatcher(() => { ClipsList = GUIContext.Clips.Local.ToList(); });
                NotifyDataCollectionUpdated(nameof(GUIContext.Clips));
            }), "RefreshClipsList");
        }

        private void RefreshCommandsList()
        {
            LogWriter.DebugLog("RefreshCommandsList", DebugLogTypes.DataManager,
                $"Reloading Commands data into the database context.");
            PostActionQueue(new Task(async () =>
            {
                await GUIContext.Commands.LoadAsync();
                ThreadManager.AddTaskToGUIDispatcher(() => { CommandsList = GUIContext.Commands.Local.ToList(); });
                NotifyDataCollectionUpdated(nameof(GUIContext.Commands));
            }), "RefreshCommandsList");
        }

        private void RefreshCommandsUserList()
        {
            LogWriter.DebugLog("RefreshCommandsUserList", DebugLogTypes.DataManager,
                $"Reloading CommandsUser data into the database context.");
            PostActionQueue(new Task(async () =>
            {
                await GUIContext.CommandsUser.LoadAsync();
                ThreadManager.AddTaskToGUIDispatcher(() => { CommandsUserList = GUIContext.CommandsUser.Local.ToList(); });
                NotifyDataCollectionUpdated(nameof(GUIContext.CommandsUser));
            }), "RefreshCommandsUserList");
        }

        private void RefreshCurrencyList()
        {
            LogWriter.DebugLog("RefreshCurrencyList", DebugLogTypes.DataManager,
                $"Reloading Currency data into the database context.");
            PostActionQueue(new Task(async () =>
            {
                await GUIContext.Currency.LoadAsync();
                ThreadManager.AddTaskToGUIDispatcher(() => { CurrencyList = GUIContext.Currency.Local.ToList(); });
                NotifyDataCollectionUpdated(nameof(GUIContext.Currency));
            }), "RefreshCurrencyList");
        }

        private void RefreshCurrencyTypeList()
        {
            LogWriter.DebugLog("RefreshCurrencyTypeList", DebugLogTypes.DataManager,
                $"Reloading CurrencyType data into the database context.");
            PostActionQueue(new Task(async () =>
            {
                await GUIContext.CurrencyType.LoadAsync();
                ThreadManager.AddTaskToGUIDispatcher(() => { CurrencyTypeList = GUIContext.CurrencyType.Local.ToList(); });
                NotifyDataCollectionUpdated(nameof(GUIContext.CurrencyType));
            }), "RefreshCurrencyTypeList");
        }

        private void RefreshCustomWelcomeList()
        {
            LogWriter.DebugLog("RefreshCustomWelcomeList", DebugLogTypes.DataManager,
                $"Reloading CustomWelcome data into the database context.");
            PostActionQueue(new Task(async () =>
            {
                await GUIContext.CustomWelcome.LoadAsync();
                ThreadManager.AddTaskToGUIDispatcher(() => { CustomWelcomeList = GUIContext.CustomWelcome.Local.ToList(); });
                NotifyDataCollectionUpdated(nameof(GUIContext.CustomWelcome));
            }), "RefreshCustomWelcomeList");
        }

        private void RefreshFollowersList()
        {
            LogWriter.DebugLog("RefreshFollowersList", DebugLogTypes.DataManager,
                $"Reloading Followers data into the database context.");
            PostActionQueue(new Task(async () =>
            {
                await GUIContext.Followers.LoadAsync();
                ThreadManager.AddTaskToGUIDispatcher(() => { FollowersList = GUIContext.Followers.Local.ToList(); });
                NotifyDataCollectionUpdated(nameof(GUIContext.Followers));
            }), "RefreshFollowersList");
        }

        private void RefreshGameDeadCounterList()
        {
            LogWriter.DebugLog("RefreshGameDeadCounterList", DebugLogTypes.DataManager,
                $"Reloading GameDeadCounter data into the database context.");
            PostActionQueue(new Task(async () =>
            {
                await GUIContext.GameDeadCounter.LoadAsync();
                ThreadManager.AddTaskToGUIDispatcher(() => { GameDeadCounterList = GUIContext.GameDeadCounter.Local.ToList(); });
                NotifyDataCollectionUpdated(nameof(GUIContext.GameDeadCounter));
            }), "RefreshGameDeadCounterList");
        }

        private void RefreshGiveawayUserDataList()
        {
            LogWriter.DebugLog("RefreshGiveawayUserDataList", DebugLogTypes.DataManager,
                $"Reloading GiveawayUserData data into the database context.");
            PostActionQueue(new Task(async () =>
            {
                await GUIContext.GiveawayUserData.LoadAsync();
                ThreadManager.AddTaskToGUIDispatcher(() => { GiveawayUserDataList = GUIContext.GiveawayUserData.Local.ToList(); });
                NotifyDataCollectionUpdated(nameof(GUIContext.GiveawayUserData));
            }), "RefreshGiveawayUserDataList");
        }

        private void RefreshInRaidDataList()
        {
            LogWriter.DebugLog("RefreshInRaidDataList", DebugLogTypes.DataManager,
                $"Reloading InRaidData data into the database context.");
            PostActionQueue(new Task(async () =>
            {
                await GUIContext.InRaidData.LoadAsync();
                ThreadManager.AddTaskToGUIDispatcher(() => { InRaidDataList = GUIContext.InRaidData.Local.ToList(); });
                NotifyDataCollectionUpdated(nameof(GUIContext.InRaidData));
            }), "RefreshInRaidDataList");
        }

        private void RefreshLearnMsgsList()
        {
            LogWriter.DebugLog("RefreshLearnMsgsList", DebugLogTypes.DataManager,
                $"Reloading LearnMsgs data into the database context.");
            PostActionQueue(new Task(async () =>
            {
                await GUIContext.LearnMsgs.LoadAsync();
                ThreadManager.AddTaskToGUIDispatcher(() => { LearnMsgsList = GUIContext.LearnMsgs.Local.ToList(); });
                NotifyDataCollectionUpdated(nameof(GUIContext.LearnMsgs));
            }), "RefreshLearnMsgsList");
        }

        private void RefreshModeratorApproveList()
        {
            LogWriter.DebugLog("RefreshModeratorApproveList", DebugLogTypes.DataManager,
                $"Reloading ModeratorApprove data into the database context.");
            PostActionQueue(new Task(async () =>
            {
                await GUIContext.ModeratorApprove.LoadAsync();
                ThreadManager.AddTaskToGUIDispatcher(() => { ModeratorApproveList = GUIContext.ModeratorApprove.Local.ToList(); });
                NotifyDataCollectionUpdated(nameof(GUIContext.ModeratorApprove));
            }), "RefreshModeratorApproveList");
        }

        private void RefreshMultiChannelsList()
        {
            LogWriter.DebugLog("RefreshMultiChannelsList", DebugLogTypes.DataManager,
                $"Reloading MultiChannels data into the database context.");
            PostActionQueue(new Task(async () =>
            {
                await GUIContext.MultiChannels.LoadAsync();
                ThreadManager.AddTaskToGUIDispatcher(() => { MultiChannelsList = GUIContext.MultiChannels.Local.ToList(); });
                NotifyDataCollectionUpdated(nameof(GUIContext.MultiChannels));
            }), "RefreshMultiChannelsList");
        }

        private void RefreshMultiLiveStreamsList()
        {
            LogWriter.DebugLog("RefreshMultiLiveStreamsList", DebugLogTypes.DataManager,
                $"Reloading MultiLiveStreams data into the database context.");
            PostActionQueue(new Task(async () =>
            {
                await GUIContext.MultiLiveStreams.LoadAsync();
                ThreadManager.AddTaskToGUIDispatcher(() => { MultiLiveStreamsList = GUIContext.MultiLiveStreams.Local.ToList(); });
                NotifyDataCollectionUpdated(nameof(GUIContext.MultiLiveStreams));
            }), "RefreshMultiLiveStreamsList");
        }

        private void RefreshMultiSummaryLiveStreamsList()
        {
            LogWriter.DebugLog("RefreshMultiSummaryLiveStreamsList", DebugLogTypes.DataManager,
                $"Reloading MultiSummaryLiveStreams data into the database context.");
            PostActionQueue(new Task(async () =>
            {
                await GUIContext.MultiSummaryLiveStreams.LoadAsync();
                ThreadManager.AddTaskToGUIDispatcher(() => { MultiSummaryLiveStreamsList = GUIContext.MultiSummaryLiveStreams.Local.ToList(); });
                NotifyDataCollectionUpdated(nameof(GUIContext.MultiSummaryLiveStreams));
            }), "RefreshMultiSummaryLiveStreamsList");
        }

        private void RefreshMultiWebhooksList()
        {
            LogWriter.DebugLog("RefreshMultiWebhooksList", DebugLogTypes.DataManager,
                $"Reloading MultiWebhooks data into the database context.");
            PostActionQueue(new Task(async () =>
            {
                await GUIContext.MultiWebhooks.LoadAsync();
                ThreadManager.AddTaskToGUIDispatcher(() => { MultiWebhooksList = GUIContext.MultiWebhooks.Local.ToList(); });
                NotifyDataCollectionUpdated(nameof(GUIContext.MultiWebhooks));
            }), "RefreshMultiWebhooksList");
        }

        private void RefreshOldFollowUsersList()
        {
            LogWriter.DebugLog("RefreshOldFollowUsersList", DebugLogTypes.DataManager,
                $"Reloading OldFollowUsers data into the database context.");
            PostActionQueue(new Task(async () =>
            {
                await GUIContext.OldFollowUsers.LoadAsync();
                ThreadManager.AddTaskToGUIDispatcher(() => { OldFollowUsersList = GUIContext.OldFollowUsers.Local.ToList(); });
                NotifyDataCollectionUpdated(nameof(GUIContext.OldFollowUsers));
            }), "RefreshOldFollowUsersList");
        }

        private void RefreshOutRaidDataList()
        {
            LogWriter.DebugLog("RefreshOutRaidDataList", DebugLogTypes.DataManager,
                $"Reloading OutRaidData data into the database context.");
            PostActionQueue(new Task(async () =>
            {
                await GUIContext.OutRaidData.LoadAsync();
                ThreadManager.AddTaskToGUIDispatcher(() => { OutRaidDataList = GUIContext.OutRaidData.Local.ToList(); });
                NotifyDataCollectionUpdated(nameof(GUIContext.OutRaidData));
            }), "RefreshOutRaidDataList");
        }

        private void RefreshOverlayServicesList()
        {
            LogWriter.DebugLog("RefreshOverlayServicesList", DebugLogTypes.DataManager,
                $"Reloading OverlayServices data into the database context.");
            PostActionQueue(new Task(async () =>
            {
                await GUIContext.OverlayServices.LoadAsync();
                ThreadManager.AddTaskToGUIDispatcher(() => { OverlayServicesList = GUIContext.OverlayServices.Local.ToList(); });
                NotifyDataCollectionUpdated(nameof(GUIContext.OverlayServices));
            }), "RefreshOverlayServicesList");
        }

        private void RefreshOverlayTickerList()
        {
            LogWriter.DebugLog("RefreshOverlayTickerList", DebugLogTypes.DataManager,
                $"Reloading OverlayTicker data into the database context.");
            PostActionQueue(new Task(async () =>
            {
                await GUIContext.OverlayTicker.LoadAsync();
                ThreadManager.AddTaskToGUIDispatcher(() => { OverlayTickerList = GUIContext.OverlayTicker.Local.ToList(); });
                NotifyDataCollectionUpdated(nameof(GUIContext.OverlayTicker));
            }), "RefreshOverlayTickerList");
        }

        private void RefreshQuotesList()
        {
            LogWriter.DebugLog("RefreshQuotesList", DebugLogTypes.DataManager,
                $"Reloading Quotes data into the database context.");
            PostActionQueue(new Task(async () =>
            {
                await GUIContext.Quotes.LoadAsync();
                ThreadManager.AddTaskToGUIDispatcher(() => { QuotesList = GUIContext.Quotes.Local.ToList(); });
                NotifyDataCollectionUpdated(nameof(GUIContext.Quotes));
            }), "RefreshQuotesList");
        }

        private void RefreshShoutOutsList()
        {
            LogWriter.DebugLog("RefreshShoutOutsList", DebugLogTypes.DataManager,
                $"Reloading ShoutOuts data into the database context.");
            PostActionQueue(new Task(async () =>
            {
                await GUIContext.ShoutOuts.LoadAsync();
                ThreadManager.AddTaskToGUIDispatcher(() => { ShoutOutsList = GUIContext.ShoutOuts.Local.ToList(); });
                NotifyDataCollectionUpdated(nameof(GUIContext.ShoutOuts));
            }), "RefreshShoutOutsList");
        }

        private void RefreshStreamStatsList()
        {
            LogWriter.DebugLog("RefreshStreamStatsList", DebugLogTypes.DataManager,
                $"Reloading StreamStats data into the database context.");
            PostActionQueue(new Task(async () =>
            {
                await GUIContext.StreamStats.LoadAsync();
                ThreadManager.AddTaskToGUIDispatcher(() => { StreamStatsList = GUIContext.StreamStats.Local.ToList(); });
                NotifyDataCollectionUpdated(nameof(GUIContext.StreamStats));
            }), "RefreshStreamStatsList");
        }

        private void RefreshUsersList()
        {
            LogWriter.DebugLog("RefreshUsersList", DebugLogTypes.DataManager,
                $"Reloading Users data into the database context.");
            PostActionQueue(new Task(async () =>
            {
                LogWriter.DebugLog("RefreshUsersList",
                    DebugLogTypes.DataManager, $"Reloading Users data into the database context.");
                await GUIContext.Users.LoadAsync();
                ThreadManager.AddTaskToGUIDispatcher(() => { UsersList = GUIContext.Users.Local.ToList(); });
                LogWriter.DebugLog("RefreshUsersList",
    DebugLogTypes.DataManager, $"Notifying the DataCollection is Updated.");
                NotifyDataCollectionUpdated(nameof(GUIContext.Users));
            }), "RefreshUsersList");
        }

        private void RefreshUserStatsList()
        {
            LogWriter.DebugLog("RefreshUserStatsList", DebugLogTypes.DataManager,
                $"Reloading UserStats data into the database context.");
            PostActionQueue(new Task(async () =>
            {
                LogWriter.DebugLog("RefreshUserStatsList",
                  DebugLogTypes.DataManager, $"Reloading UserStats data into the database context.");
                await GUIContext.UserStats.LoadAsync();
                ThreadManager.AddTaskToGUIDispatcher(() => { UserStatsList = GUIContext.UserStats.Local.ToList(); });
                LogWriter.DebugLog("RefreshUserStatsList",
   DebugLogTypes.DataManager, $"Notifying the DataCollection is Updated.");
                NotifyDataCollectionUpdated(nameof(GUIContext.UserStats));
            }), "RefreshUserStatsList");
        }

        private void RefreshWebhooksList()
        {
            LogWriter.DebugLog("RefreshWebhooksList", DebugLogTypes.DataManager,
                $"Reloading Webhooks data into the database context.");
            PostActionQueue(new Task(async () =>
            {
                await GUIContext.Webhooks.LoadAsync();
                ThreadManager.AddTaskToGUIDispatcher(() => { WebhooksList = GUIContext.Webhooks.Local.ToList(); });
                NotifyDataCollectionUpdated(nameof(GUIContext.Webhooks));
            }), "RefreshWebhooksList");

        }

        #endregion
    }
}
