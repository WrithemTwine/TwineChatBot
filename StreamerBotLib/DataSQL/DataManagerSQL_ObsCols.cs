using Microsoft.EntityFrameworkCore;

using StreamerBotLib.DataSQL.Models;
using StreamerBotLib.Interfaces;

using System.Collections.ObjectModel;

namespace StreamerBotLib.DataSQL
{
    public partial class DataManagerSQL : IDataManager, IDataManagerReadOnly, IDataManagerTestMethods
    {
        #region LocalView ObservableCollection

        private SQLDBContext GUIContext;

        public async Task<ObservableCollection<Models.BanReasons>> GetBanReasonsLocalObservable()
        {
            await GUIContext.BanReasons.LoadAsync();
            return GUIContext.BanReasons.Local.ToObservableCollection();
        }

        public async Task<ObservableCollection<BanRules>> GetBanRulesLocalObservable()
        {
            await GUIContext.BanRules.LoadAsync();
            return GUIContext.BanRules.Local.ToObservableCollection();
        }

        public async Task<ObservableCollection<CategoryList>> GetCategoryListLocalObservable()
        {
            await GUIContext.CategoryList.LoadAsync();
            return GUIContext.CategoryList.Local.ToObservableCollection();
        }

        public async Task<ObservableCollection<ChannelEvents>> GetChannelEventsLocalObservable()
        {
            await GUIContext.ChannelEvents.LoadAsync();
            return GUIContext.ChannelEvents.Local.ToObservableCollection();
        }

        public async Task<ObservableCollection<Clips>> GetClipsLocalObservable()
        {
            await GUIContext.Clips.LoadAsync();
            return GUIContext.Clips.Local.ToObservableCollection();
        }

        public async Task<ObservableCollection<Commands>> GetCommandsLocalObservable()
        {
            await GUIContext.Commands.LoadAsync();
            return GUIContext.Commands.Local.ToObservableCollection();
        }

        public async Task<ObservableCollection<CommandsUser>> GetCommandsUserLocalObservable()
        {
            await GUIContext.CommandsUser.LoadAsync();
            return GUIContext.CommandsUser.Local.ToObservableCollection();
        }

        public async Task<ObservableCollection<Currency>> GetCurrencyLocalObservable()
        {
            await GUIContext.Currency.LoadAsync();
            return GUIContext.Currency.Local.ToObservableCollection();
        }

        public async Task<ObservableCollection<Models.CurrencyType>> GetCurrencyTypeLocalObservable()
        {
            await GUIContext.CurrencyType.LoadAsync();
            return GUIContext.CurrencyType.Local.ToObservableCollection();
        }

        public async Task<ObservableCollection<CustomWelcome>> GetCustomWelcomeLocalObservable()
        {
            await GUIContext.CustomWelcome.LoadAsync();
            return GUIContext.CustomWelcome.Local.ToObservableCollection();
        }

        public async Task<ObservableCollection<Followers>> GetFollowersLocalObservable()
        {
            await GUIContext.Followers.LoadAsync();
            return GUIContext.Followers.Local.ToObservableCollection();
        }

        public async Task<ObservableCollection<GameDeadCounter>> GetGameDeadCounterLocalObservable()
        {
            await GUIContext.GameDeadCounter.LoadAsync();
            return GUIContext.GameDeadCounter.Local.ToObservableCollection();
        }

        public async Task<ObservableCollection<GiveawayUserData>> GetGiveawayUserDataLocalObservable()
        {
            await GUIContext.GiveawayUserData.LoadAsync();
            return GUIContext.GiveawayUserData.Local.ToObservableCollection();
        }

        public async Task<ObservableCollection<InRaidData>> GetInRaidDataLocalObservable()
        {
            await GUIContext.InRaidData.LoadAsync();
            return GUIContext.InRaidData.Local.ToObservableCollection();
        }

        public async Task<ObservableCollection<LearnMsgs>> GetLearnMsgsLocalObservable()
        {
            await GUIContext.LearnMsgs.LoadAsync();
            return GUIContext.LearnMsgs.Local.ToObservableCollection();
        }

        public async Task<ObservableCollection<ModeratorApprove>> GetModeratorApproveLocalObservable()
        {
            await GUIContext.ModeratorApprove.LoadAsync();
            return GUIContext.ModeratorApprove.Local.ToObservableCollection();
        }

        public async Task<ObservableCollection<MultiChannels>> GetMultiChannelsLocalObservable()
        {
            await GUIContext.MultiChannels.LoadAsync();
            return GUIContext.MultiChannels.Local.ToObservableCollection();
        }

        public async Task<ObservableCollection<MultiLiveStreams>> GetMultiLiveStreamsLocalObservable()
        {
            await GUIContext.MultiLiveStreams.LoadAsync();
            return GUIContext.MultiLiveStreams.Local.ToObservableCollection();
        }

