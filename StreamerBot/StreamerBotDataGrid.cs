using StreamerBotLib.BotIOController;
using StreamerBotLib.DataSQL.Models;
using StreamerBotLib.DataSQL.TableMeta;
using StreamerBotLib.GUI;
using StreamerBotLib.GUI.Windows;
using StreamerBotLib.Models;
using StreamerBotLib.Models.Enums;
using StreamerBotLib.Models.Events;
using StreamerBotLib.Static;

using System.Collections.Concurrent;
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

        private Thread GUIDataGridUpdates { get; set; }
        private ConcurrentQueue<Task> GUIDataGridUpdateQueue { get; set; } = new();

        private GUIDataManagerViews GUIDataManagerViews { get; set; }

        #region GUI DataManager View Update Queue

        private void GUIDataGridUpdateThread()
        {
            while (!GUIDataGridUpdateQueue.IsEmpty || OptionFlags.ActiveToken)
            {
                while (GUIDataGridUpdateQueue.TryDequeue(out Task task))
                {
                    task.Start();
                    Task.Delay(200).Wait(); // Allow some time for the task to complete before processing the next one.
                }
            }
        }

        #endregion

        private void DataManagerViewLoaded()
        {
            Controller.HandleOnDataCollectionUpdated(DataManager_OnDataCollectionUpdated);
            BotController.DataBot.InitializeDataManagerViews(GUIDataManagerViews);
        }

        private void DataManager_OnDataCollectionUpdated(object sender, OnDataCollectionUpdatedEventArgs e)
        {
            //GUIDataGridUpdateQueue.Enqueue(new Task(() =>
            //{
            Dispatcher.BeginInvoke(() =>
            {
                LogWriter.DebugLog("DataManager_OnDataCollectionUpdated",
                   DebugLogTypes.GUIDataViews, $"Refreshing data for the {e.DatabaseModelName} data table.");

                if (e.RecordCountChange)
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
                        case "MultiChannels" or "MultiLiveStreams" or "MultiWebhooks" or "MultiSummaryLiveStreams":

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
                }
            });
            //}));
        }
        private void Button_ClearWatchTime_Click(object sender, RoutedEventArgs e)
        {
            Controller.ClearWatchTime();
        }
        private void Button_ClearCurrencyAccrlValues_Click(object sender, RoutedEventArgs e)
        {
            Controller.ClearAllCurrenciesValues();
        }
        private void Button_ClearNonFollowers_Click(object sender, RoutedEventArgs e)
        {
            Controller.ClearUsersNonFollowers();
        }
        private void DG_Edit_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (sender.GetType() == typeof(DataGrid))
            {
                bool FoundAddShout = ((DataGrid)sender).Name is nameof(DG_Users) or nameof(DG_Followers);
                bool FoundIsEnabled = ((DataGrid)sender).Columns.Select((c) => (string)c.Header == "Enabled").Any();
                bool FoundAddItem = ((DataGrid)sender).Name is
                       nameof(DG_CurrencyType)
                    or nameof(DG_CustomWelcome)
                    or nameof(DG_InRaids)
                    or nameof(DG_OutRaids)
                    or nameof(DG_Mod_LearnMsgs)
                    or nameof(DG_ModApprove)
                    or nameof(DG_OverlayService_Actions)
                    or nameof(DG_User_Quotes)
                    or nameof(DG_User_Shoutouts)
                    or nameof(DG_UserDefinedCommands)
                    or nameof(DG_Webhooks);

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
                nameof(DG_CurrencyType) => typeof(StreamerBotLib.DataSQL.Models.CurrencyType),
                nameof(DG_Currency) => typeof(Currency),
                nameof(DG_CustomWelcome) => typeof(CustomWelcome),
                nameof(DG_DeathCounter) => typeof(GameDeadCounter),
                nameof(DG_Followers) => typeof(Followers),
                nameof(DG_InRaids) => typeof(InRaidData),
                nameof(DG_ModApprove) => typeof(ModeratorApprove),
                nameof(DG_Mod_BanReasons) => typeof(StreamerBotLib.DataSQL.Models.BanReasons),
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
                UpdateOverlays();
            }

            PopupWindows.AddNewItem(tableMeta.SetNewEntity(SqlModel));
        }
        private void UpdateOverlays()
        {
            Controller.GetOverlayActions((actions) => PopupWindows.SetTableData(actions));
        }
        private void MenuItem_EditClick(object sender, RoutedEventArgs e)
        {
            DataGrid item = (((sender as MenuItem).Parent as ContextMenu).Parent as Popup).PlacementTarget as DataGrid;
            DGOpenEditWindow(item);
        }
        private void MenuItem_DeleteClick(object sender, RoutedEventArgs e)
        {
            DataGrid item = (((sender as MenuItem).Parent as ContextMenu).Parent as Popup).PlacementTarget as DataGrid;

            Controller.DeleteDataRows((IEnumerable<object>)item.SelectedItems, GetTableName(item));
        }
        private void MenuItem_AutoShoutClick(object sender, RoutedEventArgs e)
        {
            DataGrid item = (((sender as MenuItem).Parent as ContextMenu).Parent as Popup).PlacementTarget as DataGrid;

            foreach (UserBase dr in new List<UserBase>(item.SelectedItems.Cast<UserBase>().Select(DRV => DRV)))
            {
                Controller.AddNewAutoShoutUser(dr.UserId, dr.Platform);
            }
        }
        private void MenuItem_LiveMonitorClick(object sender, RoutedEventArgs e)
        {
            DataGrid item = (((sender as MenuItem).Parent as ContextMenu).Parent as Popup).PlacementTarget as DataGrid;

            // cast the selected items to the appropriate type based on the DataGrid
            if (item.Name is "DG_Users")
            {
                Controller.AddNewMonitorChannel([.. item.SelectedItems.Cast<Users>().Select(DRV => new LiveUser(DRV.UserName, DRV.Platform, DRV.UserId))]);
            }
            else if (item.Name is "DG_Followers")
            {
                Controller.AddNewMonitorChannel([.. item.SelectedItems.Cast<Followers>().Select(DRV => new LiveUser(DRV.User.UserName, DRV.Platform, DRV.UserId))]);
            }
        }
        private void DataGridContextMenu_EnableItems_Click(object sender, RoutedEventArgs e)
        {
            DataGrid item = (((sender as MenuItem).Parent as ContextMenu).Parent as Popup).PlacementTarget as DataGrid;

            var rows = item.SelectedItems;

            if (rows.Count > 0 && rows[0].GetType().GetProperty("IsEnabled") is not null)
            {
                foreach (var row in rows)
                {
                    row.GetType().GetProperty("IsEnabled")?.SetValue(row, true);
                }

                Controller.UpdatedIsEnabledRows(GetTableName(item));
            }
        }

        private string GetTableName(DataGrid item)
        {
            return item.Name switch
            {
                nameof(DG_Mod_BanReasons) => "BanReasons",
                nameof(DG_Mod_BanRules) => "BanRules",
                nameof(DG_CategoryList) => "CategoryList",
                nameof(DG_CommonMsgs) => "ChannelEvents",
                nameof(DG_Clips) => "Clips",
                nameof(DG_BuiltInCommands) => "Commands",
                nameof(DG_UserDefinedCommands) => "CommandsUser",
                nameof(DG_Currency) => "Currency",
                nameof(DG_CurrencyType) => "CurrencyType",
                nameof(DG_CustomWelcome) => "CustomWelcome",
                nameof(DG_Followers) => "Followers",
                nameof(DG_DeathCounter) => "GameDeadCounter",
                nameof(DG_User_Giveaway) => "GiveawayUserData",
                nameof(DG_InRaids) => "InRaidData",
                nameof(DG_Mod_LearnMsgs) => "LearnMsgs",
                nameof(DG_ModApprove) => "ModeratorApprove",
                nameof(DG_OutRaids) => "OutRaidData",
                nameof(DG_OverlayService_Actions) => "OverlayServices",
                nameof(DG_OverlayService_Ticker) => "OverlayTicker",
                nameof(DG_User_Quotes) => "Quotes",
                nameof(DG_User_Shoutouts) => "ShoutOuts",
                nameof(DG_StreamData_Stats) => "StreamStats",
                nameof(DG_Users) => "Users",
                nameof(DG_Webhooks) => "Webhooks",
                // specific to MultiLiveDataGrids.xaml, also routed through here
                "DG_Multi_ChannelNames" => "MultiChannels",
                "DG_Multi_LiveStreams" => "MultiLiveStreams",
                "DG_Multi_WebHooks" => "MultiWebhooks",
                "DG_Multi_SummaryLiveStreams" => "MultiSummaryLiveStreams",
                _ => ""
            };
        }

        private void DataGridContextMenu_DisableItems_Click(object sender, RoutedEventArgs e)
        {
            DataGrid item = (((sender as MenuItem).Parent as ContextMenu).Parent as Popup).PlacementTarget as DataGrid;

            var rows = item.SelectedItems;

            if (rows.Count > 0 && rows[0].GetType().GetProperty("IsEnabled") is not null)
            {
                foreach (var row in rows)
                {
                    row.GetType().GetProperty("IsEnabled")?.SetValue(row, false);
                }

                Controller.UpdatedIsEnabledRows(GetTableName(item));
            }
        }

        #endregion

        #region DataGrid Item editing
        private void DataGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                Controller.GUISaveDataGridEdits((sender as DataGrid).Name is "DG_BuiltInCommands" or "DG_UserDefinedCommands", GetTableName(sender as DataGrid));
            }
        }

        #endregion
    }
}
