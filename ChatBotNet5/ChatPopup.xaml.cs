using System.Windows;

namespace ChatBot_Net5
{
    /// <summary>
    /// Interaction logic for ChatPopup.xaml
    /// </summary>
    public partial class ChatPopup : Window
    {
        private Point LastMousePosition;

        public ChatPopup()
        {
            InitializeComponent();
            LastMousePosition = new(0, 0);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed && !Button_Close.IsMouseOver)
            {
                Point CurrMousePos = PointToScreen(e.GetPosition(this));
                
                if (LastMousePosition == new Point(0, 0) || LastMousePosition == CurrMousePos)
                {
                    LastMousePosition = CurrMousePos;
                }

                Left += CurrMousePos.X - LastMousePosition.X;
                Top += CurrMousePos.Y - LastMousePosition.Y;

                LastMousePosition = CurrMousePos;
            }
        }
    }
}
