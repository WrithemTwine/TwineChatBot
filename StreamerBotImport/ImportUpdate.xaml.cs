using StreamerBotLib.DataSQL;
using StreamerBotLib.Models.Events;
using StreamerBotLib.Static;

using System.Windows;

namespace StreamerBotImport
{
    /// <summary>
    /// Interaction logic for ImportUpdate.xaml
    /// </summary>
    public partial class ImportUpdate : Window
    {
        private readonly DataManagerFactory dbContextFactory = new();


        public ImportUpdate(int tables, int rows)
        {
            InitializeComponent();

            TextBlock_TableCount.Text = tables.ToString();
            TextBlock_RowCount.Text = rows.ToString();

            ProgressBar_TotalProgress.Maximum = rows;
            ProgressBar_TotalProgress.Value = 0;


            if (!OptionFlags.EFCDataImportedDataGram)
            {
                bool LogStatus = OptionFlags.LogBotStatus;  // save current logging status

                OptionFlags.LogBotStatus = true; // force logging operations to status during import

                using var context = BuildDataContext();

                ThreadManager.AddTaskToGUIDispatcher(() =>
                {
                    ImportDataSources importDataSources = new(); // load the primary database data
                    importDataSources.ConvertData(context, this); // convert data loaded from main and multilive data files
                    OptionFlags.LogBotStatus = LogStatus; // restore preferred log status after import
                    OptionFlags.EFCDataImportedDataGram = true;
                });

            }
        }
        private SQLDBContext BuildDataContext()
        {
            return dbContextFactory.CreateDbContext();
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
