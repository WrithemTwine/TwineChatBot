#define USE_OBSERVABLECOLLECTION
#define BUNDLE_REFRESHPACKAGE

using Microsoft.EntityFrameworkCore;

using StreamerBotLib.DataSQL.Models;
using StreamerBotLib.Enums;
using StreamerBotLib.Static;

#if USE_OBSERVABLECOLLECTION
using System.Collections.ObjectModel;
#endif

namespace StreamerBotLib.DataSQL.MultiContext
{
    internal partial class DataManagerSQLAsync
    {

#if USE_OBSERVABLECOLLECTION

        #region LocalView ObservableCollection

        private SQLDBContext GUIContext;

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
        private void RefreshBanReasonsList(bool RecordCountChange = false)
        {
            //PostActionQueue(new Task(() =>
            //{
                ThreadManager.AddAsyncTaskToGUIDispatcher("RefreshUserStatsObservableCollection", async () =>
                {
                    await GUIContext.BanReasons.LoadAsync();
                    NotifyDataCollectionUpdated(nameof(GUIContext.BanReasons), RecordCountChange);
                });
            //}), "BanReasons");
        }

        private void RefreshBanRulesList(bool RecordCountChange = false)
        {
            //PostActionQueue(new Task(() =>
            //{
                ThreadManager.AddAsyncTaskToGUIDispatcher("RefreshUserStatsObservableCollection", async () =>
                {
                    await GUIContext.BanRules.LoadAsync();
                    NotifyDataCollectionUpdated(nameof(GUIContext.BanRules), RecordCountChange);
                });
            //}), "BanRules");
        }

        private void RefreshCategoryListList(bool RecordCountChange = false)
        {
            //PostActionQueue(new Task(() =>
            //{
                ThreadManager.AddAsyncTaskToGUIDispatcher("RefreshUserStatsObservableCollection", async () =>
                {
                    await GUIContext.CategoryList.LoadAsync();
                    NotifyDataCollectionUpdated(nameof(GUIContext.CategoryList), RecordCountChange);
                });
            //}), "CategoryList");
        }

        private void RefreshChannelEventsList(bool RecordCountChange = false)
        {
            //PostActionQueue(new Task(() =>
            //{
                ThreadManager.AddAsyncTaskToGUIDispatcher("RefreshUserStatsObservableCollection", async () =>
                {
                    await GUIContext.ChannelEvents.LoadAsync();
                    NotifyDataCollectionUpdated(nameof(GUIContext.ChannelEvents), RecordCountChange);
                });
            //}), "ChannelEvents");
        }

        private void RefreshClipsList(bool RecordCountChange = false)
        {
            //PostActionQueue(new Task(() =>
            //{
                ThreadManager.AddAsyncTaskToGUIDispatcher("RefreshUserStatsObservableCollection", async () =>
                {
                    await GUIContext.Clips.LoadAsync();
                    NotifyDataCollectionUpdated(nameof(GUIContext.Clips), RecordCountChange);
                });
            //}), "Clips");
        }

        private void RefreshCommandsList(bool RecordCountChange = false)
        {
            //PostActionQueue(new Task(() =>
            //{
                ThreadManager.AddAsyncTaskToGUIDispatcher("RefreshUserStatsObservableCollection", async () =>
                {
                    await GUIContext.Commands.LoadAsync();
                    NotifyDataCollectionUpdated(nameof(GUIContext.Commands), RecordCountChange);
                });
            //}), "Commands");
        }

        private void RefreshCommandsUserList(bool RecordCountChange = false)
        {
            //PostActionQueue(new Task(() =>
            //{
                ThreadManager.AddAsyncTaskToGUIDispatcher("RefreshUserStatsObservableCollection", async () =>
                {
                    await GUIContext.CommandsUser.LoadAsync();
                    NotifyDataCollectionUpdated(nameof(GUIContext.CommandsUser), RecordCountChange);
                });
            //}), "CommandsUser");
        }

        private void RefreshCurrencyList(bool RecordCountChange = false)
        {
            //PostActionQueue(new Task(() =>
            //{
                ThreadManager.AddTaskToGUIDispatcher(async () =>
                {
                    await GUIContext.Currency.LoadAsync();
                    NotifyDataCollectionUpdated(nameof(GUIContext.Currency), RecordCountChange);
                });
            //}), "Currency");
        }

        private void RefreshCurrencyTypeList(bool RecordCountChange = false)
        {
            //PostActionQueue(new Task(() =>
            //{
                ThreadManager.AddAsyncTaskToGUIDispatcher("RefreshUserStatsObservableCollection", async () =>
                {
                    await GUIContext.CurrencyType.LoadAsync();
                    NotifyDataCollectionUpdated(nameof(GUIContext.CurrencyType), RecordCountChange);
                });
            //}), "CurrencyType");
        }

        private void RefreshCustomWelcomeList(bool RecordCountChange = false)
        {
            //PostActionQueue(new Task(() =>
            //{
                ThreadManager.AddAsyncTaskToGUIDispatcher("RefreshUserStatsObservableCollection", async () =>
                {
                    await GUIContext.CustomWelcome.LoadAsync();
                    NotifyDataCollectionUpdated(nameof(GUIContext.CustomWelcome), RecordCountChange);
                });
            //}), "CustomWelcome");
        }

        private void RefreshFollowersList(bool RecordCountChange = false)
        {
            //PostActionQueue(new Task(() =>
            //{
                ThreadManager.AddAsyncTaskToGUIDispatcher("RefreshUserStatsObservableCollection", async () =>
                {
                    await GUIContext.Followers.LoadAsync();
                    NotifyDataCollectionUpdated(nameof(GUIContext.Followers), RecordCountChange);
                });
            //}), "Followers");
        }

        private void RefreshGameDeadCounterList(bool RecordCountChange = false)
        {
            //PostActionQueue(new Task(() =>
            //{
                ThreadManager.AddAsyncTaskToGUIDispatcher("RefreshUserStatsObservableCollection", async () =>
                {
                    await GUIContext.GameDeadCounter.LoadAsync();
                    NotifyDataCollectionUpdated(nameof(GUIContext.GameDeadCounter), RecordCountChange);
                });
            //}), "GameDeadCounter");
        }

        private void RefreshGiveawayUserDataList(bool RecordCountChange = false)
        {
            //PostActionQueue(new Task(() =>
            //{
                ThreadManager.AddAsyncTaskToGUIDispatcher("RefreshUserStatsObservableCollection", async () =>
                {
                    await GUIContext.GiveawayUserData.LoadAsync();
                    NotifyDataCollectionUpdated(nameof(GUIContext.GiveawayUserData), RecordCountChange);
                });
            //}), "GiveawayUser");
        }

        private void RefreshInRaidDataList(bool RecordCountChange = false)
        {
            //PostActionQueue(new Task(() =>
            //{
                ThreadManager.AddAsyncTaskToGUIDispatcher("RefreshUserStatsObservableCollection", async () =>
                {
                    await GUIContext.InRaidData.LoadAsync();
                    NotifyDataCollectionUpdated(nameof(GUIContext.InRaidData), RecordCountChange);
                });
            //}), "InRaidData");
        }

        private void RefreshLearnMsgsList(bool RecordCountChange = false)
        {
            //PostActionQueue(new Task(() =>
            //{
                ThreadManager.AddAsyncTaskToGUIDispatcher("RefreshUserStatsObservableCollection", async () =>
                {
                    await GUIContext.LearnMsgs.LoadAsync();
                    NotifyDataCollectionUpdated(nameof(GUIContext.LearnMsgs), RecordCountChange);
                });
            //}), "LearnMsgs");
        }

