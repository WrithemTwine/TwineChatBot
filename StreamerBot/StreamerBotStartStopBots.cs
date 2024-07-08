using StreamerBotLib.BotClients;
using StreamerBotLib.Enums;
using StreamerBotLib.Events;
using StreamerBotLib.Overlay;
using StreamerBotLib.Static;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace StreamerBot
{
    public partial class StreamerBotWindow
    {
        private MediaOverlayPage MediaOverlayPage = null;
        private MediaOverlay MediaOverlay = null;

        private void RadioButton_MediaOverlayServer_StartBot_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        { // actually attached to "PreviewMouseLeftButtonDown" to perform before starting the bot, as this determination is needed before trying to use MediaOverlayPage object
            CheckBox_MediaOverlayEmbedGUI.IsEnabled = false;
            string TabName = "TabItem_Bot_MediaOverlay_GUI";
            if (OptionFlags.MediaOverlayEmbedGUI)
            {
                Frame OverlayFrame = new()
                {
                    Source = new("pack://application:,,,/StreamerBotLib;component/Overlay/MediaOverlayPage.xaml"),
                    IsManipulationEnabled = true,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch
                };

                OverlayFrame.Loaded += MediaOverlayServer_Loaded;

                TabItem OverlayTab = new()
                {
                    Name = TabName,
                    Header = "Overlay",
                    Content = OverlayFrame
                };

                TabControl_Bots_Content.Items.Add(OverlayTab);
            }
            else
            {
                object OverlayTab = null;

                foreach (var T in TabControl_Bots_Content.Items)
                {
                    if (((TabItem)T).Name == TabName)
                    {
                        OverlayTab = T;
                    }
                }

                if (OverlayTab != null)
                {
                    TabControl_Bots_Content.Items.Remove(OverlayTab);
                }

                MediaOverlay = new();
                MediaOverlay.Show();
            }
        }

        private void RadioButton_MediaOverlayServer_StopBot_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            CheckBox_MediaOverlayEmbedGUI.IsEnabled = true;
        }

        private void MediaOverlayServer_Loaded(object sender, RoutedEventArgs e)
        {
            MediaOverlayPage = (MediaOverlayPage)((Frame)sender).Content;
        }

        private void MediaOverlayServer_SetOverlayWindow(object sender, SetOverlayWindowEventArgs e)
        {
            if (OptionFlags.MediaOverlayEmbedGUI)
            {
                e.SetOverlay(MediaOverlayPage);
            }
            else
            {
                e.SetOverlay(MediaOverlay);
            }
        }

        private void GUI_OnBotStarted(object sender, BotStartStopEventArgs e)
        {
            _ = Dispatcher.BeginInvoke(new BotOperation(() =>
            {
                if (!e.Started)
                {
                    ToggleInputEnabled(true);
                }
                else
                {
                    ToggleInputEnabled(false);
                    RadioButton radio = e.BotName switch
                    {
                        Bots.TwitchChatBot => Radio_Twitch_StartBot,
                        Bots.TwitchClipBot => Radio_Twitch_ClipBotStart,
                        Bots.TwitchFollowBot => Radio_Twitch_FollowBotStart,
                        Bots.TwitchLiveBot => Radio_Twitch_LiveBotStart,
                        Bots.TwitchMultiBot => Radio_MultiLiveTwitch_StartBot,
                        Bots.TwitchPubSub => Radio_Twitch_PubSubBotStart,
                        Bots.MediaOverlayServer => Radio_Services_OverlayBotStart,
                        Bots.Default => throw new NotImplementedException(),
                        Bots.TwitchUserBot => throw new NotImplementedException(),
                        _ => throw new NotImplementedException()
                    };
                    HelperStartBot(radio);

                    if (e.BotName == Bots.MediaOverlayServer)
                    {
                        Controller.Systems.SendInitialTickerItems();
                    }
                }
            }), null);
        }

        private void GuiTwitchBot_OnBotFailedStart(object sender, BotStartStopEventArgs e)
        {
            _ = Dispatcher.BeginInvoke(new BotOperation(() =>
            {
                ToggleInputEnabled(true);
                RadioButton radio = e.BotName switch
                {
                    Bots.TwitchChatBot => Radio_Twitch_StopBot,
                    Bots.TwitchClipBot => Radio_Twitch_ClipBotStop,
                    Bots.TwitchFollowBot => Radio_Twitch_FollowBotStop,
                    Bots.TwitchLiveBot => Radio_Twitch_LiveBotStop,
                    Bots.TwitchMultiBot => Radio_MultiLiveTwitch_StopBot,
                    Bots.TwitchPubSub => Radio_Twitch_PubSubBotStop,
                    Bots.MediaOverlayServer => Radio_Services_OverlayBotStop,
                    Bots.Default => throw new NotImplementedException(),
                    Bots.TwitchUserBot => throw new NotImplementedException(),
                    _ => throw new NotImplementedException()
                };
                HelperStopBot(radio);
            }), null);
        }

        private void GUI_OnBotStopped(object sender, BotStartStopEventArgs e)
        {
            _ = Dispatcher.BeginInvoke(new BotOperation(() =>
            {
                ToggleInputEnabled(true);
                RadioButton radio = e.BotName switch
                {
                    Bots.TwitchChatBot => Radio_Twitch_StopBot,
                    Bots.TwitchClipBot => Radio_Twitch_ClipBotStop,
                    Bots.TwitchFollowBot => Radio_Twitch_FollowBotStop,
                    Bots.TwitchLiveBot => Radio_Twitch_LiveBotStop,
                    Bots.TwitchMultiBot => Radio_MultiLiveTwitch_StopBot,
                    Bots.TwitchPubSub => Radio_Twitch_PubSubBotStop,
                    Bots.MediaOverlayServer => Radio_Services_OverlayBotStop,
                    Bots.Default => throw new NotImplementedException(),
                    Bots.TwitchUserBot => throw new NotImplementedException(),
                    _ => throw new NotImplementedException()
                };
                HelperStopBot(radio);
            }), null);
        }

        /// <summary>
        /// Will force start all bots currently not started, regardless of "start if Live" setting is selected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GUIStartBots_Click(object sender, RoutedEventArgs e)
        {
            foreach (var B in BotOps)
            {
                _ = Dispatcher.BeginInvoke(new BotOperation(() =>
                {
                    if (B.Item2.IsEnabled) // is enabled is a check the credentials are added to the bot
                    {
                        (B.Item2.DataContext as IOModule)?.StartBot();
                    }
                }), null);
            }
        }

        /// <summary>
        /// Will stop all bots currently not stopped, regardless of "stop when offline" setting.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GUIStopBots_Click(object sender, RoutedEventArgs e)
        {
            foreach (var B in BotOps)
            {
                _ = Dispatcher.BeginInvoke(new BotOperation(() =>
                {
                    (B.Item2.DataContext as IOModule)?.StopBot();
                }), null);
            }
        }

        /// <summary>
        /// Manages the GUI updates for when bots are started. Hides and enables buttons 
        /// to set the GUI for bots started and allows the user to Stop Bots.
        /// </summary>
        /// <param name="rb">The radio button of the started bot.</param>
        private static void HelperStartBot(RadioButton rb)
        {
            rb.IsChecked = true;

            foreach (UIElement child in (VisualTreeHelper.GetParent(rb) as WrapPanel).Children)
            {
                if (child.GetType() == typeof(RadioButton))
                {
                    (child as RadioButton).IsEnabled = (child as RadioButton).IsChecked != true;
                }
                else if (child.GetType() == typeof(Label))
                {
                    Label currLabel = (Label)child;
                    if (currLabel.Name.Contains("Start"))
                    {
                        currLabel.Visibility = Visibility.Visible;
                    }
                    else if (currLabel.Name.Contains("Stop"))
                    {
                        currLabel.Visibility = Visibility.Collapsed;
                    }
                }
            }
        }

        /// <summary>
        /// Manages the GUI updates for when bots are stopped. Hides and enables buttons
        /// to set the GUI for bots stopped and allows the user to Start Bots.
        /// </summary>
        /// <param name="rb">The radio button of the stopped bot.</param>
        private static void HelperStopBot(RadioButton rb)
        {
            rb.IsChecked = true;

            foreach (UIElement child in (VisualTreeHelper.GetParent(rb) as WrapPanel).Children)
            {
                if (child.GetType() == typeof(RadioButton))
                {
                    (child as RadioButton).IsEnabled = (child as RadioButton).IsChecked != true;
                }
                else if (child.GetType() == typeof(Label))
                {
                    Label currLabel = (Label)child;
                    if (currLabel.Name.Contains("Start"))
                    {
                        currLabel.Visibility = Visibility.Collapsed;
                    }
                    else if (currLabel.Name.Contains("Stop"))
                    {
                        currLabel.Visibility = Visibility.Visible;
                    }
                }
            }
        }

    }
}
