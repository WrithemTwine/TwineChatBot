using System.Windows;
using System.Windows.Controls;

namespace StreamerBotLib.Overlay
{
    /// <summary>
    /// Interaction logic for MediaOverlay.xaml
    /// </summary>
    public partial class MediaOverlay : Window
    {
        public event EventHandler<EventArgs> UserHideWindow;

        public MediaOverlay()
        {
            InitializeComponent();
        }

        public void AddVisibilityEvent(EventHandler<EventArgs> HideWindow = null)
        {
            UserHideWindow += HideWindow;
        }

        public void CloseApp()
        {
            Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            UserHideWindow?.Invoke(this, new());

            ((MediaOverlayPage)((Frame)Content).Content).StopController();
        }
    }
}
