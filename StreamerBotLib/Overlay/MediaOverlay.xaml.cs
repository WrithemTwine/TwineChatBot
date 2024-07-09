using StreamerBotLib.Themes;

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

        public void WindowThemeChanged(object sender, RoutedEventArgs e)
        {
            SetTheme();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SetTheme();
        }

        public void AddVisibilityEvent(EventHandler<EventArgs> HideWindow = null)
        {
            UserHideWindow += HideWindow;
        }

        /// <summary>
        /// Updates the current theme per the user's selection.
        /// </summary>
        private void SetTheme()
        {
            Application.Current.Resources.MergedDictionaries[0].Source = new(ThemeSelector.GetCurrentTheme(), UriKind.Absolute);
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
