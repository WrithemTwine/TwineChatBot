using Microsoft.EntityFrameworkCore;

using StreamerBotLib.DataSQL;
using StreamerBotLib.DataSQL.Models;
using StreamerBotLib.Enums;
using StreamerBotLib.Interfaces;
using StreamerBotLib.Models;
using StreamerBotLib.Systems;

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Runtime.CompilerServices;
using System.Windows.Documents;

namespace StreamerBotLib.GUI
{
    public class GUIDataManagerViews : INotifyPropertyChanged
    {
        #region DataManager TableViews
        private readonly SQLDBContext context = new DataManagerFactory().CreateDbContext();

        private IDataManager DataManager { get; set; }

        // datatable views to display the data in the GUI

        public List<string> KindsWebhooks { get; private set; } = new(Enum.GetNames(typeof(WebhooksKind)));

        public FlowDocument ChatData { get; private set; }
        public ObservableCollection<string> CurrUserList { get; private set; }

        public ObservableCollection<UserJoin> JoinCollection { get; set; }
        public ObservableCollection<string> CommandCollection { get; set; } = [];
        public ObservableCollection<string> GiveawayCollection { get; set; }
        public int CurrFollowers
        {
            get
            {
                lock (GUIDataManagerLock.Lock)
                {
                    return Followers.Count(f => f.IsFollower);
                }
            }
        }

        public int CurrUserCount
        {
            get
            {
                lock (GUIDataManagerLock.Lock)
                {
                    return Users.Count;
                }
            }
        }

        public int CurrBuiltInComCount
        {
            get
            {
                lock (GUIDataManagerLock.Lock)
                {
                    return BuiltInCommands.Count;
                }
            }
        }

        public int CurrUserComsCount
        {
            get
            {
                lock (GUIDataManagerLock.Lock)
                {
                    return Commands.Count;
                }
            }
        }

        public ObservableCollection<ChannelEvents> ChannelEvents { get; private set; }
        public ObservableCollection<Users> Users => context.Users.Local.ToObservableCollection();
        public ObservableCollection<Followers> Followers => context.Followers.Local.ToObservableCollection();
        public ObservableCollection<Webhooks> WebHooks { get; private set; }
        public ObservableCollection<Currency> Currency { get; private set; }
        public ObservableCollection<DataSQL.Models.CurrencyType> CurrencyType { get; private set; }
        public ObservableCollection<Commands> BuiltInCommands { get; private set; }
        public ObservableCollection<Commands> Commands { get; private set; }
        public ObservableCollection<StreamStats> StreamStats { get; private set; }
        public ObservableCollection<ShoutOuts> ShoutOuts { get; private set; }
        public ObservableCollection<CategoryList> CategoryList { get; private set; }
        public ObservableCollection<Clips> Clips { get; private set; }
        public ObservableCollection<InRaidData> InRaidData { get; private set; }
        public ObservableCollection<OutRaidData> OutRaidData { get; private set; }
        public ObservableCollection<GiveawayUserData> GiveawayUserData { get; private set; }
        public ObservableCollection<CustomWelcome> CustomWelcomeData { get; private set; }
        public ObservableCollection<LearnMsgs> LearnMsgs { get; private set; }
        public ObservableCollection<BanRules> BanRules { get; private set; }
        public ObservableCollection<DataSQL.Models.BanReasons> BanReasons { get; private set; }
        public ObservableCollection<OverlayServices> OverlayService { get; private set; }
        public ObservableCollection<OverlayTicker> OverlayTicker { get; private set; }
        public ObservableCollection<ModeratorApprove> ModeratorApprove { get; private set; }
        public ObservableCollection<GameDeadCounter> GameDeadCounter { get; private set; }
        public ObservableCollection<Quotes> Quotes { get; private set; }

