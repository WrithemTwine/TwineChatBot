﻿
using StreamerBotLib.Data;
using StreamerBotLib.Enums;
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

        #region DataManager TableViews

        // datatable views to display the data in the GUI

        public List<string> KindsWebhooks { get; private set; } = new(System.Enum.GetNames(typeof(WebhooksKind)));

        public FlowDocument ChatData { get; private set; }
        public ObservableCollection<string> CurrUserList { get; private set; }

        public ObservableCollection<UserJoin> JoinCollection { get; set; }
        public ObservableCollection<string> CommandCollection { get; set; } = [];
        public ObservableCollection<string> GiveawayCollection { get; set; }
        public int CurrFollowers
        {
            get
            {
                return Followers.Table.Select("IsFollower=true").Length;
            }
        }

        public int CurrUserCount
        {
            get
            {
                return Users.Count;
            }
        }

        public int CurrBuiltInComCount
        {
            get
            {
                return BuiltInCommands.Count;
            }
        }

        public int CurrUserComsCount
        {
            get
            {
                return Commands.Count;
            }
        }

        public DataView ChannelEvents { get; private set; } // DataSource.ChannelEventsDataTable
        public DataView Users { get; private set; }  // DataSource.UsersDataTable
        public DataView Followers { get; private set; } // DataSource.FollowersDataTable
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
        public DataView CustomWelcomeData { get; private set; } // DataSource.CustomWelcomeDataTable
        public DataView LearnMsgs { get; private set; }
        public DataView BanRules { get; private set; }
        public DataView BanReasons { get; private set; }
        public DataView OverlayService { get; private set; }
        public DataView OverlayTicker { get; private set; }
        public DataView ModeratorApprove { get; private set; }
        public DataView GameDeadCounter { get; private set; }

        public DataView Quotes { get; private set; }

        #endregion

        public GUIDataManagerViews()
        {
            ChatData = ActionSystem.ChatData;
            JoinCollection = ActionSystem.JoinCollection;
            GiveawayCollection = ActionSystem.GiveawayCollection;
            CurrUserList = ActionSystem.CurrUserJoin;
            SetDataTableViews((DataManager)SystemsController.DataManage);
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
            Users = new(dataManager._DataSource.Users, null, $"{dataManager._DataSource.Users.LastDateSeenColumn.ColumnName} DESC", DataViewRowState.CurrentRows);
            Followers = new(dataManager._DataSource.Followers, null, $"{dataManager._DataSource.Followers.FollowedDateColumn.ColumnName} DESC", DataViewRowState.CurrentRows);
            Discord = dataManager._DataSource.Discord.DefaultView;
            CurrencyType = dataManager._DataSource.CurrencyType.DefaultView;
            Currency = dataManager._DataSource.Currency.DefaultView;
            BuiltInCommands = new(dataManager._DataSource.Commands, $"{dataManager._DataSource.Commands.CmdNameColumn.ColumnName} IN (" + ComFilter() + ")", dataManager._DataSource.Commands.CmdNameColumn.ColumnName, DataViewRowState.CurrentRows);
            Commands = new(dataManager._DataSource.Commands, $"{dataManager._DataSource.Commands.CmdNameColumn.ColumnName} NOT IN (" + ComFilter() + ")", dataManager._DataSource.Commands.CmdNameColumn.ColumnName, DataViewRowState.CurrentRows);
            StreamStats = new(dataManager._DataSource.StreamStats, null, $"{dataManager._DataSource.StreamStats.StreamStartColumn.ColumnName} DESC", DataViewRowState.CurrentRows);
            ShoutOuts = dataManager._DataSource.ShoutOuts.DefaultView;
            Category = dataManager._DataSource.CategoryList.DefaultView;
            Clips = dataManager._DataSource.Clips.DefaultView;
            InRaidData = new(dataManager._DataSource.InRaidData, null, $"{dataManager._DataSource.InRaidData.DateTimeColumn.ColumnName} DESC", DataViewRowState.CurrentRows);
            OutRaidData = new(dataManager._DataSource.OutRaidData, null, $"{dataManager._DataSource.OutRaidData.DateTimeColumn.ColumnName} DESC", DataViewRowState.CurrentRows);
            GiveawayUserData = dataManager._DataSource.GiveawayUserData.DefaultView;
            CustomWelcomeData = dataManager._DataSource.CustomWelcome.DefaultView;
            LearnMsgs = dataManager._DataSource.LearnMsgs.DefaultView;
            BanRules = dataManager._DataSource.BanRules.DefaultView;
            BanReasons = dataManager._DataSource.BanReasons.DefaultView;
            OverlayService = dataManager._DataSource.OverlayServices.DefaultView;
            OverlayTicker = dataManager._DataSource.OverlayTicker.DefaultView;
            ModeratorApprove = dataManager._DataSource.ModeratorApprove.DefaultView;
            GameDeadCounter = dataManager._DataSource.GameDeadCounter.DefaultView;
            Quotes = dataManager._DataSource.Quotes.DefaultView;

            /**/
            ChannelEvents.ListChanged += DataView_ListChanged;
            Users.ListChanged += DataView_ListChanged;
            Followers.ListChanged += DataView_ListChanged;
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
            CustomWelcomeData.ListChanged += DataView_ListChanged;
            LearnMsgs.ListChanged += DataView_ListChanged;
            BanRules.ListChanged += DataView_ListChanged;
            BanReasons.ListChanged += DataView_ListChanged;
            OverlayService.ListChanged += DataView_ListChanged;
            OverlayTicker.ListChanged += DataView_ListChanged;
            ModeratorApprove.ListChanged += DataView_ListChanged;
            GameDeadCounter.ListChanged += DataView_ListChanged;
            Quotes.ListChanged += DataView_ListChanged;
            /**/

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
}
