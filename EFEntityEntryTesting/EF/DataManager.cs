using EFEntityEntryTesting.Enums;
using EFEntityEntryTesting.Static;

using Microsoft.EntityFrameworkCore;

using System.Collections.ObjectModel;

namespace EFEntityEntryTesting.EF
{
    internal class DataManager
    {
        private readonly EFTestDataContext _context;
        private readonly EFTestDataContext _currencycontext;

        private readonly EFTestDataContext _GUIcontext;

        internal event EventHandler<OnDataCollectionUpdatedEventArgs> OnDataCollectionChanged;

        public DataManager()
        {
            _context = new EFTestDataContext();
            _context.Database.EnsureCreated();
            _context.SaveChanges();

            SetData();

            _GUIcontext = new();
            _currencycontext = new();
        }

        private void SetData()
        {
            if (_context.Users.Count() == 0)
            {
                DateTime now = DateTime.Now;

                Random randomId = new();

                CurrencyType currencyType = _context.CurrencyType.Add(new(15, 4, 10000000, "TestBucks")).Entity;
                _context.SaveChanges();

                for (int x = 0; x < 50; x++)
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
            _GUIcontext.Dispose();
        }

        #region Observable Collections

        public ObservableCollection<Users> GetUsersObsCol()
        {
            lock (_GUIcontext)
            {
                _GUIcontext.Users.Load();
                return _GUIcontext.Users.Local.ToObservableCollection();
            }
        }

        public ObservableCollection<Currency> GetCurrObsCol()
        {
            lock (_GUIcontext)
            {
                _GUIcontext.Currency.Load();
                return _GUIcontext.Currency.Local.ToObservableCollection();
            }
        }

        public ObservableCollection<CurrencyType> GetCurrTypeObsCol()
        {
            lock (_GUIcontext)
            {
                _GUIcontext.CurrencyType.Load();
                return _GUIcontext.CurrencyType.Local.ToObservableCollection();
            }
        }

        private async Task RefreshUsersObsColAsync()
        {
            await _GUIcontext.Users.LoadAsync();
            OnDataCollectionChanged?.Invoke(this, new(nameof(_GUIcontext.Users)));
        }
        private async Task RefreshCurrencyObsColAsync()
        {
            await _GUIcontext.Currency.LoadAsync();
            OnDataCollectionChanged?.Invoke(this, new(nameof(_GUIcontext.Currency)));
        }

        #endregion

        #region Posting Data

        private Random RandomUsers = new();

        public List<string> GetUsers(int count)
        {
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
            await Task.Run(() =>
            {
                foreach (Users user in from u in _context.Users
                                       where userId.Contains(u.UserId)
                                       select u)
                {
                    user.CurrLoginDate = dateTime;
                    user.LastDateSeen = dateTime;
                    LogWriter.DebugLog("UsersJoined", Models.DebugLogTypes.DataManager, $"{user.UserName} joined, curr login {user.CurrLoginDate}, last date {user.LastDateSeen}.");
                }
            });

            await _context.SaveChangesAsync();
            await RefreshUsersObsColAsync();
        }

        public async Task PostUsersLeftAsync(List<string> userId, DateTime dateTime, Platform platform = Platform.Twitch)
        {
            await Task.Run(() =>
            {
                foreach (Users user in from u in _context.Users
                                       where userId.Contains(u.UserId)
                                       select u)
                {
                    user.LastDateSeen = dateTime;
                    LogWriter.DebugLog("UsersLeft", Models.DebugLogTypes.DataManager, $"{user.UserName} left, last date {user.LastDateSeen}.");
                }
            });

            await _context.SaveChangesAsync();
            await RefreshUsersObsColAsync();
        }

        public async Task UpdateCurrencyAsync(ICollection<string> Users, DateTime dateTime, Platform platform = Platform.Twitch)
        {
            await Task.Run(() =>
            {
                foreach (Users user in from u in _currencycontext.Users
                                       where Users.Contains(u.UserId)
                                       select u)
                {
                    TimeSpan clock = dateTime - user.LastDateSeen;

                    UserStats stats = (from u in _currencycontext.UserStats
                                       where u.UserId == user.UserId
                                       select u).FirstOrDefault();

                    stats.WatchTime += clock;
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
            });

            await _currencycontext.SaveChangesAsync();
            await RefreshCurrencyObsColAsync();
        }

        #endregion
    }
}
