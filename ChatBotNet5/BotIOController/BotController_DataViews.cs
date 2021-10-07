using ChatBot_Net5.Data;
using ChatBot_Net5.Enum;
using ChatBot_Net5.Systems;

using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Reflection;

namespace ChatBot_Net5.BotIOController
{
    partial class BotController
    {
        #region DataManager TableViews
        // datatable views to display the data in the GUI

        public List<string> KindsWebhooks { get; private set; } = new(System.Enum.GetNames(typeof(WebhooksKind)));

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
        public DataView RaidData { get; private set; } // DataSource.RaidDataDataTable

        #endregion

        /// <summary>
        /// Used in class object construction to build assign the DataTable views for the GUI, requires <c>SystemsController</c> to be initialized.
        /// </summary>
        private void SetDataTableViews()
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

            DataManager dataManager = SystemsController.DataManage;

            ChannelEvents = dataManager._DataSource.ChannelEvents.DefaultView;
            Users = new(dataManager._DataSource.Users, null, "UserName", DataViewRowState.CurrentRows);
            Followers = new(dataManager._DataSource.Followers, null, "FollowedDate", DataViewRowState.CurrentRows);
            Discord = dataManager._DataSource.Discord.DefaultView;
            CurrencyType = new(dataManager._DataSource.CurrencyType, null, "CurrencyName", DataViewRowState.CurrentRows);
            Currency = new(dataManager._DataSource.Currency, null, "UserName", DataViewRowState.CurrentRows);
            BuiltInCommands = new(dataManager._DataSource.Commands, "CmdName IN (" + ComFilter() + ")", "CmdName", DataViewRowState.CurrentRows);
            Commands = new(dataManager._DataSource.Commands, "CmdName NOT IN (" + ComFilter() + ")", "CmdName", DataViewRowState.CurrentRows);
            StreamStats = new(dataManager._DataSource.StreamStats, null, "StreamStart", DataViewRowState.CurrentRows);
            ShoutOuts = new(dataManager._DataSource.ShoutOuts, null, "UserName", DataViewRowState.CurrentRows);
            Category = new(dataManager._DataSource.CategoryList, null, "Id", DataViewRowState.CurrentRows);
            Clips = new(dataManager._DataSource.Clips, null, "Id", DataViewRowState.CurrentRows);
            RaidData = new(dataManager._DataSource.RaidData, null, "Id", DataViewRowState.CurrentRows);

            ChannelEvents.ListChanged += ChannelEvents_ListChanged;
            Users.ListChanged += Users_ListChanged;
            Followers.ListChanged += Followers_ListChanged;
            Discord.ListChanged += Discord_ListChanged;
            CurrencyType.ListChanged += CurrencyType_ListChanged;
            Currency.ListChanged += Currency_ListChanged;
            BuiltInCommands.ListChanged += BuiltInCommands_ListChanged;
            Commands.ListChanged += Commands_ListChanged;
            StreamStats.ListChanged += StreamStats_ListChanged;
            ShoutOuts.ListChanged += ShoutOuts_ListChanged;
            Category.ListChanged += Category_ListChanged;
            Clips.ListChanged += Clips_ListChanged;
            RaidData.ListChanged += RaidData_ListChanged;
        }

        private void RaidData_ListChanged(object sender, ListChangedEventArgs e)
        {
            NotifyPropertyChanged(nameof(RaidData));
        }
        private void Clips_ListChanged(object sender, ListChangedEventArgs e)
        {
            NotifyPropertyChanged(nameof(Clips));
        }
        private void Category_ListChanged(object sender, ListChangedEventArgs e)
        {
            NotifyPropertyChanged(nameof(Category));
        }
        private void ShoutOuts_ListChanged(object sender, ListChangedEventArgs e)
        {
            NotifyPropertyChanged(nameof(ShoutOuts));
        }
        private void StreamStats_ListChanged(object sender, ListChangedEventArgs e)
        {
            NotifyPropertyChanged(nameof(StreamStats));
        }
        private void Commands_ListChanged(object sender, ListChangedEventArgs e)
        {
            NotifyPropertyChanged(nameof(Commands));
        }
        private void BuiltInCommands_ListChanged(object sender, ListChangedEventArgs e)
        {
            NotifyPropertyChanged(nameof(BuiltInCommands));
        }
        private void Currency_ListChanged(object sender, ListChangedEventArgs e)
        {
            NotifyPropertyChanged(nameof(Currency));
        }
        private void CurrencyType_ListChanged(object sender, ListChangedEventArgs e)
        {
            NotifyPropertyChanged(nameof(CurrencyType));
        }
        private void Discord_ListChanged(object sender, ListChangedEventArgs e)
        {
            NotifyPropertyChanged(nameof(Currency));
        }
        private void Followers_ListChanged(object sender, ListChangedEventArgs e)
        {
            NotifyPropertyChanged(nameof(Followers));
        }
        private void Users_ListChanged(object sender, ListChangedEventArgs e)
        {
            NotifyPropertyChanged(nameof(Users));
        }
        private void ChannelEvents_ListChanged(object sender, ListChangedEventArgs e)
        {
            NotifyPropertyChanged(nameof(ChannelEvents));
        }
    }
}
