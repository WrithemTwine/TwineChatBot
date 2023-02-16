using StreamerBotLib.Enums;
using StreamerBotLib.Events;
using StreamerBotLib.Overlay.Communication;
using StreamerBotLib.Overlay.Control;
using StreamerBotLib.Overlay.Enums;
using StreamerBotLib.Overlay.GUI;
using StreamerBotLib.Overlay.Models;
using StreamerBotLib.Static;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace StreamerBotLib.Overlay
{
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

            UserHideWindow += HideWindow;
        }

        #region Connect to Main App
        public EventHandler<OverlayActionType> GetOverlayActionReceivedHandler()
        {
            return ReceivedOverlayEvent;
        }

        public void ReceivedOverlayEvent(object sender, OverlayActionType e)
        {
            Controller.SendAlert(new OverlayPage() { OverlayType = e.OverlayType.ToString(), OverlayHyperText = ProcessHyperText.ProcessOverlay(e) });
            GUIData.UpdateStat(e.OverlayType.ToString());
        }

        public EventHandler<UpdatedTickerItemsEventArgs> GetupdatedTickerReceivedHandler()
        {
            return TickerReceivedEvent;
        }

        public void TickerReceivedEvent(object sender, UpdatedTickerItemsEventArgs e)
        {
            Controller.SetTickerData(e.TickerItems);
        }

        public void CloseApp(bool Token = false)
        {            
            Close();
        }

        #endregion

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (OptionFlags.MediaOverlayMediaPort != 0 && OptionFlags.MediaOverlayAutoStart)
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
                TextBox_PortNumber.IsEnabled = false;
                CheckBox_OptionSamePage.IsEnabled = false;
            }
            else
            {
                RadioButton_OverlayServer_Start.IsEnabled = true;
                RadioButton_OverlayServer_Stop.IsEnabled = false;
                TextBox_PortNumber.IsEnabled = true;
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

            if (OptionFlags.MediaOverlayTickerMulti)
            {
                // if multi, just send 1 item and the style is set
                GUIData.AddEditPage(OverlayTickerItem.LastFollower);
            }
            else
            {
                GUIData.AddEditPage(new List<OverlayTickerItem>(from SelectedTickerItem S in TickerFormatter.selectedTickerItems
                                        select (OverlayTickerItem)Enum.Parse(typeof(OverlayTickerItem), S.OverlayTickerItem)).ToArray());

            }

            TabControl_OverlayStyles.TabStripPlacement = Dock.Bottom;

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
            Controller.UpdateTicker();
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
            Controller.UpdateTicker();
            UpdateLinks();
        }
    }
}
