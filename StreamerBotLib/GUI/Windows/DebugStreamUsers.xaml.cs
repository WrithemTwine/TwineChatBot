
namespace StreamerBotLib.GUI.Windows
{
    using System.Windows;

    /// <summary>
    /// Interaction logic for DebugStreamUsers.xaml
    /// </summary>
    public partial class DebugStreamUsers : Window
    {
        public event EventHandler AddDebugUsers;

        public DebugStreamUsers()
        {
            InitializeComponent();
        }

        private void Button_AddRandomUsers_Click(object sender, RoutedEventArgs e)
        {
            AddDebugUsers?.Invoke(this, new());
        }

    }
}
