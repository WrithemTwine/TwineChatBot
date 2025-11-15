#define SingleContext0

#if SingleContext

using EFEntityEntryTesting.Enums;
using EFEntityEntryTesting.Static;

using Microsoft.EntityFrameworkCore;

using System.Collections.ObjectModel;

namespace EFEntityEntryTesting.EF
{
    internal class DataManager
    {
        private readonly EFTestDataContext _context;

        internal event EventHandler<OnDataCollectionUpdatedEventArgs> OnDataCollectionChanged;

        //private List<Users> UsersList { get; set; } = [];
        //private List<Currency> CurrencyList { get; set; } = [];
        //private List<CurrencyType> CurrencyTypeList { get; set; } = [];

        public DataManager()
        {
            _context = new();
            _context.Database.EnsureCreated();
            _context.SaveChanges();

            SetData();
        }

        private void SetData()
        {
            using EFTestDataContext _context = new();

            if (!_context.Users.Any())
            {
                DateTime now = DateTime.Now;

                Random randomId = new();

                CurrencyType currencyType = _context.CurrencyType.Add(new(15, 4, 10000000, "TestBucks")).Entity;
                _context.SaveChanges();

                for (int x = 0; x < 15; x++)
                {
                    string userId = randomId.Next(40000, 1000000).ToString();
                    string userName = $"User{x}";
                    _context.Users.Add(new(now, now, now, userId, userName, Platform.Twitch));
                    _context.UserStats.Add(new(watchTime: new TimeSpan(0), 0, 0, 0, 0, userId: userId, platform: Platform.Twitch));
                    _context.Currency.Add(new(userId: userId, currencyName: currencyType.CurrencyName, platform: Platform.Twitch));
                    _context.SaveChanges();
                }
            }
        }

        public void Closed()
        {
            _context.Dispose();
        }

        #region Observable Collections

        public ObservableCollection<Users> GetUsersObsCol()
        {
            //lock (_GUIcontext)
            //{
            _context.Users.Load();
            //UsersList = _GUIcontext.Users.Local.ToList();
            return _context.Users.Local.ToObservableCollection();
            //}
        }

        public ObservableCollection<Currency> GetCurrObsCol()
        {
            //lock (_GUIcontext)
            //{
            _context.Currency.Load();
            //CurrencyList = _GUIcontext.Currency.Local.ToList();
            return _context.Currency.Local.ToObservableCollection();
            //}
        }

        public ObservableCollection<CurrencyType> GetCurrTypeObsCol()
        {
            //lock (_GUIcontext)
            //{
            _context.CurrencyType.Load();
            //CurrencyTypeList = _GUIcontext.CurrencyType.Local.ToList();
            return _context.CurrencyType.Local.ToObservableCollection();
            //}
        }

        private void RefreshUsersObsCol()
        {
            //lock (_GUIcontext)
            //{
            //await _GUIcontext.Users.LoadAsync();

            //ThreadManager.AddTaskToGUIDispatcher(() =>
            //{
            //    UsersList.Clear();
            //    UsersList.AddRange([.. _GUIcontext.Users.Local]);
            //});

            OnDataCollectionChanged?.Invoke(this, new(nameof(_context.Users)));
            //}
        }

        private void RefreshCurrencyObsCol()
        {
            //lock (_GUIcontext)
            //{
            //await _GUIcontext.Currency.LoadAsync();

            //ThreadManager.AddTaskToGUIDispatcher(() =>
            //{
            //    CurrencyList.Clear();
            //    CurrencyList.AddRange([.. _GUIcontext.Currency.Local]);
            //});

            OnDataCollectionChanged?.Invoke(this, new(nameof(_context.Currency)));
            //}
        }

        #endregion

        #region Posting Data

        private Random RandomUsers = new();

        public List<string> GetUsers(int count)
        {
            using EFTestDataContext _context = new();

            lock (_context)
            {
                List<string> output = [];

                for (int x = 0; x < count; x++)
                {
                    output.Add(_context.Users.ToList()[RandomUsers.Next(_context.Users.Count())].UserId);
                }
                return output;
            }
        }

