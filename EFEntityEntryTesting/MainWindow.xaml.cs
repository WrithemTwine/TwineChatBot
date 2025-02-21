using EFEntityEntryTesting.EF;
using EFEntityEntryTesting.GUI;
using EFEntityEntryTesting.Static;

using System.Windows;

namespace EFEntityEntryTesting
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        internal static DataManager DataManager { get; private set; }

        private List<string> UsersJoined { get; } = [];

        private GUIDataManager GUIDataManager { get; set; }

        public MainWindow()
        {
            OptionFlags.ActiveToken = true;
            DataManager = new();
            DataManager.OnDataCollectionChanged += DataManager_OnDataCollectionChanged;

            InitializeComponent();

            GUIDataManager = (GUIDataManager)Resources["GUIDataManager"];

            SetStatusUpdate("Loaded");
        }

        private void SetStatusUpdate(string text)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                StatusBar_StatusUpdate.Text = text;
                LogWriter.WriteLog(text);
            }));
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            OptionFlags.ActiveToken = false;
            DataManager.Closed();
        }

        private void Button_StartStream_Click(object sender, RoutedEventArgs e)
        {
            Button_StartStream.IsEnabled = false;
            Button_RandomUsers.IsEnabled = true;
            Button_EndStream.IsEnabled = true;

            SetStatusUpdate("Stream Started");

            OptionFlags.IsStreamOnline = true;
            ThreadManager.CreateThreadStart("Button_StartStream", () => StartCurrency());
            AddUsersToStream();
        }

        private void Button_RandomUsers_Click(object sender, RoutedEventArgs e)
        {
            AddUsersToStream();
        }

        private async void Button_EndStream_Click(object sender, RoutedEventArgs e)
        {
            Button_StartStream.IsEnabled = true;
            Button_RandomUsers.IsEnabled = false;
            Button_EndStream.IsEnabled = false;

            OptionFlags.IsStreamOnline = false;
            DataManager.PostUsersLeftAsync(UsersJoined, DateTime.Now);
            UsersJoined.Clear();
        }
        private void DataManager_OnDataCollectionChanged(object sender, OnDataCollectionUpdatedEventArgs e)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                switch (e.DatabaseModelName)
                {
                    case "Users":
                        DB_Users.Items.Refresh();
                        break;
                    case "Currency":
                        Db_Currency.Items.Refresh();
                        break;
                    default:
                        break;
                }
                GUIDataManager.NotifyChange(e.DatabaseModelName);
                SetStatusUpdate($"Updated {DateTime.Now} - {e.DatabaseModelName} table");
            }));
        }

        private const int maxusers = 30;
        private readonly Random random = new();

        private void AddUsersToStream()
        {
            ThreadManager.CreateThreadStart("AddUsersToStream", async () =>
            {
                DateTime currTime = DateTime.Now;
                List<string> CurrUsers = DataManager.GetUsers(random.Next(maxusers));

                List<string> NewUsers = CurrUsers.Except(UsersJoined).ToList();
                List<string> OldUsers = UsersJoined.Except(CurrUsers).ToList();
                await DataManager.PostUsersJoinedAsync(NewUsers, currTime);
                DataManager.PostUsersLeftAsync(OldUsers, currTime);
                UsersJoined.Clear();
                UsersJoined.AddRange(CurrUsers);

                SetStatusUpdate($"Updated Users - {currTime}");
            });
        }

        private async void StartCurrency()
        {
            while (OptionFlags.IsStreamOnline && OptionFlags.ActiveToken)
            {
                DataManager.UpdateCurrencyAsync(UsersJoined, DateTime.Now);
                Thread.Sleep(6000);
            }
        }
    }
}
