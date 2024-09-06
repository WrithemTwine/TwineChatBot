using Microsoft.EntityFrameworkCore;

using StreamerBotLib.Enums;

namespace StreamerBotLib.DataSQL.Models
{
    [PrimaryKey(nameof(UserId), nameof(UserName), nameof(Platform), nameof(CurrencyName))]
    [Index(nameof(UserName), nameof(CurrencyName), IsUnique = true)]

#if DEBUG_EFMODELS_NODEFAULTPARAM
    public class Currency(
        string userId,
        string userName,
        Platform platform,
                          double value,
                          string currencyName)
#else
    public class Currency(
        string userId = null,
        string userName = null,
        Platform platform = default,
                          double value = 0,
                          string currencyName = "") 
#endif
        : UserBase(userId: userId, userName: userName, platform: platform)
    {
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
