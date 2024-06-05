using System.Windows;
using System.Windows.Threading;

namespace SimpleTestFeature
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static Dispatcher MainDispatcher => Dispatcher.CurrentDispatcher;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string x = "";
        }
    }
}
