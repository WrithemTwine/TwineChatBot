#define EFC9_OBSCOL_ISSUE_REFRESHCONTEXT_DATA  // This is a workaround for the issue with ObservableCollection in EF Core 9

#if EFC9_OBSCOL_ISSUE_REFRESHCONTEXT_DATA

using Microsoft.EntityFrameworkCore;

using StreamerBotLib.DataSQL.Models;
using StreamerBotLib.Models;
using StreamerBotLib.Models.Enums;
using StreamerBotLib.Static;

using System.Collections.ObjectModel;

namespace StreamerBotLib.DataSQL.EFC9
{
    internal partial class DataManagerSQLAsync
    {
        private SQLDBContext GUIContext;


        #region Binding Collections

        private ObservableCollection<Users> Users = [];
        private ObservableCollection<Followers> Followers = [];
        private ObservableCollection<CommandsUser> CommandsUser = [];
        private ObservableCollection<Commands> Commands = [];

        private ObservableCollection<BanRules> BanRules = [];
        private ObservableCollection<Models.BanReasons> BanReasons = [];
        private ObservableCollection<CategoryList> CategoryList = [];
        private ObservableCollection<ChannelEvents> ChannelEvents = [];
        private ObservableCollection<Clips> Clips = [];
        private ObservableCollection<Currency> Currency = [];
        private ObservableCollection<Models.CurrencyType> CurrencyType = [];
        private ObservableCollection<CustomWelcome> CustomWelcome = [];
        private ObservableCollection<GameDeadCounter> GameDeadCounter = [];
        private ObservableCollection<GiveawayUserData> GiveawayUserData = [];
        private ObservableCollection<InRaidData> InRaidData = [];
        private ObservableCollection<LearnMsgs> LearnMsgs = [];
        private ObservableCollection<ModeratorApprove> ModeratorApprove = [];
        private ObservableCollection<OldFollowUsers> OldFollowUsers = [];
        private ObservableCollection<OutRaidData> OutRaidData = [];
        private ObservableCollection<OverlayServices> OverlayServices = [];
        private ObservableCollection<OverlayTicker> OverlayTicker = [];
        private ObservableCollection<Quotes> Quotes = [];
        private ObservableCollection<ShoutOuts> ShoutOuts = [];
        private ObservableCollection<StreamStats> StreamStats = [];
        private ObservableCollection<UserStats> UserStats = [];
        private ObservableCollection<Webhooks> Webhooks = [];

        #region MultiLive Collections
        private ObservableCollection<MultiWebhooks> MultiWebhooks = [];
        private ObservableCollection<MultiChannels> MultiChannels = [];
        private ObservableCollection<MultiLiveStreams> MultiLiveStreams = [];
        private ObservableCollection<MultiSummaryLiveStreams> MultiSummaryLiveStreams = [];
        #endregion
        #endregion


        #region LocalView ObservableCollection

        internal object GetICollection(DataTables dataTable)
        {
            LogWriter.DebugLog("GetObservableCollection", DebugLogTypes.DataManager,
                $"Getting the observable collection for {dataTable}.");

            return dataTable switch
            {
                DataTables.BanReasons => GetBanReasonsLocalObservableAsync().Result,
                DataTables.BanRules => GetBanRulesLocalObservableAsync().Result,
                DataTables.CategoryList => GetCategoryListLocalObservableAsync().Result,
                DataTables.ChannelEvents => GetChannelEventsLocalObservableAsync().Result,
                DataTables.Clips => GetClipsLocalObservableAsync().Result,
                DataTables.Commands => GetCommandsLocalObservableAsync().Result,
                DataTables.CommandsBase => throw new NotImplementedException(),
                DataTables.CommandsUser => GetCommandsUserLocalObservableAsync().Result,
                DataTables.Currency => GetCurrencyLocalObservableAsync().Result,
                DataTables.CurrencyType => GetCurrencyTypeLocalObservableAsync().Result,
                DataTables.CustomWelcome => GetCustomWelcomeLocalObservableAsync().Result,
                DataTables.Followers => GetFollowersLocalObservableAsync().Result,
                DataTables.GameDeadCounter => GetGameDeadCounterLocalObservableAsync().Result,
                DataTables.GiveawayUserData => GetGiveawayUserDataLocalObservableAsync().Result,
                DataTables.InRaidData => GetInRaidDataLocalObservableAsync().Result,
                DataTables.LearnMsgs => GetLearnMsgsLocalObservableAsync().Result,
                DataTables.ModeratorApprove => GetModeratorApproveLocalObservableAsync().Result,
                DataTables.MultiChannels => GetMultiChannelsLocalObservableAsync().Result,
                DataTables.MultiLiveStreams => GetMultiLiveStreamsLocalObservableAsync().Result,
                DataTables.MultiSummaryLiveStreams => GetMultiSummaryLiveStreamsLocalObservableAsync().Result,
                DataTables.MultiWebhooks => GetMultiWebhooksLocalObservableAsync().Result,
                DataTables.OldFollowUsers => GetOldFollowUsersLocalObservableAsync().Result,
                DataTables.OutRaidData => GetOutRaidDataLocalObservableAsync().Result,
                DataTables.OverlayServices => GetOverlayServicesLocalObservableAsync().Result,
                DataTables.OverlayTicker => GetOverlayTickerLocalObservableAsync().Result,
                DataTables.Quotes => GetQuotesLocalObservableAsync().Result,
                DataTables.ShoutOuts => GetShoutOutsLocalObservableAsync().Result,
                DataTables.StreamStats => GetStreamStatsLocalObservableAsync().Result,
                DataTables.UserBase => throw new NotImplementedException(),
                DataTables.Users => GetUsersLocalObservableAsync().Result,
                DataTables.UserStats => GetUserStatsLocalObservableAsync().Result,
                DataTables.Webhooks => GetWebhooksLocalObservableAsync().Result,
                DataTables.WebhooksBase => throw new NotImplementedException(),
                _ => throw new NotImplementedException()
            };
        }

        private Task<ObservableCollection<Models.BanReasons>> GetBanReasonsLocalObservableAsync()
        {
            return Task.Run(async () =>
            {
                await GUIContext.BanReasons.LoadAsync();
                BanReasons.AddRange(GUIContext.BanReasons.Local.ToList());

                return BanReasons;
            });
        }

        private Task<ObservableCollection<BanRules>> GetBanRulesLocalObservableAsync()
        {
            return Task.Run(async () =>
            {
                await GUIContext.BanRules.LoadAsync();
                BanRules.AddRange(GUIContext.BanRules.Local.ToList());
                return BanRules;
            });
        }

