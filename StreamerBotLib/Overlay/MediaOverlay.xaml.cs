using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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
