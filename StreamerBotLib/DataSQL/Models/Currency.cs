namespace StreamerBotLib.DataSQL.Models
{
    public class Currency(uint id, string currencyName, string userName = null, double value = 0, Users user = null) : CurrencyBase(id, currencyName)
    {
        public string UserName { get; set; } = userName;
        public double Value { get; set; } = value;

        public Users User { get; set; } = user;
    }
}
