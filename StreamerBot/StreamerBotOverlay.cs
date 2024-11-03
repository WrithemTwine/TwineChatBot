using StreamerBotLib.BotClients;

using System.Windows;
using System.Windows.Controls;

namespace StreamerBot
{
    public partial class StreamerBotWindow
    {
        #region Overlay Service

        private void TabItem_Overlays_GotFocus(object sender, RoutedEventArgs e)
        {
            BeginGiveawayChannelPtsUpdate();
        }
        private void TabItem_ModApprove_GotFocus(object sender, RoutedEventArgs e)
        {
            BeginGiveawayChannelPtsUpdate();
        }

        private void Button_Overlay_PauseAlerts_Click(object sender, RoutedEventArgs e)
        {
            ((sender as CheckBox).DataContext as BotOverlayServer).SetPauseAlert((sender as CheckBox).IsChecked == true);
        }

        private void Button_Overlay_ClearAlerts_Click(object sender, RoutedEventArgs e)
        {
            ((sender as Button).DataContext as BotOverlayServer).SetClearAlerts();
        }

        private void UpdateOverlayChannelPointList(List<string> channelPointNames)
        {
            Controller.Systems.SetChannelRewardList(channelPointNames);
        }

        #endregion
    }
}
