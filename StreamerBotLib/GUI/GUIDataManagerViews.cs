using StreamerBotLib.DataSQL.Models;
using StreamerBotLib.Interfaces;
using StreamerBotLib.Models;
using StreamerBotLib.Static;
using StreamerBotLib.Systems;

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Documents;

namespace StreamerBotLib.GUI
{
    public class GUIDataManagerViews : INotifyPropertyChanged
    {
        // TODO: determine how to preload the Observable Collections into the DataGrid controls - instead of relying on GUI focus
        #region DataManager TableViews
        //private readonly SQLDBContext context = new DataManagerFactory().CreateDbContext();
        private IDataManager DataManager { get; }

        public FlowDocument ChatData { get; private set; }
        public ObservableCollection<string> CurrUserList { get; private set; }

        public ObservableCollection<UserJoin> JoinCollection { get; set; }
        public ObservableCollection<string> CommandCollection { get; set; } = [];
        public ObservableCollection<LiveUser> GiveawayCollection { get; set; }

        public int CurrFollowers => Followers?.Count(f => f.IsFollower) ?? 0;
        public int CurrUserCount => Users?.Count ?? 0;
        public int CurrBuiltInComCount => Commands?.Count ?? 0;
        public int CurrUserComsCount => CommandsUser?.Count ?? 0;

        public ObservableCollection<Users> Users { get; private set; }
        public ObservableCollection<Followers> Followers { get; private set; }
        public ObservableCollection<CommandsUser> CommandsUser { get; private set; }
        public ObservableCollection<Commands> Commands { get; private set; }

        public ObservableCollection<BanRules> BanRules { get; private set; }
        public ObservableCollection<BanReasons> BanReasons { get; private set; }
        public ObservableCollection<CategoryList> CategoryList { get; private set; }
        public ObservableCollection<ChannelEvents> ChannelEvents { get; private set; }
        public ObservableCollection<Clips> Clips { get; private set; }
        public ObservableCollection<Currency> Currency { get; private set; }
        public ObservableCollection<CurrencyType> CurrencyType { get; private set; }
        public ObservableCollection<CustomWelcome> CustomWelcome { get; private set; }
        public ObservableCollection<GameDeadCounter> GameDeadCounter { get; private set; }
        public ObservableCollection<GiveawayUserData> GiveawayUserData { get; private set; }
        public ObservableCollection<InRaidData> InRaidData { get; private set; }
        public ObservableCollection<LearnMsgs> LearnMsgs { get; private set; }
        public ObservableCollection<ModeratorApprove> ModeratorApprove { get; private set; }
        public ObservableCollection<OldFollowUsers> OldFollowUsers { get; private set; }
        public ObservableCollection<OutRaidData> OutRaidData { get; private set; }
        public ObservableCollection<OverlayServices> OverlayServices { get; private set; }
        public ObservableCollection<OverlayTicker> OverlayTicker { get; private set; }
        public ObservableCollection<Quotes> Quotes { get; private set; }
        public ObservableCollection<ShoutOuts> ShoutOuts { get; private set; }
        public ObservableCollection<StreamStats> StreamStats { get; private set; }
        public ObservableCollection<UserStats> UserStats { get; private set; }
        public ObservableCollection<Webhooks> Webhooks { get; private set; }

        #region MultiLive Collections
        public ObservableCollection<MultiWebhooks> MultiWebhooks { get; private set; }
        public ObservableCollection<MultiChannels> MultiChannels { get; private set; }
        public ObservableCollection<MultiLiveStreams> MultiLiveStreams { get; private set; }
        public ObservableCollection<MultiSummaryLiveStreams> MultiSummaryLiveStreams { get; private set; }

        public ObservableCollection<ArchiveMultiStream> CleanupList => DataManager.GetCleanupList();
        public string MultiLiveStatusLog => DataManager.MultiLiveStatusLog;
        #endregion
        #endregion

        public static event EventHandler<EventArgs> DataViewsLoaded;

        public GUIDataManagerViews()
        {
            DataManager = SystemsController.DataManage;
            DataManager.OnDataCollectionUpdated += DataManager_OnDataCollectionUpdated;

            ChatData = ActionSystem.ChatData;
            JoinCollection = ActionSystem.JoinCollection;
            GiveawayCollection = ActionSystem.GiveawayCollection;
            CurrUserList = ActionSystem.CurrUserJoin;

            SetObservables();
            SetCommandCollection();

            DataViewsLoaded?.Invoke(this, new());
        }

