using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace StreamerBot.Twitch
{
    /// <summary>
    /// Interaction logic for AuthCodeTokenHelp.xaml
    /// </summary>
    public partial class AuthCodeTokenHelp : Page
    {
        public AuthCodeTokenHelp()
        {
            InitializeComponent();
        }

        private async void PreviewMouseLeftButton_SelectAll(object sender, MouseButtonEventArgs e)
        {
            await Application.Current.Dispatcher.InvokeAsync((sender as TextBox).SelectAll);
        }
    }
}
