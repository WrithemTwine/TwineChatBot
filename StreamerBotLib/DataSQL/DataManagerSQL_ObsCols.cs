using Microsoft.EntityFrameworkCore;

using StreamerBotLib.DataSQL.Models;
using StreamerBotLib.Enums;
using StreamerBotLib.Interfaces;
using StreamerBotLib.Static;

using System.Collections.Concurrent;
using System.Collections.ObjectModel;

namespace StreamerBotLib.DataSQL
{
    public partial class DataManagerSQL : IDataManager, IDataManagerReadOnly, IDataManagerTestMethods
    {
        private readonly ConcurrentQueue<Action> RefreshObsCollections = [];
        private bool IsProcessRefreshThreadStarted;

        #region LocalView ObservableCollection

        private readonly SQLDBContext GUIContext;

        public object GetObservableCollection(DataTables dataTable)
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

        #region Refresh Collections
        private void RefreshBanReasonsObservableCollection()
        {
            PostNewObsCollectionTask(async () =>
            {
                await GUIContext.BanReasons.LoadAsync();
                NotifyDataCollectionUpdated(nameof(GUIContext.BanReasons));
            });
        }

        private void RefreshBanRulesObservableCollection()
        {
            PostNewObsCollectionTask(async () =>
            {
                await GUIContext.BanRules.LoadAsync();
                NotifyDataCollectionUpdated(nameof(GUIContext.BanRules));
            });
        }

        private void RefreshCategoryListObservableCollection()
        {
            PostNewObsCollectionTask(async () =>
            {
                await GUIContext.CategoryList.LoadAsync();
                NotifyDataCollectionUpdated(nameof(GUIContext.CategoryList));
            });
        }

        private void RefreshChannelEventsObservableCollection()
        {
            PostNewObsCollectionTask(async () =>
            {
                await GUIContext.ChannelEvents.LoadAsync();
                NotifyDataCollectionUpdated(nameof(GUIContext.ChannelEvents));
            });
        }

        private void RefreshClipsObservableCollection()
        {
            PostNewObsCollectionTask(async () =>
            {
                await GUIContext.Clips.LoadAsync();
                NotifyDataCollectionUpdated(nameof(GUIContext.Clips));
            });
        }

        private void RefreshCommandsObservableCollection()
        {
            PostNewObsCollectionTask(async () =>
            {
                await GUIContext.Commands.LoadAsync();
                NotifyDataCollectionUpdated(nameof(GUIContext.Commands));
            });
        }

        private void RefreshCommandsUserObservableCollection()
        {
            PostNewObsCollectionTask(async () =>
            {
                await GUIContext.CommandsUser.LoadAsync();
                NotifyDataCollectionUpdated(nameof(GUIContext.CommandsUser));
            });
        }

        private void RefreshCurrencyObservableCollection()
        {
            PostNewObsCollectionTask(async () =>
            {
                await GUIContext.Currency.LoadAsync();
                NotifyDataCollectionUpdated(nameof(GUIContext.Currency));
            });
        }

        private void RefreshCurrencyTypeObservableCollection()
        {
            PostNewObsCollectionTask(async () =>
            {
                await GUIContext.CurrencyType.LoadAsync();
                NotifyDataCollectionUpdated(nameof(GUIContext.CurrencyType));
            });
        }

        private void RefreshCustomWelcomeObservableCollection()
        {
            PostNewObsCollectionTask(async () =>
            {
                await GUIContext.CustomWelcome.LoadAsync();
                NotifyDataCollectionUpdated(nameof(GUIContext.CustomWelcome));
            });
        }

        private void RefreshFollowersObservableCollection()
        {
            PostNewObsCollectionTask(async () =>
            {
                await GUIContext.Followers.LoadAsync();
                NotifyDataCollectionUpdated(nameof(GUIContext.Followers));
            });
        }

        private void RefreshGameDeadCounterObservableCollection()
        {
            PostNewObsCollectionTask(async () =>
            {
                await GUIContext.GameDeadCounter.LoadAsync();
                NotifyDataCollectionUpdated(nameof(GUIContext.GameDeadCounter));
            });
        }

        private void RefreshGiveawayUserDataObservableCollection()
        {
            PostNewObsCollectionTask(async () =>
            {
                await GUIContext.GiveawayUserData.LoadAsync();
                NotifyDataCollectionUpdated(nameof(GUIContext.GiveawayUserData));
            });
        }

        private void RefreshInRaidDataObservableCollection()
        {
            PostNewObsCollectionTask(async () =>
            {
                await GUIContext.InRaidData.LoadAsync();
                NotifyDataCollectionUpdated(nameof(GUIContext.InRaidData));
            });
        }

        private void RefreshLearnMsgsObservableCollection()
        {
            PostNewObsCollectionTask(async () =>
            {
                await GUIContext.LearnMsgs.LoadAsync();
                NotifyDataCollectionUpdated(nameof(GUIContext.LearnMsgs));
            });
        }