        private Task<ObservableCollection<CategoryList>> GetCategoryListLocalObservableAsync()
        {
            return Task.Run(async () =>
            {
                await GUIContext.CategoryList.LoadAsync();
                CategoryList.AddRange(GUIContext.CategoryList.Local.ToList());
                return CategoryList;
            });
        }

        private Task<ObservableCollection<ChannelEvents>> GetChannelEventsLocalObservableAsync()
        {
            return Task.Run(async () =>
            {
                await GUIContext.ChannelEvents.LoadAsync();
                ChannelEvents.AddRange(GUIContext.ChannelEvents.Local.ToList());
                return ChannelEvents;
            });
        }

        private Task<ObservableCollection<Clips>> GetClipsLocalObservableAsync()
        {
            return Task.Run(async () =>
            {
                await GUIContext.Clips.LoadAsync();
                Clips.AddRange(GUIContext.Clips.Local.ToList());
                return Clips;
            });
        }

        private Task<ObservableCollection<Commands>> GetCommandsLocalObservableAsync()
        {
            return Task.Run(async () =>
            {
                await GUIContext.Commands.LoadAsync();
                Commands.AddRange(GUIContext.Commands.Local.ToList());
                return Commands;
            });
        }

        private Task<ObservableCollection<CommandsUser>> GetCommandsUserLocalObservableAsync()
        {
            return Task.Run(async () =>
            {
                await GUIContext.CommandsUser.LoadAsync();
                CommandsUser.AddRange(GUIContext.CommandsUser.Local.ToList());
                return CommandsUser;
            });
        }

        private Task<ObservableCollection<Currency>> GetCurrencyLocalObservableAsync()
        {
            return Task.Run(async () =>
            {
                await GUIContext.Currency.LoadAsync();
                Currency.AddRange(GUIContext.Currency.Local.ToList());
                return Currency;
            });
        }

        private Task<ObservableCollection<Models.CurrencyType>> GetCurrencyTypeLocalObservableAsync()
        {
            return Task.Run(async () =>
            {
                await GUIContext.CurrencyType.LoadAsync();
                CurrencyType.AddRange(GUIContext.CurrencyType.Local.ToList());
                return CurrencyType;
            });
        }

        private Task<ObservableCollection<CustomWelcome>> GetCustomWelcomeLocalObservableAsync()
        {
            return Task.Run(async () =>
            {
                await GUIContext.CustomWelcome.LoadAsync();
                CustomWelcome.AddRange(GUIContext.CustomWelcome.Local.ToList());
                return CustomWelcome;
            });
        }

        private Task<ObservableCollection<Followers>> GetFollowersLocalObservableAsync()
        {
            return Task.Run(async () =>
            {
                await GUIContext.Followers.LoadAsync();
                Followers.AddRange(GUIContext.Followers.Local.ToList());
                return Followers;
            });
        }

        private Task<ObservableCollection<GameDeadCounter>> GetGameDeadCounterLocalObservableAsync()
        {
            return Task.Run(async () =>
            {
                await GUIContext.GameDeadCounter.LoadAsync();
                GameDeadCounter.AddRange(GUIContext.GameDeadCounter.Local.ToList());
                return GameDeadCounter;
            });
        }

        private Task<ObservableCollection<GiveawayUserData>> GetGiveawayUserDataLocalObservableAsync()
        {
            return Task.Run(async () =>
            {
                await GUIContext.GiveawayUserData.LoadAsync();
                GiveawayUserData.AddRange(GUIContext.GiveawayUserData.Local.ToList());
                return GiveawayUserData;
            });
        }

        private Task<ObservableCollection<InRaidData>> GetInRaidDataLocalObservableAsync()
        {
            return Task.Run(async () =>
            {
                await GUIContext.InRaidData.LoadAsync();
                InRaidData.AddRange(GUIContext.InRaidData.Local.ToList());
                return InRaidData;
            });
        }

        private Task<ObservableCollection<LearnMsgs>> GetLearnMsgsLocalObservableAsync()
        {
            return Task.Run(async () =>
            {
                await GUIContext.LearnMsgs.LoadAsync();
                LearnMsgs.AddRange(GUIContext.LearnMsgs.Local.ToList());
                return LearnMsgs;
            });
        }

        private Task<ObservableCollection<ModeratorApprove>> GetModeratorApproveLocalObservableAsync()
        {
            return Task.Run(async () =>
            {
                await GUIContext.ModeratorApprove.LoadAsync();
                ModeratorApprove.AddRange(GUIContext.ModeratorApprove.Local.ToList());
                return ModeratorApprove;
            });
        }

        private Task<ObservableCollection<MultiChannels>> GetMultiChannelsLocalObservableAsync()
        {
            return Task.Run(async () =>
            {
                await GUIContext.MultiChannels.LoadAsync();
                MultiChannels.AddRange(GUIContext.MultiChannels.Local.ToList());
                return MultiChannels;
            });
        }

        private Task<ObservableCollection<MultiLiveStreams>> GetMultiLiveStreamsLocalObservableAsync()
        {
            return Task.Run(async () =>
            {
                await GUIContext.MultiLiveStreams.LoadAsync();
                MultiLiveStreams.AddRange(GUIContext.MultiLiveStreams.Local.ToList());
                return MultiLiveStreams;
            });
        }

        private Task<ObservableCollection<MultiWebhooks>> GetMultiWebhooksLocalObservableAsync()
        {
            return Task.Run(async () =>
            {
                await GUIContext.MultiWebhooks.LoadAsync();
                MultiWebhooks.AddRange(GUIContext.MultiWebhooks.Local.ToList());
                return MultiWebhooks;
            });
        }

        private Task<ObservableCollection<MultiSummaryLiveStreams>> GetMultiSummaryLiveStreamsLocalObservableAsync()
        {
            return Task.Run(async () =>
            {
                await GUIContext.MultiSummaryLiveStreams.LoadAsync();
                MultiSummaryLiveStreams.AddRange(GUIContext.MultiSummaryLiveStreams.Local.ToList());
                return MultiSummaryLiveStreams;
            });
        }

        private Task<ObservableCollection<OldFollowUsers>> GetOldFollowUsersLocalObservableAsync()
        {
            return Task.Run(async () =>
            {
                await GUIContext.OldFollowUsers.LoadAsync();
                OldFollowUsers.AddRange(GUIContext.OldFollowUsers.Local.ToList());
                return OldFollowUsers;
            });
        }

        private Task<ObservableCollection<OutRaidData>> GetOutRaidDataLocalObservableAsync()
        {
            return Task.Run(async () =>
            {
                await GUIContext.OutRaidData.LoadAsync();
                OutRaidData.AddRange(GUIContext.OutRaidData.Local.ToList());
                return OutRaidData;
            });
        }

