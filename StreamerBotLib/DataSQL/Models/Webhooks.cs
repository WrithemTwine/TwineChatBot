using Microsoft.EntityFrameworkCore;

using StreamerBotLib.Enums;

using System.ComponentModel.DataAnnotations.Schema;

namespace StreamerBotLib.DataSQL.Models
{
    [PrimaryKey(nameof(Id))]
    [Index(nameof(WebhooksSource), nameof(Server), nameof(Kind))]

#if DEBUG_EFMODELS_NODEFAULTPARAM
    public class Webhooks(int id,
                         bool isEnabled,
                         WebhooksSource webhooksSource,
                         string server,
                         WebhooksKind kind,
                         bool addEveryone,
                         Uri webhook)
#else
    public class Webhooks(int id = 0,
                         bool isEnabled = false,
                         WebhooksSource webhooksSource = default,
                         string server = null,
                         WebhooksKind kind = default,
                         bool addEveryone = false,
                         Uri webhook = default)
#endif
      : EntityBase
    {

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; } = id;
        public bool IsEnabled { get; set; } = isEnabled;
        public WebhooksSource WebhooksSource { get; set; } = webhooksSource;
        public string Server { get; set; } = server;
        public WebhooksKind Kind { get; set; } = kind;
        public bool AddEveryone { get; set; } = addEveryone;
        public Uri Webhook { get; set; } = webhook;
    }
}
