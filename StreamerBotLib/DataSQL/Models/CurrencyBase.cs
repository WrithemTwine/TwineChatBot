using Microsoft.EntityFrameworkCore;

using System.ComponentModel.DataAnnotations.Schema;

namespace StreamerBotLib.DataSQL.Models
{
    [Index(nameof(CurrencyName), IsUnique = true)]
    public abstract class CurrencyBase(int id, string currencyName)
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; } = id;
        public string CurrencyName { get; set; } = currencyName;
    }
}
