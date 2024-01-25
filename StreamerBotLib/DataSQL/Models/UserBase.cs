using Microsoft.EntityFrameworkCore;

using StreamerBotLib.Enums;

using System.ComponentModel.DataAnnotations.Schema;

namespace StreamerBotLib.DataSQL.Models
{
    [Index(nameof(UserId), nameof(UserName), nameof(Platform))]
    public abstract class UserBase(uint id = 0, string userId = null, string userName = null, Platform platform = default)
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public uint Id { get; set; } = id;
        public string UserId { get; set; } = userId;
        public string UserName { get; set; } = userName;
        public Platform Platform { get; set; } = platform;
    }
}