        private void RefreshModeratorApproveObservableCollection()
        {
            PostNewObsCollectionTask(async () =>
            {
                await GUIContext.ModeratorApprove.LoadAsync();
                NotifyDataCollectionUpdated(nameof(GUIContext.ModeratorApprove));
            });
        }

        private void RefreshMultiChannelsObservableCollection()
        {
            PostNewObsCollectionTask(async () =>
            {
                await GUIContext.MultiChannels.LoadAsync();
                NotifyDataCollectionUpdated(nameof(GUIContext.MultiChannels));
            });
        }

        private void RefreshMultiLiveStreamsObservableCollection()
        {
            PostNewObsCollectionTask(async () =>
            {
                await GUIContext.MultiLiveStreams.LoadAsync();
                NotifyDataCollectionUpdated(nameof(GUIContext.MultiLiveStreams));
            });
        }

        private void RefreshMultiSummaryLiveStreamsObservableCollection()
        {
            PostNewObsCollectionTask(async () =>
            {
                await GUIContext.MultiSummaryLiveStreams.LoadAsync();
                NotifyDataCollectionUpdated(nameof(GUIContext.MultiSummaryLiveStreams));
            });
        }

        private void RefreshMultiWebhooksObservableCollection()
        {
            PostNewObsCollectionTask(async () =>
            {
                await GUIContext.MultiWebhooks.LoadAsync();
                NotifyDataCollectionUpdated(nameof(GUIContext.MultiWebhooks));
            });
        }

        private void RefreshOldFollowUsersObservableCollection()
        {
            PostNewObsCollectionTask(async () =>
            {
                await GUIContext.OldFollowUsers.LoadAsync();
                NotifyDataCollectionUpdated(nameof(GUIContext.OldFollowUsers));
            });
        }

        private void RefreshOutRaidDataObservableCollection()
        {
            PostNewObsCollectionTask(async () =>
            {
                await GUIContext.OutRaidData.LoadAsync();
                NotifyDataCollectionUpdated(nameof(GUIContext.OutRaidData));
            });
        }

        private void RefreshOverlayServicesObservableCollection()
        {
            PostNewObsCollectionTask(async () =>
            {
                await GUIContext.OverlayServices.LoadAsync();
                NotifyDataCollectionUpdated(nameof(GUIContext.OverlayServices));
            });
        }

        private void RefreshOverlayTickerObservableCollection()
        {
            PostNewObsCollectionTask(async () =>
            {
                await GUIContext.OverlayTicker.LoadAsync();
                NotifyDataCollectionUpdated(nameof(GUIContext.OverlayTicker));
            });
        }

        private void RefreshQuotesObservableCollection()
        {
            PostNewObsCollectionTask(async () =>
            {
                await GUIContext.Quotes.LoadAsync();
                NotifyDataCollectionUpdated(nameof(GUIContext.Quotes));
            });
        }

        private void RefreshShoutOutsObservableCollection()
        {
            PostNewObsCollectionTask(async () =>
            {
                await GUIContext.ShoutOuts.LoadAsync();
                NotifyDataCollectionUpdated(nameof(GUIContext.ShoutOuts));
            });
        }

        private void RefreshStreamStatsObservableCollection()
        {
            PostNewObsCollectionTask(async () =>
            {
                await GUIContext.StreamStats.LoadAsync();
                NotifyDataCollectionUpdated(nameof(GUIContext.StreamStats));
            });
        }

        private void RefreshUsersObservableCollection()
        {
            PostNewObsCollectionTask(async () =>
            {
                await GUIContext.Users.LoadAsync();
                NotifyDataCollectionUpdated(nameof(GUIContext.Users));
            });
        }

        private void RefreshUserStatsObservableCollection()
        {
            PostNewObsCollectionTask(async () =>
            {
                await GUIContext.UserStats.LoadAsync();
                NotifyDataCollectionUpdated(nameof(GUIContext.UserStats));
            });
        }

        private void RefreshWebhooksObservableCollection()
        {
            PostNewObsCollectionTask(async () =>
            {
                await GUIContext.Webhooks.LoadAsync();
                NotifyDataCollectionUpdated(nameof(GUIContext.Webhooks));
            });
        }

        #endregion

        private void PostNewObsCollectionTask(Action updateTask)
        {
            if (!IsProcessRefreshThreadStarted)
            {
                IsProcessRefreshThreadStarted = true;
                ThreadManager.CreateThreadStart("PostNewObsCollectionTask", ProcessRefreshTasksAsync);
            }

            RefreshObsCollections.Enqueue(updateTask);
        }

        private void ProcessRefreshTasksAsync()
        {
            while (OptionFlags.ActiveToken)
            {
                while (RefreshObsCollections.TryDequeue(out var obsCollectionAction))
                {
                    ThreadManager.AddTaskToGUIDispatcher(obsCollectionAction);
                }

                Thread.Sleep(500);
            }
        }
    }
}