        public async Task PostUsersJoinedAsync(List<string> userId, DateTime dateTime, Platform platform = Platform.Twitch)
        {
            await Task.Run(async () =>
            {
                //using EFTestDataContext _context = new();

                foreach (Users user in from u in _context.Users
                                       where userId.Contains(u.UserId)
                                       select u)
                {
                    user.CurrLoginDate = dateTime;
                    user.LastDateSeen = dateTime;
                    LogWriter.DebugLog("UsersJoined", Models.DebugLogTypes.DataManager, $"{user.UserName} joined, curr login {user.CurrLoginDate}, last date {user.LastDateSeen}.");
                }

                await _context.SaveChangesAsync(true);
                RefreshUsersObsCol();
            });

        }

        public async Task PostUsersLeftAsync(List<string> userId, DateTime dateTime, Platform platform = Platform.Twitch)
        {
            //using EFTestDataContext _context = new();

            foreach (Users user in from u in _context.Users
                                   where userId.Contains(u.UserId)
                                   select u)
            {
                user.LastDateSeen = dateTime;
                LogWriter.DebugLog("UsersLeft", Models.DebugLogTypes.DataManager, $"{user.UserName} left, last date {user.LastDateSeen}.");
            }

            await _context.SaveChangesAsync(true);
            RefreshUsersObsCol();
        }

        public async Task UpdateCurrencyAsync(ICollection<string> Users, DateTime dateTime, Platform platform = Platform.Twitch)
        {
            //using EFTestDataContext context = new();

            foreach (Users user in from u in _context.Users
                                   where Users.Contains(u.UserId)
                                   select u)
            {
                TimeSpan clock = dateTime - user.LastDateSeen;

                user.UserStats.WatchTime += clock;
                foreach (Currency currency in user.Currency)
                {
                    currency.Value =
                        Math.Min(
                            currency.CurrencyType.MaxValue,
                            Math.Round(currency.Value + (currency.CurrencyType.AccrueAmt
                                        * (clock.TotalSeconds / currency.CurrencyType.Seconds)), 2)
                        );
                }
                user.LastDateSeen = dateTime;
                LogWriter.DebugLog("UpdateCurrency", Models.DebugLogTypes.DataManager,
                    $"{user.UserName} currency, curr login {user.CurrLoginDate}, last date {user.LastDateSeen}, currency {user.Currency.FirstOrDefault()?.Value}.");
            }

            await _context.SaveChangesAsync(true);
            RefreshUsersObsCol();
            RefreshCurrencyObsCol();
        }

        #endregion
    }
}


#else
using EFEntityEntryTesting.Enums;
using EFEntityEntryTesting.Static;

using Microsoft.EntityFrameworkCore;

using System.Collections.ObjectModel;

namespace EFEntityEntryTesting.EF
{
    internal class DataManager
    {
        private readonly EFTestDataContext _GUIcontext;

        internal event EventHandler<OnDataCollectionUpdatedEventArgs> OnDataCollectionChanged;

        private ObservableCollection<Users> UsersList = [];
        private ObservableCollection<Currency> CurrencyList = [];
        private ObservableCollection<CurrencyType> CurrencyTypeList = [];

        public DataManager()
        {
            using EFTestDataContext _context = new();
            _context.Database.EnsureCreated();
            _context.SaveChanges();

            SetData();

            _GUIcontext = new();
        }

        private void SetData()
        {
            using EFTestDataContext _context = new();

            if (!_context.Users.Any())
            {
                DateTime now = DateTime.Now;

                Random randomId = new();

                CurrencyType currencyType = _context.CurrencyType.Add(new(15, 4, 10000000, "TestBucks")).Entity;
                _context.SaveChanges();

                for (int x = 0; x < 15; x++)
                {
                    string userId = randomId.Next(40000, 1000000).ToString();
                    string userName = $"User{x}";
                    _context.Users.Add(new(now, now, now, userId, userName, Platform.Twitch));
                    _context.UserStats.Add(new(watchTime: new TimeSpan(0), 0, 0, 0, 0, userId: userId, platform: Platform.Twitch));
                    _context.Currency.Add(new(userId: userId, currencyName: currencyType.CurrencyName, platform: Platform.Twitch));
                    _context.SaveChanges();
                }
            }
        }

        public void Closed()
        {
            _GUIcontext.Dispose();
        }

        #region Observable Collections

        public ObservableCollection<Users> GetUsersObsCol()
        {
            _GUIcontext.Users.Load();

            UsersList.Clear();
            UsersList.AddRange(_GUIcontext.Users.Local.ToList());
            return UsersList;
        }

