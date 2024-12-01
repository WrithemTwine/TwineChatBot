using Microsoft.EntityFrameworkCore;

using StreamerBotLib.DataSQL.Models;
using StreamerBotLib.Interfaces;
using StreamerBotLib.Static;

using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Reflection;

namespace StreamerBotLib.DataSQL
{
    public partial class DataManagerSQL : IDataManager, IDataManagerReadOnly, IDataManagerTestMethods
    {
        private ConcurrentQueue<Task> RefreshObsCollections = new();
        private bool IsProcessRefreshThreadStarted;

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
        private void RefreshBanReasonsObservableCollection()
        {
            PostNewObsCollectionTask(new Task(async () =>
            {
                await GUIContext.BanReasons.LoadAsync();
                NotifyDataCollectionUpdated(nameof(GUIContext.BanReasons));
            }));
        }

        private void RefreshBanRulesObservableCollection()
        {
            PostNewObsCollectionTask(new Task(async () =>
            {
                await GUIContext.BanRules.LoadAsync();
                NotifyDataCollectionUpdated(nameof(GUIContext.BanRules));
            }));
        }

        private void RefreshCategoryListObservableCollection()
        {
            PostNewObsCollectionTask(new Task(async () =>
            {
                await GUIContext.CategoryList.LoadAsync();
                NotifyDataCollectionUpdated(nameof(GUIContext.CategoryList));
            }));
        }

        private void RefreshChannelEventsObservableCollection()
        {
            PostNewObsCollectionTask(new Task(async () =>
            {
                await GUIContext.ChannelEvents.LoadAsync();
                NotifyDataCollectionUpdated(nameof(GUIContext.ChannelEvents));
            }));
        }

        private void RefreshClipsObservableCollection()
        {
            PostNewObsCollectionTask(new Task(async () =>
            {
                await GUIContext.Clips.LoadAsync();
                NotifyDataCollectionUpdated(nameof(GUIContext.Clips));
            }));
        }

        private void RefreshCommandsObservableCollection()
        {
            PostNewObsCollectionTask(new Task(async () =>
            {
                await GUIContext.Commands.LoadAsync();
                NotifyDataCollectionUpdated(nameof(GUIContext.Commands));
            }));
        }

        private void RefreshCommandsUserObservableCollection()
        {
            PostNewObsCollectionTask(new Task(async () =>
            {
                await GUIContext.CommandsUser.LoadAsync();
                NotifyDataCollectionUpdated(nameof(GUIContext.CommandsUser));
            }));
        }

        private void RefreshCurrencyObservableCollection()
        {
            PostNewObsCollectionTask(new Task(async () =>
            {
                await GUIContext.Currency.LoadAsync();
                NotifyDataCollectionUpdated(nameof(GUIContext.Currency));
            }));
        }

        private void RefreshCurrencyTypeObservableCollection()
        {
            PostNewObsCollectionTask(new Task(async () =>
            {
                await GUIContext.CurrencyType.LoadAsync();
                NotifyDataCollectionUpdated(nameof(GUIContext.CurrencyType));
            }));
        }

        private void RefreshCustomWelcomeObservableCollection()
        {
            PostNewObsCollectionTask(new Task(async () =>
            {
                await GUIContext.CustomWelcome.LoadAsync();
                NotifyDataCollectionUpdated(nameof(GUIContext.CustomWelcome));
            }));
        }

        private void RefreshFollowersObservableCollection()
        {
            PostNewObsCollectionTask(new Task(async () =>
            {
                await GUIContext.Followers.LoadAsync();
                NotifyDataCollectionUpdated(nameof(GUIContext.Followers));
            }));
        }

        private void RefreshGameDeadCounterObservableCollection()
        {
            PostNewObsCollectionTask(new Task(async () =>
            {
                await GUIContext.GameDeadCounter.LoadAsync();
                NotifyDataCollectionUpdated(nameof(GUIContext.GameDeadCounter));
            }));
        }

        private void RefreshGiveawayUserDataObservableCollection()
        {
            PostNewObsCollectionTask(new Task(async () =>
            {
                await GUIContext.GiveawayUserData.LoadAsync();
                NotifyDataCollectionUpdated(nameof(GUIContext.GiveawayUserData));
            }));
        }

        private void RefreshInRaidDataObservableCollection()
        {
            PostNewObsCollectionTask(new Task(async () =>
            {
                await GUIContext.InRaidData.LoadAsync();
                NotifyDataCollectionUpdated(nameof(GUIContext.InRaidData));
            }));
        }

        private void RefreshLearnMsgsObservableCollection()
        {
            PostNewObsCollectionTask(new Task(async () =>
            {
                await GUIContext.LearnMsgs.LoadAsync();
                NotifyDataCollectionUpdated(nameof(GUIContext.LearnMsgs));
            }));
        }

