#define USE_OBSERVABLECOLLECTION

using StreamerBotLib.DataSQL.Models;
using StreamerBotLib.Events;
using StreamerBotLib.Interfaces;
using StreamerBotLib.Models;
using StreamerBotLib.Static;
using StreamerBotLib.Systems;

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Data;
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

#if USE_OBSERVABLECOLLECTION
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

        public List<ArchiveMultiStream> CleanupList { get; private set; }
        public string MultiLiveStatusLog { get => DataManager.MultiLiveStatusLog; }
        #endregion

#else
        public static List<CategoryList> CurrCategoryList { get; private set; }

        public List<Users> Users { get; private set; }
        public List<Followers> Followers { get; private set; }
        public List<CommandsUser> CommandsUser { get; private set; }
        public List<Commands> Commands { get; private set; }

        public List<BanRules> BanRules { get; private set; }
        public List<BanReasons> BanReasons { get; private set; }
        public List<CategoryList> CategoryList { get; private set; }
        public List<ChannelEvents> ChannelEvents { get; private set; }
        public List<Clips> Clips { get; private set; }
        public List<Currency> Currency { get; private set; }
        public List<CurrencyType> CurrencyType { get; private set; }
        public List<CustomWelcome> CustomWelcome { get; private set; }
        public List<GameDeadCounter> GameDeadCounter { get; private set; }
        public List<GiveawayUserData> GiveawayUserData { get; private set; }
        public List<InRaidData> InRaidData { get; private set; }
        public List<LearnMsgs> LearnMsgs { get; private set; }
        public List<ModeratorApprove> ModeratorApprove { get; private set; }
        public List<OldFollowUsers> OldFollowUsers { get; private set; }
        public List<OutRaidData> OutRaidData { get; private set; }
        public List<OverlayServices> OverlayServices { get; private set; }
        public List<OverlayTicker> OverlayTicker { get; private set; }
        public List<Quotes> Quotes { get; private set; }
        public List<ShoutOuts> ShoutOuts { get; private set; }
        public List<StreamStats> StreamStats { get; private set; }
        public List<UserStats> UserStats { get; private set; }
        public List<Webhooks> Webhooks { get; private set; }

        #region MultiLive Collections
        public List<MultiWebhooks> MultiWebhooks { get; private set; }
        public List<MultiChannels> MultiChannels { get; private set; }
        public List<MultiLiveStreams> MultiLiveStreams { get; private set; }
        public List<MultiSummaryLiveStreams> MultiSummaryLiveStreams { get; private set; }

        public ObservableCollection<ArchiveMultiStream> CleanupList { get; private set; }
        public string MultiLiveStatusLog { get => DataManager.MultiLiveStatusLog; }
        #endregion
