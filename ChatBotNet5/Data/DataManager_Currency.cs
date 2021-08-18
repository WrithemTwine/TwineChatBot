using ChatBot_Net5.Static;

using System;
using System.Collections.Generic;

namespace ChatBot_Net5.Data
{
    public partial class DataManager
    {
        /// <summary>
        /// Overall update currency accruals for all users active during an online stream
        /// </summary>
        /// <param name="dateTime">The date for the calculation</param>
        internal void UpdateCurrency(DateTime dateTime)
        {
            lock (_DataSource.Users)
            {
                foreach (DataSource.UsersRow users in _DataSource.Users.Select())
                {
                    if (users.CurrLoginDate >= CurrStreamStart) // detected online user
                    {
                        UpdateCurrency(users, dateTime);
                    }
                }
            }

            SaveData();
            OnPropertyChanged(nameof(Users));
            OnPropertyChanged(nameof(Currency));
        }

        /// <summary>
        /// Process currency accruals per user, if currency type is defined, otherwise currency accruals are ignored. Afterward, the 'CurrLoginDate' is updated.
        /// </summary>
        /// <param name="User">The user to evaluate.</param>
        /// <param name="CurrTime">The time to update and accrue the currency.</param>
        internal void UpdateCurrency(DataSource.UsersRow User, DateTime CurrTime)
        {
            // when currency accrual is started, set each currency
            if (OptionFlags.TwitchCurrencyStart)
            {
                DataSource.CurrencyTypeRow[] currencyType = (DataSource.CurrencyTypeRow[])_DataSource.CurrencyType.Select();
                List<DataSource.CurrencyRow> currrow = new List<DataSource.CurrencyRow>((DataSource.CurrencyRow[]) User.GetChildRows("Users_CurrencyAccrued"));
                TimeSpan currencyclock = CurrTime - User.CurrLoginDate; // the amount of time changed for the currency accrual calculation

                lock (_DataSource.Currency)  // lock for multithreading
                {
                    if (currencyType.Length > 0) // no currency, no accruing currency
                    {
                        if (currrow.Count != currencyType.Length) // meaning at least 1 user currency row isn't created or matches the currency types
                        {
                            foreach (DataSource.CurrencyTypeRow CR in currencyType)
                            {
                                DataSource.CurrencyRow found = currrow.Find((f) => f.CurrencyName == CR.CurrencyName); // look for the row for each currency
                                if (found == null)
                                {
                                    _DataSource.Currency.AddCurrencyRow(User.Id, User.UserName, CR.CurrencyName, CR.AccrueAmt * (currencyclock.TotalSeconds / CR.Seconds));
                                }
                                else
                                {
                                    found.Value += CR.AccrueAmt * (currencyclock.TotalSeconds / CR.Seconds);
                                }
                            }
                        }
                        else // otherwise, we assume all currency types are handled in rows, since there's a data integrity relation to update or delete when currency types change
                        {
                            foreach (DataSource.CurrencyRow U in currrow)
                            {
                                DataSource.CurrencyTypeRow typeRow = (DataSource.CurrencyTypeRow)U.GetParentRow("Currency_CurrencyAccrued");
                                U.Value += typeRow.AccrueAmt * (currencyclock.TotalSeconds / typeRow.Seconds);
                            }
                        }
                    }
                }
            }

            // set the current login date, always set regardless if currency accrual is started
            User.CurrLoginDate = CurrTime;
            return;
        }
    }
}