        private Task<ObservableCollection<OverlayServices>> GetOverlayServicesLocalObservableAsync()
        {
            return Task.Run(async () =>
            {
                await GUIContext.OverlayServices.LoadAsync();
                OverlayServices.AddRange(GUIContext.OverlayServices.Local.ToList());
                return OverlayServices;
            });
        }

        private Task<ObservableCollection<OverlayTicker>> GetOverlayTickerLocalObservableAsync()
        {
            return Task.Run(async () =>
            {
                await GUIContext.OverlayTicker.LoadAsync();
                OverlayTicker.AddRange(GUIContext.OverlayTicker.Local.ToList());
                return OverlayTicker;
            });
        }

        private Task<ObservableCollection<Quotes>> GetQuotesLocalObservableAsync()
        {
            return Task.Run(async () =>
            {
                await GUIContext.Quotes.LoadAsync();
                Quotes.AddRange(GUIContext.Quotes.Local.ToList());
                return Quotes;
            });
        }

        private Task<ObservableCollection<ShoutOuts>> GetShoutOutsLocalObservableAsync()
        {
            return Task.Run(async () =>
            {
                await GUIContext.ShoutOuts.LoadAsync();
                ShoutOuts.AddRange(GUIContext.ShoutOuts.Local.ToList());
                return ShoutOuts;
            });
        }

        private Task<ObservableCollection<StreamStats>> GetStreamStatsLocalObservableAsync()
        {
            return Task.Run(async () =>
            {
                await GUIContext.StreamStats.LoadAsync();
                StreamStats.AddRange(GUIContext.StreamStats.Local.ToList());
                return StreamStats;
            });
        }

        private Task<ObservableCollection<Users>> GetUsersLocalObservableAsync()
        {
            return Task.Run(async () =>
            {
                await GUIContext.Users.LoadAsync();
                Users.AddRange(GUIContext.Users.Local.ToList());
                return Users;
            });
        }

        private Task<ObservableCollection<UserStats>> GetUserStatsLocalObservableAsync()
        {
            return Task.Run(async () =>
            {
                await GUIContext.UserStats.LoadAsync();
                UserStats.AddRange(GUIContext.UserStats.Local.ToList());
                return UserStats;
            });
        }

        private Task<ObservableCollection<Webhooks>> GetWebhooksLocalObservableAsync()
        {
            return Task.Run(async () =>
            {
                await GUIContext.Webhooks.LoadAsync();
                Webhooks.AddRange(GUIContext.Webhooks.Local.ToList());
                return Webhooks;
            });
        }

        #endregion

        internal void NotifyDataCollectionUpdated(string TableName, bool RecordCountChange = false)
        {
            OnDataCollectionUpdated?.Invoke(this, new(TableName, RecordCountChange));
        }

        #region Refresh Collections
        private async Task RefreshBanReasonsList(bool RecordCountChange = false)
        {
            await Task.Run(() =>
            {
                ThreadManager.AddTaskToGUIDispatcher(async () =>
                {
                    GUIContext.ChangeTracker.Clear();
                    await GUIContext.BanReasons.LoadAsync();
                    BanReasons.Clear();
                    BanReasons.AddRange(GUIContext.BanReasons.Local.ToList());
                    NotifyDataCollectionUpdated(nameof(GUIContext.BanReasons), RecordCountChange);
                });
            });
        }

        private async Task RefreshBanRulesList(bool RecordCountChange = false)
        {
            await Task.Run(() =>
            {
                ThreadManager.AddTaskToGUIDispatcher(async () =>
                {
                    GUIContext.ChangeTracker.Clear();
                    await GUIContext.BanRules.LoadAsync();
                    BanRules.Clear();
                    BanRules.AddRange(GUIContext.BanRules.Local.ToList());
                    NotifyDataCollectionUpdated(nameof(GUIContext.BanRules), RecordCountChange);
                });
            });
        }

        private async Task RefreshCategoryListList(bool RecordCountChange = false)
        {
            await Task.Run(() =>
            {
                ThreadManager.AddTaskToGUIDispatcher(async () =>
                {
                    GUIContext.ChangeTracker.Clear();
                    await GUIContext.CategoryList.LoadAsync();
                    CategoryList.Clear();
                    CategoryList.AddRange(GUIContext.CategoryList.Local.ToList());
                    NotifyDataCollectionUpdated(nameof(GUIContext.CategoryList), RecordCountChange);
                });
            });
        }

        private async Task RefreshChannelEventsList(bool RecordCountChange = false)
        {
            await Task.Run(() =>
            {
                ThreadManager.AddTaskToGUIDispatcher(async () =>
                {
                    GUIContext.ChangeTracker.Clear();
                    await GUIContext.ChannelEvents.LoadAsync();
                    ChannelEvents.Clear();
                    ChannelEvents.AddRange(GUIContext.ChannelEvents.Local.ToList());
                    NotifyDataCollectionUpdated(nameof(GUIContext.ChannelEvents), RecordCountChange);
                });
            });
        }

        private async Task RefreshClipsList(bool RecordCountChange = false)
        {
            await Task.Run(() =>
            {
                ThreadManager.AddTaskToGUIDispatcher(async () =>
                {
                    GUIContext.ChangeTracker.Clear();
                    await GUIContext.Clips.LoadAsync();
                    Clips.Clear();
                    Clips.AddRange(GUIContext.Clips.Local.ToList());
                    NotifyDataCollectionUpdated(nameof(GUIContext.Clips), RecordCountChange);
                });
            });
        }

        private async Task RefreshCommandsList(bool RecordCountChange = false)
        {
            await Task.Run(() =>
            {
                ThreadManager.AddTaskToGUIDispatcher(async () =>
                {
                    GUIContext.ChangeTracker.Clear();
                    await GUIContext.Commands.LoadAsync();
                    Commands.Clear();
                    Commands.AddRange(GUIContext.Commands.Local.ToList());

                    NotifyDataCollectionUpdated(nameof(GUIContext.Commands), RecordCountChange);
                });
            });
        }

        private async Task RefreshCommandsUserList(bool RecordCountChange = false)
        {
            await Task.Run(() =>
            {
                ThreadManager.AddTaskToGUIDispatcher(async () =>
                {
                    GUIContext.ChangeTracker.Clear();
                    await GUIContext.CommandsUser.LoadAsync();
                    CommandsUser.Clear();
                    CommandsUser.AddRange(GUIContext.CommandsUser.Local.ToList());
                    NotifyDataCollectionUpdated(nameof(GUIContext.CommandsUser), RecordCountChange);
                });
            });
        }

