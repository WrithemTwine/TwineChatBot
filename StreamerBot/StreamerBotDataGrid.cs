using StreamerBotLib.BotIOController;
using StreamerBotLib.DataSQL.Models;
using StreamerBotLib.DataSQL.TableMeta;
using StreamerBotLib.Events;
using StreamerBotLib.GUI.Windows;
using StreamerBotLib.Models;
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

        private void DataManagerLoaded()
        {
            SystemsController.DataManage.OnDataCollectionUpdated += DataManager_OnDataCollectionUpdated;
        }

        private void DataManager_OnDataCollectionUpdated(object sender, OnDataCollectionUpdatedEventArgs e)
        {
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
                    DG_CurrencyAccrual.Items.Refresh();
                    break;
                case "CurrencyType":
                    DG_Currency.Items.Refresh();
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
                case "MultiMsgEndPoints":

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
            }
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
                bool FoundIsEnabled = SystemsController.CheckField(((DataGrid)sender).ItemsSource.GetType().BaseType.GetGenericArguments()[0].Name, "IsEnabled");

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
                    }
                    else if (M.GetType() == typeof(Separator))
                    {
                        if (((Separator)M).Name == "DataGridContextMenu_Separator")
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

            Type CurrDGModel = item.Items.SourceCollection.GetType().GetGenericArguments()[0];

            TableMeta tableMeta = new();
            tableMeta.SetNewEntity(CurrDGModel);

            PopupWindows.AddNewItem(tableMeta, item.Items);
        }

        private void MenuItem_EditClick(object sender, RoutedEventArgs e)
        {
            DataGrid item = (((sender as MenuItem).Parent as ContextMenu).Parent as Popup).PlacementTarget as DataGrid;

            TableMeta tableMeta = new();
            tableMeta.SetExistingEntity(item.SelectedItem);

            PopupWindows.EditExistingItem(tableMeta);
        }

        private void MenuItem_DeleteClick(object sender, RoutedEventArgs e)
        {
            DataGrid item = (((sender as MenuItem).Parent as ContextMenu).Parent as Popup).PlacementTarget as DataGrid;

            foreach (object R in item.SelectedItems)
            {
                item.Items.Remove(R);
            }
        }

        private void MenuItem_AutoShoutClick(object sender, RoutedEventArgs e)
        {
            DataGrid item = (((sender as MenuItem).Parent as ContextMenu).Parent as Popup).PlacementTarget as DataGrid;

            foreach (UserBase dr in new List<UserBase>(item.SelectedItems.Cast<UserBase>().Select(DRV => DRV)))
            {
                BotController.AddNewAutoShoutUser(dr.UserName, dr.UserId, dr.Platform);
            }
        }

        private void MenuItem_LiveMonitorClick(object sender, RoutedEventArgs e)
        {
            DataGrid item = (((sender as MenuItem).Parent as ContextMenu).Parent as Popup).PlacementTarget as DataGrid;
            Controller.Systems.AddNewMonitorChannel(new List<LiveUser>(item.SelectedItems.Cast<UserBase>().Select(DRV => new LiveUser(DRV.UserName, DRV.Platform, DRV.UserId))));
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

    }
}
