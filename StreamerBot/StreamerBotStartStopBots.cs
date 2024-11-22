using StreamerBotLib.BotClients;
using StreamerBotLib.Enums;
using StreamerBotLib.Events;
using StreamerBotLib.Overlay;
using StreamerBotLib.Static;

using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace StreamerBot
{
    public partial class StreamerBotWindow
    {
        private MediaOverlayPage MediaOverlayPage = null;
        private MediaOverlay MediaOverlay = null;

        private void MediaOverlayServer_SetOverlayWindow(object sender, SetOverlayWindowEventArgs e)
        {
            ThreadManager.CreateThreadStart(MethodBase.GetCurrentMethod().Name, () =>
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    e.SetOverlay(((MediaOverlayPage)((Frame)TabItem_Bots_OverlayTab.Content).Content));
                }), null);
            });
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
                        Bots.TwitchBotEventSub => Radio_Twitch_BotEventSubStart,
                        Bots.TwitchClipBot => Radio_Twitch_ClipBotStart,
                        Bots.TwitchMultiBot => Radio_Twitch_LiveBotStart,
                        Bots.TwitchStreamerEventSubScopes => Radio_Twitch_StreamerEventSubStart,
                        Bots.MediaOverlayServer => Radio_Services_OverlayBotStart,
                        Bots.TwitchHelixBot => throw new NotImplementedException(),
                        Bots.DiscordWebhooks => throw new NotImplementedException(),
                        Bots.TwitchBotSendChatClient => throw new NotImplementedException(),
                        Bots.TwitchTokenBot => throw new NotImplementedException(),
                        Bots.Default => throw new NotImplementedException(),
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
                    Bots.TwitchClipBot => Radio_Twitch_ClipBotStop,
                    Bots.TwitchMultiBot => Radio_Twitch_LiveBotStop,
                    Bots.TwitchBotEventSub => Radio_Twitch_BotEventSubStop,
                    Bots.TwitchStreamerEventSubScopes => Radio_Twitch_StreamerEventSubStop,
                    Bots.MediaOverlayServer => Radio_Services_OverlayBotStop,
                    Bots.TwitchHelixBot => throw new NotImplementedException(),
                    Bots.DiscordWebhooks => throw new NotImplementedException(),
                    Bots.TwitchBotSendChatClient => throw new NotImplementedException(),
                    Bots.TwitchTokenBot => throw new NotImplementedException(),
                    Bots.Default => throw new NotImplementedException(),
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
                    Bots.TwitchClipBot => Radio_Twitch_ClipBotStop,
                    Bots.TwitchMultiBot => Radio_Twitch_LiveBotStop,
                    Bots.TwitchBotEventSub => Radio_Twitch_BotEventSubStop,
                    Bots.TwitchStreamerEventSubScopes => Radio_Twitch_StreamerEventSubStop,
                    Bots.MediaOverlayServer => Radio_Services_OverlayBotStop,
                    Bots.TwitchBotSendChatClient => Radio_Twitch_BotEventSubStop,
                    Bots.TwitchHelixBot => throw new NotImplementedException(),
                    Bots.DiscordWebhooks => throw new NotImplementedException(),
                    Bots.TwitchTokenBot => throw new NotImplementedException(),
                    Bots.Default => throw new NotImplementedException(),
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
                if (B.Item3.IsEnabled) // isenabled is a check the credentials are added to the bot
                {

                    DispatchStartBot(B.Item3.DataContext as IOModule);
                }
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
                DispatchStopBot(B.Item3.DataContext as IOModule);
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