        private async Task RefreshCurrencyList(bool RecordCountChange = false)
        {
            await Task.Run(() =>
            {
                ThreadManager.AddTaskToGUIDispatcher(async () =>
                {
                    GUIContext.ChangeTracker.Clear();
                    await GUIContext.Currency.LoadAsync();
                    Currency.Clear();
                    Currency.AddRange(GUIContext.Currency.Local.ToList());
                    NotifyDataCollectionUpdated(nameof(GUIContext.Currency), RecordCountChange);
                });
            });
        }

        private async Task RefreshCurrencyTypeList(bool RecordCountChange = false)
        {
            await Task.Run(() =>
            {
                ThreadManager.AddTaskToGUIDispatcher(async () =>
                {
                    GUIContext.ChangeTracker.Clear();
                    await GUIContext.CurrencyType.LoadAsync();
                    CurrencyType.Clear();
                    CurrencyType.AddRange(GUIContext.CurrencyType.Local.ToList());
                    NotifyDataCollectionUpdated(nameof(GUIContext.CurrencyType), RecordCountChange);
                });
            });
        }

        private async Task RefreshCustomWelcomeList(bool RecordCountChange = false)
        {
            await Task.Run(() =>
            {
                ThreadManager.AddTaskToGUIDispatcher(async () =>
                {
                    GUIContext.ChangeTracker.Clear();
                    await GUIContext.CustomWelcome.LoadAsync();
                    CustomWelcome.Clear();
                    CustomWelcome.AddRange(GUIContext.CustomWelcome.Local.ToList());
                    NotifyDataCollectionUpdated(nameof(GUIContext.CustomWelcome), RecordCountChange);
                });
            });
        }

        private async Task RefreshFollowersList(bool RecordCountChange = false)
        {
            await Task.Run(() =>
            {
                ThreadManager.AddTaskToGUIDispatcher(async () =>
                {
                    GUIContext.ChangeTracker.Clear();
                    await GUIContext.Followers.LoadAsync();
                    Followers.Clear();
                    Followers.AddRange(GUIContext.Followers.Local.ToList());
                    NotifyDataCollectionUpdated(nameof(GUIContext.Followers), RecordCountChange);
                });
            });
        }

        private async Task RefreshGameDeadCounterList(bool RecordCountChange = false)
        {
            await Task.Run(() =>
            {
                ThreadManager.AddTaskToGUIDispatcher(async () =>
                {
                    GUIContext.ChangeTracker.Clear();
                    await GUIContext.GameDeadCounter.LoadAsync();
                    GameDeadCounter.Clear();
                    GameDeadCounter.AddRange(GUIContext.GameDeadCounter.Local.ToList());
                    NotifyDataCollectionUpdated(nameof(GUIContext.GameDeadCounter), RecordCountChange);
                });
            });
        }

        private async Task RefreshGiveawayUserDataList(bool RecordCountChange = false)
        {
            await Task.Run(() =>
            {
                ThreadManager.AddTaskToGUIDispatcher(async () =>
                {
                    GUIContext.ChangeTracker.Clear();
                    await GUIContext.GiveawayUserData.LoadAsync();
                    GiveawayUserData.Clear();
                    GiveawayUserData.AddRange(GUIContext.GiveawayUserData.Local.ToList());
                    NotifyDataCollectionUpdated(nameof(GUIContext.GiveawayUserData), RecordCountChange);
                });
            });
        }

        private async Task RefreshInRaidDataList(bool RecordCountChange = false)
        {
            await Task.Run(() =>
            {
                ThreadManager.AddTaskToGUIDispatcher(async () =>
                {
                    GUIContext.ChangeTracker.Clear();
                    await GUIContext.InRaidData.LoadAsync();
                    InRaidData.Clear();
                    InRaidData.AddRange(GUIContext.InRaidData.Local.ToList());
                    NotifyDataCollectionUpdated(nameof(GUIContext.InRaidData), RecordCountChange);
                });
            });
        }

        private async Task RefreshLearnMsgsList(bool RecordCountChange = false)
        {
            await Task.Run(() =>
            {
                ThreadManager.AddTaskToGUIDispatcher(async () =>
                {
                    GUIContext.ChangeTracker.Clear();
                    await GUIContext.LearnMsgs.LoadAsync();
                    LearnMsgs.Clear();
                    LearnMsgs.AddRange(GUIContext.LearnMsgs.Local.ToList());
                    NotifyDataCollectionUpdated(nameof(GUIContext.LearnMsgs), RecordCountChange);
                });
            });
        }

        private async Task RefreshModeratorApproveList(bool RecordCountChange = false)
        {
            await Task.Run(() =>
            {
                ThreadManager.AddTaskToGUIDispatcher(async () =>
                {
                    GUIContext.ChangeTracker.Clear();
                    await GUIContext.ModeratorApprove.LoadAsync();
                    ModeratorApprove.Clear();
                    ModeratorApprove.AddRange(GUIContext.ModeratorApprove.Local.ToList());
                    NotifyDataCollectionUpdated(nameof(GUIContext.ModeratorApprove), RecordCountChange);
                });
            });
        }

        private async Task RefreshMultiChannelsList(bool RecordCountChange = false)
        {
            await Task.Run(() =>
            {
                ThreadManager.AddTaskToGUIDispatcher(async () =>
                {
                    GUIContext.ChangeTracker.Clear();
                    await GUIContext.MultiChannels.LoadAsync();
                    MultiChannels.Clear();
                    MultiChannels.AddRange(GUIContext.MultiChannels.Local.ToList());
                    NotifyDataCollectionUpdated(nameof(GUIContext.MultiChannels), RecordCountChange);
                });
            });
        }

        private async Task RefreshMultiLiveStreamsList(bool RecordCountChange = false)
        {
            await Task.Run(() =>
            {
                ThreadManager.AddTaskToGUIDispatcher(async () =>
                {
                    GUIContext.ChangeTracker.Clear();
                    await GUIContext.MultiLiveStreams.LoadAsync();
                    MultiLiveStreams.Clear();
                    MultiLiveStreams.AddRange(GUIContext.MultiLiveStreams.Local.ToList());
                    NotifyDataCollectionUpdated(nameof(GUIContext.MultiLiveStreams), RecordCountChange);
                });
            });
        }

        private async Task RefreshMultiSummaryLiveStreamsList(bool RecordCountChange = false)
        {
            await Task.Run(() =>
            {
                ThreadManager.AddTaskToGUIDispatcher(async () =>
                {
                    GUIContext.ChangeTracker.Clear();
                    await GUIContext.MultiSummaryLiveStreams.LoadAsync();
                    MultiSummaryLiveStreams.Clear();
                    MultiSummaryLiveStreams.AddRange(GUIContext.MultiSummaryLiveStreams.Local.ToList());
                    NotifyDataCollectionUpdated(nameof(GUIContext.MultiSummaryLiveStreams), RecordCountChange);
                });
            });
        }

