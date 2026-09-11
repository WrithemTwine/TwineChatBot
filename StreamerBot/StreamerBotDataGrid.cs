using StreamerBotLib.BotIOController;
using StreamerBotLib.DataSQL.AccessPolicy;
using StreamerBotLib.DataSQL.Models;
using StreamerBotLib.DataSQL.TableMeta;
using StreamerBotLib.GUI;
using StreamerBotLib.GUI.Data;
using StreamerBotLib.Models;
using StreamerBotLib.Models.Enums;
using StreamerBotLib.Models.Events;
using StreamerBotLib.Static;

using System.Data;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace StreamerBot
{
    public partial class StreamerBotWindow
    {
        #region DataGrid Columns and Editing
        private ManageDataEdit PopupWindows { get; set; }
        private Thread GUIDataGridUpdates { get; set; }
        private GUIDataManagerViews GUIDataManagerViews { get; set; }

        private void DataGrid_Initialized(object sender, EventArgs e)
        {
            string ParseColumnPath(DataGridColumn column)
            {
                return column switch
                {
                    DataGridBoundColumn b when b.Binding is Binding bind
                     => bind.Path?.Path,
                    DataGridComboBoxColumn c
                        => (c.SelectedItemBinding as Binding)?.Path?.Path
                        ?? (c.SelectedValueBinding as Binding)?.Path?.Path,
                    DataGridTemplateColumn c => GetAllBindingPaths(c).FirstOrDefault(),
                    _ => column.SortMemberPath
                };
            }

            string GetColumnPath(DataGridColumn column)
            {
                string subpath = ParseColumnPath(column);

                return subpath?.Contains('.') == true ? subpath[..subpath.IndexOf('.')] : subpath;
            }

            DataGrid curr = sender as DataGrid;

            Table AccessPermissions = PermissionDigest.GetTablePermissions(GetTableName(curr));

            curr.CanUserAddRows = false; // don't add inline DataGrid rows
            curr.CanUserDeleteRows = AccessPermissions.MenuAccess.DeleteRow;
            curr.IsReadOnly = AccessPermissions.IsDataGridReadOnly;

            foreach (var c in curr.Columns)
            {
                Column currColumn = PermissionDigest.GetColumnPermissions(GetTableName(curr), GetColumnPath(c));

                c.IsReadOnly = currColumn?.IsDataGridReadOnly ?? false;
            }
        }

        private static List<string> GetAllBindingPaths(DataGridTemplateColumn column, bool fromEditingTemplate = false)
        {
            var result = new List<string>();
            if (column == null) return result;

            DataTemplate template = fromEditingTemplate
                ? column.CellEditingTemplate
                : column.CellTemplate;

            if (template == null) return result;

            var content = template.LoadContent();
            if (content is FrameworkElement fe)
            {
                FindBindingsRecursive(fe, result);
            }

            return result.Distinct().ToList();
        }

        private static void FindBindingsRecursive(object element, List<string> paths)
        {
            if (element == null) return;

            // Check all dependency properties for bindings
            var dpFields = element.GetType()
                .GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy)
                .Where(f => f.FieldType == typeof(DependencyProperty));

            foreach (var dpField in dpFields)
            {
                var dp = (DependencyProperty)dpField.GetValue(null);
                var binding = BindingOperations.GetBinding(element as DependencyObject, dp);
                if (binding?.Path?.Path != null)
                {
                    paths.Add(binding.Path.Path);
                }
            }

            // Recurse into children
            if (element is Panel panel)
            {
                foreach (var child in panel.Children)
                    FindBindingsRecursive(child, paths);
            }
            else if (element is ContentControl cc && cc.Content != null)
            {
                FindBindingsRecursive(cc.Content, paths);
            }
            else if (element is ItemsControl ic && ic.ItemsSource == null)
            {
                foreach (var item in ic.Items)
                    FindBindingsRecursive(item, paths);
            }
        }

        #region View - New-Edit Data Records - PopuWindow
        private void MenuItem_AddClick(object sender, RoutedEventArgs e)
        {
            DataGrid item = GetMenuDataGrid(sender);

            DGOpenEditWindow(item, true);
        }

        private void DGOpenEditWindow(DataGrid item, bool IsNew)
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
                "DG_Multi_WebHooks" => typeof(MultiWebhooks),
                "DG_Multi_ChannelNames" => typeof(MultiChannels),
                "DG_Multi_LiveStreamStats" => typeof(MultiLiveStreams),
                "DG_Multi_SummaryLiveStreamStats" => typeof(MultiSummaryLiveStreams),

                _ => typeof(object)
            };

            TableMeta tableMeta = new();

            Controller.GetOverlayActions(PopupWindows.SetTableData);

            PopupWindows.EditItem(
                // establish existing or new record entity
                (IsNew ?
                          tableMeta.SetNewEntity(SqlModel)
                        : tableMeta.SetExistingEntity(item.SelectedItem))
                , IsNew
                , item);
        }

        private void MenuItem_EditClick(object sender, RoutedEventArgs e)
        {
            DataGrid item = GetMenuDataGrid(sender);

            DGOpenEditWindow(item, false);
        }

        #endregion

        private void DataManagerViewLoaded()
        {
            Controller.HandleOnDataCollectionUpdated(DataManager_OnDataCollectionUpdated);
            BotController.DataBot.InitializeDataManagerViews(GUIDataManagerViews);

            ContextMenu LrnMsgMenuItems = Resources["DataGrid_LearnMsgsContextMenu"] as ContextMenu;
            // setup the LearnMsgs context menu for showing "MsgTypes" items so the user can bulk update data rows for a certain message type
            foreach (string LM in Enum.GetNames<MsgTypes>())
            {
                MenuItem temp = new() { Header = LM };
                temp.Click += MenuItem_LearnMsgTypeClick;

                LrnMsgMenuItems.Items.Add(temp);
            }
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
                        case "OldFollowUsers":
                            DG_OldFollowUsers.Items.Refresh();
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
        private readonly Dictionary<string, string> MenuAccessMap = new()
        {
            {"DataGridContextMenu_AddItem", "AddRow" },
            {"DataGridContextMenu_EditItem", "EditRow" },
            {"DataGridContextMenu_DeleteItems", "DeleteRow" },
            {"DataGridContextMenu_AutoShout", "AutoShout" },
            {"DataGridContextMenu_LiveMonitor", "MonitorLive" },
            {"DataGridContextMenu_EnableItems", "EnableItems" },
            {"DataGridContextMenu_DisableItems", "DisableItems" }
        };

        private void DG_Edit_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (sender.GetType() == typeof(DataGrid))
            {
                // menu access permissions for the current DataGrid table
                MenuAccess tableMenuAccess = PermissionDigest.GetTableMenuAccess(GetTableName((DataGrid)sender));

                foreach (var M in ((ContextMenu)Resources["DataGrid_ContextMenu"]).Items)
                {
                    if (M.GetType() == typeof(MenuItem))
                    {
                        MenuItem menuitem = (MenuItem)M;
                        menuitem.Visibility = (bool)tableMenuAccess[MenuAccessMap[menuitem.Name]] ? Visibility.Visible : Visibility.Collapsed;
                    }
                    else if (M.GetType() == typeof(Separator))
                    {
                        if (((Separator)M).Name == "DataGridContextMenu_Separator1")
                        {
                            ((Separator)M).Visibility = tableMenuAccess.AutoShout || tableMenuAccess.MonitorLive ? Visibility.Visible : Visibility.Collapsed;
                        }
                        else if (((Separator)M).Name == "DataGridContextMenu_Separator2")
                        {
                            ((Separator)M).Visibility = tableMenuAccess.EnableItems || tableMenuAccess.DisableItems ? Visibility.Visible : Visibility.Collapsed;
                        }
                    }
                }
            }

            //if (sender.GetType() == typeof(DataGrid))
            //{
            //    bool FoundAddShout = ((DataGrid)sender).Name is nameof(DG_Users) or nameof(DG_Followers);
            //    bool FoundIsEnabled = ((DataGrid)sender).Columns.Any((c) => (string)c.Header == "Enabled");
            //    bool FoundAddItem = ((DataGrid)sender).Name is
            //           nameof(DG_CurrencyType)
            //        or nameof(DG_CustomWelcome)
            //        or nameof(DG_InRaids)
            //        or nameof(DG_OutRaids)
            //        or nameof(DG_Mod_LearnMsgs)
            //        or nameof(DG_ModApprove)
            //        or nameof(DG_OverlayService_Actions)
            //        or nameof(DG_User_Quotes)
            //        or nameof(DG_User_Shoutouts)
            //        or nameof(DG_UserDefinedCommands)
            //        or nameof(DG_Webhooks);

            //    foreach (var M in ((ContextMenu)Resources["DataGrid_ContextMenu"]).Items)
            //    {
            //        if (M.GetType() == typeof(MenuItem))
            //        {
            //            if (((MenuItem)M).Name is "DataGridContextMenu_AutoShout" or "DataGridContextMenu_LiveMonitor")
            //            {
            //                ((MenuItem)M).Visibility = FoundAddShout ? Visibility.Visible : Visibility.Collapsed;
            //            }
            //            else if (((MenuItem)M).Name is "DataGridContextMenu_EnableItems" or "DataGridContextMenu_DisableItems")
            //            {
            //                ((MenuItem)M).IsEnabled = FoundIsEnabled;
            //            }
            //            else if (((MenuItem)M).Name is "DataGridContextMenu_AddItem")
            //            {
            //                ((MenuItem)M).Visibility = FoundAddItem ? Visibility.Visible : Visibility.Collapsed;
            //            }
            //            else if (((MenuItem)M).Name is "DataGridContextMenu_DeleteItems")
            //            {
            //                ((MenuItem)M).Visibility = ((DataGrid)sender).CanUserDeleteRows ? Visibility.Visible : Visibility.Collapsed;
            //            }
            //        }
            //        else if (M.GetType() == typeof(Separator))
            //        {
            //            if (((Separator)M).Name == "DataGridContextMenu_Separator1")
            //            {
            //                ((Separator)M).Visibility = FoundAddShout ? Visibility.Visible : Visibility.Collapsed;
            //            }
            //        }
            //    }
            //}
        }
        private static DataGrid GetMenuDataGrid(object sender)
        {
            return (((sender as MenuItem).Parent as ContextMenu).Parent as Popup).PlacementTarget as DataGrid;
        }

        private void MenuItem_DeleteClick(object sender, RoutedEventArgs e)
        {
            DataGrid item = GetMenuDataGrid(sender);

            Controller.DeleteDataRows((IEnumerable<object>)item.SelectedItems, GetTableName(item));
        }

        private void MenuItem_LearnMsgTypeClick(object sender, RoutedEventArgs e)
        {
            DataGrid CurrLrnMsg = GetMenuDataGrid(sender);
            MsgTypes SelectedType = Enum.Parse<MsgTypes>((string)(sender as MenuItem).Header);

            foreach (LearnMsgs row in CurrLrnMsg.SelectedItems)
            {
                row.MsgType = SelectedType;
            }

            Controller.GUISaveDataGridEdits(false, GetTableName(CurrLrnMsg));
        }


        private void DG_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DataGrid curr = sender as DataGrid;

            if (PermissionDigest.GetTablePermissions(GetTableName(curr)).MenuAccess.EditRow)
            {
                DGOpenEditWindow(curr, false);
            }
            else if (PermissionDigest.GetTablePermissions(GetTableName(curr)).MenuAccess.AddRow)
            {
                DGOpenEditWindow(curr, true);
            }
        }

        private void DG_PreviewKeyDown_Click(object sender, System.Windows.Input.KeyEventArgs e)
        {
            DataGrid item = sender as DataGrid;
            if (e.Key == System.Windows.Input.Key.Delete && item.SelectedItems.Count > 0 && item.CanUserDeleteRows)
            {
                Controller.DeleteDataRows((IEnumerable<object>)item.SelectedItems, GetTableName(item));
            }
        }
        private void MultiLive_DG_PreviewKeyDown_Click(object sender, PreviewKeyDownDeleteRowsEventArgs e)
        {
            DG_PreviewKeyDown_Click(e.DataGridSender, e.e);
        }
        private void MenuItem_AutoShoutClick(object sender, RoutedEventArgs e)
        {
            DataGrid item = GetMenuDataGrid(sender);

            foreach (UserBase dr in new List<UserBase>(item.SelectedItems.Cast<UserBase>().Select(DRV => DRV)))
            {
                Controller.AddNewAutoShoutUser(dr.UserId, dr.Platform);
            }
        }
        private void MenuItem_LiveMonitorClick(object sender, RoutedEventArgs e)
        {
            DataGrid item = GetMenuDataGrid(sender);

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
            DataGrid item = GetMenuDataGrid(sender);

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
                nameof(DG_CommandPlatformMessages) => "CommandPlatformMessages",
                nameof(DG_Currency) => "Currency",
                nameof(DG_CurrencyType) => "CurrencyType",
                nameof(DG_CustomWelcome) => "CustomWelcome",
                nameof(DG_Followers) => "Followers",
                nameof(DG_OldFollowUsers) => "OldFollowUsers",
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
                "DG_Multi_LiveStreamStats" => "MultiLiveStreams",
                "DG_Multi_WebHooks" => "MultiWebhooks",
                "DG_Multi_SummaryLiveStreamStats" => "MultiSummaryLiveStreams",
                _ => ""
            };
        }
        private void DataGridContextMenu_DisableItems_Click(object sender, RoutedEventArgs e)
        {
            DataGrid item = GetMenuDataGrid(sender);

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

        #region DataGrid ListBox Editing (for CategoryList and ChannelEvents "EventType" column)

        private ListBox _categoryListBox;

        private void DataGridEdit_ListBox_Category_Initialized(object sender, EventArgs e)
        {
            _categoryListBox = sender as ListBox;
        }

        private void DataGridEdit_ListBox_PreviewLeftMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            e.Handled = true;

            // Find the actual CheckBox that was clicked
            DependencyObject current = e.OriginalSource as DependencyObject;
            while (current != null && current is not CheckBox && current is not ListBox)
                current = VisualTreeHelper.GetParent(current);

            if (current is CheckBox checkBox)
            {
                checkBox.IsChecked = !checkBox.IsChecked;
                // Now run the “All” logic
                DGrid_Category_ListBoxItem_CheckBox_Checked(checkBox, e);
            }
        }

        private void DataGridEdit_ListBox_SourceUpdated(object sender, DataTransferEventArgs e)
        {
            if (e.Source is List<CheckBox> checkBoxes)
            {
                foreach (var cb in checkBoxes)
                {
                    cb.Checked += DGrid_Category_ListBoxItem_CheckBox_Checked;
                    cb.Unchecked += DGrid_Category_ListBoxItem_CheckBox_Checked;
                }
            }
        }

        private void DataGridEdit_ListBox_CategorySelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            e.Handled = true;

            //ListBox CatList = ((ListBox)sender);
            //DataGrid_ListBox_Edit_SetSelectedItem(CatList);
        }

        private static void DataGrid_ListBox_Edit_SetSelectedItem(ListBox CatList)
        {
            CheckBox CurrBox = (CheckBox)CatList.SelectedItem;
            List<string> selected = [];
            CurrBox.IsChecked = !CurrBox.IsChecked;

            bool? SelectAll = null;

            // first check what is currently selected
            // if "All" was now selected, work with the other items
            if (CurrBox.Content.ToString() == "All")
            {
                SelectAll = CurrBox.IsChecked == true;
            }
            // if other items were selected, then we uncheck "All"
            else if (CurrBox.IsChecked == true)
            {
                SelectAll = false;
            }

            //CatList.SelectedItems.Clear();

            // step through every checkbox item
            foreach (CheckBox c in CatList.ItemsSource)
            {
                // look for the "All" item, set it to the found item
                if (c.Content.ToString() == "All")
                {
                    c.IsChecked = SelectAll;
                }
                // if the "All" item is selected, uncheck all of the other items
                else if (SelectAll == true)
                {
                    c.IsChecked = false;
                }

                if (c.IsChecked == true)
                {
                    //    CatList.SelectedItems.Add(c.Content);
                    selected.Add(c.Content.ToString());
                }
            }

            if (selected.Count == 0 || selected.Contains("All"))
            {
                CatList.SelectedItem = new List<string> { "All" };
            }
            else
            {
                CatList.SelectedItem = selected;
            }
        }

        private void DGrid_Category_ListBoxItem_CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            var checkBox = sender as CheckBox;
            if (_categoryListBox.ItemsSource is not ICollection<CheckBox> items) return;

            bool allJustChecked = (string)checkBox.Content == "All" && checkBox.IsChecked == true;

            List<string> selected = [];

            if (allJustChecked)
            {
                // User turned “All” on → clear everything else
                foreach (var cb in items)
                {
                    cb.IsChecked = (string)cb.Content == "All";
                }
                selected = ["All"];
            }
            else
            {
                // User clicked a normal category
                var allItem = items.FirstOrDefault(c => (string)c.Content == "All");
                allItem?.IsChecked = false;

                selected = [.. items
                    .Where(c => c.IsChecked == true)
                    .Select(c => (string)c.Content)];

                // If nothing left checked, force “All”
                if (selected.Count == 0)
                {
                    allItem?.IsChecked = true;
                    selected = ["All"];
                }
            }

            _categoryListBox.SelectedItem = selected;
        }

        #endregion

        #endregion

        #region DataGrid Item editing
        private void DataGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                LogWriter.DebugLog("DataGrid_RowEditEnding", DebugLogTypes.GUIDataViews, $"Committing edit for the {GetTableName(sender as DataGrid)} data table.");
                Controller.GUISaveDataGridEdits((sender as DataGrid).Name is "DG_BuiltInCommands" or "DG_UserDefinedCommands", GetTableName(sender as DataGrid));
            }
        }

        #endregion
    }
}
