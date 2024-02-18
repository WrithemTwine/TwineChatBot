using Microsoft.EntityFrameworkCore;

namespace StreamerBotLib.DataSQL.Models
{
    [PrimaryKey(nameof(CurrencyName))]
    [Index(nameof(CurrencyName), IsUnique = true)]
    public abstract class CurrencyBase(string currencyName) : EntityBase
    {
        public string CurrencyName { get; set; } = currencyName;
    }
}
