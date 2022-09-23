using System.Windows;

namespace SimpleTestFeature
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Dispatcher.Invoke(() =>
            {
                _ = typeof(MainWindow).InvokeMember(
                    "UpdateGUI",
                    System.Reflection.BindingFlags.InvokeMethod,
                    null,
                    this,
                    new[] { "Invoke Member worked." });
            });

        }

        public void UpdateGUI(string Text)
        {
            TextBlock_GUI_Text.Text = Text;
        }
    }
}