        private async Task RefreshMultiWebhooksList(bool RecordCountChange = false)
        {
            await Task.Run(() =>
            {
                ThreadManager.AddTaskToGUIDispatcher(async () =>
                {
                    GUIContext.ChangeTracker.Clear();
                    await GUIContext.MultiWebhooks.LoadAsync();
                    MultiWebhooks.Clear();
                    MultiWebhooks.AddRange(GUIContext.MultiWebhooks.Local.ToList());
                    NotifyDataCollectionUpdated(nameof(GUIContext.MultiWebhooks), RecordCountChange);
                });
            });
        }

        private async Task RefreshOldFollowUsersList(bool RecordCountChange = false)
        {
            await Task.Run(() =>
            {
                ThreadManager.AddTaskToGUIDispatcher(async () =>
                {
                    GUIContext.ChangeTracker.Clear();
                    await GUIContext.OldFollowUsers.LoadAsync();
                    OldFollowUsers.Clear();
                    OldFollowUsers.AddRange(GUIContext.OldFollowUsers.Local.ToList());
                    NotifyDataCollectionUpdated(nameof(GUIContext.OldFollowUsers), RecordCountChange);
                });
            });
        }

        private async Task RefreshOutRaidDataList(bool RecordCountChange = false)
        {
            await Task.Run(() =>
            {
                ThreadManager.AddTaskToGUIDispatcher(async () =>
                {
                    GUIContext.ChangeTracker.Clear();
                    await GUIContext.OutRaidData.LoadAsync();
                    OutRaidData.Clear();
                    OutRaidData.AddRange(GUIContext.OutRaidData.Local.ToList());
                    NotifyDataCollectionUpdated(nameof(GUIContext.OutRaidData), RecordCountChange);
                });
            });
        }

        private async Task RefreshOverlayServicesList(bool RecordCountChange = false)
        {
            await Task.Run(() =>
            {
                ThreadManager.AddTaskToGUIDispatcher(async () =>
                {
                    GUIContext.ChangeTracker.Clear();
                    await GUIContext.OverlayServices.LoadAsync();
                    OverlayServices.Clear();
                    OverlayServices.AddRange(GUIContext.OverlayServices.Local.ToList());
                    NotifyDataCollectionUpdated(nameof(GUIContext.OverlayServices), RecordCountChange);
                });
            });
        }

        private async Task RefreshOverlayTickerList(bool RecordCountChange = false)
        {
            await Task.Run(() =>
            {
                ThreadManager.AddTaskToGUIDispatcher(async () =>
                {
                    GUIContext.ChangeTracker.Clear();
                    await GUIContext.OverlayTicker.LoadAsync();
                    OverlayTicker.Clear();
                    OverlayTicker.AddRange(GUIContext.OverlayTicker.Local.ToList());
                    NotifyDataCollectionUpdated(nameof(GUIContext.OverlayTicker), RecordCountChange);
                });
            });
        }

        private async Task RefreshQuotesList(bool RecordCountChange = false)
        {
            await Task.Run(() =>
            {
                ThreadManager.AddTaskToGUIDispatcher(async () =>
                {
                    GUIContext.ChangeTracker.Clear();
                    await GUIContext.Quotes.LoadAsync();
                    Quotes.Clear();
                    Quotes.AddRange(GUIContext.Quotes.Local.ToList());
                    NotifyDataCollectionUpdated(nameof(GUIContext.Quotes), RecordCountChange);
                });
            });
        }

        private async Task RefreshShoutOutsList(bool RecordCountChange = false)
        {
            await Task.Run(() =>
            {
                ThreadManager.AddTaskToGUIDispatcher(async () =>
                {
                    GUIContext.ChangeTracker.Clear();
                    await GUIContext.ShoutOuts.LoadAsync();
                    ShoutOuts.Clear();
                    ShoutOuts.AddRange(GUIContext.ShoutOuts.Local.ToList());
                    NotifyDataCollectionUpdated(nameof(GUIContext.ShoutOuts), RecordCountChange);
                });
            });
        }

        private async Task RefreshStreamStatsList(bool RecordCountChange = false)
        {
            await Task.Run(() =>
            {
                ThreadManager.AddTaskToGUIDispatcher(async () =>
                {
                    GUIContext.ChangeTracker.Clear();
                    await GUIContext.StreamStats.LoadAsync();
                    StreamStats.Clear();
                    StreamStats.AddRange(GUIContext.StreamStats.Local.ToList());
                    NotifyDataCollectionUpdated(nameof(GUIContext.StreamStats), RecordCountChange);
                });
            });
        }

        private async Task RefreshUsersList(bool RecordCountChange = false)
        {
            await Task.Run(() =>
            {
                ThreadManager.AddTaskToGUIDispatcher(async () =>
                {
                    await GUIContext.UserStats.LoadAsync();
                    await GUIContext.Users.LoadAsync();
                    Users.Clear();
                    UserStats.Clear();
                    Users.AddRange(GUIContext.Users.Local.ToList());
                    UserStats.AddRange(GUIContext.UserStats.Local.ToList());
                    NotifyDataCollectionUpdated(nameof(GUIContext.Users), RecordCountChange);
                    NotifyDataCollectionUpdated(nameof(GUIContext.UserStats), RecordCountChange);
                });
            });
        }

        private async Task RefreshUserStatsList(bool RecordCountChange = false)
        {
            await Task.Run(() =>
            {
                ThreadManager.AddTaskToGUIDispatcher(async () =>
                {
                    GUIContext.ChangeTracker.Clear();
                    await GUIContext.UserStats.LoadAsync();
                    UserStats.Clear();
                    UserStats.AddRange(GUIContext.UserStats.Local.ToList());
                    NotifyDataCollectionUpdated(nameof(GUIContext.UserStats), RecordCountChange);
                });
            });
        }

        private async Task RefreshWebhooksList(bool RecordCountChange = false)
        {
            await Task.Run(() =>
            {
                ThreadManager.AddTaskToGUIDispatcher(async () =>
                {
                    GUIContext.ChangeTracker.Clear();
                    await GUIContext.Webhooks.LoadAsync();
                    Webhooks.Clear();
                    Webhooks.AddRange(GUIContext.Webhooks.Local.ToList());
                    NotifyDataCollectionUpdated(nameof(GUIContext.Webhooks), RecordCountChange);
                });
            });
        }

        #endregion
    }
}

#else

using Microsoft.EntityFrameworkCore;

using StreamerBotLib.DataSQL.Models;
using StreamerBotLib.Models.Enums;
using StreamerBotLib.Static;

using System.Collections.ObjectModel;

