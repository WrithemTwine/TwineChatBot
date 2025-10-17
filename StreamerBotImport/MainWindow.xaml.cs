using StreamerBotLib.DataSQL;

using System.Windows;

namespace StreamerBotImport
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DataManagerSQL dataManagagerSQL;

        public MainWindow()
        {
            InitializeComponent();

            dataManagagerSQL = new();
        }
    }
}