        private void RefreshModeratorApproveList(bool RecordCountChange = false)
        {
            //PostActionQueue(new Task(() =>
            //{
                ThreadManager.AddAsyncTaskToGUIDispatcher("RefreshUserStatsObservableCollection", async () =>
                {
                    await GUIContext.ModeratorApprove.LoadAsync();
                    NotifyDataCollectionUpdated(nameof(GUIContext.ModeratorApprove), RecordCountChange);
                });
            //}), "ModeratorApprove");
        }

        private void RefreshMultiChannelsList(bool RecordCountChange = false)
        {
            //PostActionQueue(new Task(() =>
            //{
                ThreadManager.AddAsyncTaskToGUIDispatcher("RefreshUserStatsObservableCollection", async () =>
                {
                    await GUIContext.MultiChannels.LoadAsync();
                    NotifyDataCollectionUpdated(nameof(GUIContext.MultiChannels), RecordCountChange);
                });
            //}), "MultiChannels");
        }

        private void RefreshMultiLiveStreamsList(bool RecordCountChange = false)
        {
            //PostActionQueue(new Task(() =>
            //{
                ThreadManager.AddAsyncTaskToGUIDispatcher("RefreshUserStatsObservableCollection", async () =>
                {
                    await GUIContext.MultiLiveStreams.LoadAsync();
                    NotifyDataCollectionUpdated(nameof(GUIContext.MultiLiveStreams), RecordCountChange);
                });
            //}), "MultiLiveStreams");
        }

        private void RefreshMultiSummaryLiveStreamsList(bool RecordCountChange = false)
        {
            //PostActionQueue(new Task(() =>
            //{
                ThreadManager.AddAsyncTaskToGUIDispatcher("RefreshUserStatsObservableCollection", async () =>
                {
                    await GUIContext.MultiSummaryLiveStreams.LoadAsync();
                    NotifyDataCollectionUpdated(nameof(GUIContext.MultiSummaryLiveStreams), RecordCountChange);
                });
            //}), "MultiSummaryLiveStreams");
        }

        private void RefreshMultiWebhooksList(bool RecordCountChange = false)
        {
            //PostActionQueue(new Task(() =>
            //{
                ThreadManager.AddAsyncTaskToGUIDispatcher("RefreshUserStatsObservableCollection", async () =>
                {
                    await GUIContext.MultiWebhooks.LoadAsync();
                    NotifyDataCollectionUpdated(nameof(GUIContext.MultiWebhooks), RecordCountChange);
                });
            //}), "MultiWebhooks");
        }

        private void RefreshOldFollowUsersList(bool RecordCountChange = false)
        {
            //PostActionQueue(new Task(() =>
            //{
                ThreadManager.AddAsyncTaskToGUIDispatcher("RefreshUserStatsObservableCollection", async () =>
                {
                    await GUIContext.OldFollowUsers.LoadAsync();
                    NotifyDataCollectionUpdated(nameof(GUIContext.OldFollowUsers), RecordCountChange);
                });
            //}), "OldFollowUsers");
        }

        private void RefreshOutRaidDataList(bool RecordCountChange = false)
        {
            //PostActionQueue(new Task(() =>
            //{
                ThreadManager.AddAsyncTaskToGUIDispatcher("RefreshUserStatsObservableCollection", async () =>
                {
                    await GUIContext.OutRaidData.LoadAsync();
                    NotifyDataCollectionUpdated(nameof(GUIContext.OutRaidData), RecordCountChange);
                });
            //}), "OutRaidData");
        }

        private void RefreshOverlayServicesList(bool RecordCountChange = false)
        {
            //PostActionQueue(new Task(() =>
            //{
                ThreadManager.AddAsyncTaskToGUIDispatcher("RefreshUserStatsObservableCollection", async () =>
                {
                    await GUIContext.OverlayServices.LoadAsync();
                    NotifyDataCollectionUpdated(nameof(GUIContext.OverlayServices), RecordCountChange);
                });
            //}), "OverlayServices");
        }

        private void RefreshOverlayTickerList(bool RecordCountChange = false)
        {
            //PostActionQueue(new Task(() =>
            //{
                ThreadManager.AddAsyncTaskToGUIDispatcher("RefreshUserStatsObservableCollection", async () =>
                {
                    await GUIContext.OverlayTicker.LoadAsync();
                    NotifyDataCollectionUpdated(nameof(GUIContext.OverlayTicker), RecordCountChange);
                });
            //}), "OverlayTicker");
        }

        private void RefreshQuotesList(bool RecordCountChange = false)
        {
            //PostActionQueue(new Task(() =>
            //{
                ThreadManager.AddAsyncTaskToGUIDispatcher("RefreshUserStatsObservableCollection", async () =>
                {
                    await GUIContext.Quotes.LoadAsync();
                    NotifyDataCollectionUpdated(nameof(GUIContext.Quotes), RecordCountChange);
                });
            //}), "Quotes");
        }

        private void RefreshShoutOutsList(bool RecordCountChange = false)
        {
            //PostActionQueue(new Task(() =>
            //{
                ThreadManager.AddAsyncTaskToGUIDispatcher("RefreshUserStatsObservableCollection", async () =>
                {
                    await GUIContext.ShoutOuts.LoadAsync();
                    NotifyDataCollectionUpdated(nameof(GUIContext.ShoutOuts), RecordCountChange);
                });
            //}), "ShoutOUts");
        }

        private void RefreshStreamStatsList(bool RecordCountChange = false)
        {
            //PostActionQueue(new Task(() =>
            //{
                ThreadManager.AddAsyncTaskToGUIDispatcher("RefreshUserStatsObservableCollection", async () =>
                {
                    await GUIContext.StreamStats.LoadAsync();
                    NotifyDataCollectionUpdated(nameof(GUIContext.StreamStats), RecordCountChange);
                });
            //}), "StreamStats");
        }

        private void RefreshUsersList(bool RecordCountChange = false)
        {
            //PostActionQueue(new Task(() =>
            //{
                ThreadManager.AddAsyncTaskToGUIDispatcher("RefreshUsersObservableCollection", async () =>
                {
#if DEBUG
                    LogWriter.DebugLog("RefreshUsersObservableCollection",
                            DebugLogTypes.SpecialPurpose, $"Reloading Users data into the database context.");
#endif

                    LogWriter.DebugLog("RefreshUsersObservableCollection",
                            DebugLogTypes.DataManager, $"Reloading Users data into the database context.");
                    await GUIContext.UserStats.LoadAsync();
                    await GUIContext.Users.LoadAsync();
                    LogWriter.DebugLog("RefreshUsersObservableCollection",
                            DebugLogTypes.DataManager, $"Notifying the DataCollection is Updated.");
                    NotifyDataCollectionUpdated(nameof(GUIContext.Users), RecordCountChange);
                });
            //}), "Users");
        }

        private void RefreshUserStatsList(bool RecordCountChange = false)
        {
            //PostActionQueue(new Task(() =>
            //{
                ThreadManager.AddAsyncTaskToGUIDispatcher("RefreshUserStatsObservableCollection", async () =>
                {
                    LogWriter.DebugLog("RefreshUserStatsObservableCollection",
                      DebugLogTypes.DataManager, $"Reloading UserStats data into the database context.");
                    await GUIContext.UserStats.LoadAsync();
                    LogWriter.DebugLog("RefreshUserStatsObservableCollection",
       DebugLogTypes.DataManager, $"Notifying the DataCollection is Updated.");
                    NotifyDataCollectionUpdated(nameof(GUIContext.UserStats), RecordCountChange);
                });
            //}), "UserStats");
        }

