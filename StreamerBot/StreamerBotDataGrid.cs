using StreamerBotLib.BotIOController;
using StreamerBotLib.DataSQL.Models;
using StreamerBotLib.DataSQL.TableMeta;
using StreamerBotLib.Events;
using StreamerBotLib.GUI;
using StreamerBotLib.GUI.Windows;
using StreamerBotLib.Models;
using StreamerBotLib.Static;
using StreamerBotLib.Systems;

using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace StreamerBot
{
    public partial class StreamerBotWindow
    {
        #region DataGrid Columns and Editing
        private ManageWindows PopupWindows { get; set; } = new();

        private event EventHandler<OnDataCollectionUpdatedEventArgs> OnDataGridUpdated;

        private void DataManagerLoaded()
        {
            GUIDataManagerViews.DataViewsLoaded += GUIDataManagerViews_DataViewsLoaded;
            SystemsController.DataManage.OnDataCollectionUpdated += DataManager_OnDataCollectionUpdated;
        }

        private void GUIDataManagerViews_DataViewsLoaded(object sender, EventArgs e)
        {
            GUIDataManagerViews gUIDataManagerViews = Resources["DataViews"] as GUIDataManagerViews;
            OnDataGridUpdated += gUIDataManagerViews.DataManager_OnDataCollectionUpdated;
        }

        private void DataManager_OnDataCollectionUpdated(object sender, OnDataCollectionUpdatedEventArgs e)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                LogWriter.DebugLog("DataManager_OnDataCollectionUpdated",
                   StreamerBotLib.Enums.DebugLogTypes.GUIDataViews, $"Refreshing data for the {e.DatabaseModelName} data table.");
                switch (e.DatabaseModelName)
                {
                    case "BanReasons":
                        DG_Mod_BanReasons.Items.Refresh();
                        break;
                    case "BanRules":
                        DG_Mod_BanRules.Items.Refresh();
                        break;
                    case "CategoryList":
                        DG_CategoryList.Items.Refresh();
                        break;
                    case "ChannelEvents":
                        DG_CommonMsgs.Items.Refresh();
                        break;
                    case "Clips":
                        DG_Clips.Items.Refresh();
                        break;
                    case "Commands":
                        DG_BuiltInCommands.Items.Refresh();
                        break;
                    case "CommandsUser":
                        DG_UserDefinedCommands.Items.Refresh();
                        break;
                    case "Currency":
                        DG_Currency.Items.Refresh();
                        break;
                    case "CurrencyType":
                        DG_CurrencyType.Items.Refresh();
                        break;
                    case "CustomWelcome":
                        DG_CustomWelcome.Items.Refresh();
                        break;
                    case "Followers":
                        DG_Followers.Items.Refresh();
                        break;
                    case "GameDeadCounter":
                        DG_DeathCounter.Items.Refresh();
                        break;
                    case "GiveawayUserData":
                        DG_User_Giveaway.Items.Refresh();
                        break;
                    case "InRaidData":
                        DG_InRaids.Items.Refresh();
                        break;
                    case "LearnMsgs":
                        DG_Mod_LearnMsgs.Items.Refresh();
                        break;
                    case "ModeratorApprove":
                        DG_ModApprove.Items.Refresh();
                        break;
                    case "MultiChannels":

                        break;
                    case "MultiLiveStreams":

                        break;
                    case "MultiWebhooks":

                        break;
                    case "MultiSummaryLiveStreams":

                        break;
                    case "OutRaidData":
                        DG_OutRaids.Items.Refresh();
                        break;
                    case "OverlayServices":
                        DG_OverlayService_Actions.Items.Refresh();
                        break;
                    case "OverlayTicker":
                        DG_OverlayService_Ticker.Items.Refresh();
                        break;
                    case "Quotes":
                        DG_User_Quotes.Items.Refresh();
                        break;
                    case "ShoutOuts":
                        DG_User_Shoutouts.Items.Refresh();
                        break;
                    case "StreamStats":
                        DG_StreamData_Stats.Items.Refresh();
                        break;
                    case "Users" or "UserStats":
                        DG_Users.Items.Refresh();
                        break;
                    case "Webhooks":
                        DG_Webhooks.Items.Refresh();
                        break;
                    case null:
                        break;
                    default:
                        break;
                }

                OnDataGridUpdated?.Invoke(this, new(e.DatabaseModelName));
            }));
        }
        private void Button_ClearWatchTime_Click(object sender, RoutedEventArgs e)
        {
            BotController.ClearWatchTime();
        }
        private void Button_ClearCurrencyAccrlValues_Click(object sender, RoutedEventArgs e)
        {
            BotController.ClearAllCurrenciesValues();
        }
        private void Button_ClearNonFollowers_Click(object sender, RoutedEventArgs e)
        {
            BotController.ClearUsersNonFollowers();
        }
        private void DG_Edit_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (sender.GetType() == typeof(DataGrid))
            {
                bool FoundAddShout = ((DataGrid)sender).Name is "DG_Users" or "DG_Followers";
                bool FoundIsEnabled = ((DataGrid)sender).Columns.Select((c) => (string)c.Header == "IsEnabled").Any();
                bool FoundAddItem = ((DataGrid)sender).Name is
                       "DG_CurrencyType"
                    or "DG_CustomWelcome"
                    or "DG_InRaids"
                    or "DG_Mod_LearnMsgs"
                    or "DG_ModApprove"
                    or "DG_OutRaids"
                    or "DG_OverlayService_Actions"
                    or "DG_User_Quotes"
                    or "DG_User_Shoutouts"
                    or "DG_UserDefinedCommands"
                    or "DG_Webhooks";

                foreach (var M in ((ContextMenu)Resources["DataGrid_ContextMenu"]).Items)
                {
                    if (M.GetType() == typeof(MenuItem))
                    {
                        if (((MenuItem)M).Name is "DataGridContextMenu_AutoShout" or "DataGridContextMenu_LiveMonitor")
                        {
                            ((MenuItem)M).Visibility = FoundAddShout ? Visibility.Visible : Visibility.Collapsed;
                        }
                        else if (((MenuItem)M).Name is "DataGridContextMenu_EnableItems" or "DataGridContextMenu_DisableItems")
                        {
                            ((MenuItem)M).IsEnabled = FoundIsEnabled;
                        }
                        else if (((MenuItem)M).Name is "DataGridContextMenu_AddItem")
                        {
                            ((MenuItem)M).Visibility = FoundAddItem ? Visibility.Visible : Visibility.Collapsed;
                        }
                        else if (((MenuItem)M).Name is "DataGridContextMenu_DeleteItems")
                        {
                            ((MenuItem)M).Visibility = ((DataGrid)sender).CanUserDeleteRows ? Visibility.Visible : Visibility.Collapsed;
                        }
                    }
                    else if (M.GetType() == typeof(Separator))
                    {
                        if (((Separator)M).Name == "DataGridContextMenu_Separator1")
                        {
                            ((Separator)M).Visibility = FoundAddShout ? Visibility.Visible : Visibility.Collapsed;
                        }
                    }
                }
            }
        }
        private void MenuItem_AddClick(object sender, RoutedEventArgs e)
        {
            DataGrid item = (((sender as MenuItem).Parent as ContextMenu).Parent as Popup).PlacementTarget as DataGrid;


            DGOpenEditWindow(item);
        }
        private void DGOpenEditWindow(DataGrid item)
        {
            Type SqlModel = item.Name switch
            {
                nameof(DG_BuiltInCommands) => typeof(Commands),
                nameof(DG_CategoryList) => typeof(CategoryList),
                nameof(DG_Clips) => typeof(Clips),
                nameof(DG_CurrencyType) => typeof(CurrencyType),
                nameof(DG_Currency) => typeof(Currency),
                nameof(DG_CustomWelcome) => typeof(CustomWelcome),
                nameof(DG_DeathCounter) => typeof(GameDeadCounter),
                nameof(DG_Followers) => typeof(Followers),
                nameof(DG_InRaids) => typeof(InRaidData),
                nameof(DG_ModApprove) => typeof(ModeratorApprove),
                nameof(DG_Mod_BanReasons) => typeof(BanReasons),
                nameof(DG_Mod_BanRules) => typeof(BanRules),
                nameof(DG_Mod_LearnMsgs) => typeof(LearnedMessage),
                nameof(DG_OldFollowUsers) => typeof(OldFollowUsers),
                nameof(DG_OutRaids) => typeof(OutRaidData),
                nameof(DG_OverlayService_Actions) => typeof(OverlayServices),
                nameof(DG_OverlayService_Ticker) => typeof(OverlayTicker),
                nameof(DG_StreamData_Stats) => typeof(StreamStats),
                nameof(DG_UserDefinedCommands) => typeof(CommandsUser),
                nameof(DG_Users) => typeof(Users),
                nameof(DG_User_Giveaway) => typeof(GiveawayUserData),
                nameof(DG_User_Quotes) => typeof(Quotes),
                nameof(DG_User_Shoutouts) => typeof(ShoutOuts),
                nameof(DG_Webhooks) => typeof(Webhooks),
                _ => typeof(object)
            };

            TableMeta tableMeta = new();

            if (item.Name is "DG_OverlayService_Actions" or "DG_ModApprove")
            {
                PopupWindows.SetTableData(Controller.Systems.GetOverlayActions());
            }

            PopupWindows.AddNewItem(tableMeta.SetNewEntity(SqlModel));
        }
        private void MenuItem_EditClick(object sender, RoutedEventArgs e)
        {
            DataGrid item = (((sender as MenuItem).Parent as ContextMenu).Parent as Popup).PlacementTarget as DataGrid;
            DGOpenEditWindow(item);
        }
        private void MenuItem_DeleteClick(object sender, RoutedEventArgs e)
        {
            DataGrid item = (((sender as MenuItem).Parent as ContextMenu).Parent as Popup).PlacementTarget as DataGrid;

            foreach (object R in item.SelectedItems)
            {
                // TODO: fix menu delete click - item.ItemsSource.
            }
        }
        private void MenuItem_AutoShoutClick(object sender, RoutedEventArgs e)
        {
            DataGrid item = (((sender as MenuItem).Parent as ContextMenu).Parent as Popup).PlacementTarget as DataGrid;

            foreach (UserBase dr in new List<UserBase>(item.SelectedItems.Cast<UserBase>().Select(DRV => DRV)))
            {
                BotController.AddNewAutoShoutUser(dr.UserId, dr.Platform);
            }
        }
        private void MenuItem_LiveMonitorClick(object sender, RoutedEventArgs e)
        {
            DataGrid item = (((sender as MenuItem).Parent as ContextMenu).Parent as Popup).PlacementTarget as DataGrid;
            Controller.Systems.AddNewMonitorChannel(new List<LiveUser>(item.SelectedItems.Cast<Users>().Select(DRV => new LiveUser(DRV.UserName, DRV.Platform, DRV.UserId))));
        }
        private void DataGridContextMenu_EnableItems_Click(object sender, RoutedEventArgs e)
        {
            DataGrid item = (((sender as MenuItem).Parent as ContextMenu).Parent as Popup).PlacementTarget as DataGrid;

            SystemsController.UpdateIsEnabledRows(new List<DataRow>(item.SelectedItems.Cast<DataRowView>().Select(DRV => DRV.Row)), true);
        }
        private void DataGridContextMenu_DisableItems_Click(object sender, RoutedEventArgs e)
        {
            DataGrid item = (((sender as MenuItem).Parent as ContextMenu).Parent as Popup).PlacementTarget as DataGrid;

            SystemsController.UpdateIsEnabledRows(new List<DataRow>(item.SelectedItems.Cast<DataRowView>().Select(DRV => DRV.Row)), false);
        }

        #endregion

        #region DataGrid Item editing

        private void DataGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                Controller.Systems.GUISaveDataGridEdits((sender as DataGrid).Name is "DG_BuiltInCommands" or "DG_UserDefinedCommands");
            }
        }

        #endregion
    }
}