namespace StreamerBotLib.DataSQL.EFC9
{
    internal partial class DataManagerSQLAsync
    {
        private SQLDBContext GUIContext;

        #region LocalView ObservableCollection

        internal object GetICollection(DataTables dataTable)
        {
            LogWriter.DebugLog("GetObservableCollection", DebugLogTypes.DataManager,
                $"Getting the observable collection for {dataTable}.");

            return dataTable switch
            {
                DataTables.BanReasons => GetBanReasonsLocalObservableAsync().Result,
                DataTables.BanRules => GetBanRulesLocalObservableAsync().Result,
                DataTables.CategoryList => GetCategoryListLocalObservableAsync().Result,
                DataTables.ChannelEvents => GetChannelEventsLocalObservableAsync().Result,
                DataTables.Clips => GetClipsLocalObservableAsync().Result,
                DataTables.Commands => GetCommandsLocalObservableAsync().Result,
                DataTables.CommandsBase => throw new NotImplementedException(),
                DataTables.CommandsUser => GetCommandsUserLocalObservableAsync().Result,
                DataTables.Currency => GetCurrencyLocalObservableAsync().Result,
                DataTables.CurrencyType => GetCurrencyTypeLocalObservableAsync().Result,
                DataTables.CustomWelcome => GetCustomWelcomeLocalObservableAsync().Result,
                DataTables.Followers => GetFollowersLocalObservableAsync().Result,
                DataTables.GameDeadCounter => GetGameDeadCounterLocalObservableAsync().Result,
                DataTables.GiveawayUserData => GetGiveawayUserDataLocalObservableAsync().Result,
                DataTables.InRaidData => GetInRaidDataLocalObservableAsync().Result,
                DataTables.LearnMsgs => GetLearnMsgsLocalObservableAsync().Result,
                DataTables.ModeratorApprove => GetModeratorApproveLocalObservableAsync().Result,
                DataTables.MultiChannels => GetMultiChannelsLocalObservableAsync().Result,
                DataTables.MultiLiveStreams => GetMultiLiveStreamsLocalObservableAsync().Result,
                DataTables.MultiSummaryLiveStreams => GetMultiSummaryLiveStreamsLocalObservableAsync().Result,
                DataTables.MultiWebhooks => GetMultiWebhooksLocalObservableAsync().Result,
                DataTables.OldFollowUsers => GetOldFollowUsersLocalObservableAsync().Result,
                DataTables.OutRaidData => GetOutRaidDataLocalObservableAsync().Result,
                DataTables.OverlayServices => GetOverlayServicesLocalObservableAsync().Result,
                DataTables.OverlayTicker => GetOverlayTickerLocalObservableAsync().Result,
                DataTables.Quotes => GetQuotesLocalObservableAsync().Result,
                DataTables.ShoutOuts => GetShoutOutsLocalObservableAsync().Result,
                DataTables.StreamStats => GetStreamStatsLocalObservableAsync().Result,
                DataTables.UserBase => throw new NotImplementedException(),
                DataTables.Users => GetUsersLocalObservableAsync().Result,
                DataTables.UserStats => GetUserStatsLocalObservableAsync().Result,
                DataTables.Webhooks => GetWebhooksLocalObservableAsync().Result,
                DataTables.WebhooksBase => throw new NotImplementedException(),
                _ => throw new NotImplementedException()
            };
        }

        private Task<ObservableCollection<Models.BanReasons>> GetBanReasonsLocalObservableAsync()
        {
            return Task.Run(async () =>
            {
                await GUIContext.BanReasons.LoadAsync();
                return GUIContext.BanReasons.Local.ToObservableCollection();
            });
        }

        private Task<ObservableCollection<BanRules>> GetBanRulesLocalObservableAsync()
        {
            return Task.Run(async () =>
            {
                await GUIContext.BanRules.LoadAsync();
                return GUIContext.BanRules.Local.ToObservableCollection();
            });
        }

        private Task<ObservableCollection<CategoryList>> GetCategoryListLocalObservableAsync()
        {
            return Task.Run(async () =>
            {
                await GUIContext.CategoryList.LoadAsync();
                return GUIContext.CategoryList.Local.ToObservableCollection();
            });
        }

        private Task<ObservableCollection<ChannelEvents>> GetChannelEventsLocalObservableAsync()
        {
            return Task.Run(async () =>
            {
                await GUIContext.ChannelEvents.LoadAsync();
                return GUIContext.ChannelEvents.Local.ToObservableCollection();
            });
        }

        private Task<ObservableCollection<Clips>> GetClipsLocalObservableAsync()
        {
            return Task.Run(async () =>
            {
                await GUIContext.Clips.LoadAsync();
                return GUIContext.Clips.Local.ToObservableCollection();
            });
        }

        private Task<ObservableCollection<Commands>> GetCommandsLocalObservableAsync()
        {
            return Task.Run(async () =>
            {
                await GUIContext.Commands.LoadAsync();
                return GUIContext.Commands.Local.ToObservableCollection();
            });
        }

        private Task<ObservableCollection<CommandsUser>> GetCommandsUserLocalObservableAsync()
        {
            return Task.Run(async () =>
            {
                await GUIContext.CommandsUser.LoadAsync();
                return GUIContext.CommandsUser.Local.ToObservableCollection();
            });
        }

        private Task<ObservableCollection<Currency>> GetCurrencyLocalObservableAsync()
        {
            return Task.Run(async () =>
            {
                await GUIContext.Currency.LoadAsync();
                return GUIContext.Currency.Local.ToObservableCollection();
            });
        }

        private Task<ObservableCollection<Models.CurrencyType>> GetCurrencyTypeLocalObservableAsync()
        {
            return Task.Run(async () =>
            {
                await GUIContext.CurrencyType.LoadAsync();
                return GUIContext.CurrencyType.Local.ToObservableCollection();
            });
        }

        private Task<ObservableCollection<CustomWelcome>> GetCustomWelcomeLocalObservableAsync()
        {
            return Task.Run(async () =>
            {
                await GUIContext.CustomWelcome.LoadAsync();
                return GUIContext.CustomWelcome.Local.ToObservableCollection();
            });
        }

        private Task<ObservableCollection<Followers>> GetFollowersLocalObservableAsync()
        {
            return Task.Run(async () =>
            {
                await GUIContext.Followers.LoadAsync();
                return GUIContext.Followers.Local.ToObservableCollection();
            });
        }

        private Task<ObservableCollection<GameDeadCounter>> GetGameDeadCounterLocalObservableAsync()
        {
            return Task.Run(async () =>
            {
            await GUIContext.GameDeadCounter.LoadAsync();
            return GUIContext.GameDeadCounter.Local.ToObservableCollection();
            });
        }

