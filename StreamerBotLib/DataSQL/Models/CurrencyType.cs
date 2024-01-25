namespace StreamerBotLib.DataSQL.Models
{
    public class CurrencyType(uint id, string currencyName, double accrueAmt, int seconds = 0, uint maxValue = 0) : CurrencyBase(id, currencyName)
    {
        public double AccrueAmt { get; set; } = accrueAmt;
        public int Seconds { get; set; } = seconds;
        public uint MaxValue { get; set; } = maxValue;
    }
}
