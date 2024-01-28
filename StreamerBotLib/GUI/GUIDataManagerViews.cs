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
        // TODO: Add probable datamanagerview sorting options, per user input

        private SQLDBContext sqlDBContext;

        #region DataManager TableViews
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
                    return Followers.Count(f=>f.IsFollower);
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
        public ObservableCollection<Users> Users { get; private set; } 
        public ObservableCollection<Followers> Followers { get; private set; }
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
            var context = new SQLDBContext();

            ChannelEvents = new(context.ChannelEvents);
            Users = new(from U in context.Users orderby U.LastDateSeen descending select U);
            Followers = new(from F in context.Followers orderby F.StatusChangeDate descending, F.FollowedDate descending select F);
            WebHooks = new(context.Webhooks);
            CurrencyType = new(context.CurrencyType);
            Currency = new(context.Currency);
            BuiltInCommands = new(from C in context.Commands.IntersectBy(Enum.GetNames<DefaultCommand>(), f => f.CmdName) select C); 
            Commands = new(from C in context.Commands.ExceptBy(Enum.GetNames<DefaultCommand>(), f=>f.CmdName ) select C);
            StreamStats = new(from SS in context.StreamStats orderby SS.StreamStart descending select SS);
            ShoutOuts = new(context.ShoutOuts);
            CategoryList = new(context.CategoryList);
            Clips = new(context.Clips);
            InRaidData = new(from IR in context.InRaidData orderby IR.RaidDate descending select IR);
            OutRaidData = new(from OR in context.OutRaidData orderby OR.RaidDate descending select OR);
            GiveawayUserData = new(context.GiveawayUserData);
            CustomWelcomeData = new(context.CustomWelcome);
            LearnMsgs = new(context.LearnMsgs);
            BanRules = new(context.BanRules);
            BanReasons = new(context.BanReasons);
            OverlayService = new(context.OverlayServices);
            OverlayTicker = new(context.OverlayTicker);
            ModeratorApprove = new(context.ModeratorApprove);
            GameDeadCounter = new(context.GameDeadCounter);
            Quotes = new(context.Quotes);

            SetCommandCollection();
        }

        /// <summary>
        /// Handles when a data table view changes, to update the GUI and refresh the status bar item counts.
        /// </summary>
        /// <param name="sender">Object invoking the event.</param>
        /// <param name="e">The parameters sent with the event.</param>
        private void DataView_ListChanged(object sender, ListChangedEventArgs e)
        {
            lock (GUIDataManagerLock.Lock)
            {
                DataView dataView = (DataView)sender;
                NotifyPropertyChanged(nameof(dataView.Table));

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
