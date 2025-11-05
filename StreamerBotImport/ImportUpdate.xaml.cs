using StreamerBotImport.Events;
using StreamerBotImport.Import;

using StreamerBotLib.DataSQL;
using StreamerBotLib.Static;

using System.Windows;
using System.Windows.Controls;

namespace StreamerBotImport
{
    /// <summary>
    /// Interaction logic for ImportUpdate.xaml
    /// </summary>
    public partial class ImportUpdate : Page
    {
        private readonly DataManagerFactory dbContextFactory = new();

        public event EventHandler? ImportCompleted;

        public ImportUpdate()
        {
            InitializeComponent();
        }

        public void BeginImport()
        {
            DataManagerSQL ImportDM = new();

            ImportDataSources importDataSources = new(); // load the primary database data

            importDataSources.ProgressUpdate += HandleImplementProgressUpdate;
            importDataSources.ImportCompleted += HandleImportCompleted;
            importDataSources.UpdatedTableData += (s, e) =>
            {
                ThreadManager.AddTaskToGUIDispatcher(() =>
                {
                    TextBlock_TableCount.Text = e.TableCount.ToString();
                    TextBlock_RowCount.Text = e.RowCount.ToString();
                    ProgressBar_TotalProgress.Maximum = e.RowCount;
                    ProgressBar_TotalProgress.Value = 0;
                });
            };

            if (!OptionFlags.EFCDataImportedDataGram)
            {
                bool LogStatus = OptionFlags.LogBotStatus;  // save current logging status

                OptionFlags.LogBotStatus = true; // force logging operations to status during import

                ThreadManager.AddTaskToGUIDispatcher(() =>
                {
                    using var context = BuildDataContext();
                    importDataSources.ConvertData(context, new DataManagerSQL()); // convert data loaded from main and multilive data files
                    OptionFlags.LogBotStatus = LogStatus; // restore preferred log status after import
                    OptionFlags.EFCDataImportedDataGram = true;
                });
            }
            else
            {
                StackPanel_Import_Complete.Visibility = Visibility.Visible;
                ImportCompleted?.Invoke(this, EventArgs.Empty);
            }
        }

        private SQLDBContext BuildDataContext()
        {
            return dbContextFactory.CreateDbContext();
        }

        public void HandleImplementProgressUpdate(object sender, ImportDataProgressUpdateEventArgs e)
        {
            ThreadManager.AddTaskToGUIDispatcher(() =>
            {
                ProgressBar_TotalProgress.Value = e.CurrentProgressAmount;
            });
        }

        public void HandleImportCompleted(object sender, EventArgs e)
        {
            ThreadManager.AddTaskToGUIDispatcher(() =>
            {
                ProgressBar_TotalProgress.Value = ProgressBar_TotalProgress.Maximum;
                StackPanel_Import_Complete.Visibility = Visibility.Visible;
                ImportCompleted?.Invoke(this, EventArgs.Empty);
            });
        }
    }
}
