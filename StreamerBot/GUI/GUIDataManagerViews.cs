using StreamerBot.Enums;
using StreamerBot.Systems;
using StreamerBot.Data;

using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Runtime.CompilerServices;
using System.Windows.Documents;
using StreamerBot.Models;
using System.Collections.ObjectModel;
using System.Linq;

namespace StreamerBot.GUI
{
    public class GUIDataManagerViews : INotifyPropertyChanged
    {
        #region DataManager TableViews
        // datatable views to display the data in the GUI

        public List<string> KindsWebhooks { get; private set; } = new(System.Enum.GetNames(typeof(WebhooksKind)));

        public FlowDocument ChatData { get; private set; }

        public ObservableCollection<UserJoin> JoinCollection { get; set; }
        public ObservableCollection<string> CommandCollection { get; set; } = new();

        public DataView ChannelEvents { get; private set; } // DataSource.ChannelEventsDataTable
        public DataView Users { get; private set; }  // DataSource.UsersDataTable
        public DataView Followers { get; private set; } // DataSource.FollowersDataTable
        public DataView CurrFollowers { get; private set; }
        public DataView Discord { get; private set; } // DataSource.DiscordDataTable
        public DataView Currency { get; private set; }  // DataSource.CurrencyDataTable
        public DataView CurrencyType { get; private set; }  // DataSource.CurrencyTypeDataTable
        public DataView BuiltInCommands { get; private set; } // DataSource.CommandsDataTable
        public DataView Commands { get; private set; }  // DataSource.CommandsDataTable
        public DataView StreamStats { get; private set; } // DataSource.StreamStatsTable
        public DataView ShoutOuts { get; private set; } // DataSource.ShoutOutsTable
        public DataView Category { get; private set; } // DataSource.CategoryTable
        public DataView Clips { get; private set; }  // DataSource.ClipsDataTable
        public DataView InRaidData { get; private set; } // DataSource.InRaidDataDataTable
        public DataView OutRaidData { get; private set; } // DataSource.OutRaidDataDataTable
        public DataView GiveawayUserData { get; private set; } // DataSource.GiveawayUserDataDataTable

        /// <summary>
        /// provide delegate to method for saving the database data
        /// </summary>
        private delegate void SaveTableDataDelegate();
        private SaveTableDataDelegate SaveTableData;

        #endregion

        public GUIDataManagerViews()
        {
            ChatData = SystemsBase.ChatData;
            JoinCollection = SystemsBase.JoinCollection;
            SetDataTableViews(SystemsController.DataManage);
            //SaveTableData = SystemsController.DataManage.NotifySaveData; // the database save data method
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Used in class object construction to build assign the DataTable views for the GUI, requires <c>SystemsController</c> to be initialized.
        /// </summary>
        private void SetDataTableViews(DataManager dataManager)
        {
            static string ComFilter()
            {
                string filter = string.Empty;

                foreach (DefaultCommand d in System.Enum.GetValues(typeof(DefaultCommand)))
                {
                    filter += "'" + d.ToString() + "',";
                }

                foreach (DefaultSocials s in System.Enum.GetValues(typeof(DefaultSocials)))
                {
                    filter += "'" + s.ToString() + "',";
                }

                return filter == string.Empty ? "" : filter[0..^1];
            }

            ChannelEvents = dataManager._DataSource.ChannelEvents.DefaultView;
            Users = new(dataManager._DataSource.Users, null, "UserName", DataViewRowState.CurrentRows);
            Followers = new(dataManager._DataSource.Followers, null, "FollowedDate", DataViewRowState.CurrentRows);
            CurrFollowers = new(dataManager._DataSource.Followers, "IsFollower=True", "FollowedDate", DataViewRowState.CurrentRows);
            Discord = dataManager._DataSource.Discord.DefaultView;
            CurrencyType = new(dataManager._DataSource.CurrencyType, null, "CurrencyName", DataViewRowState.CurrentRows);
            Currency = new(dataManager._DataSource.Currency, null, "UserName", DataViewRowState.CurrentRows);
            BuiltInCommands = new(dataManager._DataSource.Commands, "CmdName IN (" + ComFilter() + ")", "CmdName", DataViewRowState.CurrentRows);
            Commands = new(dataManager._DataSource.Commands, "CmdName NOT IN (" + ComFilter() + ")", "CmdName", DataViewRowState.CurrentRows);
            StreamStats = new(dataManager._DataSource.StreamStats, null, "StreamStart", DataViewRowState.CurrentRows);
            ShoutOuts = new(dataManager._DataSource.ShoutOuts, null, "UserName", DataViewRowState.CurrentRows);
            Category = new(dataManager._DataSource.CategoryList, null, "Id", DataViewRowState.CurrentRows);
            Clips = new(dataManager._DataSource.Clips, null, "Id", DataViewRowState.CurrentRows);
            InRaidData = new(dataManager._DataSource.InRaidData, null, "Id", DataViewRowState.CurrentRows);
            OutRaidData = new(dataManager._DataSource.OutRaidData, null, "Id", DataViewRowState.CurrentRows);
            GiveawayUserData = new(dataManager._DataSource.GiveawayUserData, null, "Id", DataViewRowState.CurrentRows);

            ChannelEvents.ListChanged += DataView_ListChanged;
            Users.ListChanged += DataView_ListChanged;
            Followers.ListChanged += DataView_ListChanged;
            CurrFollowers.ListChanged += DataView_ListChanged;
            Discord.ListChanged += DataView_ListChanged;
            CurrencyType.ListChanged += DataView_ListChanged;
            Currency.ListChanged += DataView_ListChanged;
            BuiltInCommands.ListChanged += DataView_ListChanged;
            Commands.ListChanged += DataView_ListChanged;
            StreamStats.ListChanged += DataView_ListChanged;
            ShoutOuts.ListChanged += DataView_ListChanged;
            Category.ListChanged += DataView_ListChanged;
            Clips.ListChanged += DataView_ListChanged;
            InRaidData.ListChanged += DataView_ListChanged;
            OutRaidData.ListChanged += DataView_ListChanged;
            GiveawayUserData.ListChanged += DataView_ListChanged;

            SetCommandCollection();
        }

        private void DataView_ListChanged(object sender, ListChangedEventArgs e)
        {
            NotifyPropertyChanged(nameof(sender));

            // refresh the 'status bar' count items
            NotifyPropertyChanged(nameof(Users));
            NotifyPropertyChanged(nameof(CurrFollowers));
            NotifyPropertyChanged(nameof(BuiltInCommands));
            NotifyPropertyChanged(nameof(Commands));

            if (sender == Commands)
            {
                SetCommandCollection();
            }
            //SaveTableData();
        }

        private void SetCommandCollection()
        {
            foreach (DataSource.CommandsRow c in from DataSource.CommandsRow c in Commands.Table.Select()
                              where !CommandCollection.Contains(c.CmdName)
                              orderby c.CmdName
                              select c)
            {
                CommandCollection.Add(c.CmdName);
            }
        }
    }
}
