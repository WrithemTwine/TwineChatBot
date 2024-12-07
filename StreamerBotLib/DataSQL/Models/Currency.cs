using Microsoft.EntityFrameworkCore;

using StreamerBotLib.Enums;

using System.Diagnostics.CodeAnalysis;

namespace StreamerBotLib.DataSQL.Models
{
    [PrimaryKey(nameof(UserId), nameof(Platform), nameof(CurrencyName))]
    [Index(nameof(UserId), nameof(CurrencyName), IsUnique = true)]

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
        Platform platform = default,
                          double value = 0,
                          string currencyName = "")
#endif
        : UserBase(userId: userId, platform: platform)
    {
        public double Value { get; set; } = value;
        public string CurrencyName { get; set; } = currencyName;

        [AllowNull]
        public Users? User { get; set; }
        [AllowNull]
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
