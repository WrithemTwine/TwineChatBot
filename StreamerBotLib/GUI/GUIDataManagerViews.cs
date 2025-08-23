using StreamerBotLib.DataSQL.Models;
using StreamerBotLib.Models;
using StreamerBotLib.Models.Enums;
using StreamerBotLib.Models.Events;
using StreamerBotLib.Models.Interfaces;
using StreamerBotLib.Static;
using StreamerBotLib.Systems;

using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;

using BanReasons = StreamerBotLib.DataSQL.Models.BanReasons;
using CurrencyType = StreamerBotLib.DataSQL.Models.CurrencyType;

namespace StreamerBotLib.GUI
{
    public class GUIDataManagerViews : INotifyPropertyChanged, IGUIDataManagerViews
    {
        #region DataManager TableViews
        private Action<bool, Action<IEnumerable<string>>> GetDataCommands { get; set; }

        public FlowDocument ChatData { get; private set; }
        public ObservableCollection<string> CurrUserList { get; private set; }

        public ObservableCollection<UserJoin> JoinCollection { get; set; }
        public ObservableCollection<string> CommandCollection { get; set; } = [];
        public ObservableCollection<LiveUser> GiveawayCollection { get; set; }

        public int CurrFollowers => Followers?.Count(f => f.IsFollower) ?? 0;
        public int CurrUserCount => Users?.Count ?? 0;
        public int CurrBuiltInComCount => Commands?.Count ?? 0;
        public int CurrUserComsCount => CommandsUser?.Count ?? 0;

        public static ObservableCollection<CategoryList> CurrCategoryList { get; private set; }
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

        internal List<ArchiveMultiStream> cleanupList;
        internal List<string> multiLiveStatusLog;

        public List<ArchiveMultiStream> CleanupList { get => cleanupList; }
        public List<string> MultiLiveStatusLog { get => multiLiveStatusLog; }
        #endregion

        #endregion

        public GUIDataManagerViews()
        {
            cleanupList = [];
            multiLiveStatusLog = [];
        }

        public void SetSystemCollections(ActionSystem actionSystem)
        {
            ChatData = actionSystem.ChatData;
            JoinCollection = actionSystem.JoinCollection;
            GiveawayCollection = actionSystem.GiveawayCollection;
            CurrUserList = actionSystem.CurrUserJoin;
        }