        #region MultiLive Collections
        public ObservableCollection<MultiMsgEndPoints> MultiMsgEndPoints { get; private set; }
        public ObservableCollection<MultiChannels> MultiChannels { get; private set; }
        public ObservableCollection<MultiLiveStream> MultiLiveStreams { get; private set; }
        public ObservableCollection<MultiSummaryLiveStream> MultiSummaryLiveStream { get; private set; }

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
            SetDataTableViews();
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
        /// Used in class object construction to build assign the DataTable views for the GUI, requires <c>SystemsController</c> to be initialized.
        /// </summary>
        private void SetDataTableViews()
        {
            context.ChannelEvents.Load();
            context.Webhooks.Load();
            context.CurrencyType.Load();
            context.Currency.Load();
            context.ShoutOuts.Load();
            context.CategoryList.Load();
            context.Clips.Load();
            context.GiveawayUserData.Load();
            context.CustomWelcome.Load();
            context.LearnMsgs.Load();
            context.BanReasons.Load();
            context.BanRules.Load();
            context.OverlayServices.Load();
            context.OverlayTicker.Load();
            context.ModeratorApprove.Load();
            context.GameDeadCounter.Load();
            context.Quotes.Load();
            context.MultiChannels.Load();
            context.MultiLiveStreams.Load();
            context.MultiMsgEndPoints.Load();
            context.MultiSummaryLiveStreams.Load();
            context.Users.OrderBy(x => x.LastDateSeen).Load();
            context.Followers.OrderByDescending((x) => x.StatusChangeDate).OrderByDescending((f) => f.FollowedDate).Load();
            context.StreamStats.OrderByDescending((s) => s.StreamStart).Load();
            context.InRaidData.OrderByDescending((r) => r.RaidDate).Load();
            context.OutRaidData.OrderByDescending((r) => r.RaidDate).Load();

            ChannelEvents = context.ChannelEvents.Local.ToObservableCollection();
            WebHooks = context.Webhooks.Local.ToObservableCollection();
            CurrencyType = context.CurrencyType.Local.ToObservableCollection();
            Currency = context.Currency.Local.ToObservableCollection();
            ShoutOuts = context.ShoutOuts.Local.ToObservableCollection();
            CategoryList = context.CategoryList.Local.ToObservableCollection();
            Clips = context.Clips.Local.ToObservableCollection();
            GiveawayUserData = context.GiveawayUserData.Local.ToObservableCollection();
            CustomWelcomeData = context.CustomWelcome.Local.ToObservableCollection();
            LearnMsgs = context.LearnMsgs.Local.ToObservableCollection();
            BanRules = context.BanRules.Local.ToObservableCollection();
            BanReasons = context.BanReasons.Local.ToObservableCollection();
            OverlayService = context.OverlayServices.Local.ToObservableCollection();
            OverlayTicker = context.OverlayTicker.Local.ToObservableCollection();
            ModeratorApprove = context.ModeratorApprove.Local.ToObservableCollection();
            GameDeadCounter = context.GameDeadCounter.Local.ToObservableCollection();
            Quotes = context.Quotes.Local.ToObservableCollection();
            MultiMsgEndPoints = context.MultiMsgEndPoints.Local.ToObservableCollection();
            MultiChannels = context.MultiChannels.Local.ToObservableCollection();
            MultiLiveStreams = context.MultiLiveStreams.Local.ToObservableCollection();
            MultiSummaryLiveStream = context.MultiSummaryLiveStreams.Local.ToObservableCollection();
            StreamStats = context.StreamStats.Local.ToObservableCollection();
            InRaidData = context.InRaidData.Local.ToObservableCollection();
            OutRaidData = context.OutRaidData.Local.ToObservableCollection();

            Commands = new(
                (from C in context.Commands
                 join D in (from def in Enum.GetNames<DefaultCommand>().Union(Enum.GetNames<DefaultSocials>().ToList())
                            select def) on C.CmdName equals D into UsrCmds
                 from UC in UsrCmds.DefaultIfEmpty()
                 where UC == null
                 orderby C.CmdName
                 select C
                ).AsTracking());

            BuiltInCommands = new(
                (from C in context.Commands
                 join D in
                     (from def in Enum.GetNames<DefaultCommand>().Union(Enum.GetNames<DefaultSocials>().ToList())
                      select def) on C.CmdName equals D into DefCmds
                 from DC in DefCmds.DefaultIfEmpty()
                 where DC != null
                 orderby C.CmdName
                 select C
                ).AsTracking());

            Users.CollectionChanged += DataGrid_CollectionChanged;
            Followers.CollectionChanged += DataGrid_CollectionChanged;
            BuiltInCommands.CollectionChanged += DataGrid_CollectionChanged;
            Commands.CollectionChanged += DataGrid_CollectionChanged;

            SetCommandCollection();
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