        private void RefreshModeratorApproveObservableCollection()
        {
            PostNewObsCollectionTask(new Task(async () =>
            {
                await GUIContext.ModeratorApprove.LoadAsync();
                NotifyDataCollectionUpdated(nameof(GUIContext.ModeratorApprove));
            }));
        }

        private void RefreshMultiChannelsObservableCollection()
        {
            PostNewObsCollectionTask(new Task(async () =>
            {
                await GUIContext.MultiChannels.LoadAsync();
                NotifyDataCollectionUpdated(nameof(GUIContext.MultiChannels));
            }));
        }

        private void RefreshMultiLiveStreamsObservableCollection()
        {
            PostNewObsCollectionTask(new Task(async () =>
            {
                await GUIContext.MultiLiveStreams.LoadAsync();
                NotifyDataCollectionUpdated(nameof(GUIContext.MultiLiveStreams));
            }));
        }

        private void RefreshMultiSummaryLiveStreamsObservableCollection()
        {
            PostNewObsCollectionTask(new Task(async () =>
            {
                await GUIContext.MultiSummaryLiveStreams.LoadAsync();
                NotifyDataCollectionUpdated(nameof(GUIContext.MultiSummaryLiveStreams));
            }));
        }

        private void RefreshMultiWebhooksObservableCollection()
        {
            PostNewObsCollectionTask(new Task(async () =>
            {
                await GUIContext.MultiWebhooks.LoadAsync();
                NotifyDataCollectionUpdated(nameof(GUIContext.MultiWebhooks));
            }));
        }

        private void RefreshOldFollowUsersObservableCollection()
        {
            PostNewObsCollectionTask(new Task(async () =>
            {
                await GUIContext.OldFollowUsers.LoadAsync();
                NotifyDataCollectionUpdated(nameof(GUIContext.OldFollowUsers));
            }));
        }

        private void RefreshOutRaidDataObservableCollection()
        {
            PostNewObsCollectionTask(new Task(async () =>
            {
                await GUIContext.OutRaidData.LoadAsync();
                NotifyDataCollectionUpdated(nameof(GUIContext.OutRaidData));
            }));
        }

        private void RefreshOverlayServicesObservableCollection()
        {
            PostNewObsCollectionTask(new Task(async () =>
            {
                await GUIContext.OverlayServices.LoadAsync();
                NotifyDataCollectionUpdated(nameof(GUIContext.OverlayServices));
            }));
        }

        private void RefreshOverlayTickerObservableCollection()
        {
            PostNewObsCollectionTask(new Task(async () =>
            {
                await GUIContext.OverlayTicker.LoadAsync();
                NotifyDataCollectionUpdated(nameof(GUIContext.OverlayTicker));
            }));
        }

        private void RefreshQuotesObservableCollection()
        {
            PostNewObsCollectionTask(new Task(async () =>
            {
                await GUIContext.Quotes.LoadAsync();
                NotifyDataCollectionUpdated(nameof(GUIContext.Quotes));
            }));
        }

        private void RefreshShoutOutsObservableCollection()
        {
            PostNewObsCollectionTask(new Task(async () =>
            {
                await GUIContext.ShoutOuts.LoadAsync();
                NotifyDataCollectionUpdated(nameof(GUIContext.ShoutOuts));
            }));
        }

        private void RefreshStreamStatsObservableCollection()
        {
            PostNewObsCollectionTask(new Task(async () =>
            {
                await GUIContext.StreamStats.LoadAsync();
                NotifyDataCollectionUpdated(nameof(GUIContext.StreamStats));
            }));
        }

        private void RefreshUsersObservableCollection()
        {
            PostNewObsCollectionTask(new Task(async () =>
            {
                await GUIContext.Users.LoadAsync();
                NotifyDataCollectionUpdated(nameof(GUIContext.Users));
            }));
        }

        private void RefreshUserStatsObservableCollection()
        {
            PostNewObsCollectionTask(new Task(async () =>
            {
                await GUIContext.UserStats.LoadAsync();
                NotifyDataCollectionUpdated(nameof(GUIContext.UserStats));
            }));
        }

        private void RefreshWebhooksObservableCollection()
        {
            PostNewObsCollectionTask(new Task(async () =>
            {
                await GUIContext.Webhooks.LoadAsync();
                NotifyDataCollectionUpdated(nameof(GUIContext.Webhooks));
            }));
        }

        #endregion

        private void PostNewObsCollectionTask(Task updateTask)
        {
            if (!IsProcessRefreshThreadStarted)
            {
                IsProcessRefreshThreadStarted = true;
                ThreadManager.CreateThreadStart(MethodBase.GetCurrentMethod().Name, ProcessRefreshTasks);
            }

            RefreshObsCollections.Enqueue(updateTask);
        }

        private void ProcessRefreshTasks()
        {
            while (OptionFlags.ActiveToken)
            {
                while (RefreshObsCollections.TryDequeue(out var obsCollectionTask))
                {
                    ThreadManager.AddTaskToGUIDispatcher(obsCollectionTask);
                }

                Thread.Sleep(500);
            }
        }
    }
}
