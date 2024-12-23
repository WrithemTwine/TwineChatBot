using EFEntityEntryTesting.Enums;

using Microsoft.EntityFrameworkCore;

using System.Collections.ObjectModel;

namespace EFEntityEntryTesting.EF
{
    internal class DataManager
    {
        private readonly EFTestDataContext _context;

        private readonly EFTestDataContext _GUIcontext;

        internal event EventHandler<OnDataCollectionUpdatedEventArgs> OnDataCollectionChanged;

        public DataManager()
        {
            _context = new EFTestDataContext();
            _context.Database.EnsureCreated();
            _context.SaveChanges();

            SetData();

            _GUIcontext = new EFTestDataContext();
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
                    _context.Currency.Add(new(userId: userId, currencyName: currencyType.CurrencyName, platform: Platform.Twitch));
                }
                _context.SaveChanges();
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

        private async Task RefreshUsersObsCol()
        {
            await _GUIcontext.Users.LoadAsync();
            OnDataCollectionChanged?.Invoke(this, new(nameof(_GUIcontext.Users)));
        }
        private async Task RefreshCurrencyObsCol()
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

        public void PostUsersJoined(List<string> userId, DateTime dateTime, Platform platform = Platform.Twitch)
        {
            lock (_context)
            {
                _context.Users.IntersectBy(userId, (u) => u.UserId).ForEachAsync((c) =>
                {
                    c.CurrLoginDate = dateTime;
                    c.LastDateSeen = dateTime;
                });
                _context.SaveChanges();
                RefreshUsersObsCol();
            }
        }

        public void PostUsersLeft(List<string> userId, DateTime dateTime, Platform platform = Platform.Twitch)
        {
            lock (_context)
            {
                _context.Users.IntersectBy(userId, (u) => u.UserId).ForEachAsync((c) =>
                {
                    c.LastDateSeen = dateTime;
                });
                _context.SaveChanges();
                RefreshUsersObsCol();
            }
        }

        public void UpdateCurrency(ICollection<string> Users, DateTime dateTime, Platform platform = Platform.Twitch)
        {
            lock (_context)
            {
                _context.Users.Join(Users, (u) => u.UserId, (user) => user, (dbusers, curr) => dbusers).ForEachAsync((u) =>
                {
                    TimeSpan clock = dateTime - u.LastDateSeen;
                    foreach (Currency currency in u.Currency)
                    {
                        currency.Value =
                            Math.Min(
                                currency.CurrencyType.MaxValue,
                                Math.Round((currency.Value + currency.CurrencyType.AccrueAmt)
                                            * (clock.TotalSeconds / currency.CurrencyType.Seconds), 2)
                            );
                    }
                    u.LastDateSeen = dateTime;
                });
                _context.SaveChanges();
                RefreshCurrencyObsCol();
            }
        }

        #endregion
    }
}