        public void SetDataManagerViews(DataBot dataBot, Action<bool, Action<IEnumerable<string>>> callback)
        {
            GetDataCommands = callback;

            // set specific collection changed events for StatusBar data
            dataBot.GetICollection(DataTables.Users, (source) => AssignCollection(source, nameof(Users), true)); // Users = (ObservableCollection<Users>)DataManager.GetICollection(DataTables.Users);
            dataBot.GetICollection(DataTables.Followers, (source) => AssignCollection(source, nameof(Followers), true));
            dataBot.GetICollection(DataTables.CommandsUser, (source) => AssignCollection(source, nameof(CommandsUser), true));
            dataBot.GetICollection(DataTables.Commands, (source) => AssignCollection(source, nameof(Commands), true));

            // continue setting collections for remaining tables
            dataBot.GetICollection(DataTables.BanReasons, (source) => AssignCollection(source, nameof(BanReasons)));
            dataBot.GetICollection(DataTables.BanRules, (source) => AssignCollection(source, nameof(BanRules)));
            dataBot.GetICollection(DataTables.CategoryList, (source) => AssignCollection(source, nameof(CategoryList)));
            dataBot.GetICollection(DataTables.ChannelEvents, (source) => AssignCollection(source, nameof(ChannelEvents)));
            dataBot.GetICollection(DataTables.Clips, (source) => AssignCollection(source, nameof(Clips)));
            dataBot.GetICollection(DataTables.Currency, (source) => AssignCollection(source, nameof(Currency)));
            dataBot.GetICollection(DataTables.CurrencyType, (source) => AssignCollection(source, nameof(CurrencyType)));
            dataBot.GetICollection(DataTables.CustomWelcome, (source) => AssignCollection(source, nameof(CustomWelcome)));
            dataBot.GetICollection(DataTables.GameDeadCounter, (source) => AssignCollection(source, nameof(GameDeadCounter)));
            dataBot.GetICollection(DataTables.GiveawayUserData, (source) => AssignCollection(source, nameof(GiveawayUserData)));
            dataBot.GetICollection(DataTables.InRaidData, (source) => AssignCollection(source, nameof(InRaidData)));
            dataBot.GetICollection(DataTables.LearnMsgs, (source) => AssignCollection(source, nameof(LearnMsgs)));
            dataBot.GetICollection(DataTables.ModeratorApprove, (source) => AssignCollection(source, nameof(ModeratorApprove)));
            dataBot.GetICollection(DataTables.MultiChannels, (source) => AssignCollection(source, nameof(MultiChannels)));
            dataBot.GetICollection(DataTables.MultiLiveStreams, (source) => AssignCollection(source, nameof(MultiLiveStreams)));
            dataBot.GetICollection(DataTables.MultiSummaryLiveStreams, (source) => AssignCollection(source, nameof(MultiSummaryLiveStreams)));
            dataBot.GetICollection(DataTables.MultiWebhooks, (source) => AssignCollection(source, nameof(MultiWebhooks)));
            dataBot.GetICollection(DataTables.OldFollowUsers, (source) => AssignCollection(source, nameof(OldFollowUsers)));
            dataBot.GetICollection(DataTables.OutRaidData, (source) => AssignCollection(source, nameof(OutRaidData)));
            dataBot.GetICollection(DataTables.OverlayServices, (source) => AssignCollection(source, nameof(OverlayServices)));
            dataBot.GetICollection(DataTables.OverlayTicker, (source) => AssignCollection(source, nameof(OverlayTicker)));
            dataBot.GetICollection(DataTables.Quotes, (source) => AssignCollection(source, nameof(Quotes)));
            dataBot.GetICollection(DataTables.ShoutOuts, (source) => AssignCollection(source, nameof(ShoutOuts)));
            dataBot.GetICollection(DataTables.StreamStats, (source) => AssignCollection(source, nameof(StreamStats)));
            dataBot.GetICollection(DataTables.UserStats, (source) => AssignCollection(source, nameof(UserStats)));
            dataBot.GetICollection(DataTables.Webhooks, (source) => AssignCollection(source, nameof(Webhooks)));

            dataBot.SetCleanupList(ref cleanupList);
            dataBot.SetMultiStatusLog(ref multiLiveStatusLog);

            SetCommandCollection();

            dataBot.InitializeDataManagerCollectionUpdateEvent(DataManager_OnDataCollectionUpdated);
        }

