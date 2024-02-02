using StreamerBotLib.DataSQL.Models;
using StreamerBotLib.Enums;
using StreamerBotLib.Events;
using StreamerBotLib.GUI.Windows;
using StreamerBotLib.Models;
using StreamerBotLib.Static;

using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace StreamerBotLib.MultiLive
{
    /// <summary>
    /// Interaction logic for MultiLiveDataGrids.xaml
    /// </summary>
    public partial class MultiLiveDataGrids : Page
    {
        public event EventHandler<MultiLiveSummarizeEventArgs> SummarizeChannels;

        private EventHandler<RoutedEventArgs> m_Routed;
        private EventHandler<TextChangedEventArgs> m_TextChanged;

        private ManageWindows PopupWindows { get; set; } = new();

        public MultiLiveDataGrids()
        {
            InitializeComponent();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.GUIMultiLive, "Right-click menu is clicked.");

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
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.GUIMultiLive, $"Setting visual area IsEnabled={IsEnabled}.");

            Grid_MultiUserLiveMonitor.IsEnabled = IsEnabled;
        }

        public void SetHandlers(EventHandler<RoutedEventArgs> SettingsLostFocus, EventHandler<TextChangedEventArgs> TextChanged)
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.GUIMultiLive, "Setting handlers for textbox and lost focus.");

            m_Routed = SettingsLostFocus;
            m_TextChanged = TextChanged;
        }

        //private void DG_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        //{
        //    DataGrid item = sender as DataGrid;

        //    if (((DataGrid)sender).Name != "DG_Multi_ChannelNames")
        //    {
        //        Popup_DataEdit(item, false);
        //    }
        //}

        //private void DG_MultiEdit_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        //{
        //    if (sender.GetType() == typeof(DataGrid))
        //    {
        //        //bool FoundAddEdit = ((DataGrid)sender).Name == "DG_Multi_WebHooks" || (((DataGrid)sender).Name == "DG_Multi_ChannelNames");
        //        bool FoundIsEnabled = typeof(MultiMsgEndPoints) == ((sender as DataGrid).ItemsSource).GetType();

        //        foreach (var M in ((ContextMenu)Resources["DataGrid_Multi_ContextMenu"]).Items)
        //        {
        //            if (M.GetType() == typeof(MenuItem))
        //            {
        //                switch (((MenuItem)M).Name)
        //                {
        //                    //case "DataGridContextMenu_Multi_AddItem":
        //                    //    ((MenuItem)M).IsEnabled = FoundAddEdit;
        //                    //    break;
        //                    //case "DataGridContextMenu_Multi_DeleteItem":
        //                    //    ((MenuItem)M).IsEnabled = true;
        //                    //    break;
        //                    case "DataGridContextMenu_Multi_EnableItems" or "DataGridContextMenu_Multi_DisableItems":
        //                        ((MenuItem)M).IsEnabled = FoundIsEnabled;
        //                        break;
        //                }
        //            }
        //        }
        //    }
        //}

        private void Settings_LostFocus(object sender, RoutedEventArgs e)
        {
            m_Routed?.Invoke(sender, e);
        }

        private void TB_BotActivityLog_TextChanged(object sender, TextChangedEventArgs e)
        {
            m_TextChanged?.Invoke(sender, e);
        }

        //private void Popup_DataEdit(DataGrid sourceDataGrid, bool AddNew = true)
        //{
        //    if (AddNew)
        //    {
        //        DataView CurrdataView = (DataView)sourceDataGrid.ItemsSource;
        //        if (CurrdataView != null)
        //        {
        //            PopupWindows.DataGridAddNewItem(MultiLiveData, CurrdataView.Table);
        //        }
        //    }
        //    else
        //    {
        //        DataRowView dataView = (DataRowView)sourceDataGrid.SelectedItem;
        //        if (dataView != null)
        //        {
        //            PopupWindows.DataGridEditItem(MultiLiveData, dataView.Row.Table, dataView.Row);
        //        }
        //    }
        //}

        //private void MenuItem_AddClick(object sender, RoutedEventArgs e)
        //{
        //    DataGrid item = (((sender as MenuItem).Parent as ContextMenu).Parent as Popup).PlacementTarget as DataGrid;

        //    Popup_DataEdit(item);
        //}

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
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.GUIMultiLive, "Setting selected webhook items 'IsEnabled' to enabled.");

            IEnumerable<MultiMsgEndPoints> item = (IEnumerable<MultiMsgEndPoints>)((((sender as MenuItem).Parent as ContextMenu).Parent as Popup).PlacementTarget as DataGrid).SelectedItems;
            SetMultiChannelIsEnabled(item, true);
        }

        private static void SetMultiChannelIsEnabled(IEnumerable<MultiMsgEndPoints> item, bool IsEnabled)
        {
            foreach (MultiMsgEndPoints msgEndPoints in item)
            {
                msgEndPoints.IsEnabled = IsEnabled;
            }
        }

        private void DataGridContextMenu_DisableItems_Click(object sender, RoutedEventArgs e)
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.GUIMultiLive, "Setting selected webhook items 'IsEnabled' to disabled.");

            IEnumerable<MultiMsgEndPoints> item = (IEnumerable<MultiMsgEndPoints>)((((sender as MenuItem).Parent as ContextMenu).Parent as Popup).PlacementTarget as DataGrid).SelectedItems;
            SetMultiChannelIsEnabled(item, false);
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
                        if (dc.Header.ToString() is not "Id" and not "ChannelName")
                        {
                            Collapse(dc);
                        }
                    }
                    break;
            }
        }

        private void ComboBox_DropDownOpened(object sender, EventArgs e)
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.GUIMultiLive, "Computing the date with stream count data for the combo box.");

            SummarizeChannels?.Invoke(this, new()
            {
                CallbackAction = () =>
                {
                    Dispatcher.BeginInvoke(() =>
                    {
                        ComboBox_SummarizeLive_List.Items.Refresh();
                    });
                }
            });
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.GUIMultiLive, "Enable the button to permit actually summarizing the stream data.");

            if (((ComboBox)sender).SelectedItem != null)
            {
                Button_StartSummarizingLiveData.IsEnabled = true;
            }
        }

        private void Button_StartSummarizingLiveData_Click(object sender, RoutedEventArgs e)
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.GUIMultiLive, "Disabling buttons and sending a summarizing message to the data manager.");

            TabItem_DailyData.IsEnabled = false;
            TabItem_SummaryDailyData.IsEnabled = false;
            Button_StartSummarizingLiveData.IsEnabled = false;

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
                    });
                }
            }
                                    );
        }
    }
}
