
using MediaOverlayServer.Control;
using MediaOverlayServer.Enums;
using MediaOverlayServer.GUI;
using MediaOverlayServer.Models;
using MediaOverlayServer.Properties;
using MediaOverlayServer.Server;
using MediaOverlayServer.Static;

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace MediaOverlayServer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private OverlayController Controller { get; }
        private GUIData GUIData { get; }

        public MainWindow()
        {
            if (Settings.Default.SettingsUpgrade)
            {
                Settings.Default.Upgrade();
                Settings.Default.SettingsUpgrade = false;
            }
            OptionFlags.SetSettings();
            OptionFlags.ActiveToken = true;

            Controller = new();

            InitializeComponent();

            GUIData = (GUIData)Resources["GUIAppData"];
        }
 
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if(OptionFlags.MediaOverlayPort!=0 && OptionFlags.AutoStart)
            {
                RadioButton_OverlayServer_Start.IsChecked = true;
            }

            AddEditPages();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            OptionFlags.ActiveToken = false;
            Controller.StopServer();
        }

        private void CheckBox_Click_SaveSettings(object sender, RoutedEventArgs e)
        {
            OptionFlags.SetSettings();

            if((sender as CheckBox).Name == "CheckBox_OptionSamePage")
            {
                AddEditPages();
            }
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
            } else
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
            GUIData.UpdateLinks();
        }

        private void AddEditPages()
        {
            GUIData.ClearEditPages();
            if (OptionFlags.UseSameOverlayStyle)
            {
                GUIData.AddEditPage(OverlayTypes.None.ToString());
            }
            else
            {
                foreach (string O in Enum.GetNames(typeof(OverlayTypes)))
                {
                    GUIData.AddEditPage(O);
                }
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

            OptionFlags.MediaOverlayPort = TwineBotWebServer.ValidatePort(int.Parse(port.Text));
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
