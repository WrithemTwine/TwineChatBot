using EFEntityEntryTesting.EF;
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

        public MainWindow()
        {
            OptionFlags.ActiveToken = true;
            DataManager = new();
            DataManager.OnDataCollectionChanged += DataManager_OnDataCollectionChanged;

            InitializeComponent();

            SetStatusUpdate("Loaded");
        }

        private void SetStatusUpdate(string text)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                StatusBar_StatusUpdate.Text = text;
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

        private void Button_EndStream_Click(object sender, RoutedEventArgs e)
        {
            Button_StartStream.IsEnabled = true;
            Button_RandomUsers.IsEnabled = false;
            Button_EndStream.IsEnabled = false;

            OptionFlags.IsStreamOnline = false;
            lock (UsersJoined)
            {
                DataManager.PostUsersLeft(UsersJoined, DateTime.Now);
                UsersJoined.Clear();
            }
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
            }));
        }

        private const int maxusers = 30;
        private readonly Random random = new();

        private void AddUsersToStream()
        {
            ThreadManager.CreateThreadStart("AddUsersToStream", () =>
            {
                DateTime currTime = DateTime.Now;
                List<string> CurrUsers = DataManager.GetUsers(random.Next(maxusers));

                lock (UsersJoined)
                {
                    List<string> NewUsers = CurrUsers.Except(UsersJoined).ToList();
                    List<string> OldUsers = UsersJoined.Except(CurrUsers).ToList();
                    DataManager.PostUsersJoined(NewUsers, currTime);
                    DataManager.PostUsersLeft(OldUsers, currTime);
                    UsersJoined.Clear();
                    UsersJoined.AddRange(CurrUsers);
                }

                SetStatusUpdate($"Updated Users - {currTime}");
            });
        }

        private void StartCurrency()
        {
            while (OptionFlags.IsStreamOnline && OptionFlags.ActiveToken)
            {
                lock (UsersJoined)
                {
                    DataManager.UpdateCurrency(UsersJoined, DateTime.Now);
                }
                Thread.Sleep(2000);
            }
        }
    }
}
