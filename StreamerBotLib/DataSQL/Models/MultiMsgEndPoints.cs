using StreamerBotLib.Enums;

namespace StreamerBotLib.DataSQL.Models
{
    public class MultiMsgEndPoints(int id = 0,
                                   bool isEnabled = false,
                                   WebhooksSource webhooksSource = WebhooksSource.Discord,
                                   string server = null,
                                   WebhooksKind kind = WebhooksKind.Live,
                                   bool addEveryone = false) : Webhooks(id, isEnabled, webhooksSource, server, kind, addEveryone)
    {
    }
}