        public async Task<ObservableCollection<MultiWebhooks>> GetMultiWebhooksLocalObservable()
        {
            await GUIContext.MultiWebhooks.LoadAsync();
            return GUIContext.MultiWebhooks.Local.ToObservableCollection();
        }

        public async Task<ObservableCollection<MultiSummaryLiveStreams>> GetMultiSummaryLiveStreamsLocalObservable()
        {
            await GUIContext.MultiSummaryLiveStreams.LoadAsync();
            return GUIContext.MultiSummaryLiveStreams.Local.ToObservableCollection();
        }

        public async Task<ObservableCollection<OldFollowUsers>> GetOldFollowUsersLocalObservable()
        {
            await GUIContext.OldFollowUsers.LoadAsync();
            return GUIContext.OldFollowUsers.Local.ToObservableCollection();
        }

        public async Task<ObservableCollection<OutRaidData>> GetOutRaidDataLocalObservable()
        {
            await GUIContext.OutRaidData.LoadAsync();
            return GUIContext.OutRaidData.Local.ToObservableCollection();
        }

        public async Task<ObservableCollection<OverlayServices>> GetOverlayServicesLocalObservable()
        {
            await GUIContext.OverlayServices.LoadAsync();
            return GUIContext.OverlayServices.Local.ToObservableCollection();
        }

        public async Task<ObservableCollection<OverlayTicker>> GetOverlayTickerLocalObservable()
        {
            await GUIContext.OverlayTicker.LoadAsync();
            return GUIContext.OverlayTicker.Local.ToObservableCollection();
        }

        public async Task<ObservableCollection<Quotes>> GetQuotesLocalObservable()
        {
            await GUIContext.Quotes.LoadAsync();
            return GUIContext.Quotes.Local.ToObservableCollection();
        }

        public async Task<ObservableCollection<ShoutOuts>> GetShoutOutsLocalObservable()
        {
            await GUIContext.ShoutOuts.LoadAsync();
            return GUIContext.ShoutOuts.Local.ToObservableCollection();
        }

        public async Task<ObservableCollection<StreamStats>> GetStreamStatsLocalObservable()
        {
            await GUIContext.StreamStats.LoadAsync();
            return GUIContext.StreamStats.Local.ToObservableCollection();
        }

        public async Task<ObservableCollection<Users>> GetUsersLocalObservable()
        {
            await GUIContext.Users.LoadAsync();
            return GUIContext.Users.Local.ToObservableCollection();
        }

        public async Task<ObservableCollection<UserStats>> GetUserStatsLocalObservable()
        {
            await GUIContext.UserStats.LoadAsync();
            return GUIContext.UserStats.Local.ToObservableCollection();
        }

        public async Task<ObservableCollection<Webhooks>> GetWebhooksLocalObservable()
        {
            await GUIContext.Webhooks.LoadAsync();
            return GUIContext.Webhooks.Local.ToObservableCollection();
        }

        #endregion

        #region Refresh Collections
        public async Task RefreshBanReasonsObservableCollection()
        {
            await GUIContext.BanReasons.LoadAsync();
            NotifyDataCollectionUpdated(nameof(GUIContext.BanReasons));
        }

        public async Task RefreshBanRulesObservableCollection()
        {
            await GUIContext.BanRules.LoadAsync();
            NotifyDataCollectionUpdated(nameof(GUIContext.BanRules));
        }

        public async Task RefreshCategoryListObservableCollection()
        {
            await GUIContext.CategoryList.LoadAsync();
            NotifyDataCollectionUpdated(nameof(GUIContext.CategoryList));
        }

        public async Task RefreshChannelEventsObservableCollection()
        {
            await GUIContext.ChannelEvents.LoadAsync();
            NotifyDataCollectionUpdated(nameof(GUIContext.ChannelEvents));
        }

        public async Task RefreshClipsObservableCollection()
        {
            await GUIContext.Clips.LoadAsync();
            NotifyDataCollectionUpdated(nameof(GUIContext.Clips));
        }

        public async Task RefreshCommandsObservableCollection()
        {
            await GUIContext.Commands.LoadAsync();
            NotifyDataCollectionUpdated(nameof(GUIContext.Commands));
        }

        public async Task RefreshCommandsUserObservableCollection()
        {
            await GUIContext.CommandsUser.LoadAsync();
            NotifyDataCollectionUpdated(nameof(GUIContext.CommandsUser));
        }

        public async Task RefreshCurrencyObservableCollection()
        {
            await GUIContext.Currency.LoadAsync();
            NotifyDataCollectionUpdated(nameof(GUIContext.Currency));
        }

