using Microsoft.EntityFrameworkCore;

namespace EFContextTest.EF
{
    [PrimaryKey(nameof(CurrencyName))]
    [Index(nameof(CurrencyName), IsUnique = true)]
    public class CurrencyType(
                              double accrueAmt = 0,
                              int seconds = 0,
                              int maxValue = 0,
                              string currencyName = "")
    {

        public double AccrueAmt { get; set; } = accrueAmt;
        public int Seconds { get; set; } = seconds;
        public int MaxValue { get; set; } = maxValue;
        public string CurrencyName { get; set; } = currencyName;

        public ICollection<Currency> Currency { get; } = [];
    }
}
