using System.Windows;
using System.Windows.Controls;

namespace ChatBot_Net5
{
    /// <summary>
    /// Interaction logic for ChatPopup.xaml
    /// </summary>
    public partial class ChatPopup : Page
    {
        public ChatPopup()
        {
            InitializeComponent();
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
        }
    }
}
