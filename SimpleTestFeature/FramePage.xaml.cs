using System.Windows.Controls;

namespace SimpleTestFeature
{
    /// <summary>
    /// Interaction logic for FramePage.xaml
    /// </summary>
    public partial class FramePage : Page
    {
        public FramePage()
        {
            InitializeComponent();
        }

        public void SetDataContext(TestText ItemList)
        {
            DataContext = ItemList;
        }
    }
}
