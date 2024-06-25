using StreamerBotLib.DataSQL;
using StreamerBotLib.DataSQL.Models;
using StreamerBotLib.Interfaces;
using StreamerBotLib.Models;
using StreamerBotLib.Systems;

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Documents;

namespace StreamerBotLib.GUI
{
    public class GUIDataManagerViews : INotifyPropertyChanged
    {
        #region DataManager TableViews
        //private readonly SQLDBContext context = new DataManagerFactory().CreateDbContext();
        private IDataManager DataManager { get; }

        public FlowDocument ChatData { get; private set; }
        public ObservableCollection<string> CurrUserList { get; private set; }

        public ObservableCollection<UserJoin> JoinCollection { get; set; }
        public ObservableCollection<string> CommandCollection { get; set; } = [];
        public ObservableCollection<string> GiveawayCollection { get; set; }
        public int CurrFollowers
        {
            get
            {
               
                    return Followers?.Count(f => f.IsFollower) ?? 0;                
            }
        }

        public int CurrUserCount
        {
            get
            {                
                    return Users?.Count ?? 0;                
            }
        }

        public int CurrBuiltInComCount
        {
            get
            {                
                    return Commands?.Count ?? 0;                
            }
        }

        public int CurrUserComsCount
        {
            get
            {                
                    return CommandsUser?.Count ?? 0;                
            }
        }

        public ObservableCollection<ChannelEvents> ChannelEvents { get; private set; }
        public ObservableCollection<Users> Users { get; private set; }
        public ObservableCollection<Followers> Followers { get; private set; }
        public ObservableCollection<Webhooks> Webhooks { get; private set; }
        public ObservableCollection<Currency> Currency { get; private set; }
        public ObservableCollection<CurrencyType> CurrencyType { get; private set; }
        public ObservableCollection<CommandsUser> CommandsUser { get; private set; }
        public ObservableCollection<Commands> Commands { get; private set; }
        public ObservableCollection<StreamStats> StreamStats { get; private set; }
        public ObservableCollection<ShoutOuts> ShoutOuts { get; private set; }
        public ObservableCollection<CategoryList> CategoryList { get; private set; }
        public ObservableCollection<Clips> Clips { get; private set; }
        public ObservableCollection<InRaidData> InRaidData { get; private set; }
        public ObservableCollection<OutRaidData> OutRaidData { get; private set; }
        public ObservableCollection<GiveawayUserData> GiveawayUserData { get; private set; }
        public ObservableCollection<CustomWelcome> CustomWelcome { get; private set; }
        public ObservableCollection<LearnMsgs> LearnMsgs { get; private set; }
        public ObservableCollection<BanRules> BanRules { get; private set; }
        public ObservableCollection<BanReasons> BanReasons { get; private set; }
        public ObservableCollection<OverlayServices> OverlayServices { get; private set; }
        public ObservableCollection<OverlayTicker> OverlayTicker { get; private set; }
        public ObservableCollection<ModeratorApprove> ModeratorApprove { get; private set; }
        public ObservableCollection<GameDeadCounter> GameDeadCounter { get; private set; }
        public ObservableCollection<Quotes> Quotes { get; private set; }
        public ObservableCollection<UserStats> UserStats { get; private set; }

        #region MultiLive Collections
        public ObservableCollection<MultiMsgEndPoints> MultiMsgEndPoints { get; private set; }
        public ObservableCollection<MultiChannels> MultiChannels { get; private set; }
        public ObservableCollection<MultiLiveStreams> MultiLiveStreams { get; private set; }
        public ObservableCollection<MultiSummaryLiveStreams> MultiSummaryLiveStreams { get; private set; }

        public ObservableCollection<ArchiveMultiStream> CleanupList => DataManager.CleanupList;
        public string MultiLiveStatusLog => DataManager.MultiLiveStatusLog;
        #endregion
        #endregion