        private Task<ObservableCollection<GiveawayUserData>> GetGiveawayUserDataLocalObservableAsync()
        {
            return Task.Run(async () =>
            {
            await GUIContext.GiveawayUserData.LoadAsync();
            return GUIContext.GiveawayUserData.Local.ToObservableCollection();
            });
        }

        private Task<ObservableCollection<InRaidData>> GetInRaidDataLocalObservableAsync()
        {
            return Task.Run(async () =>
            {
                await GUIContext.InRaidData.LoadAsync();
                return GUIContext.InRaidData.Local.ToObservableCollection();
            });
        }

        private Task<ObservableCollection<LearnMsgs>> GetLearnMsgsLocalObservableAsync()
        {
            return Task.Run(async () =>
            {
            await GUIContext.LearnMsgs.LoadAsync();
            return GUIContext.LearnMsgs.Local.ToObservableCollection();
            });
        }

        private Task<ObservableCollection<ModeratorApprove>> GetModeratorApproveLocalObservableAsync()
        {
            return Task.Run(async () =>
            {
            await GUIContext.ModeratorApprove.LoadAsync();
            return GUIContext.ModeratorApprove.Local.ToObservableCollection();
            });
        }

        private Task<ObservableCollection<MultiChannels>> GetMultiChannelsLocalObservableAsync()
        {
            return Task.Run(async () =>
            {
            await GUIContext.MultiChannels.LoadAsync();
            return GUIContext.MultiChannels.Local.ToObservableCollection();
            });
        }

        private Task<ObservableCollection<MultiLiveStreams>> GetMultiLiveStreamsLocalObservableAsync()
        {
            return Task.Run(async () =>
            {
            await GUIContext.MultiLiveStreams.LoadAsync();
            return GUIContext.MultiLiveStreams.Local.ToObservableCollection();
            });
        }

        private Task<ObservableCollection<MultiWebhooks>> GetMultiWebhooksLocalObservableAsync()
        {
            return Task.Run(async () =>
            {
                await GUIContext.MultiWebhooks.LoadAsync();
                return GUIContext.MultiWebhooks.Local.ToObservableCollection();
            });
        }

        private Task<ObservableCollection<MultiSummaryLiveStreams>> GetMultiSummaryLiveStreamsLocalObservableAsync()
        {
            return Task.Run(async () =>
            {
                await GUIContext.MultiSummaryLiveStreams.LoadAsync();
                return GUIContext.MultiSummaryLiveStreams.Local.ToObservableCollection();
            });
        }

        private Task<ObservableCollection<OldFollowUsers>> GetOldFollowUsersLocalObservableAsync()
        {
            return Task.Run(async () =>
            {
                await GUIContext.OldFollowUsers.LoadAsync();
                return GUIContext.OldFollowUsers.Local.ToObservableCollection();
            });
        }

        private Task<ObservableCollection<OutRaidData>> GetOutRaidDataLocalObservableAsync()
        {
            return Task.Run(async () =>
            {
                await GUIContext.OutRaidData.LoadAsync();
                return GUIContext.OutRaidData.Local.ToObservableCollection();
            });
        }

        private Task<ObservableCollection<OverlayServices>> GetOverlayServicesLocalObservableAsync()
        {
            return Task.Run(async () =>
            {
                await GUIContext.OverlayServices.LoadAsync();
                return GUIContext.OverlayServices.Local.ToObservableCollection();
            });
        }

        private Task<ObservableCollection<OverlayTicker>> GetOverlayTickerLocalObservableAsync()
        {
            return Task.Run(async () =>
            {
                await GUIContext.OverlayTicker.LoadAsync();
                return GUIContext.OverlayTicker.Local.ToObservableCollection();
            });
        }

        private Task<ObservableCollection<Quotes>> GetQuotesLocalObservableAsync()
        {
            return Task.Run(async () =>
            {
                await GUIContext.Quotes.LoadAsync();
                return GUIContext.Quotes.Local.ToObservableCollection();
            });
        }

        private Task<ObservableCollection<ShoutOuts>> GetShoutOutsLocalObservableAsync()
        {
            return Task.Run(async () =>
            {
                await GUIContext.ShoutOuts.LoadAsync();
                return GUIContext.ShoutOuts.Local.ToObservableCollection();
            });
        }

        private Task<ObservableCollection<StreamStats>> GetStreamStatsLocalObservableAsync()
        {
            return Task.Run(async () =>
            {
                await GUIContext.StreamStats.LoadAsync();
                return GUIContext.StreamStats.Local.ToObservableCollection();
            });
        }

        private Task<ObservableCollection<Users>> GetUsersLocalObservableAsync()
        {
            return Task.Run(async () =>
            {
                await GUIContext.Users.LoadAsync();
                return GUIContext.Users.Local.ToObservableCollection();
            });
        }

        private Task<ObservableCollection<UserStats>> GetUserStatsLocalObservableAsync()
        {
            return Task.Run(async () =>
            {
                await GUIContext.UserStats.LoadAsync();
                return GUIContext.UserStats.Local.ToObservableCollection();
            });
        }

        private Task<ObservableCollection<Webhooks>> GetWebhooksLocalObservableAsync()
        {
            return Task.Run(async () =>
            {
                await GUIContext.Webhooks.LoadAsync();
                return GUIContext.Webhooks.Local.ToObservableCollection();
            });
        }

        #endregion

        internal void NotifyDataCollectionUpdated(string TableName, bool RecordCountChange = false)
        {
            OnDataCollectionUpdated?.Invoke(this, new(TableName, RecordCountChange));
        }

        #region Refresh Collections
        private async Task RefreshBanReasonsList(bool RecordCountChange = false)
        {
            await GUIContext.BanReasons.LoadAsync();
            NotifyDataCollectionUpdated(nameof(GUIContext.BanReasons), RecordCountChange);
        }

        private async Task RefreshBanRulesList(bool RecordCountChange = false)
        {
            await GUIContext.BanRules.LoadAsync();
            NotifyDataCollectionUpdated(nameof(GUIContext.BanRules), RecordCountChange);
        }

        private async Task RefreshCategoryListList(bool RecordCountChange = false)
        {
            await GUIContext.CategoryList.LoadAsync();
            NotifyDataCollectionUpdated(nameof(GUIContext.CategoryList), RecordCountChange);
        }

        private async Task RefreshChannelEventsList(bool RecordCountChange = false)
        {
            await GUIContext.ChannelEvents.LoadAsync();
            NotifyDataCollectionUpdated(nameof(GUIContext.ChannelEvents), RecordCountChange);
        }

