using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
