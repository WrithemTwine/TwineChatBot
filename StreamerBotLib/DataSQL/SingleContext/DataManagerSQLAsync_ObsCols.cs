using Microsoft.EntityFrameworkCore;

using StreamerBotLib.DataSQL.Models;
using StreamerBotLib.Enums;
using StreamerBotLib.Static;

using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace StreamerBotLib.DataSQL.SingleContext
{
    internal partial class DataManagerSQLAsync
    {
        #region LocalView List

        internal object GetICollection(DataTables dataTable)
        {
            LogWriter.DebugLog("GetList", DebugLogTypes.DataManager,
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
        private void UpdatedCollection(object sender, NotifyCollectionChangedEventArgs e)
        {
        }

        private Task<ObservableCollection<Models.BanReasons>> GetBanReasonsLocalObservableAsync()
        {
            return Task.Run(async () =>
            {
                await context.BanReasons.LoadAsync();
                var result = context.BanReasons.Local.ToObservableCollection();
                result.CollectionChanged += UpdatedCollection;
                return result;
            });
        }


        private Task<ObservableCollection<BanRules>> GetBanRulesLocalObservableAsync()
        {
            return Task.Run(async () =>
            {
                await context.BanRules.LoadAsync();
                var result = context.BanRules.Local.ToObservableCollection();
                result.CollectionChanged += UpdatedCollection;
                return result;
            });
        }

        private Task<ObservableCollection<CategoryList>> GetCategoryListLocalObservableAsync()
        {
            return Task.Run(async () =>
            {
                await context.CategoryList.LoadAsync();
                var result = context.CategoryList.Local.ToObservableCollection();
                result.CollectionChanged += UpdatedCollection;
                return result;
            });
        }

        private Task<ObservableCollection<ChannelEvents>> GetChannelEventsLocalObservableAsync()
        {
            return Task.Run(async () =>
            {
                await context.ChannelEvents.LoadAsync();
                var result = context.ChannelEvents.Local.ToObservableCollection();
                result.CollectionChanged += UpdatedCollection;
                return result;
            });
        }

        private Task<ObservableCollection<Clips>> GetClipsLocalObservableAsync()
        {
            return Task.Run(async () =>
            {
                await context.Clips.LoadAsync();
                var result = context.Clips.Local.ToObservableCollection();
                result.CollectionChanged += UpdatedCollection;
                return result;
            });
        }

        private Task<ObservableCollection<Commands>> GetCommandsLocalObservableAsync()
        {
            return Task.Run(async () =>
            {
                await context.Commands.LoadAsync();
                var result = context.Commands.Local.ToObservableCollection();
                result.CollectionChanged += UpdatedCollection;
                return result;
            });
        }

        private Task<ObservableCollection<CommandsUser>> GetCommandsUserLocalObservableAsync()
        {
            return Task.Run(async () =>
            {
                await context.CommandsUser.LoadAsync();
                var result = context.CommandsUser.Local.ToObservableCollection();
                result.CollectionChanged += UpdatedCollection;
                return result;
            });
        }

        private Task<ObservableCollection<Currency>> GetCurrencyLocalObservableAsync()
        {
            return Task.Run(async () =>
            {
                await context.Currency.LoadAsync();
                var result = context.Currency.Local.ToObservableCollection();
                result.CollectionChanged += UpdatedCollection;
                return result;
            });
        }

        private Task<ObservableCollection<Models.CurrencyType>> GetCurrencyTypeLocalObservableAsync()
        {
            return Task.Run(async () =>
            {
                await context.CurrencyType.LoadAsync();
                var result = context.CurrencyType.Local.ToObservableCollection();
                result.CollectionChanged += UpdatedCollection;
                return result;
            });
        }

        private Task<ObservableCollection<CustomWelcome>> GetCustomWelcomeLocalObservableAsync()
        {
            return Task.Run(async () =>
            {
                await context.CustomWelcome.LoadAsync();
                var result = context.CustomWelcome.Local.ToObservableCollection();
                result.CollectionChanged += UpdatedCollection;
                return result;
            });
        }

        private Task<ObservableCollection<Followers>> GetFollowersLocalObservableAsync()
        {
            return Task.Run(async () =>
            {
                await context.Followers.LoadAsync();
                var result = context.Followers.Local.ToObservableCollection();
                result.CollectionChanged += UpdatedCollection;
                return result;
            });
        }

        private Task<ObservableCollection<GameDeadCounter>> GetGameDeadCounterLocalObservableAsync()
        {
            return Task.Run(async () =>
            {
                await context.GameDeadCounter.LoadAsync();
                var result = context.GameDeadCounter.Local.ToObservableCollection();
                result.CollectionChanged += UpdatedCollection;
                return result;
            });
        }

        private Task<ObservableCollection<GiveawayUserData>> GetGiveawayUserDataLocalObservableAsync()
        {
            return Task.Run(async () =>
            {
                await context.GiveawayUserData.LoadAsync();
                var result = context.GiveawayUserData.Local.ToObservableCollection();
                result.CollectionChanged += UpdatedCollection;
                return result;
            });
        }

        private Task<ObservableCollection<InRaidData>> GetInRaidDataLocalObservableAsync()
        {
            return Task.Run(async () =>
            {
                await context.InRaidData.LoadAsync();
                var result = context.InRaidData.Local.ToObservableCollection();
                result.CollectionChanged += UpdatedCollection;
                return result;
            });
        }

        private Task<ObservableCollection<LearnMsgs>> GetLearnMsgsLocalObservableAsync()
        {
            return Task.Run(async () =>
            {
                await context.LearnMsgs.LoadAsync();
                var result = context.LearnMsgs.Local.ToObservableCollection();
                result.CollectionChanged += UpdatedCollection;
                return result;
            });
        }

        private Task<ObservableCollection<ModeratorApprove>> GetModeratorApproveLocalObservableAsync()
        {
            return Task.Run(async () =>
            {
                await context.ModeratorApprove.LoadAsync();
                var result = context.ModeratorApprove.Local.ToObservableCollection();
                result.CollectionChanged += UpdatedCollection;
                return result;
            });
        }

        private Task<ObservableCollection<MultiChannels>> GetMultiChannelsLocalObservableAsync()
        {
            return Task.Run(async () =>
            {
                await context.MultiChannels.LoadAsync();
                var result = context.MultiChannels.Local.ToObservableCollection();
                result.CollectionChanged += UpdatedCollection;
                return result;
            });
        }

        private Task<ObservableCollection<MultiLiveStreams>> GetMultiLiveStreamsLocalObservableAsync()
        {
            return Task.Run(async () =>
            {
                await context.MultiLiveStreams.LoadAsync();
                var result = context.MultiLiveStreams.Local.ToObservableCollection();
                result.CollectionChanged += UpdatedCollection;
                return result;
            });
        }

        private Task<ObservableCollection<MultiWebhooks>> GetMultiWebhooksLocalObservableAsync()
        {
            return Task.Run(async () =>
            {
                await context.MultiWebhooks.LoadAsync();
                var result = context.MultiWebhooks.Local.ToObservableCollection();
                result.CollectionChanged += UpdatedCollection;
                return result;
            });
        }

        private Task<ObservableCollection<MultiSummaryLiveStreams>> GetMultiSummaryLiveStreamsLocalObservableAsync()
        {
            return Task.Run(async () =>
            {
                await context.MultiSummaryLiveStreams.LoadAsync();
                var result = context.MultiSummaryLiveStreams.Local.ToObservableCollection();
                result.CollectionChanged += UpdatedCollection;
                return result;
            });
        }

        private Task<ObservableCollection<OldFollowUsers>> GetOldFollowUsersLocalObservableAsync()
        {
            return Task.Run(async () =>
            {
                await context.OldFollowUsers.LoadAsync();
                var result = context.OldFollowUsers.Local.ToObservableCollection();
                result.CollectionChanged += UpdatedCollection;
                return result;
            });
        }

        private Task<ObservableCollection<OutRaidData>> GetOutRaidDataLocalObservableAsync()
        {
            return Task.Run(async () =>
            {
                await context.OutRaidData.LoadAsync();
                var result = context.OutRaidData.Local.ToObservableCollection();
                result.CollectionChanged += UpdatedCollection;
                return result;
            });
        }

        private Task<ObservableCollection<OverlayServices>> GetOverlayServicesLocalObservableAsync()
        {
            return Task.Run(async () =>
            {
                await context.OverlayServices.LoadAsync();
                var result = context.OverlayServices.Local.ToObservableCollection();
                result.CollectionChanged += UpdatedCollection;
                return result;
            });
        }

        private Task<ObservableCollection<OverlayTicker>> GetOverlayTickerLocalObservableAsync()
        {
            return Task.Run(async () =>
            {
                await context.OverlayTicker.LoadAsync();
                var result = context.OverlayTicker.Local.ToObservableCollection();
                result.CollectionChanged += UpdatedCollection;
                return result;
            });
        }

        private Task<ObservableCollection<Quotes>> GetQuotesLocalObservableAsync()
        {
            return Task.Run(async () =>
            {
                await context.Quotes.LoadAsync();
                var result = context.Quotes.Local.ToObservableCollection();
                result.CollectionChanged += UpdatedCollection;
                return result;
            });
        }

        private Task<ObservableCollection<ShoutOuts>> GetShoutOutsLocalObservableAsync()
        {
            return Task.Run(async () =>
            {
                await context.ShoutOuts.LoadAsync();
                var result = context.ShoutOuts.Local.ToObservableCollection();
                result.CollectionChanged += UpdatedCollection;
                return result;
            });
        }

        private Task<ObservableCollection<StreamStats>> GetStreamStatsLocalObservableAsync()
        {
            return Task.Run(async () =>
            {
                await context.StreamStats.LoadAsync();
                var result = context.StreamStats.Local.ToObservableCollection();
                result.CollectionChanged += UpdatedCollection;
                return result;
            });
        }

        private Task<ObservableCollection<Users>> GetUsersLocalObservableAsync()
        {
            return Task.Run(async () =>
            {
                await context.Users.LoadAsync();
                var result = context.Users.Local.ToObservableCollection();
                result.CollectionChanged += UpdatedCollection;
                return result;
            });
        }

        private Task<ObservableCollection<UserStats>> GetUserStatsLocalObservableAsync()
        {
            return Task.Run(async () =>
            {
                await context.UserStats.LoadAsync();
                var result = context.UserStats.Local.ToObservableCollection();
                result.CollectionChanged += UpdatedCollection;
                return result;
            });
        }

        private Task<ObservableCollection<Webhooks>> GetWebhooksLocalObservableAsync()
        {
            return Task.Run(async () =>
            {
                await context.Webhooks.LoadAsync();
                var result = context.Webhooks.Local.ToObservableCollection();
                result.CollectionChanged += UpdatedCollection;
                return result;
            });
        }

        #endregion

        internal void NotifyDataCollectionUpdated(string TableName, bool RecordCountChange = false)
        {
            OnDataCollectionUpdated?.Invoke(this, new(TableName, RecordCountChange));
        }

        #region Refresh Collections
        private void RefreshBanReasonsList()
        {
            NotifyDataCollectionUpdated(nameof(context.BanReasons));
        }

        private void RefreshBanRulesList()
        {
            NotifyDataCollectionUpdated(nameof(context.BanRules));
        }

        private void RefreshCategoryListList()
        {
            NotifyDataCollectionUpdated(nameof(context.CategoryList));
        }

        private void RefreshChannelEventsList()
        {
            NotifyDataCollectionUpdated(nameof(context.ChannelEvents));
        }

        private void RefreshClipsList()
        {
            NotifyDataCollectionUpdated(nameof(context.Clips));
        }

        private void RefreshCommandsList()
        {
            NotifyDataCollectionUpdated(nameof(context.Commands));
        }

        private void RefreshCommandsUserList()
        {
            NotifyDataCollectionUpdated(nameof(context.CommandsUser));
        }

        private void RefreshCurrencyList()
        {
            NotifyDataCollectionUpdated(nameof(context.Currency));
        }

        private void RefreshCurrencyTypeList()
        {
            NotifyDataCollectionUpdated(nameof(context.CurrencyType));
        }

        private void RefreshCustomWelcomeList()
        {
            NotifyDataCollectionUpdated(nameof(context.CustomWelcome));
        }

        private void RefreshFollowersList()
        {
            NotifyDataCollectionUpdated(nameof(context.Followers));
        }

        private void RefreshGameDeadCounterList()
        {
            NotifyDataCollectionUpdated(nameof(context.GameDeadCounter));
        }

        private void RefreshGiveawayUserDataList()
        {
            NotifyDataCollectionUpdated(nameof(context.GiveawayUserData));
        }

        private void RefreshInRaidDataList()
        {
            NotifyDataCollectionUpdated(nameof(context.InRaidData));
        }

        private void RefreshLearnMsgsList()
        {
            NotifyDataCollectionUpdated(nameof(context.LearnMsgs));
        }

        private void RefreshModeratorApproveList()
        {
            NotifyDataCollectionUpdated(nameof(context.ModeratorApprove));
        }

        private void RefreshMultiChannelsList()
        {
            NotifyDataCollectionUpdated(nameof(context.MultiChannels));
        }

        private void RefreshMultiLiveStreamsList()
        {
            NotifyDataCollectionUpdated(nameof(context.MultiLiveStreams));
        }

        private void RefreshMultiSummaryLiveStreamsList()
        {
            NotifyDataCollectionUpdated(nameof(context.MultiSummaryLiveStreams));
        }

        private void RefreshMultiWebhooksList()
        {
            NotifyDataCollectionUpdated(nameof(context.MultiWebhooks));
        }

        private void RefreshOldFollowUsersList()
        {
            NotifyDataCollectionUpdated(nameof(context.OldFollowUsers));
        }

        private void RefreshOutRaidDataList()
        {
            NotifyDataCollectionUpdated(nameof(context.OutRaidData));
        }

        private void RefreshOverlayServicesList()
        {
            NotifyDataCollectionUpdated(nameof(context.OverlayServices));
        }

        private void RefreshOverlayTickerList()
        {
            NotifyDataCollectionUpdated(nameof(context.OverlayTicker));
        }

        private void RefreshQuotesList()
        {
            NotifyDataCollectionUpdated(nameof(context.Quotes));
        }

        private void RefreshShoutOutsList()
        {
            NotifyDataCollectionUpdated(nameof(context.ShoutOuts));
        }

        private void RefreshStreamStatsList()
        {
            NotifyDataCollectionUpdated(nameof(context.StreamStats));
        }

        private void RefreshUsersList()
        {
            NotifyDataCollectionUpdated(nameof(context.Users));
        }

        private void RefreshUserStatsList()
        {
            NotifyDataCollectionUpdated(nameof(context.UserStats));
        }

        private void RefreshWebhooksList()
        {
            NotifyDataCollectionUpdated(nameof(context.Webhooks));
        }

        #endregion

    }

    /*

#define USE_OBSERVABLECOLLECTION1
#define BUNDLE_REFRESHPACKAGE1
#define USE_LIST

#if USE_OBSERVABLECOLLECTION
using System.Collections.ObjectModel;
#endif

namespace StreamerBotLib.DataSQL.SingleContext
{
        internal partial class DataManagerSQLAsync
    {

#if USE_OBSERVABLECOLLECTION

        #region LocalView ObservableCollection

        private readonly SQLDBContext context;

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
                await context.BanReasons.LoadAsync();
                return context.BanReasons.Local.ToObservableCollection();
            });
        }

        private Task<ObservableCollection<BanRules>> GetBanRulesLocalObservableAsync()
        {
            return Task.Run(async () =>
            {
                await context.BanRules.LoadAsync();
                return context.BanRules.Local.ToObservableCollection();
            });
        }

        private Task<ObservableCollection<CategoryList>> GetCategoryListLocalObservableAsync()
        {
            return Task.Run(async () =>
            {
                await context.CategoryList.LoadAsync();
                return context.CategoryList.Local.ToObservableCollection();
            });
        }

        private Task<ObservableCollection<ChannelEvents>> GetChannelEventsLocalObservableAsync()
        {
            return Task.Run(async () =>
            {
                await context.ChannelEvents.LoadAsync();
                return context.ChannelEvents.Local.ToObservableCollection();
            });
        }

        private Task<ObservableCollection<Clips>> GetClipsLocalObservableAsync()
        {
            return Task.Run(async () =>
            {
                await context.Clips.LoadAsync();
                return context.Clips.Local.ToObservableCollection();
            });
        }

        private Task<ObservableCollection<Commands>> GetCommandsLocalObservableAsync()
        {
            return Task.Run(async () =>
            {
                await context.Commands.LoadAsync();
                return context.Commands.Local.ToObservableCollection();
            });
        }

        private Task<ObservableCollection<CommandsUser>> GetCommandsUserLocalObservableAsync()
        {
            return Task.Run(async () =>
            {
                await context.CommandsUser.LoadAsync();
                return context.CommandsUser.Local.ToObservableCollection();
            });
        }

        private Task<ObservableCollection<Currency>> GetCurrencyLocalObservableAsync()
        {
            return Task.Run(async () =>
            {
                await context.Currency.LoadAsync();
                return context.Currency.Local.ToObservableCollection();
            });
        }

        private Task<ObservableCollection<Models.CurrencyType>> GetCurrencyTypeLocalObservableAsync()
        {
            return Task.Run(async () =>
            {
                await context.CurrencyType.LoadAsync();
                return context.CurrencyType.Local.ToObservableCollection();
            });
        }

        private Task<ObservableCollection<CustomWelcome>> GetCustomWelcomeLocalObservableAsync()
        {
            return Task.Run(async () =>
            {
                await context.CustomWelcome.LoadAsync();
                return context.CustomWelcome.Local.ToObservableCollection();
            });
        }

        private Task<ObservableCollection<Followers>> GetFollowersLocalObservableAsync()
        {
            return Task.Run(async () =>
            {
                await context.Followers.LoadAsync();
                return context.Followers.Local.ToObservableCollection();
            });
        }

        private Task<ObservableCollection<GameDeadCounter>> GetGameDeadCounterLocalObservableAsync()
        {
            return Task.Run(async () =>
            {
                await context.GameDeadCounter.LoadAsync();
                return context.GameDeadCounter.Local.ToObservableCollection();
            });
        }

        private Task<ObservableCollection<GiveawayUserData>> GetGiveawayUserDataLocalObservableAsync()
        {
            return Task.Run(async () =>
            {
                await context.GiveawayUserData.LoadAsync();
                return context.GiveawayUserData.Local.ToObservableCollection();
            });
        }

        private Task<ObservableCollection<InRaidData>> GetInRaidDataLocalObservableAsync()
        {
            return Task.Run(async () =>
            {
                await context.InRaidData.LoadAsync();
                return context.InRaidData.Local.ToObservableCollection();
            });
        }

        private Task<ObservableCollection<LearnMsgs>> GetLearnMsgsLocalObservableAsync()
        {
            return Task.Run(async () =>
            {
                await context.LearnMsgs.LoadAsync();
                return context.LearnMsgs.Local.ToObservableCollection();
            });
        }

        private Task<ObservableCollection<ModeratorApprove>> GetModeratorApproveLocalObservableAsync()
        {
            return Task.Run(async () =>
            {
                await context.ModeratorApprove.LoadAsync();
                return context.ModeratorApprove.Local.ToObservableCollection();
            });
        }

        private Task<ObservableCollection<MultiChannels>> GetMultiChannelsLocalObservableAsync()
        {
            return Task.Run(async () =>
            {
                await context.MultiChannels.LoadAsync();
                return context.MultiChannels.Local.ToObservableCollection();
            });
        }

        private Task<ObservableCollection<MultiLiveStreams>> GetMultiLiveStreamsLocalObservableAsync()
        {
            return Task.Run(async () =>
            {
                await context.MultiLiveStreams.LoadAsync();
                return context.MultiLiveStreams.Local.ToObservableCollection();
            });
        }

        private Task<ObservableCollection<MultiWebhooks>> GetMultiWebhooksLocalObservableAsync()
        {
            return Task.Run(async () =>
            {
                await context.MultiWebhooks.LoadAsync();
                return context.MultiWebhooks.Local.ToObservableCollection();
            });
        }

        private Task<ObservableCollection<MultiSummaryLiveStreams>> GetMultiSummaryLiveStreamsLocalObservableAsync()
        {
            return Task.Run(async () =>
            {
                await context.MultiSummaryLiveStreams.LoadAsync();
                return context.MultiSummaryLiveStreams.Local.ToObservableCollection();
            });
        }

        private Task<ObservableCollection<OldFollowUsers>> GetOldFollowUsersLocalObservableAsync()
        {
            return Task.Run(async () =>
            {
                await context.OldFollowUsers.LoadAsync();
                return context.OldFollowUsers.Local.ToObservableCollection();
            });
        }

        private Task<ObservableCollection<OutRaidData>> GetOutRaidDataLocalObservableAsync()
        {
            return Task.Run(async () =>
            {
                await context.OutRaidData.LoadAsync();
                return context.OutRaidData.Local.ToObservableCollection();
            });
        }

        private Task<ObservableCollection<OverlayServices>> GetOverlayServicesLocalObservableAsync()
        {
            return Task.Run(async () =>
            {
                await context.OverlayServices.LoadAsync();
                return context.OverlayServices.Local.ToObservableCollection();
            });
        }

        private Task<ObservableCollection<OverlayTicker>> GetOverlayTickerLocalObservableAsync()
        {
            return Task.Run(async () =>
            {
                await context.OverlayTicker.LoadAsync();
                return context.OverlayTicker.Local.ToObservableCollection();
            });
        }

        private Task<ObservableCollection<Quotes>> GetQuotesLocalObservableAsync()
        {
            return Task.Run(async () =>
            {
                await context.Quotes.LoadAsync();
                return context.Quotes.Local.ToObservableCollection();
            });
        }

        private Task<ObservableCollection<ShoutOuts>> GetShoutOutsLocalObservableAsync()
        {
            return Task.Run(async () =>
            {
                await context.ShoutOuts.LoadAsync();
                return context.ShoutOuts.Local.ToObservableCollection();
            });
        }

        private Task<ObservableCollection<StreamStats>> GetStreamStatsLocalObservableAsync()
        {
            return Task.Run(async () =>
            {
                await context.StreamStats.LoadAsync();
                return context.StreamStats.Local.ToObservableCollection();
            });
        }

        private Task<ObservableCollection<Users>> GetUsersLocalObservableAsync()
        {
            return Task.Run(async () =>
            {
                await context.Users.LoadAsync();
                return context.Users.Local.ToObservableCollection();
            });
        }

        private Task<ObservableCollection<UserStats>> GetUserStatsLocalObservableAsync()
        {
            return Task.Run(async () =>
            {
                await context.UserStats.LoadAsync();
                return context.UserStats.Local.ToObservableCollection();
            });
        }

        private Task<ObservableCollection<Webhooks>> GetWebhooksLocalObservableAsync()
        {
            return Task.Run(async () =>
            {
                await context.Webhooks.LoadAsync();
                return context.Webhooks.Local.ToObservableCollection();
            });
        }

        #endregion

        internal void NotifyDataCollectionUpdated(string TableName, object TableData = null)
        {
            OnDataCollectionUpdated?.Invoke(this, new(TableName));
        }

        #region Refresh Collections
        private void RefreshBanReasonsList()
        {
            PostActionQueue(() =>
            {
                ThreadManager.AddAsyncTaskToGUIDispatcher("RefreshUserStatsObservableCollection", async () =>
                {
                    await context.BanReasons.LoadAsync();
                    NotifyDataCollectionUpdated(nameof(context.BanReasons));
                });
            }, "BanReasons");
        }

        private void RefreshBanRulesList()
        {
            PostActionQueue(() =>
            {
                ThreadManager.AddAsyncTaskToGUIDispatcher("RefreshUserStatsObservableCollection", async () =>
                {
                    await context.BanRules.LoadAsync();
                    NotifyDataCollectionUpdated(nameof(context.BanRules));
                });
            }, "BanRules");
        }

        private void RefreshCategoryListList()
        {
            PostActionQueue(() =>
            {
                ThreadManager.AddAsyncTaskToGUIDispatcher("RefreshUserStatsObservableCollection", async () =>
                {
                    await context.CategoryList.LoadAsync();
                    NotifyDataCollectionUpdated(nameof(context.CategoryList));
                });
            }, "CategoryList");
        }

        private void RefreshChannelEventsList()
        {
            PostActionQueue(() =>
            {
                ThreadManager.AddAsyncTaskToGUIDispatcher("RefreshUserStatsObservableCollection", async () =>
                {
                    await context.ChannelEvents.LoadAsync();
                    NotifyDataCollectionUpdated(nameof(context.ChannelEvents));
                });
            }, "ChannelEvents");
        }

        private void RefreshClipsList()
        {
            PostActionQueue(() =>
            {
                ThreadManager.AddAsyncTaskToGUIDispatcher("RefreshUserStatsObservableCollection", async () =>
                {
                    await context.Clips.LoadAsync();
                    NotifyDataCollectionUpdated(nameof(context.Clips));
                });
            }, "Clips");
        }

        private void RefreshCommandsList()
        {
            PostActionQueue(() =>
            {
                ThreadManager.AddAsyncTaskToGUIDispatcher("RefreshUserStatsObservableCollection", async () =>
                {
                    await context.Commands.LoadAsync();
                    NotifyDataCollectionUpdated(nameof(context.Commands));
                });
            }, "Commands");
        }

        private void RefreshCommandsUserList()
        {
            PostActionQueue(() =>
            {
                ThreadManager.AddAsyncTaskToGUIDispatcher("RefreshUserStatsObservableCollection", async () =>
                {
                    await context.CommandsUser.LoadAsync();
                    NotifyDataCollectionUpdated(nameof(context.CommandsUser));
                });
            }, "CommandsUser");
        }

        private void RefreshCurrencyList()
        {
            PostActionQueue(() =>
            {
                ThreadManager.AddTaskToGUIDispatcher(async () =>
                {
                    await context.Currency.LoadAsync();
                    NotifyDataCollectionUpdated(nameof(context.Currency));
                });
            }, "Currency");
        }

        private void RefreshCurrencyTypeList()
        {
            PostActionQueue(() =>
            {
                ThreadManager.AddAsyncTaskToGUIDispatcher("RefreshUserStatsObservableCollection", async () =>
                {
                    await context.CurrencyType.LoadAsync();
                    NotifyDataCollectionUpdated(nameof(context.CurrencyType));
                });
            }, "CurrencyType");
        }

        private void RefreshCustomWelcomeList()
        {
            PostActionQueue(() =>
            {
                ThreadManager.AddAsyncTaskToGUIDispatcher("RefreshUserStatsObservableCollection", async () =>
                {
                    await context.CustomWelcome.LoadAsync();
                    NotifyDataCollectionUpdated(nameof(context.CustomWelcome));
                });
            }, "CustomWelcome");
        }

        private void RefreshFollowersList()
        {
            PostActionQueue(() =>
            {
                ThreadManager.AddAsyncTaskToGUIDispatcher("RefreshUserStatsObservableCollection", async () =>
                {
                    await context.Followers.LoadAsync();
                    NotifyDataCollectionUpdated(nameof(context.Followers));
                });
            }, "Followers");
        }

        private void RefreshGameDeadCounterList()
        {
            PostActionQueue(() =>
            {
                ThreadManager.AddAsyncTaskToGUIDispatcher("RefreshUserStatsObservableCollection", async () =>
                {
                    await context.GameDeadCounter.LoadAsync();
                    NotifyDataCollectionUpdated(nameof(context.GameDeadCounter));
                });
            }, "GameDeadCounter");
        }

        private void RefreshGiveawayUserDataList()
        {
            PostActionQueue(() =>
            {
                ThreadManager.AddAsyncTaskToGUIDispatcher("RefreshUserStatsObservableCollection", async () =>
                {
                    await context.GiveawayUserData.LoadAsync();
                    NotifyDataCollectionUpdated(nameof(context.GiveawayUserData));
                });
            }, "GiveawayUser");
        }

        private void RefreshInRaidDataList()
        {
            PostActionQueue(() =>
            {
                ThreadManager.AddAsyncTaskToGUIDispatcher("RefreshUserStatsObservableCollection", async () =>
                {
                    await context.InRaidData.LoadAsync();
                    NotifyDataCollectionUpdated(nameof(context.InRaidData));
                });
            }, "InRaidData");
        }

        private void RefreshLearnMsgsList()
        {
            PostActionQueue(() =>
            {
                ThreadManager.AddAsyncTaskToGUIDispatcher("RefreshUserStatsObservableCollection", async () =>
                {
                    await context.LearnMsgs.LoadAsync();
                    NotifyDataCollectionUpdated(nameof(context.LearnMsgs));
                });
            }, "LearnMsgs");
        }

        private void RefreshModeratorApproveList()
        {
            PostActionQueue(() =>
            {
                ThreadManager.AddAsyncTaskToGUIDispatcher("RefreshUserStatsObservableCollection", async () =>
                {
                    await context.ModeratorApprove.LoadAsync();
                    NotifyDataCollectionUpdated(nameof(context.ModeratorApprove));
                });
            }, "ModeratorApprove");
        }

        private void RefreshMultiChannelsList()
        {
            PostActionQueue(() =>
            {
                ThreadManager.AddAsyncTaskToGUIDispatcher("RefreshUserStatsObservableCollection", async () =>
                {
                    await context.MultiChannels.LoadAsync();
                    NotifyDataCollectionUpdated(nameof(context.MultiChannels));
                });
            }, "MultiChannels");
        }

        private void RefreshMultiLiveStreamsList()
        {
            PostActionQueue(() =>
            {
                ThreadManager.AddAsyncTaskToGUIDispatcher("RefreshUserStatsObservableCollection", async () =>
                {
                    await context.MultiLiveStreams.LoadAsync();
                    NotifyDataCollectionUpdated(nameof(context.MultiLiveStreams));
                });
            }, "MultiLiveStreams");
        }

        private void RefreshMultiSummaryLiveStreamsList()
        {
            PostActionQueue(() =>
            {
                ThreadManager.AddAsyncTaskToGUIDispatcher("RefreshUserStatsObservableCollection", async () =>
                {
                    await context.MultiSummaryLiveStreams.LoadAsync();
                    NotifyDataCollectionUpdated(nameof(context.MultiSummaryLiveStreams));
                });
            }, "MultiSummaryLiveStreams");
        }

        private void RefreshMultiWebhooksList()
        {
            PostActionQueue(() =>
            {
                ThreadManager.AddAsyncTaskToGUIDispatcher("RefreshUserStatsObservableCollection", async () =>
                {
                    await context.MultiWebhooks.LoadAsync();
                    NotifyDataCollectionUpdated(nameof(context.MultiWebhooks));
                });
            }, "MultiWebhooks");
        }

        private void RefreshOldFollowUsersList()
        {
            PostActionQueue(() =>
            {
                ThreadManager.AddAsyncTaskToGUIDispatcher("RefreshUserStatsObservableCollection", async () =>
                {
                    await context.OldFollowUsers.LoadAsync();
                    NotifyDataCollectionUpdated(nameof(context.OldFollowUsers));
                });
            }, "OldFollowUsers");
        }

        private void RefreshOutRaidDataList()
        {
            PostActionQueue(() =>
            {
                ThreadManager.AddAsyncTaskToGUIDispatcher("RefreshUserStatsObservableCollection", async () =>
                {
                    await context.OutRaidData.LoadAsync();
                    NotifyDataCollectionUpdated(nameof(context.OutRaidData));
                });
            }, "OutRaidData");
        }

        private void RefreshOverlayServicesList()
        {
            PostActionQueue(() =>
            {
                ThreadManager.AddAsyncTaskToGUIDispatcher("RefreshUserStatsObservableCollection", async () =>
                {
                    await context.OverlayServices.LoadAsync();
                    NotifyDataCollectionUpdated(nameof(context.OverlayServices));
                });
            }, "OverlayServices");
        }

        private void RefreshOverlayTickerList()
        {
            PostActionQueue(() =>
            {
                ThreadManager.AddAsyncTaskToGUIDispatcher("RefreshUserStatsObservableCollection", async () =>
                {
                    await context.OverlayTicker.LoadAsync();
                    NotifyDataCollectionUpdated(nameof(context.OverlayTicker));
                });
            }, "OverlayTicker");
        }

        private void RefreshQuotesList()
        {
            PostActionQueue(() =>
            {
                ThreadManager.AddAsyncTaskToGUIDispatcher("RefreshUserStatsObservableCollection", async () =>
                {
                    await context.Quotes.LoadAsync();
                    NotifyDataCollectionUpdated(nameof(context.Quotes));
                });
            }, "Quotes");
        }

        private void RefreshShoutOutsList()
        {
            PostActionQueue(() =>
            {
                ThreadManager.AddAsyncTaskToGUIDispatcher("RefreshUserStatsObservableCollection", async () =>
                {
                    await context.ShoutOuts.LoadAsync();
                    NotifyDataCollectionUpdated(nameof(context.ShoutOuts));
                });
            }, "ShoutOUts");
        }

        private void RefreshStreamStatsList()
        {
            PostActionQueue(() =>
            {
                ThreadManager.AddAsyncTaskToGUIDispatcher("RefreshUserStatsObservableCollection", async () =>
                {
                    await context.StreamStats.LoadAsync();
                    NotifyDataCollectionUpdated(nameof(context.StreamStats));
                });
            }, "StreamStats");
        }

        private void RefreshUsersList()
        {
            PostActionQueue(() =>
            {
                ThreadManager.AddAsyncTaskToGUIDispatcher("RefreshUsersObservableCollection", async () =>
                {
                    LogWriter.DebugLog("RefreshUsersObservableCollection",
                            DebugLogTypes.DataManager, $"Reloading Users data into the database context.");
                    await context.Users.LoadAsync();
                    LogWriter.DebugLog("RefreshUsersObservableCollection",
                            DebugLogTypes.DataManager, $"Notifying the DataCollection is Updated.");
                    NotifyDataCollectionUpdated(nameof(context.Users));
                });
            }, "Users");
        }

        private void RefreshUserStatsList()
        {
            PostActionQueue(() =>
            {
                ThreadManager.AddAsyncTaskToGUIDispatcher("RefreshUserStatsObservableCollection", async () =>
                {
                    LogWriter.DebugLog("RefreshUserStatsObservableCollection",
                      DebugLogTypes.DataManager, $"Reloading UserStats data into the database context.");
                    await context.UserStats.LoadAsync();
                    LogWriter.DebugLog("RefreshUserStatsObservableCollection",
       DebugLogTypes.DataManager, $"Notifying the DataCollection is Updated.");
                    NotifyDataCollectionUpdated(nameof(context.UserStats));
                });
            }, "UserStats");
        }

        private void RefreshWebhooksList()
        {
            PostActionQueue(() =>
            {
                ThreadManager.AddAsyncTaskToGUIDispatcher("RefreshWebhooksObservableCollection", async () =>
                {
                    await context.Webhooks.LoadAsync();
                    NotifyDataCollectionUpdated(nameof(context.Webhooks));
                });
            }, "Webhooks");

        }

        #endregion

#elif USE_LIST

#elif BUNDLE_REFRESHPACKAGE
  
        #region LocalView List

        private readonly SQLDBContext context;

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

                await context.BanReasons.LoadAsync();
                return context.BanReasons.Local.ToList();
            });
        }

        private Task<List<BanRules>> GetBanRulesLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetBanRulesLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading BanRules data into the database context.");
                await context.BanRules.LoadAsync();
                return context.BanRules.Local.ToList();
            });
        }

        private Task<List<CategoryList>> GetCategoryListLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetCategoryListLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading CategoryList data into the database context.");
                await context.CategoryList.LoadAsync();
                return context.CategoryList.Local.ToList();
            });
        }

        private Task<List<ChannelEvents>> GetChannelEventsLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetChannelEventsLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading ChannelEvents data into the database context.");
                await context.ChannelEvents.LoadAsync();
                return context.ChannelEvents.Local.ToList();
            });
        }

        private Task<List<Clips>> GetClipsLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetClipsLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading Clips data into the database context.");
                await context.Clips.LoadAsync();
                return context.Clips.Local.ToList();
            });
        }

        private Task<List<Commands>> GetCommandsLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetCommandsLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading Commands data into the database context.");
                await context.Commands.LoadAsync();
                return context.Commands.Local.ToList();
            });
        }

        private Task<List<CommandsUser>> GetCommandsUserLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetCommandsUserLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading CommandsUser data into the database context.");
                await context.CommandsUser.LoadAsync();
                return context.CommandsUser.Local.ToList();
            });
        }

        private Task<List<Currency>> GetCurrencyLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetCurrencyLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading Currency data into the database context.");
                await context.Currency.LoadAsync();
                return context.Currency.Local.ToList();
            });
        }

        private Task<List<Models.CurrencyType>> GetCurrencyTypeLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetCurrencyTypeLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading CurrencyType data into the database context.");
                await context.CurrencyType.LoadAsync();
                return context.CurrencyType.Local.ToList();
            });
        }

        private Task<List<CustomWelcome>> GetCustomWelcomeLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetCustomWelcomeLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading CustomWelcome data into the database context.");
                await context.CustomWelcome.LoadAsync();
                return context.CustomWelcome.Local.ToList();
            });
        }

        private Task<List<Followers>> GetFollowersLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetFollowersLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading Followers data into the database context.");
                await context.Followers.LoadAsync();
                return context.Followers.Local.ToList();
            });
        }

        private Task<List<GameDeadCounter>> GetGameDeadCounterLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetGameDeadCounterLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading GameDeadCounter data into the database context.");

                await context.GameDeadCounter.LoadAsync();
                return context.GameDeadCounter.Local.ToList();
            });
        }

        private Task<List<GiveawayUserData>> GetGiveawayUserDataLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetGiveawayUserDataLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading GiveawayUserData data into the database context.");

                await context.GiveawayUserData.LoadAsync();
                return context.GiveawayUserData.Local.ToList();
            });
        }

        private Task<List<InRaidData>> GetInRaidDataLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetInRaidDataLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading InRaidData data into the database context.");
                await context.InRaidData.LoadAsync();
                return context.InRaidData.Local.ToList();
            });
        }

        private Task<List<LearnMsgs>> GetLearnMsgsLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetLearnMsgsLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading LearnMsgs data into the database context.");
                await context.LearnMsgs.LoadAsync();
                return context.LearnMsgs.Local.ToList();
            });
        }

        private Task<List<ModeratorApprove>> GetModeratorApproveLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetModeratorApproveLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading ModeratorApprove data into the database context.");
                await context.ModeratorApprove.LoadAsync();
                return context.ModeratorApprove.Local.ToList();
            });
        }

        private Task<List<MultiChannels>> GetMultiChannelsLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetMultiChannelsLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading MultiChannels data into the database context.");
                await context.MultiChannels.LoadAsync();
                return context.MultiChannels.Local.ToList();
            });
        }

        private Task<List<MultiLiveStreams>> GetMultiLiveStreamsLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetMultiLiveStreamsLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading MultiLiveStreams data into the database context.");
                await context.MultiLiveStreams.LoadAsync();
                return context.MultiLiveStreams.Local.ToList();
            });
        }

        private Task<List<MultiWebhooks>> GetMultiWebhooksLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetMultiWebhooksLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading MultiWebhooks data into the database context.");
                await context.MultiWebhooks.LoadAsync();
                return context.MultiWebhooks.Local.ToList();
            });
        }

        private Task<List<MultiSummaryLiveStreams>> GetMultiSummaryLiveStreamsLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetMultiSummaryLiveStreamsLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading MultiSummaryLiveStreams data into the database context.");
                await context.MultiSummaryLiveStreams.LoadAsync();
                return context.MultiSummaryLiveStreams.Local.ToList();
            });
        }

        private Task<List<OldFollowUsers>> GetOldFollowUsersLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetOldFollowUsersLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading OldFollowUsers data into the database context.");
                await context.OldFollowUsers.LoadAsync();
                return context.OldFollowUsers.Local.ToList();
            });
        }

        private Task<List<OutRaidData>> GetOutRaidDataLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetOutRaidDataLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading OutRaidData data into the database context.");
                await context.OutRaidData.LoadAsync();
                return context.OutRaidData.Local.ToList();
            });
        }

        private Task<List<OverlayServices>> GetOverlayServicesLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetOverlayServicesLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading OverlayServices data into the database context.");
                await context.OverlayServices.LoadAsync();
                return context.OverlayServices.Local.ToList();
            });
        }

        private Task<List<OverlayTicker>> GetOverlayTickerLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetOverlayTickerLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading OverlayTicker data into the database context.");
                await context.OverlayTicker.LoadAsync();
                return context.OverlayTicker.Local.ToList();
            });
        }

        private Task<List<Quotes>> GetQuotesLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetQuotesLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading Quotes data into the database context.");
                await context.Quotes.LoadAsync();
                return context.Quotes.Local.ToList();
            });
        }

        private Task<List<ShoutOuts>> GetShoutOutsLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetShoutOutsLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading ShoutOuts data into the database context.");
                await context.ShoutOuts.LoadAsync();
                return context.ShoutOuts.Local.ToList();
            });
        }

        private Task<List<StreamStats>> GetStreamStatsLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetStreamStatsLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading StreamStats data into the database context.");
                await context.StreamStats.LoadAsync();
                return context.StreamStats.Local.ToList();
            });
        }

        private Task<List<Users>> GetUsersLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetUsersLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading Users data into the database context.");
                await context.Users.LoadAsync();
                return context.Users.Local.ToList();
            });
        }

        private Task<List<UserStats>> GetUserStatsLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetUserStatsLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading UserStats data into the database context.");
                await context.UserStats.LoadAsync();
                return context.UserStats.Local.ToList();
            });
        }

        private Task<List<Webhooks>> GetWebhooksLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetWebhooksLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading Webhooks data into the database context.");
                await context.Webhooks.LoadAsync();
                return context.Webhooks.Local.ToList();
            });
        }

        #endregion

        internal void NotifyDataCollectionUpdated(string TableName, Action refreshAction, object tableData)
        {
            LogWriter.DebugLog("NotifyDataCollectionUpdated", DebugLogTypes.DataManager,
                $"Notifying the DataCollection is Updated for {TableName}.");
            OnDataCollectionUpdated?.Invoke(this, new(TableName, refreshAction, tableData));
        }

        #region Refresh Collections
        private void RefreshBanReasonsList()
        {
            LogWriter.DebugLog("RefreshBanReasonsList", DebugLogTypes.DataManager,
                $"Reloading BanReasons data into the database context.");
            PostActionQueue(() =>
            {
                NotifyDataCollectionUpdated(nameof(context.BanReasons), () => context.BanReasons.LoadAsync(), null);
            }, "RefreshBanReasonsList");
        }

        private void RefreshBanRulesList()
        {
            LogWriter.DebugLog("RefreshBanRulesList", DebugLogTypes.DataManager,
                $"Reloading BanRules data into the database context.");
            PostActionQueue(() =>
            {
                NotifyDataCollectionUpdated(nameof(context.BanRules), () => context.BanRules.LoadAsync(), null);
            }, "RefreshBanRulesList");
        }

        private void RefreshCategoryListList()
        {
            LogWriter.DebugLog("RefreshCategoryListList", DebugLogTypes.DataManager,
                $"Reloading CategoryList data into the database context.");
            PostActionQueue(() =>
            {
                NotifyDataCollectionUpdated(nameof(context.CategoryList), () => context.CategoryList.LoadAsync(), null);
            }, "RefreshCategoryList");
        }

        private void RefreshChannelEventsList()
        {
            LogWriter.DebugLog("RefreshChannelEventsList", DebugLogTypes.DataManager,
                $"Reloading ChannelEvents data into the database context.");
            PostActionQueue(() =>
            {
                NotifyDataCollectionUpdated(nameof(context.ChannelEvents), () => context.ChannelEvents.LoadAsync(), null);
            }, "RefreshChannelEventsList");
        }

        private void RefreshClipsList()
        {
            LogWriter.DebugLog("RefreshClipsList", DebugLogTypes.DataManager,
                $"Reloading Clips data into the database context.");
            PostActionQueue(() =>
            {
                NotifyDataCollectionUpdated(nameof(context.Clips), () => context.Clips.LoadAsync(), null);
            }, "RefreshClipsList");
        }

        private void RefreshCommandsList()
        {
            LogWriter.DebugLog("RefreshCommandsList", DebugLogTypes.DataManager,
                $"Reloading Commands data into the database context.");
            PostActionQueue(() =>
            {
                NotifyDataCollectionUpdated(nameof(context.Commands), () => context.Commands.LoadAsync(), null);
            }, "RefreshCommandsList");
        }

        private void RefreshCommandsUserList()
        {
            LogWriter.DebugLog("RefreshCommandsUserList", DebugLogTypes.DataManager,
                $"Reloading CommandsUser data into the database context.");
            PostActionQueue(() =>
            {
                NotifyDataCollectionUpdated(nameof(context.CommandsUser), () => context.CommandsUser.LoadAsync(), null);
            }, "RefreshCommandsUserList");
        }

        private void RefreshCurrencyList()
        {
            LogWriter.DebugLog("RefreshCurrencyList", DebugLogTypes.DataManager,
                $"Reloading Currency data into the database context.");
            PostActionQueue(() =>
            {
                NotifyDataCollectionUpdated(nameof(context.Currency), () => context.Currency.LoadAsync(), null);
            }, "RefreshCurrencyList");
        }

        private void RefreshCurrencyTypeList()
        {
            LogWriter.DebugLog("RefreshCurrencyTypeList", DebugLogTypes.DataManager,
                $"Reloading CurrencyType data into the database context.");
            PostActionQueue(() =>
            {
                NotifyDataCollectionUpdated(nameof(context.CurrencyType), () => context.CurrencyType.LoadAsync(), null);
            }, "RefreshCurrencyTypeList");
        }

        private void RefreshCustomWelcomeList()
        {
            LogWriter.DebugLog("RefreshCustomWelcomeList", DebugLogTypes.DataManager,
                $"Reloading CustomWelcome data into the database context.");
            PostActionQueue(() =>
            {
                NotifyDataCollectionUpdated(nameof(context.CustomWelcome), () => context.CustomWelcome.LoadAsync(), null);
            }, "RefreshCustomWelcomeList");
        }

        private void RefreshFollowersList()
        {
            LogWriter.DebugLog("RefreshFollowersList", DebugLogTypes.DataManager,
                $"Reloading Followers data into the database context.");
            PostActionQueue(() =>
            {
                NotifyDataCollectionUpdated(nameof(context.Followers), () => context.Followers.LoadAsync(), null);
            }, "RefreshFollowersList");
        }

        private void RefreshGameDeadCounterList()
        {
            LogWriter.DebugLog("RefreshGameDeadCounterList", DebugLogTypes.DataManager,
                $"Reloading GameDeadCounter data into the database context.");
            PostActionQueue(() =>
            {
                NotifyDataCollectionUpdated(nameof(context.GameDeadCounter), () => context.GameDeadCounter.LoadAsync(), null);
            }, "RefreshGameDeadCounterList");
        }

        private void RefreshGiveawayUserDataList()
        {
            LogWriter.DebugLog("RefreshGiveawayUserDataList", DebugLogTypes.DataManager,
                $"Reloading GiveawayUserData data into the database context.");
            PostActionQueue(() =>
            {
                NotifyDataCollectionUpdated(nameof(context.GiveawayUserData), () => context.GiveawayUserData.LoadAsync(), null);
            }, "RefreshGiveawayUserDataList");
        }

        private void RefreshInRaidDataList()
        {
            LogWriter.DebugLog("RefreshInRaidDataList", DebugLogTypes.DataManager,
                $"Reloading InRaidData data into the database context.");
            PostActionQueue(() =>
            {
                NotifyDataCollectionUpdated(nameof(context.InRaidData), () => context.InRaidData.LoadAsync(), null);
            }, "RefreshInRaidDataList");
        }

        private void RefreshLearnMsgsList()
        {
            LogWriter.DebugLog("RefreshLearnMsgsList", DebugLogTypes.DataManager,
                $"Reloading LearnMsgs data into the database context.");
            PostActionQueue(() =>
            {
                NotifyDataCollectionUpdated(nameof(context.LearnMsgs), () => context.LearnMsgs.LoadAsync(), null);
            }, "RefreshLearnMsgsList");
        }

        private void RefreshModeratorApproveList()
        {
            LogWriter.DebugLog("RefreshModeratorApproveList", DebugLogTypes.DataManager,
                $"Reloading ModeratorApprove data into the database context.");
            PostActionQueue(() =>
            {
                NotifyDataCollectionUpdated(nameof(context.ModeratorApprove), () => context.ModeratorApprove.LoadAsync(), null);
            }, "RefreshModeratorApproveList");
        }

        private void RefreshMultiChannelsList()
        {
            LogWriter.DebugLog("RefreshMultiChannelsList", DebugLogTypes.DataManager,
                $"Reloading MultiChannels data into the database context.");
            PostActionQueue(() =>
            {
                NotifyDataCollectionUpdated(nameof(context.MultiChannels), () => context.MultiChannels.LoadAsync(), null);
            }, "RefreshMultiChannelsList");
        }

        private void RefreshMultiLiveStreamsList()
        {
            LogWriter.DebugLog("RefreshMultiLiveStreamsList", DebugLogTypes.DataManager,
                $"Reloading MultiLiveStreams data into the database context.");
            PostActionQueue(() =>
            {
                NotifyDataCollectionUpdated(nameof(context.MultiLiveStreams), () => context.MultiLiveStreams.LoadAsync(), null);
            }, "RefreshMultiLiveStreamsList");
        }

        private void RefreshMultiSummaryLiveStreamsList()
        {
            LogWriter.DebugLog("RefreshMultiSummaryLiveStreamsList", DebugLogTypes.DataManager,
                $"Reloading MultiSummaryLiveStreams data into the database context.");
            PostActionQueue(() =>
            {
                NotifyDataCollectionUpdated(nameof(context.MultiSummaryLiveStreams), () => context.MultiSummaryLiveStreams.LoadAsync(), null);
            }, "RefreshMultiSummaryLiveStreamsList");
        }

        private void RefreshMultiWebhooksList()
        {
            LogWriter.DebugLog("RefreshMultiWebhooksList", DebugLogTypes.DataManager,
                $"Reloading MultiWebhooks data into the database context.");
            PostActionQueue(() =>
            {
                NotifyDataCollectionUpdated(nameof(context.MultiWebhooks), () => context.MultiWebhooks.LoadAsync(), null);
            }, "RefreshMultiWebhooksList");
        }

        private void RefreshOldFollowUsersList()
        {
            LogWriter.DebugLog("RefreshOldFollowUsersList", DebugLogTypes.DataManager,
                $"Reloading OldFollowUsers data into the database context.");
            PostActionQueue(() =>
            {
                NotifyDataCollectionUpdated(nameof(context.OldFollowUsers), () => context.OldFollowUsers.LoadAsync(), null);
            }, "RefreshOldFollowUsersList");
        }

        private void RefreshOutRaidDataList()
        {
            LogWriter.DebugLog("RefreshOutRaidDataList", DebugLogTypes.DataManager,
                $"Reloading OutRaidData data into the database context.");
            PostActionQueue(() =>
            {
                NotifyDataCollectionUpdated(nameof(context.OutRaidData), () => context.OutRaidData.LoadAsync(), null);
            }, "RefreshOutRaidDataList");
        }

        private void RefreshOverlayServicesList()
        {
            LogWriter.DebugLog("RefreshOverlayServicesList", DebugLogTypes.DataManager,
                $"Reloading OverlayServices data into the database context.");
            PostActionQueue(() =>
            {
                NotifyDataCollectionUpdated(nameof(context.OverlayServices), () => context.OverlayServices.LoadAsync(), null);
            }, "RefreshOverlayServicesList");
        }

        private void RefreshOverlayTickerList()
        {
            LogWriter.DebugLog("RefreshOverlayTickerList", DebugLogTypes.DataManager,
                $"Reloading OverlayTicker data into the database context.");
            PostActionQueue(() =>
            {
                NotifyDataCollectionUpdated(nameof(context.OverlayTicker), () => context.OverlayTicker.LoadAsync(), null);
            }, "RefreshOverlayTickerList");
        }

        private void RefreshQuotesList()
        {
            LogWriter.DebugLog("RefreshQuotesList", DebugLogTypes.DataManager,
                $"Reloading Quotes data into the database context.");
            PostActionQueue(() =>
            {
                NotifyDataCollectionUpdated(nameof(context.Quotes), () => context.Quotes.LoadAsync(), null);
            }, "RefreshQuotesList");
        }

        private void RefreshShoutOutsList()
        {
            LogWriter.DebugLog("RefreshShoutOutsList", DebugLogTypes.DataManager,
                $"Reloading ShoutOuts data into the database context.");
            PostActionQueue(() =>
            {
                NotifyDataCollectionUpdated(nameof(context.ShoutOuts), () => context.ShoutOuts.LoadAsync(), null);
            }, "RefreshShoutOutsList");
        }

        private void RefreshStreamStatsList()
        {
            LogWriter.DebugLog("RefreshStreamStatsList", DebugLogTypes.DataManager,
                $"Reloading StreamStats data into the database context.");
            PostActionQueue(() =>
            {
                NotifyDataCollectionUpdated(nameof(context.StreamStats), () => context.StreamStats.LoadAsync(), null);
            }, "RefreshStreamStatsList");
        }

        private void RefreshUsersList()
        {
            LogWriter.DebugLog("RefreshUsersList", DebugLogTypes.DataManager,
                $"Reloading Users data into the database context.");
            PostActionQueue(() =>
            {
                NotifyDataCollectionUpdated(nameof(context.Users), () => context.Users.LoadAsync(), null);
            }, "RefreshUsersList");
        }

        private void RefreshUserStatsList()
        {
            LogWriter.DebugLog("RefreshUserStatsList", DebugLogTypes.DataManager,
                $"Reloading UserStats data into the database context.");
            PostActionQueue(() =>
            {
                NotifyDataCollectionUpdated(nameof(context.UserStats), () => context.UserStats.LoadAsync(), null);
            }, "RefreshUserStatsList");
        }

        private void RefreshWebhooksList()
        {
            LogWriter.DebugLog("RefreshWebhooksList", DebugLogTypes.DataManager,
                $"Reloading Webhooks data into the database context.");
            PostActionQueue(() =>
            {
                NotifyDataCollectionUpdated(nameof(context.Webhooks), () => context.Webhooks.LoadAsync(), null);
            }, "RefreshWebhooksList");

        }

        #endregion

#else

        #region LocalView List

        private readonly SQLDBContext context;

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

                await context.BanReasons.LoadAsync();
                return context.BanReasons.Local.ToList();
            });
        }

        private Task<List<BanRules>> GetBanRulesLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetBanRulesLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading BanRules data into the database context.");
                await context.BanRules.LoadAsync();
                return context.BanRules.Local.ToList();
            });
        }

        private Task<List<CategoryList>> GetCategoryListLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetCategoryListLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading CategoryList data into the database context.");
                await context.CategoryList.LoadAsync();
                return context.CategoryList.Local.ToList();
            });
        }

        private Task<List<ChannelEvents>> GetChannelEventsLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetChannelEventsLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading ChannelEvents data into the database context.");
                await context.ChannelEvents.LoadAsync();
                return context.ChannelEvents.Local.ToList();
            });
        }

        private Task<List<Clips>> GetClipsLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetClipsLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading Clips data into the database context.");
                await context.Clips.LoadAsync();
                return context.Clips.Local.ToList();
            });
        }

        private Task<List<Commands>> GetCommandsLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetCommandsLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading Commands data into the database context.");
                await context.Commands.LoadAsync();
                return context.Commands.Local.ToList();
            });
        }

        private Task<List<CommandsUser>> GetCommandsUserLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetCommandsUserLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading CommandsUser data into the database context.");
                await context.CommandsUser.LoadAsync();
                return context.CommandsUser.Local.ToList();
            });
        }

        private Task<List<Currency>> GetCurrencyLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetCurrencyLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading Currency data into the database context.");
                await context.Currency.LoadAsync();
                return context.Currency.Local.ToList();
            });
        }

        private Task<List<Models.CurrencyType>> GetCurrencyTypeLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetCurrencyTypeLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading CurrencyType data into the database context.");
                await context.CurrencyType.LoadAsync();
                return context.CurrencyType.Local.ToList();
            });
        }

        private Task<List<CustomWelcome>> GetCustomWelcomeLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetCustomWelcomeLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading CustomWelcome data into the database context.");
                await context.CustomWelcome.LoadAsync();
                return context.CustomWelcome.Local.ToList();
            });
        }

        private Task<List<Followers>> GetFollowersLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetFollowersLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading Followers data into the database context.");
                await context.Followers.LoadAsync();
                return context.Followers.Local.ToList();
            });
        }

        private Task<List<GameDeadCounter>> GetGameDeadCounterLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetGameDeadCounterLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading GameDeadCounter data into the database context.");

                await context.GameDeadCounter.LoadAsync();
                return context.GameDeadCounter.Local.ToList();
            });
        }

        private Task<List<GiveawayUserData>> GetGiveawayUserDataLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetGiveawayUserDataLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading GiveawayUserData data into the database context.");

                await context.GiveawayUserData.LoadAsync();
                return context.GiveawayUserData.Local.ToList();
            });
        }

        private Task<List<InRaidData>> GetInRaidDataLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetInRaidDataLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading InRaidData data into the database context.");
                await context.InRaidData.LoadAsync();
                return context.InRaidData.Local.ToList();
            });
        }

        private Task<List<LearnMsgs>> GetLearnMsgsLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetLearnMsgsLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading LearnMsgs data into the database context.");
                await context.LearnMsgs.LoadAsync();
                return context.LearnMsgs.Local.ToList();
            });
        }

        private Task<List<ModeratorApprove>> GetModeratorApproveLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetModeratorApproveLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading ModeratorApprove data into the database context.");
                await context.ModeratorApprove.LoadAsync();
                return context.ModeratorApprove.Local.ToList();
            });
        }

        private Task<List<MultiChannels>> GetMultiChannelsLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetMultiChannelsLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading MultiChannels data into the database context.");
                await context.MultiChannels.LoadAsync();
                return context.MultiChannels.Local.ToList();
            });
        }

        private Task<List<MultiLiveStreams>> GetMultiLiveStreamsLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetMultiLiveStreamsLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading MultiLiveStreams data into the database context.");
                await context.MultiLiveStreams.LoadAsync();
                return context.MultiLiveStreams.Local.ToList();
            });
        }

        private Task<List<MultiWebhooks>> GetMultiWebhooksLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetMultiWebhooksLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading MultiWebhooks data into the database context.");
                await context.MultiWebhooks.LoadAsync();
                return context.MultiWebhooks.Local.ToList();
            });
        }

        private Task<List<MultiSummaryLiveStreams>> GetMultiSummaryLiveStreamsLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetMultiSummaryLiveStreamsLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading MultiSummaryLiveStreams data into the database context.");
                await context.MultiSummaryLiveStreams.LoadAsync();
                return context.MultiSummaryLiveStreams.Local.ToList();
            });
        }

        private Task<List<OldFollowUsers>> GetOldFollowUsersLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetOldFollowUsersLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading OldFollowUsers data into the database context.");
                await context.OldFollowUsers.LoadAsync();
                return context.OldFollowUsers.Local.ToList();
            });
        }

        private Task<List<OutRaidData>> GetOutRaidDataLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetOutRaidDataLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading OutRaidData data into the database context.");
                await context.OutRaidData.LoadAsync();
                return context.OutRaidData.Local.ToList();
            });
        }

        private Task<List<OverlayServices>> GetOverlayServicesLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetOverlayServicesLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading OverlayServices data into the database context.");
                await context.OverlayServices.LoadAsync();
                return context.OverlayServices.Local.ToList();
            });
        }

        private Task<List<OverlayTicker>> GetOverlayTickerLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetOverlayTickerLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading OverlayTicker data into the database context.");
                await context.OverlayTicker.LoadAsync();
                return context.OverlayTicker.Local.ToList();
            });
        }

        private Task<List<Quotes>> GetQuotesLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetQuotesLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading Quotes data into the database context.");
                await context.Quotes.LoadAsync();
                return context.Quotes.Local.ToList();
            });
        }

        private Task<List<ShoutOuts>> GetShoutOutsLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetShoutOutsLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading ShoutOuts data into the database context.");
                await context.ShoutOuts.LoadAsync();
                return context.ShoutOuts.Local.ToList();
            });
        }

        private Task<List<StreamStats>> GetStreamStatsLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetStreamStatsLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading StreamStats data into the database context.");
                await context.StreamStats.LoadAsync();
                return context.StreamStats.Local.ToList();
            });
        }

        private Task<List<Users>> GetUsersLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetUsersLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading Users data into the database context.");
                await context.Users.LoadAsync();
                return context.Users.Local.ToList();
            });
        }

        private Task<List<UserStats>> GetUserStatsLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetUserStatsLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading UserStats data into the database context.");
                await context.UserStats.LoadAsync();
                return context.UserStats.Local.ToList();
            });
        }

        private Task<List<Webhooks>> GetWebhooksLocalCollectionAsync()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("GetWebhooksLocalCollectionAsync", DebugLogTypes.DataManager,
                    $"Loading Webhooks data into the database context.");
                await context.Webhooks.LoadAsync();
                return context.Webhooks.Local.ToList();
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
                await context.BanReasons.LoadAsync();
                NotifyDataCollectionUpdated(nameof(context.BanReasons), context.BanReasons.Local.ToList());
            }, "RefreshBanReasonsList");
        }

        private void RefreshBanRulesList()
        {
            LogWriter.DebugLog("RefreshBanRulesList", DebugLogTypes.DataManager,
                $"Reloading BanRules data into the database context.");
            PostActionQueue(async () =>
            {
                await context.BanRules.LoadAsync();
                NotifyDataCollectionUpdated(nameof(context.BanRules), context.BanRules.Local.ToList());
            }, "RefreshBanRulesList");
        }

        private void RefreshCategoryListList()
        {
            LogWriter.DebugLog("RefreshCategoryListList", DebugLogTypes.DataManager,
                $"Reloading CategoryList data into the database context.");
            PostActionQueue(async () =>
            {
                await context.CategoryList.LoadAsync();
                NotifyDataCollectionUpdated(nameof(context.CategoryList), context.CategoryList.Local.ToList());
            }, "RefreshCategoryList");
        }

        private void RefreshChannelEventsList()
        {
            LogWriter.DebugLog("RefreshChannelEventsList", DebugLogTypes.DataManager,
                $"Reloading ChannelEvents data into the database context.");
            PostActionQueue(async () =>
            {
                await context.ChannelEvents.LoadAsync();
                NotifyDataCollectionUpdated(nameof(context.ChannelEvents), context.ChannelEvents.Local.ToList());
            }, "RefreshChannelEventsList");
        }

        private void RefreshClipsList()
        {
            LogWriter.DebugLog("RefreshClipsList", DebugLogTypes.DataManager,
                $"Reloading Clips data into the database context.");
            PostActionQueue(async () =>
            {
                await context.Clips.LoadAsync();
                NotifyDataCollectionUpdated(nameof(context.Clips), context.Clips.Local.ToList());
            }, "RefreshClipsList");
        }

        private void RefreshCommandsList()
        {
            LogWriter.DebugLog("RefreshCommandsList", DebugLogTypes.DataManager,
                $"Reloading Commands data into the database context.");
            PostActionQueue(async () =>
            {
                await context.Commands.LoadAsync();
                NotifyDataCollectionUpdated(nameof(context.Commands), context.Commands.Local.ToList());
            }, "RefreshCommandsList");
        }

        private void RefreshCommandsUserList()
        {
            LogWriter.DebugLog("RefreshCommandsUserList", DebugLogTypes.DataManager,
                $"Reloading CommandsUser data into the database context.");
            PostActionQueue(async () =>
            {
                await context.CommandsUser.LoadAsync();
                NotifyDataCollectionUpdated(nameof(context.CommandsUser), context.CommandsUser.Local.ToList());
            }, "RefreshCommandsUserList");
        }

        private void RefreshCurrencyList()
        {
            LogWriter.DebugLog("RefreshCurrencyList", DebugLogTypes.DataManager,
                $"Reloading Currency data into the database context.");
            PostActionQueue(async () =>
            {
                await context.Currency.LoadAsync();
                NotifyDataCollectionUpdated(nameof(context.Currency), context.Currency.Local.ToList());
            }, "RefreshCurrencyList");
        }

        private void RefreshCurrencyTypeList()
        {
            LogWriter.DebugLog("RefreshCurrencyTypeList", DebugLogTypes.DataManager,
                $"Reloading CurrencyType data into the database context.");
            PostActionQueue(async () =>
            {
                await context.CurrencyType.LoadAsync();
                NotifyDataCollectionUpdated(nameof(context.CurrencyType), context.CurrencyType.Local.ToList());
            }, "RefreshCurrencyTypeList");
        }

        private void RefreshCustomWelcomeList()
        {
            LogWriter.DebugLog("RefreshCustomWelcomeList", DebugLogTypes.DataManager,
                $"Reloading CustomWelcome data into the database context.");
            PostActionQueue(async () =>
            {
                await context.CustomWelcome.LoadAsync();
                NotifyDataCollectionUpdated(nameof(context.CustomWelcome), context.CustomWelcome.Local.ToList());
            }, "RefreshCustomWelcomeList");
        }

        private void RefreshFollowersList()
        {
            LogWriter.DebugLog("RefreshFollowersList", DebugLogTypes.DataManager,
                $"Reloading Followers data into the database context.");
            PostActionQueue(async () =>
            {
                await context.Followers.LoadAsync();
                NotifyDataCollectionUpdated(nameof(context.Followers), context.Followers.Local.ToList());
            }, "RefreshFollowersList");
        }

        private void RefreshGameDeadCounterList()
        {
            LogWriter.DebugLog("RefreshGameDeadCounterList", DebugLogTypes.DataManager,
                $"Reloading GameDeadCounter data into the database context.");
            PostActionQueue(async () =>
            {
                await context.GameDeadCounter.LoadAsync();
                NotifyDataCollectionUpdated(nameof(context.GameDeadCounter), context.GameDeadCounter.Local.ToList());
            }, "RefreshGameDeadCounterList");
        }

        private void RefreshGiveawayUserDataList()
        {
            LogWriter.DebugLog("RefreshGiveawayUserDataList", DebugLogTypes.DataManager,
                $"Reloading GiveawayUserData data into the database context.");
            PostActionQueue(async () =>
            {
                await context.GiveawayUserData.LoadAsync();
                NotifyDataCollectionUpdated(nameof(context.GiveawayUserData), context.GiveawayUserData.Local.ToList());
            }, "RefreshGiveawayUserDataList");
        }

        private void RefreshInRaidDataList()
        {
            LogWriter.DebugLog("RefreshInRaidDataList", DebugLogTypes.DataManager,
                $"Reloading InRaidData data into the database context.");
            PostActionQueue(async () =>
            {
                await context.InRaidData.LoadAsync();
                NotifyDataCollectionUpdated(nameof(context.InRaidData), context.InRaidData.Local.ToList());
            }, "RefreshInRaidDataList");
        }

        private void RefreshLearnMsgsList()
        {
            LogWriter.DebugLog("RefreshLearnMsgsList", DebugLogTypes.DataManager,
                $"Reloading LearnMsgs data into the database context.");
            PostActionQueue(async () =>
            {
                await context.LearnMsgs.LoadAsync();
                NotifyDataCollectionUpdated(nameof(context.LearnMsgs), context.LearnMsgs.Local.ToList());
            }, "RefreshLearnMsgsList");
        }

        private void RefreshModeratorApproveList()
        {
            LogWriter.DebugLog("RefreshModeratorApproveList", DebugLogTypes.DataManager,
                $"Reloading ModeratorApprove data into the database context.");
            PostActionQueue(async () =>
            {
                await context.ModeratorApprove.LoadAsync();
                NotifyDataCollectionUpdated(nameof(context.ModeratorApprove), context.ModeratorApprove.Local.ToList());
            }, "RefreshModeratorApproveList");
        }

        private void RefreshMultiChannelsList()
        {
            LogWriter.DebugLog("RefreshMultiChannelsList", DebugLogTypes.DataManager,
                $"Reloading MultiChannels data into the database context.");
            PostActionQueue(async () =>
            {
                await context.MultiChannels.LoadAsync();
                NotifyDataCollectionUpdated(nameof(context.MultiChannels), context.MultiChannels.Local.ToList());
            }, "RefreshMultiChannelsList");
        }

        private void RefreshMultiLiveStreamsList()
        {
            LogWriter.DebugLog("RefreshMultiLiveStreamsList", DebugLogTypes.DataManager,
                $"Reloading MultiLiveStreams data into the database context.");
            PostActionQueue(async () =>
            {
                await context.MultiLiveStreams.LoadAsync();
                NotifyDataCollectionUpdated(nameof(context.MultiLiveStreams), context.MultiLiveStreams.Local.ToList());
            }, "RefreshMultiLiveStreamsList");
        }

        private void RefreshMultiSummaryLiveStreamsList()
        {
            LogWriter.DebugLog("RefreshMultiSummaryLiveStreamsList", DebugLogTypes.DataManager,
                $"Reloading MultiSummaryLiveStreams data into the database context.");
            PostActionQueue(async () =>
            {
                await context.MultiSummaryLiveStreams.LoadAsync();
                NotifyDataCollectionUpdated(nameof(context.MultiSummaryLiveStreams), context.MultiSummaryLiveStreams.Local.ToList());
            }, "RefreshMultiSummaryLiveStreamsList");
        }

        private void RefreshMultiWebhooksList()
        {
            LogWriter.DebugLog("RefreshMultiWebhooksList", DebugLogTypes.DataManager,
                $"Reloading MultiWebhooks data into the database context.");
            PostActionQueue(async () =>
            {
                await context.MultiWebhooks.LoadAsync();
                NotifyDataCollectionUpdated(nameof(context.MultiWebhooks), context.MultiWebhooks.Local.ToList());
            }, "RefreshMultiWebhooksList");
        }

        private void RefreshOldFollowUsersList()
        {
            LogWriter.DebugLog("RefreshOldFollowUsersList", DebugLogTypes.DataManager,
                $"Reloading OldFollowUsers data into the database context.");
            PostActionQueue(async () =>
            {
                await context.OldFollowUsers.LoadAsync();
                NotifyDataCollectionUpdated(nameof(context.OldFollowUsers), context.OldFollowUsers.Local.ToList());
            }, "RefreshOldFollowUsersList");
        }

        private void RefreshOutRaidDataList()
        {
            LogWriter.DebugLog("RefreshOutRaidDataList", DebugLogTypes.DataManager,
                $"Reloading OutRaidData data into the database context.");
            PostActionQueue(async () =>
            {
                await context.OutRaidData.LoadAsync();
                NotifyDataCollectionUpdated(nameof(context.OutRaidData), context.OutRaidData.Local.ToList());
            }, "RefreshOutRaidDataList");
        }

        private void RefreshOverlayServicesList()
        {
            LogWriter.DebugLog("RefreshOverlayServicesList", DebugLogTypes.DataManager,
                $"Reloading OverlayServices data into the database context.");
            PostActionQueue(async () =>
            {
                await context.OverlayServices.LoadAsync();
                NotifyDataCollectionUpdated(nameof(context.OverlayServices), context.OverlayServices.Local.ToList());
            }, "RefreshOverlayServicesList");
        }

        private void RefreshOverlayTickerList()
        {
            LogWriter.DebugLog("RefreshOverlayTickerList", DebugLogTypes.DataManager,
                $"Reloading OverlayTicker data into the database context.");
            PostActionQueue(async () =>
            {
                await context.OverlayTicker.LoadAsync();
                NotifyDataCollectionUpdated(nameof(context.OverlayTicker), context.OverlayTicker.Local.ToList());
            }, "RefreshOverlayTickerList");
        }

        private void RefreshQuotesList()
        {
            LogWriter.DebugLog("RefreshQuotesList", DebugLogTypes.DataManager,
                $"Reloading Quotes data into the database context.");
            PostActionQueue(async () =>
            {
                await context.Quotes.LoadAsync();
                NotifyDataCollectionUpdated(nameof(context.Quotes), context.Quotes.Local.ToList());
            }, "RefreshQuotesList");
        }

        private void RefreshShoutOutsList()
        {
            LogWriter.DebugLog("RefreshShoutOutsList", DebugLogTypes.DataManager,
                $"Reloading ShoutOuts data into the database context.");
            PostActionQueue(async () =>
            {
                await context.ShoutOuts.LoadAsync();
                NotifyDataCollectionUpdated(nameof(context.ShoutOuts), context.ShoutOuts.Local.ToList());
            }, "RefreshShoutOutsList");
        }

        private void RefreshStreamStatsList()
        {
            LogWriter.DebugLog("RefreshStreamStatsList", DebugLogTypes.DataManager,
                $"Reloading StreamStats data into the database context.");
            PostActionQueue(async () =>
            {
                await context.StreamStats.LoadAsync();
                NotifyDataCollectionUpdated(nameof(context.StreamStats), context.StreamStats.Local.ToList());
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
                await context.Users.LoadAsync();
                LogWriter.DebugLog("RefreshUsersList",
                    DebugLogTypes.DataManager, $"Notifying the DataCollection is Updated.");
                NotifyDataCollectionUpdated(nameof(context.Users), context.Users.Local.ToList());
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
                await context.UserStats.LoadAsync();
                LogWriter.DebugLog("RefreshUserStatsList",
                    DebugLogTypes.DataManager, $"Notifying the DataCollection is Updated.");
                NotifyDataCollectionUpdated(nameof(context.UserStats), context.UserStats.Local.ToList());
            }, "RefreshUserStatsList");
        }

        private void RefreshWebhooksList()
        {
            LogWriter.DebugLog("RefreshWebhooksList", DebugLogTypes.DataManager,
                $"Reloading Webhooks data into the database context.");
            PostActionQueue(async () =>
            {
                await context.Webhooks.LoadAsync();
                NotifyDataCollectionUpdated(nameof(context.Webhooks), context.Webhooks.Local.ToList());
            }, "RefreshWebhooksList");

        }

        #endregion

#endif
    }
}
    */


}
