using Microsoft.EntityFrameworkCore;

using System.ComponentModel.DataAnnotations.Schema;

namespace StreamerBotLib.DataSQL.Models
{
    [Index(nameof(CurrencyName), IsUnique = true)]
    public abstract class CurrencyBase(uint id, string currencyName)
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public uint Id { get; set; } = id;
        public string CurrencyName { get; set; } = currencyName;
    }
}
