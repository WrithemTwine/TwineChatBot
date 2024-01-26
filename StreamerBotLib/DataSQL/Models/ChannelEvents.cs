using Microsoft.EntityFrameworkCore;

using System.ComponentModel.DataAnnotations.Schema;

namespace StreamerBotLib.DataSQL.Models
{
    [Index(nameof(Name), IsUnique = true)]
    public class ChannelEvents(int id = 0, string name = null, short repeatMsg = 0, bool addMe = false, bool isEnabled = false, string message = null)
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; } = id;
        public string Name { get; set; } = name;
        public short RepeatMsg { get; set; } = repeatMsg;
        public bool AddMe { get; set; } = addMe;
        public bool IsEnabled { get; set; } = isEnabled;
        public string Message { get; set; } = message;
        public string Commands { get; set; }
    }
}
