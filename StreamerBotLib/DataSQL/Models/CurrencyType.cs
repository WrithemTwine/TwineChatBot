namespace StreamerBotLib.DataSQL.Models
{
    public class CurrencyType(
                              double accrueAmt = 0,
                              int seconds = 0,
                              int maxValue = 0,
                              string currencyName = "") : CurrencyBase(currencyName)
    {
        public double AccrueAmt { get; set; } = accrueAmt;
        public int Seconds { get; set; } = seconds;
        public int MaxValue { get; set; } = maxValue;

        public ICollection<Currency> Currency { get; } = new List<Currency>();
    }
}
