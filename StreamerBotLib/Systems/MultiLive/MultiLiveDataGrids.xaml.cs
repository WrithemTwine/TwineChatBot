using StreamerBotLib.DataSQL.Models;
using StreamerBotLib.GUI.Windows;
using StreamerBotLib.Models;
using StreamerBotLib.Models.Enums;
using StreamerBotLib.Models.Events;
using StreamerBotLib.Static;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

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
        public event EventHandler<PreviewKeyDownDeleteRowsEventArgs> PreviewKeyDownDeleteRows;

        public event EventHandler<EventArgs> DebugAddNewMultiLiveData;

        private ManageWindows PopupWindows { get; set; } = new();

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

        //private void DG_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        //{
        //    DataGrid item = sender as DataGrid;

        //    if (((DataGrid)sender).Name != "DG_Multi_ChannelNames")
        //    {
        //        Popup_DataEdit(item, false);
        //    }
        //}

        private void DG_MultiEdit_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (sender.GetType() == typeof(DataGrid))
            {
                bool FoundAddEdit = ((DataGrid)sender).Name is nameof(DG_Multi_WebHooks)
                                                            or nameof(DG_Multi_ChannelNames);
                bool FoundIsEnabled = nameof(DG_Multi_WebHooks) == (sender as DataGrid).Name;

                foreach (var M in ((ContextMenu)Resources["DataGrid_Multi_ContextMenu"]).Items)
                {
                    if (M.GetType() == typeof(MenuItem))
                    {
                        switch (((MenuItem)M).Name)
                        {
                            case "DataGridContextMenu_Multi_AddItem":
                                ((MenuItem)M).IsEnabled = FoundAddEdit;
                                break;
                            case "DataGridContextMenu_Multi_DeleteItem":
                                ((MenuItem)M).IsEnabled = true;
                                break;
                            case "DataGridContextMenu_Multi_EnableItems" or "DataGridContextMenu_Multi_DisableItems":
                                ((MenuItem)M).IsEnabled = FoundIsEnabled;
                                break;
                        }
                    }
                }
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

        private void Popup_DataEdit(DataGrid item)
        {
            Type SqlModel = item.Name switch
            {
                nameof(DG_Multi_WebHooks) => typeof(MultiWebhooks),
                nameof(DG_Multi_ChannelNames) => typeof(MultiChannels),
                nameof(DG_Multi_LiveStreamStats) => typeof(MultiLiveStreams),
                nameof(DG_Multi_SummaryLiveStreamStats) => typeof(MultiSummaryLiveStreams),
                _ => typeof(object)
            };

            DataSQL.TableMeta.TableMeta tableMeta = new();
            PopupWindows.AddNewItem(tableMeta.SetNewEntity(SqlModel));
        }

        private void MenuItem_AddClick(object sender, RoutedEventArgs e)
        {
            DataGrid item = (((sender as MenuItem).Parent as ContextMenu).Parent as Popup).PlacementTarget as DataGrid;
            Popup_DataEdit(item);
        }

        //private void MenuItem_EditClick(object sender, RoutedEventArgs e)
        //{
        //    DataGrid item = (((sender as MenuItem).Parent as ContextMenu).Parent as Popup).PlacementTarget as DataGrid;

        //    Popup_DataEdit(item, false);
        //}

        //private void MenuItem_DeleteClick(object sender, RoutedEventArgs e)
        //{
        //    DataGrid item = (((sender as MenuItem).Parent as ContextMenu).Parent as Popup).PlacementTarget as DataGrid;

        //    SystemsController.DeleteRows(new List<DataRow>(item.SelectedItems.Cast<DataRowView>().Select(DRV => DRV.Row)));
        //}

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

        private void DG_Multi_ChannelNames_AutoGeneratedColumns(object sender, EventArgs e)
        {
            static void Collapse(DataGridColumn dgc)
            {
                dgc.Visibility = Visibility.Collapsed;
            }

            DataGrid dg = (DataGrid)sender;

            switch (dg.Name)
            {
                case "DG_Multi_ChannelNames":
                    foreach (DataGridColumn dc in dg.Columns)
                    {
                        if (dc.Header.ToString() is not "Id" and not "ChannelName" and not "UserId")
                        {
                            Collapse(dc);
                        }
                    }
                    break;
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

        private string GetTableName(DataGrid item)
        {
            return item.Name switch
            {
                "DG_Multi_ChannelNames" => "MultiChannels",
                "DG_Multi_LiveStreams" => "MultiLiveStreams",
                "DG_Multi_Webhooks" => "MultiWebhooks",
                "DG_Multi_SummaryLiveStreams" => "MultiSummaryLiveStreams",
                _ => ""
            };
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
        private void MenuItem_DeleteClick(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is MenuItem)
            {
                MenuItemDeleteClick?.Invoke(sender, e);
            }
        }

        private void DG_PreviewKeyDown_Click(object sender, System.Windows.Input.KeyEventArgs e)
        {
            PreviewKeyDownDeleteRows?.Invoke(this, new(sender, e));
        }
    }
}
