using StreamerBotLib.BotClients;
using StreamerBotLib.BotIOController;
using StreamerBotLib.GUI.Data;
using StreamerBotLib.Static;
using StreamerBotLib.Systems.Overlay.Enums;
using StreamerBotLib.Systems.Overlay.Models;

using System.Windows;
using System.Windows.Controls;

namespace StreamerBot
{
    public partial class StreamerBotWindow
    {
        #region Overlay Service

        private void Button_Overlay_PauseAlerts_Click(object sender, RoutedEventArgs e)
        {
            ((sender as CheckBox).DataContext as BotOverlayServer).SetPauseAlert((sender as CheckBox).IsChecked == true);
        }

        private void Button_Overlay_ClearAlerts_Click(object sender, RoutedEventArgs e)
        {
            ((sender as Button).DataContext as BotOverlayServer).SetClearAlerts();
        }

        #endregion

        #region Overlay Bulk Load

        private void OverlayService_Bulk_ComboBoxType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((sender as ComboBox).SelectedItem != null)
            {
                OverlayService_Bulk_ComboBoxAction.ItemsSource = GUIOverlayTypeAlerts.GetValue(Enum.Parse<OverlayTypes>((sender as ComboBox).SelectedItem as string));
            }
        }

        private void OverlayService_Bulk_ComboBoxAction_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            OverlayService_Bulk_ButtonLoad.IsEnabled = OverlayService_Bulk_ComboBoxAction.SelectedItem != null && OverlayService_Bulk_ComboBoxType.SelectedItem != null;
        }

        private void OverlayService_Bulk_ButtonLoad_Click(object sender, RoutedEventArgs e)
        {
            OverlayActionType overlayAction = new() { OverlayType = Enum.Parse<OverlayTypes>(OverlayService_Bulk_ComboBoxType.SelectedItem as string), ActionValue = OverlayService_Bulk_ComboBoxAction.SelectedItem as string };

            OverlayServiceBulk bulk = new(overlayAction);
            bulk.OverlayServicesBulkAddData += Bulk_OverlayServicesBulkAddData;

            bulk.Show();
        }

        private void Bulk_OverlayServicesBulkAddData(object sender, StreamerBotLib.Models.Events.OverlayBulkAddEventArgs e)
        {
            BotController.DataBot.PostBulkOverlayActions(e.Items);
        }

        #endregion
    }
}
