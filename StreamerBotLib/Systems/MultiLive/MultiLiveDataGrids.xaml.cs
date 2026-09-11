using StreamerBotLib.DataSQL.AccessPolicy;
using StreamerBotLib.Models;
using StreamerBotLib.Models.Enums;
using StreamerBotLib.Models.Events;
using StreamerBotLib.Static;

using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace StreamerBotLib.Systems.MultiLive
{
    /// <summary>
    /// Interaction logic for MultiLiveDataGrids.xaml
    /// </summary>
    public partial class MultiLiveDataGrids : Page
    {
        public event EventHandler<MultiLiveSummarizeEventArgs> SummarizeChannels;

        private event EventHandler<RoutedEventArgs> Routed;
        //private EventHandler<TextChangedEventArgs> m_TextChanged;

        public Action<bool, string> GUISaveEdits;

        public event EventHandler<AddNewMultiChannelUserEventArgs> FindMultiChannelUserId;
        public event EventHandler<AddNewMultiChannelUserEventArgs> AddNewMultiChannelUser;
        public event EventHandler<RoutedEventArgs> MenuItemDeleteClick;
        public event EventHandler<RoutedEventArgs> MenuItemEnabledClick;
        public event EventHandler<RoutedEventArgs> MenuItemDisabledClick;
        public event EventHandler<RoutedEventArgs> MenuItemAddClick;
        public event EventHandler<RoutedEventArgs> MenuItemEditClick;
        public event EventHandler<PreviewKeyDownDeleteRowsEventArgs> PreviewKeyDownDeleteRows;

        public event EventHandler<EventArgs> DebugAddNewMultiLiveData;

        public MultiLiveDataGrids()
        {
            InitializeComponent();

#if DEBUG // Show debug tools in debug builds
            SP_MultiLive_Debug.Visibility = Visibility.Visible;
#endif
            GetSummarizeData = false;
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            LogWriter.DebugLog("MenuItem_Click", DebugLogTypes.GUIMultiLive, "Right-click menu is clicked.");

            int start = TB_LiveMsg.SelectionStart;

            if (TB_LiveMsg.SelectionLength > 0)
            {
                TB_LiveMsg.Text = TB_LiveMsg.Text.Remove(start, TB_LiveMsg.SelectionLength);
            }

            TB_LiveMsg.Text = TB_LiveMsg.Text.Insert(start, (sender as MenuItem).Header.ToString());
            TB_LiveMsg.SelectionStart = start;
        }

        public void SetIsEnabled(bool IsEnabled)
        {
            LogWriter.DebugLog("SetIsEnabled", DebugLogTypes.GUIMultiLive, $"Setting visual area IsEnabled={IsEnabled}.");

            Grid_MultiUserLiveMonitor.IsEnabled = IsEnabled;
        }

        public void SetHandlers(EventHandler<RoutedEventArgs> SettingsLostFocus, EventHandler<TextChangedEventArgs> TextChanged)
        {
            LogWriter.DebugLog("SetHandlers", DebugLogTypes.GUIMultiLive, "Setting handlers for textbox and lost focus.");

            Routed += SettingsLostFocus;
        }

        private string GetTableName(DataGrid item)
        {
            return item.Name switch
            {
                nameof(DG_Multi_ChannelNames) => "MultiChannels",
                nameof(DG_Multi_LiveStreamStats) => "MultiLiveStreams",
                nameof(DG_Multi_WebHooks) => "MultiWebhooks",
                nameof(DG_Multi_SummaryLiveStreamStats) => "MultiSummaryLiveStreams",
                _ => ""
            };
        }

        //private void DG_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        //{
        //    DataGrid item = sender as DataGrid;

        //    if (((DataGrid)sender).Name != "DG_Multi_ChannelNames")
        //    {
        //        Popup_DataEdit(item, false);
        //    }
        //}
        private readonly Dictionary<string, string> MenuAccessMap = new()
        {
            {"DataGridContextMenu_Multi_AddItem", "AddRow" },
            {"DataGridContextMenu_Multi_EditItem", "EditRow" },
            {"DataGridContextMenu_Multi_DeleteItems", "DeleteRow" },
            {"DataGridContextMenu_Multi_EnableItems", "EnableItems" },
            {"DataGridContextMenu_Multi_DisableItems", "DisableItems" }
        };

        private void DG_MultiEdit_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (sender.GetType() == typeof(DataGrid))
            {
                DataGrid curr = sender as DataGrid;

                MenuAccess tableMenuAccess = PermissionDigest.GetTableMenuAccess(GetTableName(curr));

                foreach (var M in ((ContextMenu)Resources["DataGrid_Multi_ContextMenu"]).Items)
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
                            ((Separator)M).Visibility = tableMenuAccess.EnableItems || tableMenuAccess.DisableItems ? Visibility.Visible : Visibility.Collapsed;
                        }
                    }
                }
                //bool FoundAddEdit = ((DataGrid)sender).Name is nameof(DG_Multi_WebHooks)
                //                                            or nameof(DG_Multi_ChannelNames);
                //bool FoundIsEnabled = nameof(DG_Multi_WebHooks) == (sender as DataGrid).Name;

                //foreach (var M in ((ContextMenu)Resources["DataGrid_Multi_ContextMenu"]).Items)
                //{
                //    if (M.GetType() == typeof(MenuItem))
                //    {
                //        switch (((MenuItem)M).Name)
                //        {
                //            case "DataGridContextMenu_Multi_AddItem":
                //                ((MenuItem)M).IsEnabled = FoundAddEdit;
                //                break;
                //            case "DataGridContextMenu_Multi_DeleteItem":
                //                ((MenuItem)M).IsEnabled = true;
                //                break;
                //            case "DataGridContextMenu_Multi_EnableItems" or "DataGridContextMenu_Multi_DisableItems":
                //                ((MenuItem)M).IsEnabled = FoundIsEnabled;
                //                break;
                //        }
                //    }
                //}
            }
        }

        private void Settings_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!e.Handled)
            {
                e.Handled = true;

                Routed?.Invoke(sender, e);
            }
        }

        private void MenuItem_AddClick(object sender, RoutedEventArgs e)
        {
            MenuItemAddClick?.Invoke(sender, e);
        }

        private void MenuItem_EditClick(object sender, RoutedEventArgs e)
        {
            MenuItemEditClick?.Invoke(sender, e);
        }

        private void MenuItem_DeleteClick(object sender, RoutedEventArgs e)
        {
            MenuItemDeleteClick?.Invoke(sender, e);
        }

        private void DataGridContextMenu_EnableItems_Click(object sender, RoutedEventArgs e)
        {
            LogWriter.DebugLog("DataGridContextMenu_EnableItems_Click", DebugLogTypes.GUIMultiLive, "Setting selected webhook items 'IsEnabled' to enabled.");
            MenuItemEnabledClick?.Invoke(sender, e);
        }

        private void DataGridContextMenu_DisableItems_Click(object sender, RoutedEventArgs e)
        {
            LogWriter.DebugLog("DataGridContextMenu_DisableItems_Click", DebugLogTypes.GUIMultiLive, "Setting selected webhook items 'IsEnabled' to disabled.");
            MenuItemDisabledClick?.Invoke(sender, e);
        }

        private void DG_Multi_Initialized(object sender, EventArgs e)
        {
            DataGrid curr = sender as DataGrid;

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

        private bool GetSummarizeData;
        private bool StartSummarizingLiveData = false;

        private void ComboBox_DropDownOpened(object sender, EventArgs e)
        {
            LogWriter.DebugLog("ComboBox_DropDownOpened", DebugLogTypes.GUIMultiLive, "Computing the date with stream count data for the combo box.");

            if (!GetSummarizeData)
            {
                GetSummarizeData = true;
                SummarizeChannels?.Invoke(this, new()
                {
                    CallbackAction = () =>
                    {
                        Dispatcher.BeginInvoke(() =>
                        {
                            ComboBox_SummarizeLive_List.Items.Refresh();
                            GetSummarizeData = false;
                        });
                    }
                });
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LogWriter.DebugLog("ComboBox_SelectionChanged", DebugLogTypes.GUIMultiLive, "Enable the button to permit actually summarizing the stream data.");

            if (((ComboBox)sender).SelectedItem != null)
            {
                Button_StartSummarizingLiveData.IsEnabled = true;
            }
        }

        private void Button_StartSummarizingLiveData_Click(object sender, RoutedEventArgs e)
        {
            LogWriter.DebugLog("Button_StartSummarizingLiveData_Click", DebugLogTypes.GUIMultiLive, "Disabling buttons and sending a summarizing message to the data manager.");

            TabItem_DailyData.IsEnabled = false;
            TabItem_SummaryDailyData.IsEnabled = false;
            Button_StartSummarizingLiveData.IsEnabled = false;

            if (!StartSummarizingLiveData)
            {
                StartSummarizingLiveData = true;

                ArchiveMultiStream selectedItem = (ArchiveMultiStream)ComboBox_SummarizeLive_List.SelectedItem;

                SummarizeChannels?.Invoke(this, new()
                {
                    Data = selectedItem,
                    CallbackAction = () =>
                    {
                        Dispatcher.BeginInvoke(() =>
                        {
                            ComboBox_SummarizeLive_List.ClearValue(Selector.SelectedItemProperty);
                            TabItem_DailyData.IsEnabled = true;
                            TabItem_SummaryDailyData.IsEnabled = true;
                            StartSummarizingLiveData = false;
                        });
                    }
                }
                                        );
            }
        }

        public void DataManager_OnDataCollectionUpdated(object sender, OnDataCollectionUpdatedEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                switch (e.DatabaseModelName)
                {
                    case "MultiChannels":
                        DG_Multi_ChannelNames.Items.Refresh();
                        break;
                    case "MultiLiveStreams":
                        DG_Multi_LiveStreamStats.Items.Refresh();
                        break;
                    case "MultiWebhooks":
                        DG_Multi_WebHooks.Items.Refresh();
                        break;
                    case "MultiSummaryLiveStreams":
                        DG_Multi_SummaryLiveStreamStats.Items.Refresh();
                        break;
                    case "MultiLiveStatusLog":
                        LB_BotActivityLog.Items.Refresh();
                        break;
                    case null:
                        break;
                }
            }));
        }

        private void DataGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                GUISaveEdits?.Invoke((sender as DataGrid).Name is "DG_BuiltInCommands" or "DG_UserDefinedCommands", GetTableName(sender as DataGrid));
            }
        }

        private void Button_MultiUser_FindUserId_Click(object sender, RoutedEventArgs e)
        {
            Platform platform = (Platform)ComboBox_MultiUsers_LookupUser_Platform.SelectedItem;
            string UserName = TextBox_MultiUsers_LookupUser_UserName.Text;

            ComboBox_MultiUsers_LookupUser_Platform.IsEnabled = false;
            TextBox_MultiUsers_LookupUser_UserName.IsEnabled = false;
            TextBox_MultiUsers_LookupUser_UserId.IsEnabled = false;

            FindMultiChannelUserId?.Invoke(this, new(new(UserName, platform)));
        }

        private void Button_MultiUser_PostNewMultiUser_Click(object sender, RoutedEventArgs e)
        {
            Platform platform = (Platform)ComboBox_MultiUsers_LookupUser_Platform.SelectedItem;
            string UserName = TextBox_MultiUsers_LookupUser_UserName.Text;
            string UserId = TextBox_MultiUsers_LookupUser_UserId.Text;

            AddNewMultiChannelUser?.Invoke(this, new(new(UserName, platform, UserId)));

            ComboBox_MultiUsers_LookupUser_Platform.IsEnabled = true;
            TextBox_MultiUsers_LookupUser_UserName.IsEnabled = true;
            TextBox_MultiUsers_LookupUser_UserId.IsEnabled = true;
        }

        public void UpdateMultiChannelUserId(object sender, AddNewMultiChannelUserEventArgs e)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                TextBox_MultiUsers_LookupUser_UserId.Text = e.LiveUser.UserId;
            }));
        }

        private void Debug_Button_AddNew_Click(object sender, RoutedEventArgs e)
        {
            DebugAddNewMultiLiveData?.Invoke(this, EventArgs.Empty);
        }

        private void DG_PreviewKeyDown_Click(object sender, System.Windows.Input.KeyEventArgs e)
        {
            PreviewKeyDownDeleteRows?.Invoke(this, new(sender, e));
        }
    }
}
