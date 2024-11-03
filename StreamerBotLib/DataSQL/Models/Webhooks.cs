using StreamerBotLib.Enums;

namespace StreamerBotLib.DataSQL.Models
{

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
    : WebhooksBase(id, isEnabled, webhooksSource, server, kind, addEveryone, webhook)
    {

    }
}
