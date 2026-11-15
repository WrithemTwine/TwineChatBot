using StreamerBotLib.Models.Enums;
using StreamerBotLib.Static;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace StreamerBot
{
    public partial class StreamerBotWindow
    {
        private void TB_BotActivityLog_TextChanged(object sender, TextChangedEventArgs e)
        {
            (sender as TextBox).ScrollToEnd();
        }
        private void TextBox_TwitchBotLog_TextChanged(object sender, TextChangedEventArgs e)
        {
            (sender as TextBox).ScrollToEnd();
        }

        /// <summary>
        /// Manage GUI element visibility based on user selection. The intent is to add friendly protection
        /// to prevent accidentally clicking buttons and deleting, often unrecoverable, data-or the latest backup at the least.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SetVisibility(object sender, RoutedEventArgs e)
        {
            if (Button_ClearCurrencyAccrlValues != null
                && Button_ClearCurrencyAccrlValues != null
                && Button_ClearWatchTime != null
                && GroupBox_ManageDataOptions != null)
            {
                Button_ClearCurrencyAccrlValues.IsEnabled = OptionFlags.ManageClearButtonEnabled;
                Button_ClearNonFollowers.IsEnabled = OptionFlags.ManageClearButtonEnabled;
                Button_ClearWatchTime.IsEnabled = OptionFlags.ManageClearButtonEnabled;
                GroupBox_ManageDataOptions.Visibility = OptionFlags.EnableManageDataOptions ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void CheckBox_ManageData_Click(object sender, RoutedEventArgs e)
        {
            // to enable Followers, the Manage Users and Manage Categories (Stream Data) must be enabled first

            if (CheckBox_ManageUsers.IsChecked == true && CheckBox_ManageStreamStat.IsChecked == true)
            {
                CheckBox_ManageFollowers.IsEnabled = true;
            }
            else if (CheckBox_ManageStreamStat.IsChecked == false)
            {
                CheckBox_ManageFollowers.IsEnabled = false;
                CheckBox_ManageFollowers.IsChecked = false; // requires the Manage Users to be enabled
            }

            if (CheckBox_ManageFollowers.IsChecked == true)
            {
                CheckBox_ManageUsers.IsEnabled = false;
                CheckBox_ManageStreamStat.IsEnabled = false;
            }
            else
            {
                CheckBox_ManageUsers.IsEnabled = true;
                CheckBox_ManageStreamStat.IsEnabled = true;
            }

            //if (CheckBox_ManageFollowers.IsChecked == true)
            //{
            //    CheckBox_ManageUsers.IsEnabled = false;
            //    if (Settings.Default.TwitchStreamEventSubAutoStart && Radio_Twitch_StreamerEventSubStart.IsChecked == true)
            //    {
            //        Dispatcher.BeginInvoke(new BotOperation(() =>
            //        {
            //            (Radio_Twitch_StreamerEventSubStart.DataContext as IOModule).StartBot();
            //        }), null);
            //    }
            //}
            //else
            //{
            //    CheckBox_ManageUsers.IsEnabled = true;
            //    if (Radio_Twitch_StreamerEventSubStart.IsChecked == true)
            //    {
            //        Dispatcher.BeginInvoke(new BotOperation(() =>
            //        {
            //            (Radio_Twitch_StreamerEventSubStart.DataContext as IOModule).StopBot();
            //        }), null);
            //    }
            //}

            Controller.ManageDatabase();
        }

        private async void PreviewMouseLeftButton_SelectAll(object sender, MouseButtonEventArgs e)
        {
            await Application.Current.Dispatcher.InvokeAsync((sender as TextBox).SelectAll);
        }

        private void CheckBox_TwitchAdsNotify_Checked(object sender, RoutedEventArgs e)
        {
            Controller.StartTwitchAdNotifications();
        }

        private void CheckBox_Checked_PanelVisibility(object sender, RoutedEventArgs e)
        {
            ToggleButton TBSource = null;
            StackPanel SPSource = null;
            GroupBox GBSource = null;
            if (sender.GetType() == typeof(CheckBox) || sender.GetType() == typeof(RadioButton))
            {
                TBSource = (ToggleButton)sender;
            }
            else if (sender.GetType() == typeof(StackPanel))
            {
                SPSource = (StackPanel)sender;
            }
            else if (sender.GetType() == typeof(GroupBox))
            {
                GBSource = (GroupBox)sender;
            }

            static void SetVisibility(ToggleButton box, UIElement panel)
            {
                if (panel != null)
                {
                    panel.Visibility = (box.IsChecked == true) switch
                    {
                        true => Visibility.Visible,
                        false => Visibility.Collapsed
                    };
                }
            }

            // be sure this list is in XAML object order
            if (TBSource?.Name == CheckBox_RepeatCommands_Enable?.Name || SPSource?.Name == StackPanel_RepeatCommands_RepeatOptions?.Name)
            {
                SetVisibility(CheckBox_RepeatCommands_Enable, StackPanel_RepeatCommands_RepeatOptions);
                if (GroupBox_Options_RepeatCommands != null)
                {
                    GroupBox_Options_RepeatCommands.Width = CheckBox_RepeatCommands_Enable.IsChecked == true ? 460 : 230;
                }
                Controller.ActivateRepeatTimers();
            }
            else if (TBSource?.Name == RadioButton_RepeatTimer_NoAdjustment.Name || TBSource?.Name == RadioButton_RepeatTimer_SlowDownOption.Name || TBSource?.Name == RadioButton_RepeatTimer_ThresholdOption.Name || GBSource?.Name == GroupBox_RepeatTimer_ThresholdOptions.Name)
            {
                if (RadioButton_RepeatTimer_NoAdjustment != null && RadioButton_RepeatTimer_SlowDownOption != null && GroupBox_RepeatTimer_ThresholdOptions != null)
                {
                    SetVisibility(RadioButton_RepeatTimer_SlowDownOption.IsChecked == true ? RadioButton_RepeatTimer_SlowDownOption : RadioButton_RepeatTimer_ThresholdOption, GroupBox_RepeatTimer_ThresholdOptions);
                }
            }
            else if (TBSource?.Name == CheckBox_MediaOverlay_Enable.Name || SPSource?.Name == StackPanel_MediaOverlay_MediaOptions.Name)
            {
                SetVisibility(CheckBox_MediaOverlay_Enable, StackPanel_MediaOverlay_MediaOptions);

                TabItem_Overlays?.Visibility = CheckBox_MediaOverlay_Enable.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
            }
            else if (TBSource?.Name == Options_MediaOverlay_CheckBox_UseMultipleEvents.Name || SPSource?.Name == Options_MediaOverlay_StackPanel_MultiEventOptions.Name)
            {
                SetVisibility(Options_MediaOverlay_CheckBox_UseMultipleEvents, Options_MediaOverlay_StackPanel_MultiEventOptions);
            }
            else if (TBSource?.Name == Options_CheckBox_UseSchedule.Name)
            {
                SetVisibility(Options_CheckBox_UseSchedule, TabItem_Schedule);

                TabItem_Schedule?.Visibility = Options_CheckBox_UseSchedule.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
            }
            else if (TBSource?.Name == CheckBox_ModFollower_BanEnable.Name || SPSource?.Name == StackPanel_ModerateFollowers_Count.Name)
            {
                SetVisibility(CheckBox_ModFollower_BanEnable, StackPanel_ModerateFollowers_Count);
            }
            else if (TBSource?.Name == CheckBox_TwitchFollower_LimitMsgs.Name || SPSource?.Name == StackPanel_TwitchFollower_LimitMsgs_Count.Name)
            {
                SetVisibility(CheckBox_TwitchFollower_LimitMsgs, StackPanel_TwitchFollower_LimitMsgs_Count);
            }
            else if (TBSource?.Name == CheckBox_TwitchFollower_AutoRefresh.Name || SPSource?.Name == StackPanel_TwitchFollows_RefreshHrs.Name)
            {
                SetVisibility(CheckBox_TwitchFollower_AutoRefresh, StackPanel_TwitchFollows_RefreshHrs);
            }
        }

        private void CheckBox_RepeatWhenLive_UnChecked(object sender, RoutedEventArgs e)
        {
            // setup the repeat timer; setting is unchecked so the repeat timer should be started for the user
            Controller.ActivateRepeatTimers();
        }

        private void TextBox_Follower_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox src = (TextBox)sender;

            if (int.TryParse(src.Text, out int result))
            {
                if (result is < 1 or > 100)
                {
                    src.Text = (result < 1 ? 1 : result > 100 ? 100 : result).ToString();
                }
            }
        }

        private void TextBlock_MouseEnter_Visible(object sender, MouseEventArgs e)
        {
            TextBlock_AppDataDir.Visibility = Visibility.Visible;
        }

        private void TextBlock_MouseEnter_Hidden(object sender, MouseEventArgs e)
        {
            TextBlock_AppDataDir.Visibility = Visibility.Hidden;
        }

        private void Button_ResetStreamCount_Click(object sender, RoutedEventArgs e)
        {
            Controller.ResetCategoryStreamCount();
        }


        #region Bot_Ops
        private void Button_SystemEvents_Click(object sender, RoutedEventArgs e)
        {
            Controller.SetSystemEventsEnabled(((Button)sender).Name.Contains("Enabled"));
        }

        private void Button_BuiltInCommands_Click(object sender, RoutedEventArgs e)
        {
            Controller.SetBuiltInCommandsEnabled(((Button)sender).Name.Contains("Enabled"));
        }

        private void Button_UserDefinedCommands_Click(object sender, RoutedEventArgs e)
        {
            Controller.SetUserDefinedCommandsEnabled(((Button)sender).Name.Contains("Enabled"));
        }

        private void Button_DiscordWebhooks_Click(object sender, RoutedEventArgs e)
        {
            Controller.SetDiscordWebhooksEnabled(((Button)sender).Name.Contains("Enabled"));
        }

        private void ShoutUsers_Click(object sender, RoutedEventArgs e)
        {
            Controller.HandleChatCommandReceived(
                new(commandText: $"{DefaultCommand.soactive}",
                     commandArguments: [""],
                     userType: ViewerTypes.Broadcaster,
                     isBroadcaster: true,
                     displayName: OptionFlags.TwitchChannelName,
                     channel: OptionFlags.TwitchChannelName,
                     message: $"{DefaultCommand.soactive}"));
        }
        private void Button_RefreshCategory_Click(object sender, RoutedEventArgs e)
        {
            LogWriter.DebugLog("Button_RefreshCategory_Click", DebugLogTypes.GUIEvents, "User pushed the category button.");

            BeginUpdateCategory();
        }


        #endregion
    }
}
