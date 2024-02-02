using Microsoft.EntityFrameworkCore;

namespace StreamerBotLib.DataSQL.Models
{
    [Index(nameof(UserName), nameof(CurrencyName), IsUnique = false)]
    public class Currency(string currencyName,
                          string userName = null,
                          double value = 0) : CurrencyBase(currencyName)
    {
        public string UserName { get; set; } = userName;
        public double Value { get; set; } = value;

        public Users User { get; set; }
        public CurrencyType CurrencyType { get; set; }

        public static Currency operator +(Currency lhs, Currency rhs)
        {
            if (lhs != null && rhs != null)
            {
                lhs.Value += rhs.Value;
            }
            return lhs;
        }

        public void Add(Currency currency)
        {
            if (currency != default)
            {
                Value += currency.Value;
            }
        }
    }
}