        public GUIDataManagerViews()
        {
            DataManager = SystemsController.DataManage;

            ChatData = ActionSystem.ChatData;
            JoinCollection = ActionSystem.JoinCollection;
            GiveawayCollection = ActionSystem.GiveawayCollection;
            CurrUserList = ActionSystem.CurrUserJoin;

            /// <summary>
            /// Used in class object construction to build assign the DataTable views for the GUI, requires <c>SystemsController</c> to be initialized.
            /// </summary>
            BanReasons = DataManager.GetBanReasonsLocalObservable();
            BanRules = DataManager.GetBanRulesLocalObservable();
            CategoryList = DataManager.GetCategoryListLocalObservable();
            ChannelEvents = DataManager.GetChannelEventsLocalObservable();
            Clips = DataManager.GetClipsLocalObservable();
            Commands = DataManager.GetCommandsLocalObservable();
            CommandsUser = DataManager.GetCommandsUserLocalObservable();
            Currency = DataManager.GetCurrencyLocalObservable();
            CurrencyType = DataManager.GetCurrencyTypeLocalObservable();
            CustomWelcome = DataManager.GetCustomWelcomeLocalObservable();
            Followers = DataManager.GetFollowersLocalObservable();
            GameDeadCounter = DataManager.GetGameDeadCounterLocalObservable();
            GiveawayUserData = DataManager.GetGiveawayUserDataLocalObservable();
            InRaidData = DataManager.GetInRaidDataLocalObservable();
            LearnMsgs = DataManager.GetLearnMsgsLocalObservable();
            ModeratorApprove = DataManager.GetModeratorApproveLocalObservable();
            MultiChannels = DataManager.GetMultiChannelsLocalObservable();
            MultiLiveStreams = DataManager.GetMultiLiveStreamsLocalObservable();
            MultiMsgEndPoints = DataManager.GetMultiMsgEndPointsLocalObservable();
            MultiSummaryLiveStreams = DataManager.GetMultiSummaryLiveStreamsLocalObservable();
            OutRaidData = DataManager.GetOutRaidDataLocalObservable();
            OverlayServices = DataManager.GetOverlayServicesLocalObservable();
            OverlayTicker = DataManager.GetOverlayTickerLocalObservable();
            Quotes = DataManager.GetQuotesLocalObservable();
            ShoutOuts = DataManager.GetShoutOutsLocalObservable();
            StreamStats = DataManager.GetStreamStatsLocalObservable();
            Users = DataManager.GetUsersLocalObservable();
            UserStats = DataManager.GetUserStatsLocalObservable();
            Webhooks = DataManager.GetWebhooksLocalObservable();

            Users.CollectionChanged += DataGrid_CollectionChanged;
            Followers.CollectionChanged += DataGrid_CollectionChanged;
            CommandsUser.CollectionChanged += DataGrid_CollectionChanged;
            Commands.CollectionChanged += DataGrid_CollectionChanged;

            SetCommandCollection();

            DataManager.OnDataCollectionUpdated += DataManager_OnDataCollectionUpdated;
        }

        private void DataManager_OnDataCollectionUpdated(object sender, Events.OnDataCollectionUpdatedEventArgs e)
        {
            switch (e.DatabaseModelName)
            {
                case "BanReasons":
                    NotifyPropertyChanged(nameof(BanReasons));
                    break;
                case "BanRules":
                    NotifyPropertyChanged(nameof (BanReasons));
                    break;
                case "CategoryList":
                    NotifyPropertyChanged(nameof(CategoryList));
                    break;
                case "ChannelEvents":
                    NotifyPropertyChanged(nameof(ChannelEvents));
                    break;
                case "Clips":
                    NotifyPropertyChanged(nameof(Clips));
                    break;
                case "Commands":
                    NotifyPropertyChanged(nameof(Commands));
                    break;
                case "CommandsUser":
                    NotifyPropertyChanged(nameof(CommandsUser));
                    break;
                case "Currency":
                    NotifyPropertyChanged(nameof(Currency));
                    break;
                case "CurrencyType":
                    NotifyPropertyChanged(nameof(CurrencyType));
                    break;
                case "CustomWelcome":
                    NotifyPropertyChanged(nameof(CustomWelcome));
                    break;
                case "Followers":
                    NotifyPropertyChanged(nameof(Followers));
                    NotifyPropertyChanged(nameof(CurrFollowers));
                    NotifyPropertyChanged(nameof(Users));
                    break;
                case "GameDeadCounter":
                    NotifyPropertyChanged(nameof(GameDeadCounter));
                    break;
                case "GiveawayUserData":
                    NotifyPropertyChanged(nameof(GiveawayUserData));
                    break;
                case "InRaidData":
                    NotifyPropertyChanged(nameof(InRaidData));
                    break;
                case "LearnMsgs":
                    NotifyPropertyChanged(nameof(LearnMsgs));
                    break;
                case "ModeratorApprove":
                    NotifyPropertyChanged(nameof(ModeratorApprove));
                    break;
                case "MultiChannels":
                    NotifyPropertyChanged(nameof(MultiChannels));
                    break;
                case "MultiLiveStreams":
                    NotifyPropertyChanged(nameof(MultiLiveStreams));
                    break;
                case "MultiMsgEndPoints":
                    NotifyPropertyChanged(nameof(MultiMsgEndPoints));
                    break;
                case "MultiSummaryLiveStreams":
                    NotifyPropertyChanged(nameof(MultiSummaryLiveStreams));
                    break;
                case "OutRaidData":
                    NotifyPropertyChanged(nameof(OutRaidData));
                    break;
                case "OverlayServices":
                    NotifyPropertyChanged(nameof(OverlayServices));
                    break;
                case "OverlayTicker":
                    NotifyPropertyChanged(nameof(OverlayTicker));
                    break;
                case "Quotes":
                    NotifyPropertyChanged(nameof(Quotes));
                    break;
                case "ShoutOuts":
                    NotifyPropertyChanged(nameof(ShoutOuts));
                    break;
                case "StreamStats":
                    NotifyPropertyChanged(nameof(StreamStats));
                    break;
                case "Users" or "UserStats":
                   NotifyPropertyChanged(nameof(Users));
                    NotifyPropertyChanged(nameof(UserStats));
                    break;
                case "Webhooks":
                    NotifyPropertyChanged(nameof(Webhooks));
                    break;
                case null:
                    break;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Notifies a property changed for the GUI to refresh.
        /// </summary>
        /// <param name="propertyName">Name of the property which updated values.</param>
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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

                if (sender == Commands)
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

                using var context = new SQLDBContext();
                foreach (var command in context.Commands)
                {
                    CommandCollection.Add(command.CmdName);
                }
            }
        }
    }
}