        private void AssignCollection(object source, string ChangedProperty, bool AddCollectionEvent = false)
        {
            switch (ChangedProperty)
            {
                case nameof(Users):
                    Users = (ObservableCollection<Users>)source;
                    NotifyPropertyChanged(nameof(CurrUserCount));
                    break;
                case nameof(Followers):
                    Followers = (ObservableCollection<Followers>)source;
                    NotifyPropertyChanged(nameof(CurrFollowers));
                    break;
                case nameof(CommandsUser):
                    CommandsUser = (ObservableCollection<CommandsUser>)source;
                    NotifyPropertyChanged(nameof(CurrUserComsCount));
                    break;
                case nameof(Commands):
                    Commands = (ObservableCollection<Commands>)source;
                    NotifyPropertyChanged(nameof(CurrBuiltInComCount));
                    break;
                case nameof(BanReasons):
                    BanReasons = (ObservableCollection<BanReasons>)source;
                    break;
                case nameof(BanRules):
                    BanRules = (ObservableCollection<BanRules>)source;
                    break;
                case nameof(CategoryList):
                    CategoryList = (ObservableCollection<CategoryList>)source;
                    CurrCategoryList = CategoryList;
                    break;
                case nameof(ChannelEvents):
                    ChannelEvents = (ObservableCollection<ChannelEvents>)source;
                    break;
                case nameof(Clips):
                    Clips = (ObservableCollection<Clips>)source;
                    break;
                case nameof(Currency):
                    Currency = (ObservableCollection<Currency>)source;
                    break;
                case nameof(CurrencyType):
                    CurrencyType = (ObservableCollection<CurrencyType>)source;
                    break;
                case nameof(CustomWelcome):
                    CustomWelcome = (ObservableCollection<CustomWelcome>)source;
                    break;
                case nameof(GameDeadCounter):
                    GameDeadCounter = (ObservableCollection<GameDeadCounter>)source;
                    break;
                case nameof(GiveawayUserData):
                    GiveawayUserData = (ObservableCollection<GiveawayUserData>)source;
                    break;
                case nameof(InRaidData):
                    InRaidData = (ObservableCollection<InRaidData>)source;
                    break;
                case nameof(LearnMsgs):
                    LearnMsgs = (ObservableCollection<LearnMsgs>)source;
                    break;
                case nameof(ModeratorApprove):
                    ModeratorApprove = (ObservableCollection<ModeratorApprove>)source;
                    break;
                case nameof(OldFollowUsers):
                    OldFollowUsers = (ObservableCollection<OldFollowUsers>)source;
                    break;
                case nameof(OutRaidData):
                    OutRaidData = (ObservableCollection<OutRaidData>)source;
                    break;
                case nameof(OverlayServices):
                    OverlayServices = (ObservableCollection<OverlayServices>)source;
                    break;
                case nameof(OverlayTicker):
                    OverlayTicker = (ObservableCollection<OverlayTicker>)source;
                    break;
                case nameof(Quotes):
                    Quotes = (ObservableCollection<Quotes>)source;
                    break;
                case nameof(ShoutOuts):
                    ShoutOuts = (ObservableCollection<ShoutOuts>)source;
                    break;
                case nameof(StreamStats):
                    StreamStats = (ObservableCollection<StreamStats>)source;
                    break;
                case nameof(UserStats):
                    UserStats = (ObservableCollection<UserStats>)source;
                    break;
                case nameof(Webhooks):
                    Webhooks = (ObservableCollection<Webhooks>)source;
                    break;
                case nameof(MultiWebhooks):
                    MultiWebhooks = (ObservableCollection<MultiWebhooks>)source;
                    break;
                case nameof(MultiChannels):
                    MultiChannels = (ObservableCollection<MultiChannels>)source;
                    break;
                case nameof(MultiLiveStreams):
                    MultiLiveStreams = (ObservableCollection<MultiLiveStreams>)source;
                    break;
                case nameof(MultiSummaryLiveStreams):
                    MultiSummaryLiveStreams = (ObservableCollection<MultiSummaryLiveStreams>)source;
                    break;
            }

            if (AddCollectionEvent)
            {
                //((INotifyCollectionChanged)source).CollectionChanged += DataGrid_CollectionChanged;
            }

            NotifyPropertyChanged(ChangedProperty);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Notifies a property changed for the GUI to refresh.
        /// </summary>
        /// <param name="propertyName">Name of the property which updated values.</param>
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new(propertyName));
            LogWriter.DebugLog("NotifyPropertyChanged", DebugLogTypes.GUIDataViews, $"Notifying GUI for updated {propertyName} property data.");
        }

        //public void UpdatedGUIData(OnDataCollectionUpdatedEventArgs e)
        //{
        //    NotifyPropertyChanged(e.DatabaseModelName);

        //    // refresh the 'status bar' count items
        //    if (e.DatabaseModelName is "Users")
        //    {
        //        NotifyPropertyChanged(nameof(CurrUserCount));
        //    }
        //    else if (e.DatabaseModelName is "Followers")
        //    {
        //        NotifyPropertyChanged(nameof(CurrFollowers));
        //    }
        //    else if (e.DatabaseModelName is "Commands" or "CommandsUser")
        //    {
        //        SetCommandCollection();
        //        NotifyPropertyChanged(nameof(CurrBuiltInComCount));
        //        NotifyPropertyChanged(nameof(CurrUserComsCount));
        //    }
        //}

        public void DataManager_OnDataCollectionUpdated(object sender, OnDataCollectionUpdatedEventArgs e)
        {
            NotifyPropertyChanged(e.DatabaseModelName);

            // refresh the 'status bar' count items
            if (e.DatabaseModelName is "Users")
            {
                NotifyPropertyChanged(nameof(CurrUserCount));
            }
            else if (e.DatabaseModelName is "Followers")
            {
                NotifyPropertyChanged(nameof(CurrFollowers));
            }
            else if (e.DatabaseModelName is "Commands" or "CommandsUser")
            {
                SetCommandCollection();
                NotifyPropertyChanged(nameof(CurrBuiltInComCount));
                NotifyPropertyChanged(nameof(CurrUserComsCount));
            }
        }