        private void SetObservables()
        {
            ThreadManager.CreateThreadStart(".ctor_GUIDataManagerViews", () =>
            {
                Task.Run(() =>
                //ThreadManager.AddTaskToGUIDispatcher(
                //    () =>
                    {
                        // set specific collection changed events for StatusBar data
                        Users = (ObservableCollection<Users>)DataManager.GetObservableCollection(Enums.DataTables.Users);
                        Users.CollectionChanged += DataGrid_CollectionChanged;
                        NotifyPropertyChanged(nameof(Users));
                        Followers = (ObservableCollection<Followers>)DataManager.GetObservableCollection(Enums.DataTables.Followers);
                        Followers.CollectionChanged += DataGrid_CollectionChanged;
                        NotifyPropertyChanged(nameof(Followers));
                        CommandsUser = (ObservableCollection<CommandsUser>)DataManager.GetObservableCollection(Enums.DataTables.CommandsUser);
                        CommandsUser.CollectionChanged += DataGrid_CollectionChanged;
                        NotifyPropertyChanged(nameof(CommandsUser));
                        Commands = (ObservableCollection<Commands>)DataManager.GetObservableCollection(Enums.DataTables.Commands);
                        Commands.CollectionChanged += DataGrid_CollectionChanged;
                        NotifyPropertyChanged(nameof(Commands));

                        // continue setting collections for remaining tables
                        BanReasons = (ObservableCollection<BanReasons>)DataManager.GetObservableCollection(Enums.DataTables.BanReasons);
                        NotifyPropertyChanged(nameof(BanReasons));
                        BanRules = (ObservableCollection<BanRules>)DataManager.GetObservableCollection(Enums.DataTables.BanRules);
                        NotifyPropertyChanged(nameof(BanRules));
                        CategoryList = (ObservableCollection<CategoryList>)DataManager.GetObservableCollection(Enums.DataTables.CategoryList);
                        NotifyPropertyChanged(nameof(CategoryList));
                        ChannelEvents = (ObservableCollection<ChannelEvents>)DataManager.GetObservableCollection(Enums.DataTables.ChannelEvents);
                        NotifyPropertyChanged(nameof(ChannelEvents));
                        Clips = (ObservableCollection<Clips>)DataManager.GetObservableCollection(Enums.DataTables.Clips);
                        NotifyPropertyChanged(nameof(Clips));
                        Currency = (ObservableCollection<Currency>)DataManager.GetObservableCollection(Enums.DataTables.Currency);
                        NotifyPropertyChanged(nameof(Currency));
                        CurrencyType = (ObservableCollection<CurrencyType>)DataManager.GetObservableCollection(Enums.DataTables.CurrencyType);
                        NotifyPropertyChanged(nameof(CurrencyType));
                        CustomWelcome = (ObservableCollection<CustomWelcome>)DataManager.GetObservableCollection(Enums.DataTables.CustomWelcome);
                        NotifyPropertyChanged(nameof(CustomWelcome));
                        GameDeadCounter = (ObservableCollection<GameDeadCounter>)DataManager.GetObservableCollection(Enums.DataTables.GameDeadCounter);
                        NotifyPropertyChanged(nameof(GameDeadCounter));
                        GiveawayUserData = (ObservableCollection<GiveawayUserData>)DataManager.GetObservableCollection(Enums.DataTables.GiveawayUserData);
                        NotifyPropertyChanged(nameof(GiveawayUserData));
                        InRaidData = (ObservableCollection<InRaidData>)DataManager.GetObservableCollection(Enums.DataTables.InRaidData);
                        NotifyPropertyChanged(nameof(InRaidData));
                        LearnMsgs = (ObservableCollection<LearnMsgs>)DataManager.GetObservableCollection(Enums.DataTables.LearnMsgs);
                        NotifyPropertyChanged(nameof(LearnMsgs));
                        ModeratorApprove = (ObservableCollection<ModeratorApprove>)DataManager.GetObservableCollection(Enums.DataTables.ModeratorApprove);
                        NotifyPropertyChanged(nameof(ModeratorApprove));
                        MultiChannels = (ObservableCollection<MultiChannels>)DataManager.GetObservableCollection(Enums.DataTables.MultiChannels);
                        NotifyPropertyChanged(nameof(MultiChannels));
                        MultiLiveStreams = (ObservableCollection<MultiLiveStreams>)DataManager.GetObservableCollection(Enums.DataTables.MultiLiveStreams);
                        NotifyPropertyChanged(nameof(MultiLiveStreams));
                        MultiSummaryLiveStreams = (ObservableCollection<MultiSummaryLiveStreams>)DataManager.GetObservableCollection(Enums.DataTables.MultiSummaryLiveStreams);
                        NotifyPropertyChanged(nameof(MultiSummaryLiveStreams));
                        MultiWebhooks = (ObservableCollection<MultiWebhooks>)DataManager.GetObservableCollection(Enums.DataTables.MultiWebhooks);
                        NotifyPropertyChanged(nameof(MultiWebhooks));
                        OldFollowUsers = (ObservableCollection<OldFollowUsers>)DataManager.GetObservableCollection(Enums.DataTables.OldFollowUsers);
                        NotifyPropertyChanged(nameof(OldFollowUsers));
                        OutRaidData = (ObservableCollection<OutRaidData>)DataManager.GetObservableCollection(Enums.DataTables.OutRaidData);
                        NotifyPropertyChanged(nameof(OutRaidData));
                        OverlayServices = (ObservableCollection<OverlayServices>)DataManager.GetObservableCollection(Enums.DataTables.OverlayServices);
                        NotifyPropertyChanged(nameof(OverlayServices));
                        OverlayTicker = (ObservableCollection<OverlayTicker>)DataManager.GetObservableCollection(Enums.DataTables.OverlayTicker);
                        NotifyPropertyChanged(nameof(OverlayTicker));
                        Quotes = (ObservableCollection<Quotes>)DataManager.GetObservableCollection(Enums.DataTables.Quotes);
                        NotifyPropertyChanged(nameof(Quotes));
                        ShoutOuts = (ObservableCollection<ShoutOuts>)DataManager.GetObservableCollection(Enums.DataTables.ShoutOuts);
                        NotifyPropertyChanged(nameof(ShoutOuts));
                        StreamStats = (ObservableCollection<StreamStats>)DataManager.GetObservableCollection(Enums.DataTables.StreamStats);
                        NotifyPropertyChanged(nameof(StreamStats));
                        UserStats = (ObservableCollection<UserStats>)DataManager.GetObservableCollection(Enums.DataTables.UserStats);
                        NotifyPropertyChanged(nameof(UserStats));
                        Webhooks = (ObservableCollection<Webhooks>)DataManager.GetObservableCollection(Enums.DataTables.Webhooks);
                        NotifyPropertyChanged(nameof(Webhooks));

                        // refresh the 'status bar' count items
                        NotifyPropertyChanged(nameof(CurrUserCount));
                        NotifyPropertyChanged(nameof(CurrFollowers));
                        NotifyPropertyChanged(nameof(CurrBuiltInComCount));
                        NotifyPropertyChanged(nameof(CurrUserComsCount));
                    }
                );
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// Notifies a property changed for the GUI to refresh.
        /// </summary>
        /// <param name="propertyName">Name of the property which updated values.</param>
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            LogWriter.DebugLog("NotifyPropertyChanged", Enums.DebugLogTypes.GUIDataViews, $"Notifying GUI for updated {propertyName} property data.");
        }

        private void DataManager_OnDataCollectionUpdated(object sender, Events.OnDataCollectionUpdatedEventArgs e)
        {
            NotifyPropertyChanged(e.DatabaseModelName);
        }
        /// <summary>
        /// Handles when a data table view changes, to update the GUI and refresh the status bar item counts.
        /// </summary>
        /// <param name="sender">Object invoking the event.</param>
        /// <param name="e">The parameters sent with the event.</param>
        private void DataGrid_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            lock (GUIDataManagerLock.Lock)
            {
                // refresh the 'status bar' count items
                NotifyPropertyChanged(nameof(CurrUserCount));
                NotifyPropertyChanged(nameof(CurrFollowers));
                NotifyPropertyChanged(nameof(CurrBuiltInComCount));
                NotifyPropertyChanged(nameof(CurrUserComsCount));

                if (sender == Commands || sender == CommandsUser)
                {
                    SetCommandCollection();
                }
            }
        }
        /// <summary>
        /// Builds the list of commands to update the number of commands within the GUI
        /// </summary>
        private void SetCommandCollection()
        {
            lock (GUIDataManagerLock.Lock)
            {
                CommandCollection.Clear();

                foreach (var command in DataManager.GetCommandList())
                {
                    CommandCollection.Add(command);
                }
            }
        }

    }
}
