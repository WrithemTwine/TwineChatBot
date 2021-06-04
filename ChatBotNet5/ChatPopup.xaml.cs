using System.Windows;

namespace ChatBot_Net5
{
    /// <summary>
    /// Interaction logic for ChatPopup.xaml
    /// </summary>
    public partial class ChatPopup : Window
    {
        public ChatPopup()
        {
            InitializeComponent();
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            Visibility = Visibility.Collapsed;
        }
    }
}
