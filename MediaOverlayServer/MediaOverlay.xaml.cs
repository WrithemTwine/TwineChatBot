using System.Windows;

using MediaOverlayServer.Control;
using MediaOverlayServer.GUI;
using MediaOverlayServer.Properties;
using MediaOverlayServer.Static;

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

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            OptionFlags.ActiveToken = false;
        }

        private void CheckBox_Click_SaveSettings(object sender, RoutedEventArgs e)
        {
            OptionFlags.SetSettings();


        }

        private void RadioButton_OverlayServer_Start_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void RadioButton_OverlayServer_Stop_Checked(object sender, RoutedEventArgs e)
        {

        }
    }
}
