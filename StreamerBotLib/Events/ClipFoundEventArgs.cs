using TwitchLib.Api.Helix.Models.Clips.GetClips;

namespace StreamerBotLib.Events
{
    public class ClipFoundEventArgs : EventArgs
    {
        public List<Clip> ClipList { get; set; }
    }
}
