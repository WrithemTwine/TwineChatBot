using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;

using StreamerBotLib.Static;

using System.Windows;
using System.Windows.Controls;

namespace StreamerBot
{
    public partial class StreamerBotWindow
    {
        #region GitHub webpage
        /// <summary>
        /// Handles a WebView GUI control navigation, when it completes. The GitHub link resolves to a 
        /// link to a stable release link. Using this result, we can determine if a new stable version
        /// is available to the user.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WebView2_GitHub_StableVersion_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            string NewVersionLink = WebView2_GitHub_StableVersion.Source.ToString();

            // "https://github.com/WrithemTwine/TwineChatBot/releases/tag/v.1.2.10.0"
            if (NewVersionLink != OptionFlags.GitHubCheckStable)
            {
                OptionFlags.GitHubCheckStable = NewVersionLink;
            }

            string newversion = (from s in NewVersionLink.Split('/')
                                 select s).Last().Split('_').FirstOrDefault();

            string AppVersion = $"v.{App.Version.Major}.{App.Version.Minor}.{App.Version.Build}.{App.Version.Revision}";

            // check if the saved link is the default, also check if the found link doesn't have the current version
            // true=> link not default and the stable version link doesn't have the current app version in it
            if (OptionFlags.GitHubCheckStable != OptionFlags.GitHubStableLink && AppVersion.CompareTo(newversion) < 0)
            {
                StatusBarItem_NewStableVersion.Visibility = Visibility.Visible;
            }
        }

        private void WebView2Reload_Click(object sender, RoutedEventArgs e)
        {
            (((sender as Button).Parent as StackPanel).Children[1] as WebView2).Reload();
        }

        #endregion
    }
}
