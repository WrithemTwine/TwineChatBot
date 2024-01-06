using StreamerBotLib.Events;
using StreamerBotLib.Overlay.Communication;
using StreamerBotLib.Overlay.Control;
using StreamerBotLib.Overlay.Enums;
using StreamerBotLib.Overlay.GUI;
using StreamerBotLib.Overlay.Models;
using StreamerBotLib.Overlay.Static;
using StreamerBotLib.Static;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace StreamerBotLib.Overlay
{
    // TODO: test new port number or add another server to mitigate pausing videoes

    /// <summary>
    /// Interaction logic for MediaOverlay.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public event EventHandler<EventArgs> UserHideWindow;

        private OverlayController Controller { get; }
        private GUIData GUIData { get; }

        public MainWindow(EventHandler<EventArgs> HideWindow = null)
        {
            Controller = new();

            InitializeComponent();

            TabControl_OverlayStyles.TabStripPlacement = Dock.Bottom;

            GUIData = (GUIData)Resources["GUIAppData"];

            // when overlay server is offline/not started, main bot queues alerts
            // starting this bot then tries to process alerts without loading styles
            // trying to set the alert html text lead to NULL exception when no style available
            // to match the alert, so the bot crashed.
            // UpdateLinks loads the styles in case alerts are waiting to be processed
            UpdateLinks();

            UserHideWindow += HideWindow;
        }

        #region Connect to Main App
        public EventHandler<OverlayActionType> GetOverlayActionReceivedHandler()
        {
            return ReceivedOverlayEvent;
        }

        public void ReceivedOverlayEvent(object sender, OverlayActionType e)
        {
            Controller.SendAlert(new OverlayPage()
            {
                OverlayType = e.OverlayType.ToString(),
                OverlayHyperText = ProcessHyperText.ProcessOverlay(
                    e,
                        GUIData.OverlayEditStyles.Find((f) =>
                            f.OverlayType == e.OverlayType.ToString()
                        || f.OverlayType == PublicConstants.OverlayAllActions
                    ))
            });
            GUIData.UpdateStat(e.OverlayType.ToString());
        }

        public EventHandler<UpdatedTickerItemsEventArgs> GetupdatedTickerReceivedHandler()
        {
            return TickerReceivedEvent;
        }

        public void TickerReceivedEvent(object sender, UpdatedTickerItemsEventArgs e)
        {
            Controller.SetTickerData(e.TickerItems, GUIData.OverlayEditStyles);
        }

        public void CloseApp(bool Token = false)
        {
            Close();
        }

        #endregion

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (OptionFlags.MediaOverlayMediaActionPort != 0 && OptionFlags.MediaOverlayAutoServerStart)
            {
                RadioButton_OverlayServer_Start.IsChecked = true;
            }

            TickerSelections(sender, new());
            UpdateLinks();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            UserHideWindow?.Invoke(this, new());
            Controller.StopServer();
        }

        private void Click_UpdateLinks(object sender, RoutedEventArgs e)
        {
            UpdateLinks();
        }

        /// <summary>
        /// Disable user access to certain GUI elements depending on whether the HTTP Server is started.
        /// </summary>
        /// <param name="ServerStarted">If the Server is started</param>
        private void SetIsEnabled(bool ServerStarted = true)
        {
            if (ServerStarted)
            {
                RadioButton_OverlayServer_Start.IsEnabled = false;
                RadioButton_OverlayServer_Stop.IsEnabled = true;
                TextBox_ActionPortNumber.IsEnabled = false;
                TextBox_TickerPortNumber.IsEnabled = false;
                CheckBox_OptionSamePage.IsEnabled = false;
            }
            else
            {
                RadioButton_OverlayServer_Start.IsEnabled = true;
                RadioButton_OverlayServer_Stop.IsEnabled = false;
                TextBox_ActionPortNumber.IsEnabled = true;
                TextBox_TickerPortNumber.IsEnabled = true;
                CheckBox_OptionSamePage.IsEnabled = true;
            }
        }

        private void RadioButton_OverlayServer_Start_Checked(object sender, RoutedEventArgs e)
        {
            SetIsEnabled();
            Controller.StartServer();
        }

        private void AddEditPages()
        {
            GUIData.ClearEditPages();
            if (OptionFlags.MediaOverlayUseSameStyle)
            {
                GUIData.AddEditPage(OverlayTypes.None.ToString());
            }
            else
            {
                GUIData.AddEditPage(Enum.GetNames(typeof(OverlayTypes)));
            }

            if (!OptionFlags.MediaOverlayTickerMulti)
            {
                GUIData.AddEditPage(new List<OverlayTickerItem>(from SelectedTickerItem S in TickerFormatter.selectedTickerItems
                                                                select (OverlayTickerItem)Enum.Parse(typeof(OverlayTickerItem), S.OverlayTickerItem)).ToArray());
            }
            else
            {
                // if multi, just send 1 item and the style is set
                GUIData.AddEditPage(OverlayTickerItem.LastFollower);

                if (OptionFlags.MediaOverlayTickerMarquee)
                {
                    GUIData.AddEditPage(TickerStyle.MultiMarquee);
                }
                else if (OptionFlags.MediaOverlayTickerRotate)
                {
                    GUIData.AddEditPage(TickerStyle.MultiRotate);
                }
            }

            BuildStylePages();
        }

        /// <summary>
        /// The GUI presents the overlay styles relevant to the current user selections.
        /// This builds and rebuils those styles within a tabcontrol.
        /// Reads the current collection, builds a textbox to edit the data, and adds as a tab to the GUI.
        /// </summary>
        private void BuildStylePages()
        {
            if (TabControl_OverlayStyles.Items.Count > 0)
            {
                foreach (var item in TabControl_OverlayStyles.Items)
                {
                    (item as TabItem).Template = null;
                }
            }
            TabControl_OverlayStyles.Items.Clear();

            foreach (OverlayStyle O in GUIData.OverlayEditStyles)
            {
                if (O.OverlayType != OverlayTypes.None.ToString() || OptionFlags.MediaOverlayUseSameStyle)
                {
                    TextBox textBox = new()
                    {
                        DataContext = O,
                        AcceptsReturn = true
                    };

                    Binding tabContentBinding = new(nameof(O.OverlayStyleText))
                    {
                        Source = O
                    };

                    textBox.SetBinding(TextBox.TextProperty, tabContentBinding);
                    textBox.LostFocus += TextBox_LostFocus;

                    ScrollViewer sv = new() { Content = textBox };
                    DockPanel doc = new();
                    doc.Children.Add(sv);

                    TabItem newTab = new() { Header = O.OverlayType, Content = doc };

                    TabControl_OverlayStyles.Items.Add(newTab);
                }
            }

            if (TabControl_OverlayStyles.Items.Count > 0)
            {
                TabControl_OverlayStyles.SelectedIndex = 0;
            }
        }

        private void RadioButton_OverlayServer_Stop_Checked(object sender, RoutedEventArgs e)
        {
            SetIsEnabled(false);
            Controller.StopServer();
        }

        private void UpdateLinks()
        {
            AddEditPages();
            GUIData.UpdateLinks();
        }

        private async void TextBox_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            await Application.Current.Dispatcher.InvokeAsync((sender as TextBox).SelectAll);
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            ((OverlayStyle)(sender as TextBox).DataContext).SaveFile();
            Controller.UpdateTicker(GUIData.OverlayEditStyles);
        }

        private void TickerSelections(object sender, RoutedEventArgs e)
        {
            SelectedTickerItems selectedTickerItems = ((SelectedTickerItems)Resources["TickerSelectedItems"]);
            selectedTickerItems.SaveSelections();
            Controller.SetTickerItems(selectedTickerItems.GetSelectedTickers());
            UpdateLinks();
        }

        private void TickerSpecChanges(object sender, RoutedEventArgs e)
        {
            AddEditPages();
            Controller.UpdateTicker(GUIData.OverlayEditStyles);
            UpdateLinks();
        }

        /// <summary>
        /// Occurs when the user edits a ticker presentation style duration. We need to rebuild the style to 
        /// use the new duration.
        /// </summary>
        /// <param name="sender">where the event came from -unused</param>
        /// <param name="e">the info sent with the event -unused</param>
        private void MarqueeTypeSecondsLostFocus(object sender, RoutedEventArgs e)
        {
            // choose which page to update depending on selected presentation
            if (OptionFlags.MediaOverlayTickerMarquee)
            {
                GUIData.UpdateEditPage(TickerStyle.MultiMarquee);
            }
            else if (OptionFlags.MediaOverlayTickerRotate)
            {
                GUIData.UpdateEditPage(TickerStyle.MultiRotate);
            }

            // refresh the styles presented in the GUI, due to the duration change
            BuildStylePages();
        }
    }
}
