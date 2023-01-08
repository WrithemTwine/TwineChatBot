using StreamerBotLib.BotClients.Twitch;
using StreamerBotLib.Data.MultiLive;
using StreamerBotLib.GUI.Windows;
using StreamerBotLib.Systems;

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace StreamerBotLib.MultiLive
{
    /// <summary>
    /// Interaction logic for MultiLiveDataGrids.xaml
    /// </summary>
    public partial class MultiLiveDataGrids : Page
    {
        private EventHandler<RoutedEventArgs> m_Routed;
        private EventHandler<TextChangedEventArgs> m_TextChanged;

        private static MultiDataManager MultiLiveData;
        private ManageWindows PopupWindows { get; set; } = new();

        public MultiLiveDataGrids()
        {
            InitializeComponent();
        }

        public void AddNewMonitorChannel(string UserName)
        {
            MultiLiveData.PostMonitorChannel(UserName);
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
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
            Grid_MultiUserLiveMonitor.IsEnabled = IsEnabled;
        }

        public void SetLiveManagerBot(MultiDataManager MultiLiveDataManager)
        {
            Grid_MultiUserLiveMonitor.DataContext = MultiLiveDataManager;
            MultiLiveData = MultiLiveDataManager;
        }

        public void SetHandlers(EventHandler<RoutedEventArgs> SettingsLostFocus, EventHandler<TextChangedEventArgs> TextChanged)
        {
            m_Routed = SettingsLostFocus;
            m_TextChanged = TextChanged;
        }

        private void DG_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DataGrid item = sender as DataGrid;

            Popup_DataEdit(item, false);
        }

        private void DG_Edit_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (sender.GetType() == typeof(DataGrid))
            {
                bool FoundAddEdit = ((DataGrid)sender).Name is "DG_Multi_WebHooks" or "DG_Multi_ChannelNames";
                bool FoundIsEnabled = MultiLiveData.CheckField(((DataView)((DataGrid)sender).ItemsSource).Table.TableName, "IsEnabled");

                foreach (var M in ((ContextMenu)Resources["DataGrid_ContextMenu"]).Items)
                {
                    if (M.GetType() == typeof(MenuItem))
                    {
                        if (((MenuItem)M).Name is "DataGridContextMenu_AddItem")
                        {
                            ((MenuItem)M).IsEnabled = FoundAddEdit;
                        } else if (((MenuItem)M).Name is "DataGridContextMenu_DeleteItem")
                        {
                            ((MenuItem)M).IsEnabled = true;
                        }
                        else if (((MenuItem)M).Name is "DataGridContextMenu_EnableItems" or "DataGridContextMenu_DisableItems")
                        {
                            ((MenuItem)M).IsEnabled = FoundIsEnabled;
                        }
                    }
                }
            }
        }

        private void Settings_LostFocus(object sender, RoutedEventArgs e)
        {
            m_Routed?.Invoke(sender, e);
        }

        private void TB_BotActivityLog_TextChanged(object sender, TextChangedEventArgs e)
        {
            m_TextChanged?.Invoke(sender, e);
        }

        private void Popup_DataEdit(DataGrid sourceDataGrid, bool AddNew = true)
        {
            if (AddNew)
            {
                DataView CurrdataView = (DataView)sourceDataGrid.ItemsSource;
                if (CurrdataView != null)
                {
                    PopupWindows.DataGridAddNewItem(CurrdataView.Table);
                }
            }
            else
            {
                DataRowView dataView = (DataRowView)sourceDataGrid.SelectedItem;
                if (dataView != null)
                {
                    PopupWindows.DataGridEditItem(dataView.Row.Table, dataView.Row);
                }
            }
        }

        private void MenuItem_AddClick(object sender, RoutedEventArgs e)
        {
            DataGrid item = (((sender as MenuItem).Parent as ContextMenu).Parent as Popup).PlacementTarget as DataGrid;

            Popup_DataEdit(item);
        }

        private void MenuItem_EditClick(object sender, RoutedEventArgs e)
        {
            DataGrid item = (((sender as MenuItem).Parent as ContextMenu).Parent as Popup).PlacementTarget as DataGrid;

            Popup_DataEdit(item, false);
        }

        private void MenuItem_DeleteClick(object sender, RoutedEventArgs e)
        {
            DataGrid item = (((sender as MenuItem).Parent as ContextMenu).Parent as Popup).PlacementTarget as DataGrid;

            SystemsController.DeleteRows(new List<DataRow>(item.SelectedItems.Cast<DataRowView>().Select(DRV => DRV.Row)));
        }

        private void DataGridContextMenu_EnableItems_Click(object sender, RoutedEventArgs e)
        {
            DataGrid item = (((sender as MenuItem).Parent as ContextMenu).Parent as Popup).PlacementTarget as DataGrid;

            MultiLiveData.UpdateIsEnabledRows(new List<DataRow>(item.SelectedItems.Cast<DataRowView>().Select(DRV => DRV.Row)), true);
        }

        private void DataGridContextMenu_DisableItems_Click(object sender, RoutedEventArgs e)
        {
            DataGrid item = (((sender as MenuItem).Parent as ContextMenu).Parent as Popup).PlacementTarget as DataGrid;

            MultiLiveData.UpdateIsEnabledRows(new List<DataRow>(item.SelectedItems.Cast<DataRowView>().Select(DRV => DRV.Row)), false);
        }

    }
}
