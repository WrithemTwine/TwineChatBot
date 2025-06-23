
namespace StreamerBotLib.DataSQL.Models
{
    using Microsoft.EntityFrameworkCore;

    using StreamerBotLib.DataSQL.DiscriminatorEnums;
    using StreamerBotLib.Models.Enums;

    using System.ComponentModel.DataAnnotations.Schema;
    [PrimaryKey(nameof(Id))]
    [Index(nameof(WebhooksSource), nameof(Server), nameof(Kind))]

    public class WebhooksBase(int id = 0,
                           bool isEnabled = false,
                           WebhooksSource webhooksSource = default,
                           string server = null,
                           WebhooksKind kind = default,
                           bool addEveryone = false,
                           Uri webhook = default)
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

        public WebhookDataSource DataSource { get; set; }
    }
}