#endif
        #endregion

        public static event EventHandler<OnDataCollectionUpdatedEventArgs> DataViewsUpdated;

        public GUIDataManagerViews()
        {
            DataManager = SystemsController.DataManage;
            DataManager.OnDataCollectionUpdated += DataManager_OnDataCollectionUpdated;

            ChatData = ActionSystem.ChatData;
            JoinCollection = ActionSystem.JoinCollection;
            GiveawayCollection = ActionSystem.GiveawayCollection;
            CurrUserList = ActionSystem.CurrUserJoin;

            // commenting this method or using "Debug_ViewXaml" build config and building allows the xaml designer to display
            // the xaml design; otherwise, xaml designer throws an exception
#if !DEBUG_VIEWXAML
            SetObservables();
#endif
        }

        private void SetObservables()
        {
            ThreadManager.CreateThreadStart(".ctor_GUIDataManagerViews", () =>
            {

#if USE_OBSERVABLECOLLECTION

                // set specific collection changed events for StatusBar data
                Users = (ObservableCollection<Users>)DataManager.GetICollection(Enums.DataTables.Users);
                //Users.CollectionChanged += DataGrid_CollectionChanged;
                NotifyPropertyChanged(nameof(Users));
                Followers = (ObservableCollection<Followers>)DataManager.GetICollection(Enums.DataTables.Followers);
                //Followers.CollectionChanged += DataGrid_CollectionChanged;
                NotifyPropertyChanged(nameof(Followers));
                CommandsUser = (ObservableCollection<CommandsUser>)DataManager.GetICollection(Enums.DataTables.CommandsUser);
                //CommandsUser.CollectionChanged += DataGrid_CollectionChanged;
                NotifyPropertyChanged(nameof(CommandsUser));
                Commands = (ObservableCollection<Commands>)DataManager.GetICollection(Enums.DataTables.Commands);
                //Commands.CollectionChanged += DataGrid_CollectionChanged;
                NotifyPropertyChanged(nameof(Commands));

                // continue setting collections for remaining tables
                BanReasons = (ObservableCollection<BanReasons>)DataManager.GetICollection(Enums.DataTables.BanReasons);
                NotifyPropertyChanged(nameof(BanReasons));
                BanRules = (ObservableCollection<BanRules>)DataManager.GetICollection(Enums.DataTables.BanRules);
                NotifyPropertyChanged(nameof(BanRules));
                CategoryList = (ObservableCollection<CategoryList>)DataManager.GetICollection(Enums.DataTables.CategoryList);
                NotifyPropertyChanged(nameof(CategoryList));
                ChannelEvents = (ObservableCollection<ChannelEvents>)DataManager.GetICollection(Enums.DataTables.ChannelEvents);
                NotifyPropertyChanged(nameof(ChannelEvents));
                Clips = (ObservableCollection<Clips>)DataManager.GetICollection(Enums.DataTables.Clips);
                NotifyPropertyChanged(nameof(Clips));
                Currency = (ObservableCollection<Currency>)DataManager.GetICollection(Enums.DataTables.Currency);
                NotifyPropertyChanged(nameof(Currency));
                CurrencyType = (ObservableCollection<CurrencyType>)DataManager.GetICollection(Enums.DataTables.CurrencyType);
                NotifyPropertyChanged(nameof(CurrencyType));
                CustomWelcome = (ObservableCollection<CustomWelcome>)DataManager.GetICollection(Enums.DataTables.CustomWelcome);
                NotifyPropertyChanged(nameof(CustomWelcome));
                GameDeadCounter = (ObservableCollection<GameDeadCounter>)DataManager.GetICollection(Enums.DataTables.GameDeadCounter);
                NotifyPropertyChanged(nameof(GameDeadCounter));
                GiveawayUserData = (ObservableCollection<GiveawayUserData>)DataManager.GetICollection(Enums.DataTables.GiveawayUserData);
                NotifyPropertyChanged(nameof(GiveawayUserData));
                InRaidData = (ObservableCollection<InRaidData>)DataManager.GetICollection(Enums.DataTables.InRaidData);
                NotifyPropertyChanged(nameof(InRaidData));
                LearnMsgs = (ObservableCollection<LearnMsgs>)DataManager.GetICollection(Enums.DataTables.LearnMsgs);
                NotifyPropertyChanged(nameof(LearnMsgs));
                ModeratorApprove = (ObservableCollection<ModeratorApprove>)DataManager.GetICollection(Enums.DataTables.ModeratorApprove);
                NotifyPropertyChanged(nameof(ModeratorApprove));
                MultiChannels = (ObservableCollection<MultiChannels>)DataManager.GetICollection(Enums.DataTables.MultiChannels);
                NotifyPropertyChanged(nameof(MultiChannels));
                MultiLiveStreams = (ObservableCollection<MultiLiveStreams>)DataManager.GetICollection(Enums.DataTables.MultiLiveStreams);
                NotifyPropertyChanged(nameof(MultiLiveStreams));
                MultiSummaryLiveStreams = (ObservableCollection<MultiSummaryLiveStreams>)DataManager.GetICollection(Enums.DataTables.MultiSummaryLiveStreams);
                NotifyPropertyChanged(nameof(MultiSummaryLiveStreams));
                MultiWebhooks = (ObservableCollection<MultiWebhooks>)DataManager.GetICollection(Enums.DataTables.MultiWebhooks);
                NotifyPropertyChanged(nameof(MultiWebhooks));
                OldFollowUsers = (ObservableCollection<OldFollowUsers>)DataManager.GetICollection(Enums.DataTables.OldFollowUsers);
                NotifyPropertyChanged(nameof(OldFollowUsers));
                OutRaidData = (ObservableCollection<OutRaidData>)DataManager.GetICollection(Enums.DataTables.OutRaidData);
                NotifyPropertyChanged(nameof(OutRaidData));
                OverlayServices = (ObservableCollection<OverlayServices>)DataManager.GetICollection(Enums.DataTables.OverlayServices);
                NotifyPropertyChanged(nameof(OverlayServices));
                OverlayTicker = (ObservableCollection<OverlayTicker>)DataManager.GetICollection(Enums.DataTables.OverlayTicker);
                NotifyPropertyChanged(nameof(OverlayTicker));
                Quotes = (ObservableCollection<Quotes>)DataManager.GetICollection(Enums.DataTables.Quotes);
                NotifyPropertyChanged(nameof(Quotes));
                ShoutOuts = (ObservableCollection<ShoutOuts>)DataManager.GetICollection(Enums.DataTables.ShoutOuts);
                NotifyPropertyChanged(nameof(ShoutOuts));
                StreamStats = (ObservableCollection<StreamStats>)DataManager.GetICollection(Enums.DataTables.StreamStats);
                NotifyPropertyChanged(nameof(StreamStats));
                UserStats = (ObservableCollection<UserStats>)DataManager.GetICollection(Enums.DataTables.UserStats);
                NotifyPropertyChanged(nameof(UserStats));
                Webhooks = (ObservableCollection<Webhooks>)DataManager.GetICollection(Enums.DataTables.Webhooks);
                NotifyPropertyChanged(nameof(Webhooks));

#else
                // set specific collection changed events for StatusBar data
                Users = (List<Users>)DataManager.GetICollection(Enums.DataTables.Users);
                //Users.CollectionChanged += DataGrid_CollectionChanged;
                NotifyPropertyChanged(nameof(Users));
                Followers = (List<Followers>)DataManager.GetICollection(Enums.DataTables.Followers);
                //Followers.CollectionChanged += DataGrid_CollectionChanged;
                NotifyPropertyChanged(nameof(Followers));
                CommandsUser = (List<CommandsUser>)DataManager.GetICollection(Enums.DataTables.CommandsUser);
                //CommandsUser.CollectionChanged += DataGrid_CollectionChanged;
                NotifyPropertyChanged(nameof(CommandsUser));
                Commands = (List<Commands>)DataManager.GetICollection(Enums.DataTables.Commands);
                //Commands.CollectionChanged += DataGrid_CollectionChanged;
                NotifyPropertyChanged(nameof(Commands));

                // continue setting collections for remaining tables
                BanReasons = (List<BanReasons>)DataManager.GetICollection(Enums.DataTables.BanReasons);
                NotifyPropertyChanged(nameof(BanReasons));
                BanRules = (List<BanRules>)DataManager.GetICollection(Enums.DataTables.BanRules);
                NotifyPropertyChanged(nameof(BanRules));
                CategoryList = (List<CategoryList>)DataManager.GetICollection(Enums.DataTables.CategoryList);
                NotifyPropertyChanged(nameof(CategoryList));
                ChannelEvents = (List<ChannelEvents>)DataManager.GetICollection(Enums.DataTables.ChannelEvents);
                NotifyPropertyChanged(nameof(ChannelEvents));
                Clips = (List<Clips>)DataManager.GetICollection(Enums.DataTables.Clips);
                NotifyPropertyChanged(nameof(Clips));
                Currency = (List<Currency>)DataManager.GetICollection(Enums.DataTables.Currency);
                NotifyPropertyChanged(nameof(Currency));
                CurrencyType = (List<CurrencyType>)DataManager.GetICollection(Enums.DataTables.CurrencyType);
                NotifyPropertyChanged(nameof(CurrencyType));
                CustomWelcome = (List<CustomWelcome>)DataManager.GetICollection(Enums.DataTables.CustomWelcome);
                NotifyPropertyChanged(nameof(CustomWelcome));
                GameDeadCounter = (List<GameDeadCounter>)DataManager.GetICollection(Enums.DataTables.GameDeadCounter);
                NotifyPropertyChanged(nameof(GameDeadCounter));
                GiveawayUserData = (List<GiveawayUserData>)DataManager.GetICollection(Enums.DataTables.GiveawayUserData);
                NotifyPropertyChanged(nameof(GiveawayUserData));
                InRaidData = (List<InRaidData>)DataManager.GetICollection(Enums.DataTables.InRaidData);
                NotifyPropertyChanged(nameof(InRaidData));
                LearnMsgs = (List<LearnMsgs>)DataManager.GetICollection(Enums.DataTables.LearnMsgs);
                NotifyPropertyChanged(nameof(LearnMsgs));
                ModeratorApprove = (List<ModeratorApprove>)DataManager.GetICollection(Enums.DataTables.ModeratorApprove);
                NotifyPropertyChanged(nameof(ModeratorApprove));
                MultiChannels = (List<MultiChannels>)DataManager.GetICollection(Enums.DataTables.MultiChannels);
                NotifyPropertyChanged(nameof(MultiChannels));
                MultiLiveStreams = (List<MultiLiveStreams>)DataManager.GetICollection(Enums.DataTables.MultiLiveStreams);
                NotifyPropertyChanged(nameof(MultiLiveStreams));
                MultiSummaryLiveStreams = (List<MultiSummaryLiveStreams>)DataManager.GetICollection(Enums.DataTables.MultiSummaryLiveStreams);
                NotifyPropertyChanged(nameof(MultiSummaryLiveStreams));
                MultiWebhooks = (List<MultiWebhooks>)DataManager.GetICollection(Enums.DataTables.MultiWebhooks);
                NotifyPropertyChanged(nameof(MultiWebhooks));
                OldFollowUsers = (List<OldFollowUsers>)DataManager.GetICollection(Enums.DataTables.OldFollowUsers);
                NotifyPropertyChanged(nameof(OldFollowUsers));
                OutRaidData = (List<OutRaidData>)DataManager.GetICollection(Enums.DataTables.OutRaidData);
                NotifyPropertyChanged(nameof(OutRaidData));
                OverlayServices = (List<OverlayServices>)DataManager.GetICollection(Enums.DataTables.OverlayServices);
                NotifyPropertyChanged(nameof(OverlayServices));
                OverlayTicker = (List<OverlayTicker>)DataManager.GetICollection(Enums.DataTables.OverlayTicker);
                NotifyPropertyChanged(nameof(OverlayTicker));
                Quotes = (List<Quotes>)DataManager.GetICollection(Enums.DataTables.Quotes);
                NotifyPropertyChanged(nameof(Quotes));
                ShoutOuts = (List<ShoutOuts>)DataManager.GetICollection(Enums.DataTables.ShoutOuts);
                NotifyPropertyChanged(nameof(ShoutOuts));
                StreamStats = (List<StreamStats>)DataManager.GetICollection(Enums.DataTables.StreamStats);
                NotifyPropertyChanged(nameof(StreamStats));
                UserStats = (List<UserStats>)DataManager.GetICollection(Enums.DataTables.UserStats);
                NotifyPropertyChanged(nameof(UserStats));
                Webhooks = (List<Webhooks>)DataManager.GetICollection(Enums.DataTables.Webhooks);
                NotifyPropertyChanged(nameof(Webhooks));

#endif

                CleanupList = DataManager.GetCleanupList();

                // refresh the 'status bar' count items
                NotifyPropertyChanged(nameof(CurrUserCount));
                NotifyPropertyChanged(nameof(CurrFollowers));
                NotifyPropertyChanged(nameof(CurrBuiltInComCount));
                NotifyPropertyChanged(nameof(CurrUserComsCount));

                SetCommandCollection();
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

            if (propertyName is "CategoryList")
            {
                CurrCategoryList = CategoryList;
            }
        }

        public void DataManager_OnDataCollectionUpdated(object sender, OnDataCollectionUpdatedEventArgs e)
        {
#if !USE_OBSERVABLECOLLECTION
            switch (e.DatabaseModelName)
            {
                case "Users":
                    Users.Clear();
                    Users.AddRange((List<Users>)e.TableData);
                    break;
                case "Followers":
                    Followers.Clear();
                    Followers.AddRange((List<Followers>)e.TableData);
                    break;
                case "CommandsUser":
                    CommandsUser.Clear();
                    CommandsUser.AddRange((List<CommandsUser>)e.TableData);
                    break;
                case "Commands":
                    Commands.Clear();
                    Commands.AddRange((List<Commands>)e.TableData);
                    break;
                case "BanReasons":
                    BanReasons.Clear();
                    BanReasons.AddRange((List<BanReasons>)e.TableData);
                    break;
                case "BanRules":
                    BanRules.Clear();
                    BanRules.AddRange((List<BanRules>)e.TableData);
                    break;
                case "CategoryList":
                    CategoryList.Clear();
                    CategoryList.AddRange((List<CategoryList>)e.TableData);
                    break;
                case "ChannelEvents":
                    ChannelEvents.Clear();
                    ChannelEvents.AddRange((List<ChannelEvents>)e.TableData);
                    break;
                case "Clips":
                    Clips.Clear();
                    Clips.AddRange((List<Clips>)e.TableData);
                    break;
                case "Currency":
                    Currency.Clear();
                    Currency.AddRange((List<Currency>)e.TableData);
                    break;
                case "CurrencyType":
                    CurrencyType.Clear();
                    CurrencyType.AddRange((List<CurrencyType>)e.TableData);
                    break;
                case "CustomWelcome":
                    CustomWelcome.Clear();
                    CustomWelcome.AddRange((List<CustomWelcome>)e.TableData);
                    break;
                case "GameDeadCounter":
                    GameDeadCounter.Clear();
                    GameDeadCounter.AddRange((List<GameDeadCounter>)e.TableData);
                    break;
                case "GiveawayUserData":
                    GiveawayUserData.Clear();
                    GiveawayUserData.AddRange((List<GiveawayUserData>)e.TableData);
                    break;
                case "InRaidData":
                    InRaidData.Clear();
                    InRaidData.AddRange((List<InRaidData>)e.TableData);
                    break;
                case "LearnMsgs":
                    LearnMsgs.Clear();
                    LearnMsgs.AddRange((List<LearnMsgs>)e.TableData);
                    break;
                case "ModeratorApprove":
                    ModeratorApprove.Clear();
                    ModeratorApprove.AddRange((List<ModeratorApprove>)e.TableData);
                    break;
                case "MultiChannels":
                    MultiChannels.Clear();
                    MultiChannels.AddRange((List<MultiChannels>)e.TableData);
                    break;
                case "MultiLiveStreams":
                    MultiLiveStreams.Clear();
                    MultiLiveStreams.AddRange((List<MultiLiveStreams>)e.TableData);
                    break;
                case "MultiSummaryLiveStreams":
                    MultiSummaryLiveStreams.Clear();
                    MultiSummaryLiveStreams.AddRange((List<MultiSummaryLiveStreams>)e.TableData);
                    break;
                case "MultiWebhooks":
                    MultiWebhooks.Clear();
                    MultiWebhooks.AddRange((List<MultiWebhooks>)e.TableData);
                    break;
                case "OldFollowUsers":
                    OldFollowUsers.Clear();
                    OldFollowUsers.AddRange((List<OldFollowUsers>)e.TableData);
                    break;
                case "OutRaidData":
                    OutRaidData.Clear();
                    OutRaidData.AddRange((List<OutRaidData>)e.TableData);
                    break;
                case "OverlayServices":
                    OverlayServices.Clear();
                    OverlayServices.AddRange((List<OverlayServices>)e.TableData);
                    break;
                case "OverlayTicker":
                    OverlayTicker.Clear();
                    OverlayTicker.AddRange((List<OverlayTicker>)e.TableData);
                    break;
                case "Quotes":
                    Quotes.Clear();
                    Quotes.AddRange((List<Quotes>)e.TableData);
                    break;
                case "ShoutOuts":
                    ShoutOuts.Clear();
                    ShoutOuts.AddRange((List<ShoutOuts>)e.TableData);
                    break;
                case "StreamStats":
                    StreamStats.Clear();
                    StreamStats.AddRange((List<StreamStats>)e.TableData);
                    break;
                case "UserStats":
                    UserStats.Clear();
                    UserStats.AddRange((List<UserStats>)e.TableData);
                    break;
                case "Webhooks":
                    Webhooks.Clear();
                    Webhooks.AddRange((List<Webhooks>)e.TableData);
                    break;
                case "MultiLiveStatusLog":
                    break;
                default:
                    break;
            }

#endif
            DataViewsUpdated?.Invoke(this, e);

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

        /// <summary>
        /// Handles when a data table view changes, to update the GUI and refresh the status bar item counts.
        /// </summary>
        /// <param name="sender">Object invoking the event.</param>
        /// <param name="e">The parameters sent with the event.</param>
        private void DataGrid_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
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

        /// <summary>
        /// Builds the list of commands to update the number of commands within the GUI
        /// </summary>
        private void SetCommandCollection()
        {
            ThreadManager.AddTaskToGUIDispatcher(() =>
            {
                CommandCollection.Clear();

                foreach (var command in DataManager.GetCommandList())
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