        public async Task RefreshCurrencyTypeObservableCollection()
        {
            await GUIContext.CurrencyType.LoadAsync();
            NotifyDataCollectionUpdated(nameof(GUIContext.CurrencyType));
        }

        public async Task RefreshCustomWelcomeObservableCollection()
        {
            await GUIContext.CustomWelcome.LoadAsync();
            NotifyDataCollectionUpdated(nameof(GUIContext.CustomWelcome));
        }

        public async Task RefreshFollowersObservableCollection()
        {
            await GUIContext.Followers.LoadAsync();
            NotifyDataCollectionUpdated(nameof(GUIContext.Followers));
        }

        public async Task RefreshGameDeadCounterObservableCollection()
        {
            await GUIContext.GameDeadCounter.LoadAsync();
            NotifyDataCollectionUpdated(nameof(GUIContext.GameDeadCounter));
        }

        public async Task RefreshGiveawayUserDataObservableCollection()
        {
            await GUIContext.GiveawayUserData.LoadAsync();
            NotifyDataCollectionUpdated(nameof(GUIContext.GiveawayUserData));
        }

        public async Task RefreshInRaidDataObservableCollection()
        {
            await GUIContext.InRaidData.LoadAsync();
            NotifyDataCollectionUpdated(nameof(GUIContext.InRaidData));
        }

        public async Task RefreshLearnMsgsObservableCollection()
        {
            await GUIContext.LearnMsgs.LoadAsync();
            NotifyDataCollectionUpdated(nameof(GUIContext.LearnMsgs));
        }

        public async Task RefreshModeratorApproveObservableCollection()
        {
            await GUIContext.ModeratorApprove.LoadAsync();
            NotifyDataCollectionUpdated(nameof(GUIContext.ModeratorApprove));
        }

        public async Task RefreshMultiChannelsObservableCollection()
        {
            await GUIContext.MultiChannels.LoadAsync();
            NotifyDataCollectionUpdated(nameof(GUIContext.MultiChannels));
        }

        public async Task RefreshMultiLiveStreamsObservableCollection()
        {
            await GUIContext.MultiLiveStreams.LoadAsync();
            NotifyDataCollectionUpdated(nameof(GUIContext.MultiLiveStreams));
        }

        public async Task RefreshMultiSummaryLiveStreamsObservableCollection()
        {
            await GUIContext.MultiSummaryLiveStreams.LoadAsync();
            NotifyDataCollectionUpdated(nameof(GUIContext.MultiSummaryLiveStreams));
        }

        public async Task RefreshMultiWebhooksObservableCollection()
        {
            await GUIContext.MultiWebhooks.LoadAsync();
            NotifyDataCollectionUpdated(nameof(GUIContext.MultiWebhooks));
        }

        public async Task RefreshOldFollowUsersObservableCollection()
        {
            await GUIContext.OldFollowUsers.LoadAsync();
            NotifyDataCollectionUpdated(nameof(GUIContext.OldFollowUsers));
        }

        public async Task RefreshOutRaidDataObservableCollection()
        {
            await GUIContext.OutRaidData.LoadAsync();
            NotifyDataCollectionUpdated(nameof(GUIContext.OutRaidData));
        }

        public async Task RefreshOverlayServicesObservableCollection()
        {
            await GUIContext.OverlayServices.LoadAsync();
            NotifyDataCollectionUpdated(nameof(GUIContext.OverlayServices));
        }

        public async Task RefreshOverlayTickerObservableCollection()
        {
            await GUIContext.OverlayTicker.LoadAsync();
            NotifyDataCollectionUpdated(nameof(GUIContext.OverlayTicker));
        }

        public async Task RefreshQuotesObservableCollection()
        {
            await GUIContext.Quotes.LoadAsync();
            NotifyDataCollectionUpdated(nameof(GUIContext.Quotes));
        }

        public async Task RefreshShoutOutsObservableCollection()
        {
            await GUIContext.ShoutOuts.LoadAsync();
            NotifyDataCollectionUpdated(nameof(GUIContext.ShoutOuts));
        }

        public async Task RefreshStreamStatsObservableCollection()
        {
            await GUIContext.StreamStats.LoadAsync();
            NotifyDataCollectionUpdated(nameof(GUIContext.StreamStats));
        }

        public async Task RefreshUsersObservableCollection()
        {
            await GUIContext.Users.LoadAsync();
            NotifyDataCollectionUpdated(nameof(GUIContext.Users));
        }

        public async Task RefreshUserStatsObservableCollection()
        {
            await GUIContext.UserStats.LoadAsync();
            NotifyDataCollectionUpdated(nameof(GUIContext.UserStats));
        }

        public async Task RefreshWebhooksObservableCollection()
        {
            await GUIContext.Webhooks.LoadAsync();
            NotifyDataCollectionUpdated(nameof(GUIContext.Webhooks));
        }

        #endregion
    }
}
