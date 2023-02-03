using StreamerBotLib.MachineLearning.Accord.KNN;
using StreamerBotLib.Overlay.Communication;
using StreamerBotLib.Overlay.Control;
using StreamerBotLib.Overlay.Enums;
using StreamerBotLib.Overlay.GUI;
using StreamerBotLib.Overlay.Models;
using StreamerBotLib.Overlay.Server;
using StreamerBotLib.Static;

using System;
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

            UpdateLinks();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            UserHideWindow?.Invoke(this, new());
            Controller.StopServer();
        }

        private void CheckBox_Click_SaveSettings(object sender, RoutedEventArgs e)
        {
            OptionFlags.SetSettings();

            if ((sender as CheckBox).Name == "CheckBox_OptionSamePage")
            {
                UpdateLinks();
            }
        }

        private void Expander_Click_SaveSettings(object sender, RoutedEventArgs e)
        {
            OptionFlags.SetSettings();
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

        private void TextBox_PortNumber_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox port = sender as TextBox;

            OptionFlags.MediaOverlayMediaPort = TwineBotWebServer.ValidatePort(int.Parse(port.Text));
            UpdateLinks();
        }

        private void UpdateLinks()
        {
            AddEditPages();
            PrefixGenerator.GetPrefixes();
            GUIData.UpdateLinks();
        }

        private async void TextBox_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            await Application.Current.Dispatcher.InvokeAsync((sender as TextBox).SelectAll);
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            ((OverlayStyle)(sender as TextBox).DataContext).SaveFile();
        }

    }
}
