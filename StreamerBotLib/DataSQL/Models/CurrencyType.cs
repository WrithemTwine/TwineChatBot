namespace StreamerBotLib.DataSQL.Models
{
    public class CurrencyType(int id,
                              string currencyName,
                              double accrueAmt,
                              int seconds = 0,
                              int maxValue = 0) : CurrencyBase(id, currencyName)
    {
        public double AccrueAmt { get; set; } = accrueAmt;
        public int Seconds { get; set; } = seconds;
        public int MaxValue { get; set; } = maxValue;

        public Currency Currency { get; set; }
    }
}
