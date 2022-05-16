using System.Windows;

using TwineStreamerBot.MediaOverlayServer.Static;

namespace TwineStreamerBot.MediaOverlayServer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            OptionFlags.ActiveToken = true;
            OptionFlags.SetSettings();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            OptionFlags.ActiveToken = false;


        }
    }
}