        private void RefreshWebhooksList(bool RecordCountChange = false)
        {
            //PostActionQueue(new Task(() =>
            //{
                ThreadManager.AddAsyncTaskToGUIDispatcher("RefreshWebhooksObservableCollection", async () =>
                {
                    await GUIContext.Webhooks.LoadAsync();
                    NotifyDataCollectionUpdated(nameof(GUIContext.Webhooks), RecordCountChange);
                });
            //}), "Webhooks");
        }

        #endregion

#elif BUNDLE_REFRESHPACKAGE

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
                return GUIContext.BanReasons.Local.ToList();
            });
        }

        private Task<List<BanRules>> GetBanRulesLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetBanRulesLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading BanRules data into the database context.");
                await GUIContext.BanRules.LoadAsync();
                return GUIContext.BanRules.Local.ToList();
            });
        }

        private Task<List<CategoryList>> GetCategoryListLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetCategoryListLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading CategoryList data into the database context.");
                await GUIContext.CategoryList.LoadAsync();
                return GUIContext.CategoryList.Local.ToList();
            });
        }

        private Task<List<ChannelEvents>> GetChannelEventsLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetChannelEventsLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading ChannelEvents data into the database context.");
                await GUIContext.ChannelEvents.LoadAsync();
                return GUIContext.ChannelEvents.Local.ToList();
            });
        }

        private Task<List<Clips>> GetClipsLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetClipsLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading Clips data into the database context.");
                await GUIContext.Clips.LoadAsync();
                return GUIContext.Clips.Local.ToList();
            });
        }

        private Task<List<Commands>> GetCommandsLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetCommandsLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading Commands data into the database context.");
                await GUIContext.Commands.LoadAsync();
                return GUIContext.Commands.Local.ToList();
            });
        }

        private Task<List<CommandsUser>> GetCommandsUserLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetCommandsUserLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading CommandsUser data into the database context.");
                await GUIContext.CommandsUser.LoadAsync();
                return GUIContext.CommandsUser.Local.ToList();
            });
        }

        private Task<List<Currency>> GetCurrencyLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetCurrencyLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading Currency data into the database context.");
                await GUIContext.Currency.LoadAsync();
                return GUIContext.Currency.Local.ToList();
            });
        }

        private Task<List<Models.CurrencyType>> GetCurrencyTypeLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetCurrencyTypeLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading CurrencyType data into the database context.");
                await GUIContext.CurrencyType.LoadAsync();
                return GUIContext.CurrencyType.Local.ToList();
            });
        }

        private Task<List<CustomWelcome>> GetCustomWelcomeLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetCustomWelcomeLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading CustomWelcome data into the database context.");
                await GUIContext.CustomWelcome.LoadAsync();
                return GUIContext.CustomWelcome.Local.ToList();
            });
        }

        private Task<List<Followers>> GetFollowersLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetFollowersLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading Followers data into the database context.");
                await GUIContext.Followers.LoadAsync();
                return GUIContext.Followers.Local.ToList();
            });
        }

        private Task<List<GameDeadCounter>> GetGameDeadCounterLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetGameDeadCounterLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading GameDeadCounter data into the database context.");

                await GUIContext.GameDeadCounter.LoadAsync();
                return GUIContext.GameDeadCounter.Local.ToList();
            });
        }

        private Task<List<GiveawayUserData>> GetGiveawayUserDataLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetGiveawayUserDataLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading GiveawayUserData data into the database context.");

                await GUIContext.GiveawayUserData.LoadAsync();
                return GUIContext.GiveawayUserData.Local.ToList();
            });
        }

        private Task<List<InRaidData>> GetInRaidDataLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetInRaidDataLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading InRaidData data into the database context.");
                await GUIContext.InRaidData.LoadAsync();
                return GUIContext.InRaidData.Local.ToList();
            });
        }

        private Task<List<LearnMsgs>> GetLearnMsgsLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetLearnMsgsLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading LearnMsgs data into the database context.");
                await GUIContext.LearnMsgs.LoadAsync();
                return GUIContext.LearnMsgs.Local.ToList();
            });
        }

        private Task<List<ModeratorApprove>> GetModeratorApproveLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetModeratorApproveLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading ModeratorApprove data into the database context.");
                await GUIContext.ModeratorApprove.LoadAsync();
                return GUIContext.ModeratorApprove.Local.ToList();
            });
        }

        private Task<List<MultiChannels>> GetMultiChannelsLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetMultiChannelsLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading MultiChannels data into the database context.");
                await GUIContext.MultiChannels.LoadAsync();
                return GUIContext.MultiChannels.Local.ToList();
            });
        }

        private Task<List<MultiLiveStreams>> GetMultiLiveStreamsLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetMultiLiveStreamsLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading MultiLiveStreams data into the database context.");
                await GUIContext.MultiLiveStreams.LoadAsync();
                return GUIContext.MultiLiveStreams.Local.ToList();
            });
        }

        private Task<List<MultiWebhooks>> GetMultiWebhooksLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetMultiWebhooksLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading MultiWebhooks data into the database context.");
                await GUIContext.MultiWebhooks.LoadAsync();
                return GUIContext.MultiWebhooks.Local.ToList();
            });
        }

        private Task<List<MultiSummaryLiveStreams>> GetMultiSummaryLiveStreamsLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetMultiSummaryLiveStreamsLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading MultiSummaryLiveStreams data into the database context.");
                await GUIContext.MultiSummaryLiveStreams.LoadAsync();
                return GUIContext.MultiSummaryLiveStreams.Local.ToList();
            });
        }

        private Task<List<OldFollowUsers>> GetOldFollowUsersLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetOldFollowUsersLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading OldFollowUsers data into the database context.");
                await GUIContext.OldFollowUsers.LoadAsync();
                return GUIContext.OldFollowUsers.Local.ToList();
            });
        }

        private Task<List<OutRaidData>> GetOutRaidDataLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetOutRaidDataLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading OutRaidData data into the database context.");
                await GUIContext.OutRaidData.LoadAsync();
                return GUIContext.OutRaidData.Local.ToList();
            });
        }

        private Task<List<OverlayServices>> GetOverlayServicesLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetOverlayServicesLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading OverlayServices data into the database context.");
                await GUIContext.OverlayServices.LoadAsync();
                return GUIContext.OverlayServices.Local.ToList();
            });
        }

        private Task<List<OverlayTicker>> GetOverlayTickerLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetOverlayTickerLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading OverlayTicker data into the database context.");
                await GUIContext.OverlayTicker.LoadAsync();
                return GUIContext.OverlayTicker.Local.ToList();
            });
        }

        private Task<List<Quotes>> GetQuotesLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetQuotesLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading Quotes data into the database context.");
                await GUIContext.Quotes.LoadAsync();
                return GUIContext.Quotes.Local.ToList();
            });
        }

        private Task<List<ShoutOuts>> GetShoutOutsLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetShoutOutsLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading ShoutOuts data into the database context.");
                await GUIContext.ShoutOuts.LoadAsync();
                return GUIContext.ShoutOuts.Local.ToList();
            });
        }

        private Task<List<StreamStats>> GetStreamStatsLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetStreamStatsLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading StreamStats data into the database context.");
                await GUIContext.StreamStats.LoadAsync();
                return GUIContext.StreamStats.Local.ToList();
            });
        }

        private Task<List<Users>> GetUsersLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetUsersLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading Users data into the database context.");
                await GUIContext.Users.LoadAsync();
                return GUIContext.Users.Local.ToList();
            });
        }

        private Task<List<UserStats>> GetUserStatsLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetUserStatsLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading UserStats data into the database context.");
                await GUIContext.UserStats.LoadAsync();
                return GUIContext.UserStats.Local.ToList();
            });
        }

        private Task<List<Webhooks>> GetWebhooksLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetWebhooksLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading Webhooks data into the database context.");
                await GUIContext.Webhooks.LoadAsync();
                return GUIContext.Webhooks.Local.ToList();
            });
        }

        #endregion

        internal void NotifyDataCollectionUpdated(string TableName, Action action = null, string datatable = null)
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
            PostActionQueue(() =>
            {
                NotifyDataCollectionUpdated(nameof(GUIContext.BanReasons), () => GUIContext.BanReasons.LoadAsync(), null);
            }, "RefreshBanReasonsList");
        }

        private void RefreshBanRulesList()
        {
            LogWriter.DebugLog("RefreshBanRulesList", DebugLogTypes.DataManager,
                $"Reloading BanRules data into the database context.");
            PostActionQueue(() =>
            {
                NotifyDataCollectionUpdated(nameof(GUIContext.BanRules), () => GUIContext.BanRules.LoadAsync(), null);
            }, "RefreshBanRulesList");
        }

        private void RefreshCategoryListList()
        {
            LogWriter.DebugLog("RefreshCategoryListList", DebugLogTypes.DataManager,
                $"Reloading CategoryList data into the database context.");
            PostActionQueue(() =>
            {
                NotifyDataCollectionUpdated(nameof(GUIContext.CategoryList), () => GUIContext.CategoryList.LoadAsync(), null);
            }, "RefreshCategoryList");
        }

        private void RefreshChannelEventsList()
        {
            LogWriter.DebugLog("RefreshChannelEventsList", DebugLogTypes.DataManager,
                $"Reloading ChannelEvents data into the database context.");
            PostActionQueue(() =>
            {
                NotifyDataCollectionUpdated(nameof(GUIContext.ChannelEvents), () => GUIContext.ChannelEvents.LoadAsync(), null);
            }, "RefreshChannelEventsList");
        }

        private void RefreshClipsList()
        {
            LogWriter.DebugLog("RefreshClipsList", DebugLogTypes.DataManager,
                $"Reloading Clips data into the database context.");
            PostActionQueue(() =>
            {
                NotifyDataCollectionUpdated(nameof(GUIContext.Clips), () => GUIContext.Clips.LoadAsync(), null);
            }, "RefreshClipsList");
        }

        private void RefreshCommandsList()
        {
            LogWriter.DebugLog("RefreshCommandsList", DebugLogTypes.DataManager,
                $"Reloading Commands data into the database context.");
            PostActionQueue(() =>
            {
                NotifyDataCollectionUpdated(nameof(GUIContext.Commands), () => GUIContext.Commands.LoadAsync(), null);
            }, "RefreshCommandsList");
        }

        private void RefreshCommandsUserList()
        {
            LogWriter.DebugLog("RefreshCommandsUserList", DebugLogTypes.DataManager,
                $"Reloading CommandsUser data into the database context.");
            PostActionQueue(() =>
            {
                NotifyDataCollectionUpdated(nameof(GUIContext.CommandsUser), () => GUIContext.CommandsUser.LoadAsync(), null);
            }, "RefreshCommandsUserList");
        }

        private void RefreshCurrencyList()
        {
            LogWriter.DebugLog("RefreshCurrencyList", DebugLogTypes.DataManager,
                $"Reloading Currency data into the database context.");
            PostActionQueue(() =>
            {
                NotifyDataCollectionUpdated(nameof(GUIContext.Currency), () => GUIContext.Currency.LoadAsync(), null);
            }, "RefreshCurrencyList");
        }

        private void RefreshCurrencyTypeList()
        {
            LogWriter.DebugLog("RefreshCurrencyTypeList", DebugLogTypes.DataManager,
                $"Reloading CurrencyType data into the database context.");
            PostActionQueue(() =>
            {
                NotifyDataCollectionUpdated(nameof(GUIContext.CurrencyType), () => GUIContext.CurrencyType.LoadAsync(), null);
            }, "RefreshCurrencyTypeList");
        }

        private void RefreshCustomWelcomeList()
        {
            LogWriter.DebugLog("RefreshCustomWelcomeList", DebugLogTypes.DataManager,
                $"Reloading CustomWelcome data into the database context.");
            PostActionQueue(() =>
            {
                NotifyDataCollectionUpdated(nameof(GUIContext.CustomWelcome), () => GUIContext.CustomWelcome.LoadAsync(), null);
            }, "RefreshCustomWelcomeList");
        }

        private void RefreshFollowersList()
        {
            LogWriter.DebugLog("RefreshFollowersList", DebugLogTypes.DataManager,
                $"Reloading Followers data into the database context.");
            PostActionQueue(() =>
            {
                NotifyDataCollectionUpdated(nameof(GUIContext.Followers), () => GUIContext.Followers.LoadAsync(), null);
            }, "RefreshFollowersList");
        }

        private void RefreshGameDeadCounterList()
        {
            LogWriter.DebugLog("RefreshGameDeadCounterList", DebugLogTypes.DataManager,
                $"Reloading GameDeadCounter data into the database context.");
            PostActionQueue(() =>
            {
                NotifyDataCollectionUpdated(nameof(GUIContext.GameDeadCounter), () => GUIContext.GameDeadCounter.LoadAsync(), null);
            }, "RefreshGameDeadCounterList");
        }

        private void RefreshGiveawayUserDataList()
        {
            LogWriter.DebugLog("RefreshGiveawayUserDataList", DebugLogTypes.DataManager,
                $"Reloading GiveawayUserData data into the database context.");
            PostActionQueue(() =>
            {
                NotifyDataCollectionUpdated(nameof(GUIContext.GiveawayUserData), () => GUIContext.GiveawayUserData.LoadAsync(), null);
            }, "RefreshGiveawayUserDataList");
        }

        private void RefreshInRaidDataList()
        {
            LogWriter.DebugLog("RefreshInRaidDataList", DebugLogTypes.DataManager,
                $"Reloading InRaidData data into the database context.");
            PostActionQueue(() =>
            {
                NotifyDataCollectionUpdated(nameof(GUIContext.InRaidData), () => GUIContext.InRaidData.LoadAsync(), null);
            }, "RefreshInRaidDataList");
        }

        private void RefreshLearnMsgsList()
        {
            LogWriter.DebugLog("RefreshLearnMsgsList", DebugLogTypes.DataManager,
                $"Reloading LearnMsgs data into the database context.");
            PostActionQueue(() =>
            {
                NotifyDataCollectionUpdated(nameof(GUIContext.LearnMsgs), () => GUIContext.LearnMsgs.LoadAsync(), null);
            }, "RefreshLearnMsgsList");
        }

        private void RefreshModeratorApproveList()
        {
            LogWriter.DebugLog("RefreshModeratorApproveList", DebugLogTypes.DataManager,
                $"Reloading ModeratorApprove data into the database context.");
            PostActionQueue(() =>
            {
                NotifyDataCollectionUpdated(nameof(GUIContext.ModeratorApprove), () => GUIContext.ModeratorApprove.LoadAsync(), null);
            }, "RefreshModeratorApproveList");
        }

        private void RefreshMultiChannelsList()
        {
            LogWriter.DebugLog("RefreshMultiChannelsList", DebugLogTypes.DataManager,
                $"Reloading MultiChannels data into the database context.");
            PostActionQueue(() =>
            {
                NotifyDataCollectionUpdated(nameof(GUIContext.MultiChannels), () => GUIContext.MultiChannels.LoadAsync(), null);
            }, "RefreshMultiChannelsList");
        }

        private void RefreshMultiLiveStreamsList()
        {
            LogWriter.DebugLog("RefreshMultiLiveStreamsList", DebugLogTypes.DataManager,
                $"Reloading MultiLiveStreams data into the database context.");
            PostActionQueue(() =>
            {
                NotifyDataCollectionUpdated(nameof(GUIContext.MultiLiveStreams), () => GUIContext.MultiLiveStreams.LoadAsync(), null);
            }, "RefreshMultiLiveStreamsList");
        }

        private void RefreshMultiSummaryLiveStreamsList()
        {
            LogWriter.DebugLog("RefreshMultiSummaryLiveStreamsList", DebugLogTypes.DataManager,
                $"Reloading MultiSummaryLiveStreams data into the database context.");
            PostActionQueue(() =>
            {
                NotifyDataCollectionUpdated(nameof(GUIContext.MultiSummaryLiveStreams), () => GUIContext.MultiSummaryLiveStreams.LoadAsync(), null);
            }, "RefreshMultiSummaryLiveStreamsList");
        }

        private void RefreshMultiWebhooksList()
        {
            LogWriter.DebugLog("RefreshMultiWebhooksList", DebugLogTypes.DataManager,
                $"Reloading MultiWebhooks data into the database context.");
            PostActionQueue(() =>
            {
                NotifyDataCollectionUpdated(nameof(GUIContext.MultiWebhooks), () => GUIContext.MultiWebhooks.LoadAsync(), null);
            }, "RefreshMultiWebhooksList");
        }

        private void RefreshOldFollowUsersList()
        {
            LogWriter.DebugLog("RefreshOldFollowUsersList", DebugLogTypes.DataManager,
                $"Reloading OldFollowUsers data into the database context.");
            PostActionQueue(() =>
            {
                NotifyDataCollectionUpdated(nameof(GUIContext.OldFollowUsers), () => GUIContext.OldFollowUsers.LoadAsync(), null);
            }, "RefreshOldFollowUsersList");
        }

        private void RefreshOutRaidDataList()
        {
            LogWriter.DebugLog("RefreshOutRaidDataList", DebugLogTypes.DataManager,
                $"Reloading OutRaidData data into the database context.");
            PostActionQueue(() =>
            {
                NotifyDataCollectionUpdated(nameof(GUIContext.OutRaidData), () => GUIContext.OutRaidData.LoadAsync(), null);
            }, "RefreshOutRaidDataList");
        }

        private void RefreshOverlayServicesList()
        {
            LogWriter.DebugLog("RefreshOverlayServicesList", DebugLogTypes.DataManager,
                $"Reloading OverlayServices data into the database context.");
            PostActionQueue(() =>
            {
                NotifyDataCollectionUpdated(nameof(GUIContext.OverlayServices), () => GUIContext.OverlayServices.LoadAsync(), null);
            }, "RefreshOverlayServicesList");
        }

        private void RefreshOverlayTickerList()
        {
            LogWriter.DebugLog("RefreshOverlayTickerList", DebugLogTypes.DataManager,
                $"Reloading OverlayTicker data into the database context.");
            PostActionQueue(() =>
            {
                NotifyDataCollectionUpdated(nameof(GUIContext.OverlayTicker), () => GUIContext.OverlayTicker.LoadAsync(), null);
            }, "RefreshOverlayTickerList");
        }

        private void RefreshQuotesList()
        {
            LogWriter.DebugLog("RefreshQuotesList", DebugLogTypes.DataManager,
                $"Reloading Quotes data into the database context.");
            PostActionQueue(() =>
            {
                NotifyDataCollectionUpdated(nameof(GUIContext.Quotes), () => GUIContext.Quotes.LoadAsync(), null);
            }, "RefreshQuotesList");
        }

        private void RefreshShoutOutsList()
        {
            LogWriter.DebugLog("RefreshShoutOutsList", DebugLogTypes.DataManager,
                $"Reloading ShoutOuts data into the database context.");
            PostActionQueue(() =>
            {
                NotifyDataCollectionUpdated(nameof(GUIContext.ShoutOuts), () => GUIContext.ShoutOuts.LoadAsync(), null);
            }, "RefreshShoutOutsList");
        }

        private void RefreshStreamStatsList()
        {
            LogWriter.DebugLog("RefreshStreamStatsList", DebugLogTypes.DataManager,
                $"Reloading StreamStats data into the database context.");
            PostActionQueue(() =>
            {
                NotifyDataCollectionUpdated(nameof(GUIContext.StreamStats), () => GUIContext.StreamStats.LoadAsync(), null);
            }, "RefreshStreamStatsList");
        }

        private void RefreshUsersList()
        {
            LogWriter.DebugLog("RefreshUsersList", DebugLogTypes.DataManager,
                $"Reloading Users data into the database context.");
            PostActionQueue(() =>
            {
                NotifyDataCollectionUpdated(nameof(GUIContext.Users), () => GUIContext.Users.LoadAsync(), null);
            }, "RefreshUsersList");
        }

        private void RefreshUserStatsList()
        {
            LogWriter.DebugLog("RefreshUserStatsList", DebugLogTypes.DataManager,
                $"Reloading UserStats data into the database context.");
            PostActionQueue(() =>
            {
                NotifyDataCollectionUpdated(nameof(GUIContext.UserStats), () => GUIContext.UserStats.LoadAsync(), null);
            }, "RefreshUserStatsList");
        }

        private void RefreshWebhooksList()
        {
            LogWriter.DebugLog("RefreshWebhooksList", DebugLogTypes.DataManager,
                $"Reloading Webhooks data into the database context.");
            PostActionQueue(() =>
            {
                NotifyDataCollectionUpdated(nameof(GUIContext.Webhooks), () => GUIContext.Webhooks.LoadAsync(), null);
            }, "RefreshWebhooksList");

        }

        #endregion

