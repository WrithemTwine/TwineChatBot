using StreamerBotLib.Static;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace StreamerBot.Twitch
{
    /// <summary>
    /// Interaction logic for ManualTokenHelp.xaml
    /// </summary>
    public partial class ManualTokenHelp : Page
    {
        public ManualTokenHelp()
        {
            InitializeComponent();
        }

        private async void PreviewMouseLeftButton_SelectAll(object sender, MouseButtonEventArgs e)
        {
            await Application.Current.Dispatcher.InvokeAsync((sender as TextBox).SelectAll);
        }

        public void SetScopes()
        {
            if (OptionFlags.TwitchStreamerUseToken)
            {
                Help_TwitchBot_DiffAuthScopes_Bot.Visibility = Visibility.Visible;
                Help_TwitchBot_DiffAuthScopes_Streamer.Visibility = Visibility.Visible;
                Help_TwitchBot_SameAuthScopes.Visibility = Visibility.Collapsed;
            }
            else
            {
                Help_TwitchBot_DiffAuthScopes_Bot.Visibility = Visibility.Collapsed;
                Help_TwitchBot_DiffAuthScopes_Streamer.Visibility = Visibility.Collapsed;
                Help_TwitchBot_SameAuthScopes.Visibility = Visibility.Visible;
            }
        }
    }
}
