using StreamerBotLib.BotIOController;
using StreamerBotLib.MultiLive;
using StreamerBotLib.Static;

using System.Windows;

namespace StreamerBot
{
    public partial class StreamerBotWindow
    {
        #region MultiLive
        private const string MultiLiveName = "MultiUserLiveBot";

        private void SetMultiLiveActive(bool ProcessFound = false)
        {
            if (ProcessFound)
            {
                BotController.DisconnectTwitchMultiLive();
            }
            else
            {
                BotController.ConnectTwitchMultiLive();
            }

            Label_LiveStream_MultiLiveActiveMsg.Visibility = ProcessFound ? Visibility.Visible : Visibility.Collapsed;
            GroupBox_Bots_Starts_MultiLive.Visibility = ProcessFound ? Visibility.Collapsed : Visibility.Visible;


            if (GroupBox_Bots_Starts_MultiLive.Visibility == Visibility.Visible)
            {
                Radio_MultiLiveTwitch_StartBot.IsEnabled = Radio_Twitch_LiveBotStart.IsChecked == true;

                // allow edits while bot is active
                (MultiLive_Data.Content as MultiLiveDataGrids).SetIsEnabled(true);
                (MultiLive_Data.Content as MultiLiveDataGrids).SetHandlers(Settings_LostFocus, TB_BotActivityLog_TextChanged);
                (MultiLive_Data.Content as MultiLiveDataGrids).SetDataManager(guiTwitchBot.TwitchLiveMonitor.MultiLiveDataManager);
            }
            else
            {
                Radio_MultiLiveTwitch_StartBot.IsEnabled = false;
                Radio_MultiLiveTwitch_StopBot.IsChecked = true;
                Radio_MultiLiveTwitch_StopBot.IsEnabled = false;

                // prevent edits while multilive bot is inactive - avoids conflict with standalone bot
                (MultiLive_Data.Content as MultiLiveDataGrids).SetIsEnabled(false);
            }
        }

        private void Radio_Twitch_LiveBotStart_Checked(object sender, RoutedEventArgs e)
        {
            if (Radio_MultiLiveTwitch_StartBot != null)
            {
                Radio_MultiLiveTwitch_StartBot.IsEnabled = true;

                if (OptionFlags.TwitchMultiLiveAutoStart)
                {
                    Radio_MultiLiveTwitch_StartBot.IsChecked = true;
                }
            }
        }

        private void Radio_Twitch_LiveBotStop_Checked(object sender, RoutedEventArgs e)
        {
            // stop MultiLive bot when LiveMonitor bot is stopped
            if (Radio_MultiLiveTwitch_StopBot != null)
            {
                Radio_MultiLiveTwitch_StopBot.IsChecked = true;
            }
        }

        private void BC_MultiLiveTwitch_BotOp(object sender, RoutedEventArgs e)
        {
            if (sender == Radio_MultiLiveTwitch_StartBot)
            {
                BotController.StartTwitchMultiLive();
                Radio_MultiLiveTwitch_StartBot.IsEnabled = false;
                Radio_MultiLiveTwitch_StartBot.IsChecked = true;
                Radio_MultiLiveTwitch_StopBot.IsChecked = false;
                Radio_MultiLiveTwitch_StopBot.IsEnabled = true;
            }
            else if (sender == Radio_MultiLiveTwitch_StopBot)
            {
                BotController.StopTwitchMultiLive();
                Radio_MultiLiveTwitch_StartBot.IsEnabled = true;
                Radio_MultiLiveTwitch_StartBot.IsChecked = false;
                Radio_MultiLiveTwitch_StopBot.IsChecked = true;
                Radio_MultiLiveTwitch_StopBot.IsEnabled = false;
            }
        }

        #endregion
    }
}
