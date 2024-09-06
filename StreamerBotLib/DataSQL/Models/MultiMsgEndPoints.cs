using StreamerBotLib.Enums;

namespace StreamerBotLib.DataSQL.Models
{
#if DEBUG_EFMODELS_NODEFAULTPARAM
    public class MultiMsgEndPoints(int id,
                                   bool isEnabled,
                                   WebhooksSource webhooksSource,
                                   string server,
                                   WebhooksKind kind,
                                   bool addEveryone,
                                   Uri webhook)
#else
    public class MultiMsgEndPoints(int id = 0,
                                   bool isEnabled = false,
                                   WebhooksSource webhooksSource = WebhooksSource.Discord,
                                   string server = null,
                                   WebhooksKind kind = WebhooksKind.Live,
                                   bool addEveryone = false,
                                   Uri webhook = default)
#endif
 : Webhooks(id, isEnabled, webhooksSource, server, kind, addEveryone, webhook)
    {

    }
}
