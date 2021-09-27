
using System;
using System.Linq;

namespace ChatBot_Net5.Data
{
    public partial class DataManager
    {
        /// <summary>
        /// For the supplied user string, update the currency based on the supplied time to the currency accrual rates the streamer specified for the currency.
        /// </summary>
        /// <param name="User">The name of the user to find in the database.</param>
        /// <param name="dateTime">The time to base the currency calculation.</param>
        public void UpdateCurrency(string User, DateTime dateTime)
        {
            DataSource.UsersRow user = _DataSource.Users.FindByUserName(User);
            UpdateCurrency(ref user, dateTime);
            SaveData();
            OnPropertyChanged(nameof(Users));
            OnPropertyChanged(nameof(Currency));
        }

        /// <summary>
        /// Process currency accruals per user, if currency type is defined, otherwise currency accruals are ignored. Afterward, the 'CurrLoginDate' is updated.
        /// </summary>
        /// <param name="User">The user to evaluate.</param>
        /// <param name="CurrTime">The time to update and accrue the currency.</param>
        public void UpdateCurrency(ref DataSource.UsersRow User, DateTime CurrTime)
        {
            lock (_DataSource.Users)
            {
                lock (_DataSource.Currency)
                {
                    lock (_DataSource.CurrencyType)
                    {
                        if (User != null)
                        {
                            TimeSpan currencyclock = CurrTime - User.CurrLoginDate; // the amount of time changed for the currency accrual calculation

                            double ComputeCurrency(double Accrue, double Seconds)
                            {
                                return Accrue * (currencyclock.TotalSeconds / Seconds);
                            }

                            AddCurrencyRows(ref User);

                            DataSource.CurrencyTypeRow[] currencyType = (DataSource.CurrencyTypeRow[])_DataSource.CurrencyType.Select();
                            DataSource.CurrencyRow[] userCurrency = (DataSource.CurrencyRow[])_DataSource.Currency.Select("Id='" + User.Id + "'");

                            foreach (var (typeRow, currencyRow) in currencyType.SelectMany(typeRow => userCurrency.Where(currencyRow => currencyRow.CurrencyName == typeRow.CurrencyName).Select(currencyRow => (typeRow, currencyRow))))
                            {
                                currencyRow.Value += ComputeCurrency(typeRow.AccrueAmt, typeRow.Seconds);
                            }

                            // set the current login date, always set regardless if currency accrual is started
                            User.CurrLoginDate = CurrTime;
                        }
                    }
                }
            }
            OnPropertyChanged(nameof(Users));
            OnPropertyChanged(nameof(Currency));
            return;
        }

        /// <summary>
        /// Update the currency accrual for the specified user, add all currency rows per the user.
        /// </summary>
        /// <param name="usersRow">The user row containing data for creating new rows depending if the currency doesn't have a row for each currency type.</param>
        public void AddCurrencyRows(ref DataSource.UsersRow usersRow)
        {
            lock (_DataSource.CurrencyType)
            {
                DataSource.CurrencyTypeRow[] currencyTypeRows = (DataSource.CurrencyTypeRow[])_DataSource.CurrencyType.Select();
                if (usersRow != null)
                {
                    lock (_DataSource.Currency)
                    {
                        DataSource.CurrencyRow[] currencyRows = (DataSource.CurrencyRow[])_DataSource.Currency.Select("Id='" + usersRow.Id + "'");
                        foreach (DataSource.CurrencyTypeRow typeRow in currencyTypeRows)
                        {
                            bool found = false;
                            foreach (DataSource.CurrencyRow CR in currencyRows)
                            {
                                if (CR.CurrencyName == typeRow.CurrencyName)
                                {
                                    found = true;
                                }
                            }
                            if (!found)
                            {
                                _DataSource.Currency.AddCurrencyRow(usersRow.Id, usersRow, typeRow, 0);
                            }
                        }
                    }
                }
            }
            SaveData();
            OnPropertyChanged(nameof(Users));
            OnPropertyChanged(nameof(Currency));
        }

        /// <summary>
        /// For every user in the database, add currency rows for each currency type - add missing rows.
        /// </summary>
        public void AddCurrencyRows()
        {
            lock (_DataSource.Users)
            {
                System.Data.DataRow[] UserRows = _DataSource.Users.Select();
                for (int i = 0; i < UserRows.Length; i++)
                {
                    DataSource.UsersRow users = (DataSource.UsersRow)UserRows[i];
                    AddCurrencyRows(ref users);
                }
            }
            SaveData();
            OnPropertyChanged(nameof(Users));
            OnPropertyChanged(nameof(Currency));
        }

        /// <summary>
        /// Empty every currency to 0, for all users for all currencies.
        /// </summary>
        public void ClearAllCurrencyValues()
        {
            lock (_DataSource.Currency)
            {
                foreach (DataSource.CurrencyRow row in _DataSource.Currency.Select())
                {
                    row.Value = 0;
                }
            }
        }
    }
}
