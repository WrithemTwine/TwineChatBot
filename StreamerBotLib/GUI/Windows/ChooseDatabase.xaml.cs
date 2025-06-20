using StreamerBotLib.Static;

using System.Windows;
using System.Windows.Controls;

namespace StreamerBotLib.GUI.Windows
{
    /// <summary>
    /// Interaction logic for ChooseDatabase.xaml
    /// </summary>
    public partial class ChooseDatabase : Window
    {
        public event EventHandler<EventArgs> ExitApp;

        public ChooseDatabase()
        {
            InitializeComponent();

#if DEBUG || RELEASE_SQLITE
            StackPanel_Sqlite.Visibility = Visibility.Visible;
#elif RELEASE_POSTGRE
            StackPanel_PostgreSQL.Visibility = Visibility.Visible;
#elif RELEASE_SQLSERVER
            StackPanel_SQLServer.Visibility = Visibility.Visible;
#elif RELEASE_KNET
            StackPanel_KNet.Visibility = Visibility.Visible;
#elif RELEASE_COSMOS
            StackPanel_Cosmos.Visibility = Visibility.Visible;
#elif RELEASE_MYSQL
            StackPanel_MySQL.Visibility = Visibility.Visible;
#endif

        }

        private void Button_ApplyClick(object sender, RoutedEventArgs e)
        {
            // set flag the user performed the initial setup for a specific database provider
#if DEBUG || RELEASE_SQLITE
            OptionFlags.EFCDatabaseProviderSqlite = true;
#elif RELEASE_POSTGRE
            OptionFlags.EFCDatabaseProviderPostgreSQL = true;
#elif RELEASE_SQLSERVER
            OptionFlags.EFCDatabaseProviderSqlServer = true;
#elif RELEASE_KNET
            OptionFlags.EFCDatabaseProviderKNet = true;
#elif RELEASE_COSMOS
            OptionFlags.EFCDatabaseProviderCosmos = true;
#elif RELEASE_MYSQL
            OptionFlags.EFCDatabaseProviderMySql = true;
#endif
            Close();
        }

        private void Button_ExitClick(object sender, RoutedEventArgs e)
        {
            Close();
            ExitApp?.Invoke(this, EventArgs.Empty);
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            bool found = true;

            // iterate database specific textboxes for text length
#if DEBUG || RELEASE_SQLITE
            found = found && TextBox_SqliteConnect.Text.Length != 0;
#elif RELEASE_POSTGRE
            found = found && TextBox_PostgreSQLConnect.Text.Length != 0;
#elif RELEASE_SQLSERVER
            found = found && TextBox_SQLServerConnect.Text.Length != 0;
#elif RELEASE_KNET
            found = found && TextBox_KNetApplicationId.Text.Length != 0;
            found = found && TextBox_KNetBootstrapServers.Text.Length != 0;
            found = found && TextBox_KNetDatabaseName.Text.Length != 0;
#elif RELEASE_COSMOS
            found = found &&  TextBox_CosmosConnect.Text.Length != 0;
            found = found && TextBox_CosmosDatabaseName.Text.Length != 0;
#elif RELEASE_MYSQL
            found = found && TextBox_MySqlConnect.Text.Length != 0;
#endif
            Button_Apply.IsEnabled = found;
        }
    }
}
