using StreamerBotLib.Models;

namespace StreamerBotLib.Events
{
    /// <summary>
    /// The EventArgs for the GetChannelClips event.
    /// </summary>
    public class GetChannelClipsEventArgs : EventArgs
    {
        /// <summary>
        /// The ChannelName to request.
        /// </summary>
        public string ChannelName { get; set; }

        /// <summary>
        /// The action to perform when the operation concludes.
        /// </summary>
        public Action<List<Clip>> CallBackResult { get; set; }
    }
}
