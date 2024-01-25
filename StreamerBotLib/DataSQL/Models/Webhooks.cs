using Microsoft.EntityFrameworkCore;

using StreamerBotLib.Enums;

using System.ComponentModel.DataAnnotations.Schema;

namespace StreamerBotLib.DataSQL.Models
{
    [Index(nameof(Id), nameof(Server), nameof(Kind))]
    public class Webhooks(uint id = 0,
                         bool isEnabled = false,
                         WebhooksSource webhooksSource = default,
                         string server = null,
                         WebhooksKind kind = default,
                         bool addEveryone = false)
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public uint Id { get; set; } = id;
        public bool IsEnabled { get; set; } = isEnabled;
        public WebhooksSource WebhooksSource { get; set; } = webhooksSource;
        public string Server { get; set; } = server;
        public WebhooksKind Kind { get; set; } = kind;
        public bool AddEveryone { get; set; } = addEveryone;
        public Uri Webhook { get; set; }
    }
}
