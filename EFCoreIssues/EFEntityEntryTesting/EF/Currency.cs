using EFEntityEntryTesting.Enums;

using Microsoft.EntityFrameworkCore;

using System.Diagnostics;

namespace EFEntityEntryTesting.EF
{
    [PrimaryKey(nameof(UserId), nameof(Platform), nameof(CurrencyName))]
    [Index(nameof(UserId), nameof(CurrencyName), IsUnique = true)]
    [DebuggerDisplay("User={User}, CurrencyName={CurrencyName}, Value={Value}")]
    public class Currency(
        string userId = null,
        Platform platform = default,
                          double value = 0,
                          string currencyName = "")
        : UserBase(userId: userId, platform: platform)
    {
        public double Value { get; set; } = value;
        public string CurrencyName { get; set; } = currencyName;
        public Users User { get; set; } = null!;

        public CurrencyType CurrencyType { get; set; } = null!;

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
