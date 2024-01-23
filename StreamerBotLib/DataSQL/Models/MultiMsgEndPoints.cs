using StreamerBotLib.Enums;

namespace StreamerBotLib.DataSQL.Models
{
    public class MultiMsgEndPoints(uint id = 0,
                                   bool isEnabled = false,
                                   string server = null,
                                   WebhooksKind kind = WebhooksKind.Live,
                                   bool addEveryone = false) : Discord(id, isEnabled, server, kind, addEveryone)
    {
    }
}
