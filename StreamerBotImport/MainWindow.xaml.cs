using Microsoft.EntityFrameworkCore;

using StreamerBotLib.DataSQL;
using StreamerBotLib.GUI.Windows;
using StreamerBotLib.Properties;
using StreamerBotLib.Static;
using StreamerBotLib.Themes;

using System.IO;
using System.Reflection;
using System.Windows;

namespace StreamerBotImport
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly DataManagerFactory dbContextFactory = new();

        public MainWindow()
        {
            CheckSettings();

            ChooseDatabase();

            ThreadManager.SetGUIDispatcher(Dispatcher);

            using var context = dbContextFactory.CreateDbContext();
            context.Database.EnsureCreated();
            context.SaveChanges();

            InitializeComponent();

            SetTheme();

            if (Settings.Default.EFCDataImportedDataGram)
            {
                ImportFrame.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Updates the current theme per the user's selection.
        /// </summary>
        private static void SetTheme()
        {
            Application.Current.Resources.MergedDictionaries[0].Source = new(ThemeSelector.GetCurrentTheme(), UriKind.Absolute);
        }

        private void ImportFrame_LoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            ((ImportUpdate)ImportFrame.Content).ImportCompleted += MainWindow_ImportCompleted;
            ((ImportUpdate)ImportFrame.Content).BeginImport();
        }

        private void CheckSettings()
        {
            if (Settings.Default.UpgradeRequired)
            {
                Settings.Default.Upgrade();
                Settings.Default.UpgradeRequired = false;

                // reserved for clearing tokens when there is an access scope change

                //Version? thisversion = this.GetType().Assembly.GetName().Version;

                //if (thisversion?.Major == 1 && thisversion?.MajorRevision == 3 && thisversion?.Minor == 1 && thisversion?.MinorRevision == 3)
                //{ // reset the credentials, new access scopes for each token
                //    Settings.Default.TwitchAuthBotClientId = (string)Settings.Default.GetPreviousVersion("TwitchAuthClientId"); ;

                //    Settings.Default.TwitchAuthBotAccessToken = null;
                //    Settings.Default.TwitchAuthBotAuthCode = null;
                //    Settings.Default.TwitchAuthBotRefreshToken = null;
                //    Settings.Default.TwitchAuthStreamerAccessToken = null;
                //    Settings.Default.TwitchAuthStreamerAuthCode = null;
                //    Settings.Default.TwitchAuthStreamerRefreshToken = null;

                //    Settings.Default.TwitchBotAccessToken = null;
                //    Settings.Default.TwitchStreamerAccessToken = null;
                //}

                Settings.Default.Save();
            }

            ManageAppCWD();
        }

        private static void ManageAppCWD()
        {
            if (Settings.Default.AppCurrWorkingPopup)
            {
                Settings.Default.AppCurrWorkingPopup = false;
                string SaveCWDPath = GetAppDataCWD();

                MessageBoxResult boxResult = MessageBox.Show($"This application supports saving all data files at:\r\n{SaveCWDPath}\r\n\tor at the application'AppVersion current location:\r\n{Directory.GetCurrentDirectory()}\r\n\r\nPlease select 'Yes' to enable the APPData save location.\r\n\r\nPlease see 'Data/Options/Any - Data Management' to change this option.\r\n\r\nThis dialog will not re-appear unless the settings are reset.", "Decide File Save Location", MessageBoxButton.YesNo);

                if (boxResult == MessageBoxResult.Yes)
                {
                    Settings.Default.AppCurrWorkingAppData = true;
                }
            }

            if (Settings.Default.AppCurrWorkingAppData)
            {
                Directory.CreateDirectory(GetAppDataCWD());
                Directory.SetCurrentDirectory(GetAppDataCWD());
            }
        }

        private void ChooseDatabase()
        {
            if (!
#if DEBUG || DEBUG_VIEWXAML || RELEASE_SQLITE || UPDATE_NUGET_ONLY
            OptionFlags.EFCDatabaseProviderSqlite
#elif RELEASE_POSTGRE
            OptionFlags.EFCDatabaseProviderPostgreSQL
#elif RELEASE_SQLSERVER
            OptionFlags.EFCDatabaseProviderSqlServer
#elif RELEASE_KNET
            OptionFlags.EFCDatabaseProviderKNet
#elif RELEASE_COSMOS
            OptionFlags.EFCDatabaseProviderCosmos
#elif RELEASE_MYSQL || RELEASE_POMELOMYSQL
            OptionFlags.EFCDatabaseProviderMySql
#endif
            )
            {
                ChooseDatabase chooseDatabase = new();
                chooseDatabase.ExitApp += ChooseDatabase_ExitApp;
                chooseDatabase.ShowDialog();
            }
        }

        private void MainWindow_ImportCompleted(object? sender, EventArgs e)
        {
            Label_BeginMigration.Visibility = Visibility.Visible;
            using var context = dbContextFactory.CreateDbContext();
            context.Database.MigrateAsync().Wait();
            Label_EndMigration.Visibility = Visibility.Visible;
            context.SaveChanges();

            Button_Close.Visibility = Visibility.Visible;
        }

        private void ChooseDatabase_ExitApp(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                Close();
            });
        }

        private static string GetAppDataCWD()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Assembly.GetExecutingAssembly().GetName().Name, "Data");
        }

        private void Button_Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

    }
}