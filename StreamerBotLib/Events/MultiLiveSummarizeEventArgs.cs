using StreamerBotLib.Models;

namespace StreamerBotLib.Events
{
    public class MultiLiveSummarizeEventArgs : EventArgs
    {
        public ArchiveMultiStream Data { get; set; }
        public Action CallbackAction { get; set; }
    }
}
