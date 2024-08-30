using Microsoft.EntityFrameworkCore;

namespace StreamerBotLib.DataSQL.Models
{
    [PrimaryKey(nameof(UserName), nameof(CurrencyName))]
    [Index(nameof(UserName), nameof(CurrencyName), IsUnique = true)]
    public class Currency(string userName = null,
                          double value = 0,
                          string currencyName = "") : EntityBase
    {
        public string UserName { get; set; } = userName;
        public double Value { get; set; } = value;
        public string CurrencyName { get; set; } = currencyName;

        public Users? User { get; set; }
        public CurrencyType? CurrencyType { get; set; }

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
