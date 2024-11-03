using StreamerBotLib.Events;

using System.Windows;

namespace StreamerBotLib.GUI.Windows
{
    /// <summary>
    /// Interaction logic for ImportUpdate.xaml
    /// </summary>
    public partial class ImportUpdate : Window
    {
        public ImportUpdate(int tables, int rows)
        {
            InitializeComponent();

            TextBlock_TableCount.Text = tables.ToString();
            TextBlock_RowCount.Text = rows.ToString();

            ProgressBar_TotalProgress.Maximum = rows;
            ProgressBar_TotalProgress.Value = 0;
        }

        public void HandleImplementProgressUpdate(object sender, ImportDataProgressUpdateEventArgs e)
        {
            ProgressBar_TotalProgress.Value = e.CurrentProgressAmount;
        }


        public void HandleImportCompleted(object sender, EventArgs e)
        {
            ProgressBar_TotalProgress.Value = ProgressBar_TotalProgress.Maximum;
            Button_Complete.Visibility = Visibility.Visible;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