#else

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
                return GUIContext.BanReasons.Local.ToList();
            });
        }

        private Task<List<BanRules>> GetBanRulesLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetBanRulesLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading BanRules data into the database context.");
                await GUIContext.BanRules.LoadAsync();
                return GUIContext.BanRules.Local.ToList();
            });
        }

        private Task<List<CategoryList>> GetCategoryListLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetCategoryListLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading CategoryList data into the database context.");
                await GUIContext.CategoryList.LoadAsync();
                return GUIContext.CategoryList.Local.ToList();
            });
        }

        private Task<List<ChannelEvents>> GetChannelEventsLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetChannelEventsLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading ChannelEvents data into the database context.");
                await GUIContext.ChannelEvents.LoadAsync();
                return GUIContext.ChannelEvents.Local.ToList();
            });
        }

        private Task<List<Clips>> GetClipsLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetClipsLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading Clips data into the database context.");
                await GUIContext.Clips.LoadAsync();
                return GUIContext.Clips.Local.ToList();
            });
        }

        private Task<List<Commands>> GetCommandsLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetCommandsLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading Commands data into the database context.");
                await GUIContext.Commands.LoadAsync();
                return GUIContext.Commands.Local.ToList();
            });
        }

        private Task<List<CommandsUser>> GetCommandsUserLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetCommandsUserLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading CommandsUser data into the database context.");
                await GUIContext.CommandsUser.LoadAsync();
                return GUIContext.CommandsUser.Local.ToList();
            });
        }

        private Task<List<Currency>> GetCurrencyLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetCurrencyLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading Currency data into the database context.");
                await GUIContext.Currency.LoadAsync();
                return GUIContext.Currency.Local.ToList();
            });
        }

        private Task<List<Models.CurrencyType>> GetCurrencyTypeLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetCurrencyTypeLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading CurrencyType data into the database context.");
                await GUIContext.CurrencyType.LoadAsync();
                return GUIContext.CurrencyType.Local.ToList();
            });
        }

        private Task<List<CustomWelcome>> GetCustomWelcomeLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetCustomWelcomeLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading CustomWelcome data into the database context.");
                await GUIContext.CustomWelcome.LoadAsync();
                return GUIContext.CustomWelcome.Local.ToList();
            });
        }

        private Task<List<Followers>> GetFollowersLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetFollowersLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading Followers data into the database context.");
                await GUIContext.Followers.LoadAsync();
                return GUIContext.Followers.Local.ToList();
            });
        }

        private Task<List<GameDeadCounter>> GetGameDeadCounterLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetGameDeadCounterLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading GameDeadCounter data into the database context.");

                await GUIContext.GameDeadCounter.LoadAsync();
                return GUIContext.GameDeadCounter.Local.ToList();
            });
        }

        private Task<List<GiveawayUserData>> GetGiveawayUserDataLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetGiveawayUserDataLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading GiveawayUserData data into the database context.");

                await GUIContext.GiveawayUserData.LoadAsync();
                return GUIContext.GiveawayUserData.Local.ToList();
            });
        }

        private Task<List<InRaidData>> GetInRaidDataLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetInRaidDataLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading InRaidData data into the database context.");
                await GUIContext.InRaidData.LoadAsync();
                return GUIContext.InRaidData.Local.ToList();
            });
        }

        private Task<List<LearnMsgs>> GetLearnMsgsLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetLearnMsgsLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading LearnMsgs data into the database context.");
                await GUIContext.LearnMsgs.LoadAsync();
                return GUIContext.LearnMsgs.Local.ToList();
            });
        }

        private Task<List<ModeratorApprove>> GetModeratorApproveLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetModeratorApproveLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading ModeratorApprove data into the database context.");
                await GUIContext.ModeratorApprove.LoadAsync();
                return GUIContext.ModeratorApprove.Local.ToList();
            });
        }

        private Task<List<MultiChannels>> GetMultiChannelsLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetMultiChannelsLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading MultiChannels data into the database context.");
                await GUIContext.MultiChannels.LoadAsync();
                return GUIContext.MultiChannels.Local.ToList();
            });
        }

        private Task<List<MultiLiveStreams>> GetMultiLiveStreamsLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetMultiLiveStreamsLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading MultiLiveStreams data into the database context.");
                await GUIContext.MultiLiveStreams.LoadAsync();
                return GUIContext.MultiLiveStreams.Local.ToList();
            });
        }

        private Task<List<MultiWebhooks>> GetMultiWebhooksLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetMultiWebhooksLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading MultiWebhooks data into the database context.");
                await GUIContext.MultiWebhooks.LoadAsync();
                return GUIContext.MultiWebhooks.Local.ToList();
            });
        }

        private Task<List<MultiSummaryLiveStreams>> GetMultiSummaryLiveStreamsLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetMultiSummaryLiveStreamsLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading MultiSummaryLiveStreams data into the database context.");
                await GUIContext.MultiSummaryLiveStreams.LoadAsync();
                return GUIContext.MultiSummaryLiveStreams.Local.ToList();
            });
        }

        private Task<List<OldFollowUsers>> GetOldFollowUsersLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetOldFollowUsersLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading OldFollowUsers data into the database context.");
                await GUIContext.OldFollowUsers.LoadAsync();
                return GUIContext.OldFollowUsers.Local.ToList();
            });
        }

        private Task<List<OutRaidData>> GetOutRaidDataLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetOutRaidDataLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading OutRaidData data into the database context.");
                await GUIContext.OutRaidData.LoadAsync();
                return GUIContext.OutRaidData.Local.ToList();
            });
        }

        private Task<List<OverlayServices>> GetOverlayServicesLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetOverlayServicesLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading OverlayServices data into the database context.");
                await GUIContext.OverlayServices.LoadAsync();
                return GUIContext.OverlayServices.Local.ToList();
            });
        }

        private Task<List<OverlayTicker>> GetOverlayTickerLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetOverlayTickerLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading OverlayTicker data into the database context.");
                await GUIContext.OverlayTicker.LoadAsync();
                return GUIContext.OverlayTicker.Local.ToList();
            });
        }

        private Task<List<Quotes>> GetQuotesLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetQuotesLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading Quotes data into the database context.");
                await GUIContext.Quotes.LoadAsync();
                return GUIContext.Quotes.Local.ToList();
            });
        }

        private Task<List<ShoutOuts>> GetShoutOutsLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetShoutOutsLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading ShoutOuts data into the database context.");
                await GUIContext.ShoutOuts.LoadAsync();
                return GUIContext.ShoutOuts.Local.ToList();
            });
        }

        private Task<List<StreamStats>> GetStreamStatsLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetStreamStatsLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading StreamStats data into the database context.");
                await GUIContext.StreamStats.LoadAsync();
                return GUIContext.StreamStats.Local.ToList();
            });
        }

        private Task<List<Users>> GetUsersLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetUsersLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading Users data into the database context.");
                await GUIContext.Users.LoadAsync();
                return GUIContext.Users.Local.ToList();
            });
        }

        private Task<List<UserStats>> GetUserStatsLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetUserStatsLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading UserStats data into the database context.");
                await GUIContext.UserStats.LoadAsync();
                return GUIContext.UserStats.Local.ToList();
            });
        }

        private Task<List<Webhooks>> GetWebhooksLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetWebhooksLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading Webhooks data into the database context.");
                await GUIContext.Webhooks.LoadAsync();
                return GUIContext.Webhooks.Local.ToList();
            });
        }

        #endregion

        internal void NotifyDataCollectionUpdated(string TableName, object tableData)
        {
            LogWriter.DebugLog("NotifyDataCollectionUpdated", DebugLogTypes.DataManager,
                $"Notifying the DataCollection is Updated for {TableName}.");
            OnDataCollectionUpdated?.Invoke(this, new(TableName, tableData));
        }

        #region Refresh Collections
        private void RefreshBanReasonsList()
        {
            LogWriter.DebugLog("RefreshBanReasonsList", DebugLogTypes.DataManager,
                $"Reloading BanReasons data into the database context.");
            PostActionQueue(async () =>
            {
                await GUIContext.BanReasons.LoadAsync();
                NotifyDataCollectionUpdated(nameof(GUIContext.BanReasons), GUIContext.BanReasons.Local.ToList());
            }, "RefreshBanReasonsList");
        }

        private void RefreshBanRulesList()
        {
            LogWriter.DebugLog("RefreshBanRulesList", DebugLogTypes.DataManager,
                $"Reloading BanRules data into the database context.");
            PostActionQueue(async () =>
            {
                await GUIContext.BanRules.LoadAsync();
                NotifyDataCollectionUpdated(nameof(GUIContext.BanRules), GUIContext.BanRules.Local.ToList());
            }, "RefreshBanRulesList");
        }

        private void RefreshCategoryListList()
        {
            LogWriter.DebugLog("RefreshCategoryListList", DebugLogTypes.DataManager,
                $"Reloading CategoryList data into the database context.");
            PostActionQueue(async () =>
            {
                await GUIContext.CategoryList.LoadAsync();
                NotifyDataCollectionUpdated(nameof(GUIContext.CategoryList), GUIContext.CategoryList.Local.ToList());
            }, "RefreshCategoryList");
        }

        private void RefreshChannelEventsList()
        {
            LogWriter.DebugLog("RefreshChannelEventsList", DebugLogTypes.DataManager,
                $"Reloading ChannelEvents data into the database context.");
            PostActionQueue(async () =>
            {
                await GUIContext.ChannelEvents.LoadAsync();
                NotifyDataCollectionUpdated(nameof(GUIContext.ChannelEvents), GUIContext.ChannelEvents.Local.ToList());
            }, "RefreshChannelEventsList");
        }

        private void RefreshClipsList()
        {
            LogWriter.DebugLog("RefreshClipsList", DebugLogTypes.DataManager,
                $"Reloading Clips data into the database context.");
            PostActionQueue(async () =>
            {
                await GUIContext.Clips.LoadAsync();
                NotifyDataCollectionUpdated(nameof(GUIContext.Clips), GUIContext.Clips.Local.ToList());
            }, "RefreshClipsList");
        }

        private void RefreshCommandsList()
        {
            LogWriter.DebugLog("RefreshCommandsList", DebugLogTypes.DataManager,
                $"Reloading Commands data into the database context.");
            PostActionQueue(async () =>
            {
                await GUIContext.Commands.LoadAsync();
                NotifyDataCollectionUpdated(nameof(GUIContext.Commands), GUIContext.Commands.Local.ToList());
            }, "RefreshCommandsList");
        }

        private void RefreshCommandsUserList()
        {
            LogWriter.DebugLog("RefreshCommandsUserList", DebugLogTypes.DataManager,
                $"Reloading CommandsUser data into the database context.");
            PostActionQueue(async () =>
            {
                await GUIContext.CommandsUser.LoadAsync();
                NotifyDataCollectionUpdated(nameof(GUIContext.CommandsUser), GUIContext.CommandsUser.Local.ToList());
            }, "RefreshCommandsUserList");
        }

        private void RefreshCurrencyList()
        {
            LogWriter.DebugLog("RefreshCurrencyList", DebugLogTypes.DataManager,
                $"Reloading Currency data into the database context.");
            PostActionQueue(async () =>
            {
                await GUIContext.Currency.LoadAsync();
                NotifyDataCollectionUpdated(nameof(GUIContext.Currency), GUIContext.Currency.Local.ToList());
            }, "RefreshCurrencyList");
        }

        private void RefreshCurrencyTypeList()
        {
            LogWriter.DebugLog("RefreshCurrencyTypeList", DebugLogTypes.DataManager,
                $"Reloading CurrencyType data into the database context.");
            PostActionQueue(async () =>
            {
                await GUIContext.CurrencyType.LoadAsync();
                NotifyDataCollectionUpdated(nameof(GUIContext.CurrencyType), GUIContext.CurrencyType.Local.ToList());
            }, "RefreshCurrencyTypeList");
        }

        private void RefreshCustomWelcomeList()
        {
            LogWriter.DebugLog("RefreshCustomWelcomeList", DebugLogTypes.DataManager,
                $"Reloading CustomWelcome data into the database context.");
            PostActionQueue(async () =>
            {
                await GUIContext.CustomWelcome.LoadAsync();
                NotifyDataCollectionUpdated(nameof(GUIContext.CustomWelcome), GUIContext.CustomWelcome.Local.ToList());
            }, "RefreshCustomWelcomeList");
        }

        private void RefreshFollowersList()
        {
            LogWriter.DebugLog("RefreshFollowersList", DebugLogTypes.DataManager,
                $"Reloading Followers data into the database context.");
            PostActionQueue(async () =>
            {
                await GUIContext.Followers.LoadAsync();
                NotifyDataCollectionUpdated(nameof(GUIContext.Followers), GUIContext.Followers.Local.ToList());
            }, "RefreshFollowersList");
        }

        private void RefreshGameDeadCounterList()
        {
            LogWriter.DebugLog("RefreshGameDeadCounterList", DebugLogTypes.DataManager,
                $"Reloading GameDeadCounter data into the database context.");
            PostActionQueue(async () =>
            {
                await GUIContext.GameDeadCounter.LoadAsync();
                NotifyDataCollectionUpdated(nameof(GUIContext.GameDeadCounter), GUIContext.GameDeadCounter.Local.ToList());
            }, "RefreshGameDeadCounterList");
        }

        private void RefreshGiveawayUserDataList()
        {
            LogWriter.DebugLog("RefreshGiveawayUserDataList", DebugLogTypes.DataManager,
                $"Reloading GiveawayUserData data into the database context.");
            PostActionQueue(async () =>
            {
                await GUIContext.GiveawayUserData.LoadAsync();
                NotifyDataCollectionUpdated(nameof(GUIContext.GiveawayUserData), GUIContext.GiveawayUserData.Local.ToList());
            }, "RefreshGiveawayUserDataList");
        }

        private void RefreshInRaidDataList()
        {
            LogWriter.DebugLog("RefreshInRaidDataList", DebugLogTypes.DataManager,
                $"Reloading InRaidData data into the database context.");
            PostActionQueue(async () =>
            {
                await GUIContext.InRaidData.LoadAsync();
                NotifyDataCollectionUpdated(nameof(GUIContext.InRaidData), GUIContext.InRaidData.Local.ToList());
            }, "RefreshInRaidDataList");
        }

        private void RefreshLearnMsgsList()
        {
            LogWriter.DebugLog("RefreshLearnMsgsList", DebugLogTypes.DataManager,
                $"Reloading LearnMsgs data into the database context.");
            PostActionQueue(async () =>
            {
                await GUIContext.LearnMsgs.LoadAsync();
                NotifyDataCollectionUpdated(nameof(GUIContext.LearnMsgs), GUIContext.LearnMsgs.Local.ToList());
            }, "RefreshLearnMsgsList");
        }

        private void RefreshModeratorApproveList()
        {
            LogWriter.DebugLog("RefreshModeratorApproveList", DebugLogTypes.DataManager,
                $"Reloading ModeratorApprove data into the database context.");
            PostActionQueue(async () =>
            {
                await GUIContext.ModeratorApprove.LoadAsync();
                NotifyDataCollectionUpdated(nameof(GUIContext.ModeratorApprove), GUIContext.ModeratorApprove.Local.ToList());
            }, "RefreshModeratorApproveList");
        }

        private void RefreshMultiChannelsList()
        {
            LogWriter.DebugLog("RefreshMultiChannelsList", DebugLogTypes.DataManager,
                $"Reloading MultiChannels data into the database context.");
            PostActionQueue(async () =>
            {
                await GUIContext.MultiChannels.LoadAsync();
                NotifyDataCollectionUpdated(nameof(GUIContext.MultiChannels), GUIContext.MultiChannels.Local.ToList());
            }, "RefreshMultiChannelsList");
        }

        private void RefreshMultiLiveStreamsList()
        {
            LogWriter.DebugLog("RefreshMultiLiveStreamsList", DebugLogTypes.DataManager,
                $"Reloading MultiLiveStreams data into the database context.");
            PostActionQueue(async () =>
            {
                await GUIContext.MultiLiveStreams.LoadAsync();
                NotifyDataCollectionUpdated(nameof(GUIContext.MultiLiveStreams), GUIContext.MultiLiveStreams.Local.ToList());
            }, "RefreshMultiLiveStreamsList");
        }

        private void RefreshMultiSummaryLiveStreamsList()
        {
            LogWriter.DebugLog("RefreshMultiSummaryLiveStreamsList", DebugLogTypes.DataManager,
                $"Reloading MultiSummaryLiveStreams data into the database context.");
            PostActionQueue(async () =>
            {
                await GUIContext.MultiSummaryLiveStreams.LoadAsync();
                NotifyDataCollectionUpdated(nameof(GUIContext.MultiSummaryLiveStreams), GUIContext.MultiSummaryLiveStreams.Local.ToList());
            }, "RefreshMultiSummaryLiveStreamsList");
        }

        private void RefreshMultiWebhooksList()
        {
            LogWriter.DebugLog("RefreshMultiWebhooksList", DebugLogTypes.DataManager,
                $"Reloading MultiWebhooks data into the database context.");
            PostActionQueue(async () =>
            {
                await GUIContext.MultiWebhooks.LoadAsync();
                NotifyDataCollectionUpdated(nameof(GUIContext.MultiWebhooks), GUIContext.MultiWebhooks.Local.ToList());
            }, "RefreshMultiWebhooksList");
        }

        private void RefreshOldFollowUsersList()
        {
            LogWriter.DebugLog("RefreshOldFollowUsersList", DebugLogTypes.DataManager,
                $"Reloading OldFollowUsers data into the database context.");
            PostActionQueue(async () =>
            {
                await GUIContext.OldFollowUsers.LoadAsync();
                NotifyDataCollectionUpdated(nameof(GUIContext.OldFollowUsers), GUIContext.OldFollowUsers.Local.ToList());
            }, "RefreshOldFollowUsersList");
        }

        private void RefreshOutRaidDataList()
        {
            LogWriter.DebugLog("RefreshOutRaidDataList", DebugLogTypes.DataManager,
                $"Reloading OutRaidData data into the database context.");
            PostActionQueue(async () =>
            {
                await GUIContext.OutRaidData.LoadAsync();
                NotifyDataCollectionUpdated(nameof(GUIContext.OutRaidData), GUIContext.OutRaidData.Local.ToList());
            }, "RefreshOutRaidDataList");
        }

        private void RefreshOverlayServicesList()
        {
            LogWriter.DebugLog("RefreshOverlayServicesList", DebugLogTypes.DataManager,
                $"Reloading OverlayServices data into the database context.");
            PostActionQueue(async () =>
            {
                await GUIContext.OverlayServices.LoadAsync();
                NotifyDataCollectionUpdated(nameof(GUIContext.OverlayServices), GUIContext.OverlayServices.Local.ToList());
            }, "RefreshOverlayServicesList");
        }

        private void RefreshOverlayTickerList()
        {
            LogWriter.DebugLog("RefreshOverlayTickerList", DebugLogTypes.DataManager,
                $"Reloading OverlayTicker data into the database context.");
            PostActionQueue(async () =>
            {
                await GUIContext.OverlayTicker.LoadAsync();
                NotifyDataCollectionUpdated(nameof(GUIContext.OverlayTicker), GUIContext.OverlayTicker.Local.ToList());
            }, "RefreshOverlayTickerList");
        }

        private void RefreshQuotesList()
        {
            LogWriter.DebugLog("RefreshQuotesList", DebugLogTypes.DataManager,
                $"Reloading Quotes data into the database context.");
            PostActionQueue(async () =>
            {
                await GUIContext.Quotes.LoadAsync();
                NotifyDataCollectionUpdated(nameof(GUIContext.Quotes), GUIContext.Quotes.Local.ToList());
            }, "RefreshQuotesList");
        }

        private void RefreshShoutOutsList()
        {
            LogWriter.DebugLog("RefreshShoutOutsList", DebugLogTypes.DataManager,
                $"Reloading ShoutOuts data into the database context.");
            PostActionQueue(async () =>
            {
                await GUIContext.ShoutOuts.LoadAsync();
                NotifyDataCollectionUpdated(nameof(GUIContext.ShoutOuts), GUIContext.ShoutOuts.Local.ToList());
            }, "RefreshShoutOutsList");
        }

        private void RefreshStreamStatsList()
        {
            LogWriter.DebugLog("RefreshStreamStatsList", DebugLogTypes.DataManager,
                $"Reloading StreamStats data into the database context.");
            PostActionQueue(async () =>
            {
                await GUIContext.StreamStats.LoadAsync();
                NotifyDataCollectionUpdated(nameof(GUIContext.StreamStats), GUIContext.StreamStats.Local.ToList());
            }, "RefreshStreamStatsList");
        }

        private void RefreshUsersList()
        {
            LogWriter.DebugLog("RefreshUsersList", DebugLogTypes.DataManager,
                $"Reloading Users data into the database context.");
            PostActionQueue(async () =>
            {
                LogWriter.DebugLog("RefreshUsersList",
                    DebugLogTypes.DataManager, $"Reloading Users data into the database context.");
                await GUIContext.Users.LoadAsync();
                LogWriter.DebugLog("RefreshUsersList",
                    DebugLogTypes.DataManager, $"Notifying the DataCollection is Updated.");
                NotifyDataCollectionUpdated(nameof(GUIContext.Users), GUIContext.Users.Local.ToList());
            }, "RefreshUsersList");
        }

        private void RefreshUserStatsList()
        {
            LogWriter.DebugLog("RefreshUserStatsList", DebugLogTypes.DataManager,
                $"Reloading UserStats data into the database context.");
            PostActionQueue(async () =>
            {
                LogWriter.DebugLog("RefreshUserStatsList",
                  DebugLogTypes.DataManager, $"Reloading UserStats data into the database context.");
                await GUIContext.UserStats.LoadAsync();
                LogWriter.DebugLog("RefreshUserStatsList",
                    DebugLogTypes.DataManager, $"Notifying the DataCollection is Updated.");
                NotifyDataCollectionUpdated(nameof(GUIContext.UserStats), GUIContext.UserStats.Local.ToList());
            }, "RefreshUserStatsList");
        }

        private void RefreshWebhooksList()
        {
            LogWriter.DebugLog("RefreshWebhooksList", DebugLogTypes.DataManager,
                $"Reloading Webhooks data into the database context.");
            PostActionQueue(async () =>
            {
                await GUIContext.Webhooks.LoadAsync();
                NotifyDataCollectionUpdated(nameof(GUIContext.Webhooks), GUIContext.Webhooks.Local.ToList());
            }, "RefreshWebhooksList");

        }

        #endregion

#endif
    }
}
