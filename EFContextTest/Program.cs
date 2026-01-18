using EFContextTest.EF;

using Microsoft.EntityFrameworkCore;

using System.Collections.ObjectModel;

namespace EFContextTest
{
    public static class Program
    {
        private static EFTestDataContext? _GUIContext; // long-lived context for GUI operations

        private static readonly int MaxUsers = 15;

        private static readonly int UserUpdateCount = 5; // Number of users to update currency for - **must be less than or equal to MaxUsers**

        private static ObservableCollection<Users>? UserObservable;

        public static void Main(string[] args)
        {
            // Ensure the database is created and initial data is set up
            using EFTestDataContext _context = new();
            _context.Database.EnsureCreated();
            _context.SaveChanges();
            _context.Dispose();

            Console.WriteLine("Database created and initial data set up.");
            Console.WriteLine("Data has been set up in the database.");
            Console.WriteLine("--------------------------------------------------");

            // Initialize the GUI context and load user data
            _GUIContext = new EFTestDataContext();
            UserObservable = _GUIContext.Users.Local.ToObservableCollection();
            OutputUserData(_GUIContext.Users.ToList(), nameof(_GUIContext));
            OutputUserData(UserObservable.ToList(), nameof(UserObservable));

            // Set up initial data if not already present
            SetData();

            // Load user data again to show the initial state

            // added a line to clear the change tracker to ensure fresh data
            _GUIContext.ChangeTracker.Clear();

            _GUIContext.Users.Load();
            OutputUserData(_GUIContext.Users.ToList(), nameof(_GUIContext));
            OutputUserData(UserObservable.ToList(), nameof(UserObservable));

            // Create a new context to demonstrate data retrieval
            var context = new EFTestDataContext();
            OutputUserData(context.Users.ToList(), nameof(context));
            context.Dispose();

            Thread.Sleep(1500); // Sleep for a second to simulate time passing

            // Update currency for random users
            UpdateCurrency();

            Console.WriteLine("Currency updated for random users.");
            Console.WriteLine("--------------------------------------------------");

            // Load user data again to show updated state

            // added a line to clear the change tracker to ensure fresh data
            _GUIContext.ChangeTracker.Clear();

            _GUIContext.Users.Load();
            OutputUserData(_GUIContext.Users.ToList(), nameof(_GUIContext));
            OutputUserData(UserObservable.ToList(), nameof(UserObservable));

            // Create another context to demonstrate data retrieval after updates
            var context2 = new EFTestDataContext();
            OutputUserData(context2.Users.ToList(), nameof(context2));
            context2.Dispose();

            Thread.Sleep(1800); // Sleep for a second to simulate time passing

            // Update currency for random users
            UpdateCurrency();

            Console.WriteLine("Currency updated for random users.");
            Console.WriteLine("--------------------------------------------------");

            // Load user data again to show updated state

            // added a line to clear the change tracker to ensure fresh data
            _GUIContext.ChangeTracker.Clear();

            _GUIContext.Users.Load();
            OutputUserData(_GUIContext.Users.ToList(), nameof(_GUIContext));
            OutputUserData(UserObservable.ToList(), nameof(UserObservable));

            // Create another context to demonstrate data retrieval after updates
            var context3 = new EFTestDataContext();
            OutputUserData(context3.Users.ToList(), nameof(context3));
            context3.Dispose();


            _GUIContext.Dispose();
        }

        /// <summary>
        /// Populates the database with initial test data if no user records exist.
        /// </summary>
        /// <remarks>This method creates a predefined currency type and generates a set of test users,
        /// along with their associated  statistics and currency data. It is intended for initializing the database with
        /// sample data for testing purposes.</remarks>
        private static void SetData()
        {
            using EFTestDataContext _context = new();

            if (!_context.Users.Any())
            {
                DateTime now = DateTime.Now;

                Random _random = new();

                CurrencyType currencyType = _context.CurrencyType.Add(new(1500, 2, 10000000, "TestBucks")).Entity;
                _context.SaveChanges(true);

                for (int x = 0; x < MaxUsers; x++)
                {
                    string userId = _random.Next(40000, 1000000).ToString();
                    string userName = $"User{x}";
                    _context.Users.Add(new(now, now, now, userId, userName, Platform.Twitch));
                    _context.UserStats.Add(new(watchTime: new TimeSpan(0), 0, 0, 0, 0, userId: userId, platform: Platform.Twitch));
                    _context.Currency.Add(new(userId: userId, currencyName: currencyType.CurrencyName, platform: Platform.Twitch));
                    _context.SaveChanges(true);
                }
            }
        }

        /// <summary>
        /// Updates the currency values and user statistics for a subset of users in the database.
        /// </summary>
        /// <remarks>This method selects a random subset of users and updates their currency values based
        /// on the  elapsed time since they were last seen. Additionally, it increments the watch time for each  user
        /// based on the same elapsed time. Changes are persisted to the database.</remarks>
        private static void UpdateCurrency()
        {
            using EFTestDataContext _context = new();
            if (_context.Users.Any())
            {
                DateTime now = DateTime.Now;
                Random random = new();

                var users = _context.Users.Include("Currency").Include("UserStats").ToList();
                int count = users.Count();

                for (int i = 0; i < UserUpdateCount; i++)
                {
                    // Select a random user
                    int randomIndex = random.Next(0, count);

                    Users curruser = users[randomIndex];

                    TimeSpan clock = now - curruser.LastDateSeen;

                    if (curruser != null && curruser.UserStats != null)
                    {
                        curruser.UserStats.WatchTime += clock;

                        foreach (Currency currency in curruser.Currency)
                        {
                            currency.Value =
                                Math.Min(
                                    currency.CurrencyType.MaxValue,
                                    Math.Round(currency.Value + (currency.CurrencyType.AccrueAmt
                                                * (clock.TotalSeconds / currency.CurrencyType.Seconds)), 2)
                                );
                        }
                        curruser.LastDateSeen = now;
                    }
                }

                _context.SaveChanges(true);
            }
        }

        /// <summary>
        /// Outputs user data to the console, including contextual information and detailed user statistics.
        /// </summary>
        /// <remarks>This method writes user data to the console in a tabular format, including user
        /// identifiers, activity dates, platform information, watch time, and currency details. If a user has no
        /// currency data, a corresponding message is displayed.</remarks>
        /// <param name="users">A list of <see cref="Users"/> objects representing the users whose data will be displayed. Cannot be null.</param>
        /// <param name="ContextName">The name of the context associated with the user data. Cannot be null or empty.</param>
        private static void OutputUserData(List<Users> users, string ContextName)
        {
            Console.WriteLine($"Context: {ContextName}");
            Console.WriteLine("Users in Database:");
            Console.WriteLine("UserId, FirstDateSeen, LastDateSeen, UserName, Platform, WatchTime");

            foreach (Users U in users)
            {
                string Line;
                Line = $"{U.UserId}, {U.FirstDateSeen}, {U.LastDateSeen}, {U.UserName}, {U.Platform}, {U.UserStats?.WatchTime})";
                if (U.Currency.Count > 0)
                {
                    foreach (Currency C in U.Currency)
                    {
                        Line += $" Currency: {C.CurrencyName}, Amount: {C.Value}";
                    }
                }
                else
                {
                    Line += " No Currency Data";
                }

                Console.WriteLine(Line);
            }

            Console.WriteLine("--------------------------------------------------");
        }
    }
}
