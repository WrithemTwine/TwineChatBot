using System.Windows;

namespace StreamerBot.Web
{
    /// <summary>
    /// Interaction logic for AuthWebBrowser.xaml
    /// </summary>
    public partial class AuthWebBrowser : Window
    {
        public AuthWebBrowser()
        {
            InitializeComponent();
        }

        public void NavigateToURL(string URL)
        {
            TextBlock_AuthURL.Text = URL;
            AuthWebView.Source = new(URL);
            AuthWebView.EnsureCoreWebView2Async();
        }
    }
}