        public ObservableCollection<Currency> GetCurrObsCol()
        {
            _GUIcontext.Currency.Load();

            CurrencyList.Clear();
            CurrencyList.AddRange(_GUIcontext.Currency.Local.ToList());
            return CurrencyList;
        }

        public ObservableCollection<CurrencyType> GetCurrTypeObsCol()
        {
            _GUIcontext.CurrencyType.Load();
            CurrencyTypeList.Clear();

            CurrencyTypeList.AddRange(_GUIcontext.CurrencyType.Local.ToList());

            return CurrencyTypeList;
        }

        private async Task RefreshUsersObsCol()
        {
            _GUIcontext.ChangeTracker.Clear();
            await _GUIcontext.Users.LoadAsync();
            UsersList.Clear();

            UsersList.AddRange(_GUIcontext.Users.Local.ToList());

            OnDataCollectionChanged?.Invoke(this, new(nameof(_GUIcontext.Users)));
        }

        private async Task RefreshCurrencyObsCol()
        {
            _GUIcontext.ChangeTracker.Clear();

            await _GUIcontext.Currency.LoadAsync();
            CurrencyList.Clear();

            CurrencyList.AddRange(_GUIcontext.Currency.Local.ToList());

            OnDataCollectionChanged?.Invoke(this, new(nameof(_GUIcontext.Currency)));
        }

        #endregion

        #region Posting Data

        private Random RandomUsers = new();

        public List<string> GetUsers(int count)
        {
            using EFTestDataContext _context = new();

            lock (_context)
            {
                List<string> output = [];

                for (int x = 0; x < count; x++)
                {
                    output.Add(_context.Users.ToList()[RandomUsers.Next(_context.Users.Count())].UserId);
                }
                return output;
            }
        }

        public async Task PostUsersJoinedAsync(List<string> userId, DateTime dateTime, Platform platform = Platform.Twitch)
        {
            await Task.Run(async () =>
            {
                using EFTestDataContext _context = new();

                foreach (Users user in from u in _context.Users
                                       where userId.Contains(u.UserId)
                                       select u)
                {
                    user.CurrLoginDate = dateTime;
                    user.LastDateSeen = dateTime;
                    LogWriter.DebugLog("UsersJoined", Models.DebugLogTypes.DataManager, $"{user.UserName} joined, curr login {user.CurrLoginDate}, last date {user.LastDateSeen}.");
                }

                await _context.SaveChangesAsync(true);
                await RefreshUsersObsCol();
            });

        }

        public async Task PostUsersLeftAsync(List<string> userId, DateTime dateTime, Platform platform = Platform.Twitch)
        {
            using EFTestDataContext _context = new();

            foreach (Users user in from u in _context.Users
                                   where userId.Contains(u.UserId)
                                   select u)
            {
                user.LastDateSeen = dateTime;
                LogWriter.DebugLog("UsersLeft", Models.DebugLogTypes.DataManager, $"{user.UserName} left, last date {user.LastDateSeen}.");
            }

            await _context.SaveChangesAsync(true);
            await RefreshUsersObsCol();
        }

        public async Task UpdateCurrencyAsync(ICollection<string> Users, DateTime dateTime, Platform platform = Platform.Twitch)
        {
            using EFTestDataContext context = new();

            foreach (Users user in from u in context.Users
                                   where Users.Contains(u.UserId)
                                   select u)
            {
                TimeSpan clock = dateTime - user.LastDateSeen;

                user.UserStats.WatchTime += clock;
                foreach (Currency currency in user.Currency)
                {
                    currency.Value =
                        Math.Min(
                            currency.CurrencyType.MaxValue,
                            Math.Round(currency.Value + (currency.CurrencyType.AccrueAmt
                                        * (clock.TotalSeconds / currency.CurrencyType.Seconds)), 2)
                        );
                }
                user.LastDateSeen = dateTime;
                LogWriter.DebugLog("UpdateCurrency", Models.DebugLogTypes.DataManager,
                    $"{user.UserName} currency, curr login {user.CurrLoginDate}, last date {user.LastDateSeen}, currency {user.Currency.FirstOrDefault()?.Value}.");
            }

            await context.SaveChangesAsync(true);
            await RefreshUsersObsCol();
            await RefreshCurrencyObsCol();
        }

        #endregion
    }
}

#endif