        ///// <summary>
        ///// Handles when a data table view changes, to update the GUI and refresh the status bar item counts.
        ///// </summary>
        ///// <param name="sender">Object invoking the event.</param>
        ///// <param name="e">The parameters sent with the event.</param>
        //private void DataGrid_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        //{
        //    if (sender == Commands || sender == CommandsUser)
        //    {
        //        NotifyPropertyChanged(nameof(CurrBuiltInComCount));
        //        NotifyPropertyChanged(nameof(CurrUserComsCount));
        //        SetCommandCollection();
        //    }
        //    if (sender == Users)
        //    {
        //        NotifyPropertyChanged(nameof(CurrUserCount));
        //    }
        //    if (sender == Followers)
        //    {
        //        NotifyPropertyChanged(nameof(CurrFollowers));
        //    }
        //}

        /// <summary>
        /// Builds the list of commands to update the number of commands within the GUI
        /// </summary>
        private void SetCommandCollection()
        {
            GetDataCommands.Invoke(false, BuildCommandList);
        }

        private void BuildCommandList(IEnumerable<string> commands)
        {
            ThreadManager.AddTaskToGUIDispatcher(() =>
            {
                CommandCollection.Clear();
                foreach (var command in commands)
                {
                    CommandCollection.Add(command);
                }
            });
        }
    }

    #region Category Conversion

    public class ConvertCategory : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        { // source data requires SQL-safe escaping, ' , due to SQL queries using source text and mismatching- ' -in the where clause
            return FormatData.RemoveEscapeFormat((value as string) ?? "All");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        { // source data requires SQL-safe escaping, ' , due to SQL queries using source text and mismatching- ' -in the where clause
            return FormatData.AddEscapeFormat((string)value);
        }
    }

    public class EditConvertCategory : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            List<CheckBox> checkBoxes = [];
            List<string> categories = (value as List<string>);
            categories.ForEach((c) => c = FormatData.RemoveEscapeFormat(c));

            if (categories.Contains("All"))
            {
                checkBoxes.Add(new() { Content = "All", IsChecked = true });
                checkBoxes.AddRange((from C in GUIDataManagerViews.CurrCategoryList
                                     where C.Category != "All"
                                     orderby C.Category
                                     select new CheckBox() { Content = FormatData.RemoveEscapeFormat(C.Category), IsChecked = false }));
            }
            else
            {
                checkBoxes.Add(new() { Content = "All", IsChecked = false }); // add "All" selection first

                checkBoxes.AddRange((from (string, bool) C in
                                         from Cat in GUIDataManagerViews.CurrCategoryList
                                             // ignore "All" category item
                                         where Cat.Category != "All" && categories.Contains(FormatData.RemoveEscapeFormat(Cat.Category))
                                         orderby Cat.Category  // sort by category name for easier search
                                         select (Cat.Category, true)
                                     select new CheckBox() { Content = FormatData.RemoveEscapeFormat(C.Item1), IsChecked = C.Item2 }).ToList());

                checkBoxes.AddRange((from (string, bool) C in
                                         from Cat in GUIDataManagerViews.CurrCategoryList
                                             // ignore "All" category item
                                         where Cat.Category != "All" && !categories.Contains(FormatData.RemoveEscapeFormat(Cat.Category))
                                         orderby Cat.Category  // sort by category name for easier search
                                         select (Cat.Category, false)
                                     select new CheckBox() { Content = FormatData.RemoveEscapeFormat(C.Item1), IsChecked = C.Item2 }).ToList());
            }

            return checkBoxes;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ICollection<CheckBox> categories = (value as ICollection<CheckBox>);

            return
                (from C in categories
                 where (string)C.Content == "All"
                 select C.IsChecked).FirstOrDefault() == true
                ? ["All"] : new List<string>(from C in categories
                                             where C.IsChecked == true
                                             select FormatData.AddEscapeFormat((string)C.Content));
        }
    }

    #endregion
}
