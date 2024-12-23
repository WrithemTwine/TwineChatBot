using Microsoft.EntityFrameworkCore;

namespace EFEntityEntryTesting.EF
{
    [PrimaryKey(nameof(CurrencyName))]
    [Index(nameof(CurrencyName), IsUnique = true)]
#if DEBUG_EFMODELS_NODEFAULTPARAM
    public class CurrencyType(
                              double accrueAmt,
                              int seconds,
                              int maxValue,
                              string currencyName)
#else
    public class CurrencyType(
                              double accrueAmt = 0,
                              int seconds = 0,
                              int maxValue = 0,
                              string currencyName = "")
#endif
        : EntityBase
    {

        public double AccrueAmt { get; set; } = accrueAmt;
        public int Seconds { get; set; } = seconds;
        public int MaxValue { get; set; } = maxValue;
        public string CurrencyName { get; set; } = currencyName;

        public ICollection<Currency> Currency { get; } = [];
    }
}
