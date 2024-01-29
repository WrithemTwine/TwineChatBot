using Microsoft.EntityFrameworkCore;

namespace StreamerBotLib.DataSQL.Models
{
    [PrimaryKey(nameof(UserName), nameof(CurrencyName))]
    public class Currency(string currencyName,
                          string userName = null,
                          double value = 0) : CurrencyBase(currencyName)
    {
        public string UserName { get; set; } = userName;
        public double Value { get; set; } = value;

        public Users User { get; set; }
        public ICollection<CurrencyType> CurrencyType { get; } = new List<CurrencyType>();

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