        private async Task RefreshClipsList(bool RecordCountChange = false)
        {
            await GUIContext.Clips.LoadAsync();
            NotifyDataCollectionUpdated(nameof(GUIContext.Clips), RecordCountChange);
        }

        private async Task RefreshCommandsList(bool RecordCountChange = false)
        {
            await GUIContext.Commands.LoadAsync();

            NotifyDataCollectionUpdated(nameof(GUIContext.Commands), RecordCountChange);
        }

        private async Task RefreshCommandsUserList(bool RecordCountChange = false)
        {
            await GUIContext.CommandsUser.LoadAsync();
            NotifyDataCollectionUpdated(nameof(GUIContext.CommandsUser), RecordCountChange);
        }

        private async Task RefreshCurrencyList(bool RecordCountChange = false)
        {
            await GUIContext.Currency.LoadAsync();
            NotifyDataCollectionUpdated(nameof(GUIContext.Currency), RecordCountChange);
        }

        private async Task RefreshCurrencyTypeList(bool RecordCountChange = false)
        {
            await GUIContext.CurrencyType.LoadAsync();
            NotifyDataCollectionUpdated(nameof(GUIContext.CurrencyType), RecordCountChange);
        }

        private async Task RefreshCustomWelcomeList(bool RecordCountChange = false)
        {
            await GUIContext.CustomWelcome.LoadAsync();
            NotifyDataCollectionUpdated(nameof(GUIContext.CustomWelcome), RecordCountChange);
        }

        private async Task RefreshFollowersList(bool RecordCountChange = false)
        {
            await GUIContext.Followers.LoadAsync();
            NotifyDataCollectionUpdated(nameof(GUIContext.Followers), RecordCountChange);
        }

        private async Task RefreshGameDeadCounterList(bool RecordCountChange = false)
        {
            await GUIContext.GameDeadCounter.LoadAsync();
            NotifyDataCollectionUpdated(nameof(GUIContext.GameDeadCounter), RecordCountChange);
        }

        private async Task RefreshGiveawayUserDataList(bool RecordCountChange = false)
        {
            await GUIContext.GiveawayUserData.LoadAsync();
            NotifyDataCollectionUpdated(nameof(GUIContext.GiveawayUserData), RecordCountChange);
        }

        private async Task RefreshInRaidDataList(bool RecordCountChange = false)
        {
            await GUIContext.InRaidData.LoadAsync();
            NotifyDataCollectionUpdated(nameof(GUIContext.InRaidData), RecordCountChange);
        }

        private async Task RefreshLearnMsgsList(bool RecordCountChange = false)
        {
            await GUIContext.LearnMsgs.LoadAsync();
            NotifyDataCollectionUpdated(nameof(GUIContext.LearnMsgs), RecordCountChange);
        }

        private async Task RefreshModeratorApproveList(bool RecordCountChange = false)
        {
            await GUIContext.ModeratorApprove.LoadAsync();
            NotifyDataCollectionUpdated(nameof(GUIContext.ModeratorApprove), RecordCountChange);
        }

        private async Task RefreshMultiChannelsList(bool RecordCountChange = false)
        {
            await GUIContext.MultiChannels.LoadAsync();
            NotifyDataCollectionUpdated(nameof(GUIContext.MultiChannels), RecordCountChange);
        }

        private async Task RefreshMultiLiveStreamsList(bool RecordCountChange = false)
        {
            await GUIContext.MultiLiveStreams.LoadAsync();
            NotifyDataCollectionUpdated(nameof(GUIContext.MultiLiveStreams), RecordCountChange);
        }

        private async Task RefreshMultiSummaryLiveStreamsList(bool RecordCountChange = false)
        {
            await GUIContext.MultiSummaryLiveStreams.LoadAsync();
            NotifyDataCollectionUpdated(nameof(GUIContext.MultiSummaryLiveStreams), RecordCountChange);
        }

        private async Task RefreshMultiWebhooksList(bool RecordCountChange = false)
        {
            await GUIContext.MultiWebhooks.LoadAsync();
            NotifyDataCollectionUpdated(nameof(GUIContext.MultiWebhooks), RecordCountChange);
        }

        private async Task RefreshOldFollowUsersList(bool RecordCountChange = false)
        {
            await GUIContext.OldFollowUsers.LoadAsync();
            NotifyDataCollectionUpdated(nameof(GUIContext.OldFollowUsers), RecordCountChange);
        }

        private async Task RefreshOutRaidDataList(bool RecordCountChange = false)
        {
            await GUIContext.OutRaidData.LoadAsync();
            NotifyDataCollectionUpdated(nameof(GUIContext.OutRaidData), RecordCountChange);
        }

        private async Task RefreshOverlayServicesList(bool RecordCountChange = false)
        {
            await GUIContext.OverlayServices.LoadAsync();
            NotifyDataCollectionUpdated(nameof(GUIContext.OverlayServices), RecordCountChange);
        }

        private async Task RefreshOverlayTickerList(bool RecordCountChange = false)
        {
            await GUIContext.OverlayTicker.LoadAsync();
            NotifyDataCollectionUpdated(nameof(GUIContext.OverlayTicker), RecordCountChange);
        }

        private async Task RefreshQuotesList(bool RecordCountChange = false)
        {
            await GUIContext.Quotes.LoadAsync();
            NotifyDataCollectionUpdated(nameof(GUIContext.Quotes), RecordCountChange);
        }

        private async Task RefreshShoutOutsList(bool RecordCountChange = false)
        {
            await GUIContext.ShoutOuts.LoadAsync();
            NotifyDataCollectionUpdated(nameof(GUIContext.ShoutOuts), RecordCountChange);
        }

        private async Task RefreshStreamStatsList(bool RecordCountChange = false)
        {
            await GUIContext.StreamStats.LoadAsync();
            NotifyDataCollectionUpdated(nameof(GUIContext.StreamStats), RecordCountChange);
        }

        private async Task RefreshUsersList(bool RecordCountChange = false)
        {
            await GUIContext.UserStats.LoadAsync();
            await GUIContext.Users.LoadAsync();
            NotifyDataCollectionUpdated(nameof(GUIContext.Users), RecordCountChange);
            NotifyDataCollectionUpdated(nameof(GUIContext.UserStats), RecordCountChange);
        }

        private async Task RefreshUserStatsList(bool RecordCountChange = false)
        {
            await GUIContext.UserStats.LoadAsync();
            NotifyDataCollectionUpdated(nameof(GUIContext.UserStats), RecordCountChange);
        }

        private async Task RefreshWebhooksList(bool RecordCountChange = false)
        {
            await GUIContext.Webhooks.LoadAsync();
            NotifyDataCollectionUpdated(nameof(GUIContext.Webhooks), RecordCountChange);
        }

        #endregion
    }
}

#endif
