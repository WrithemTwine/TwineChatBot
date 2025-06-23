using TwitchClip = TwitchLib.Api.Helix.Models.Clips.GetClips.Clip;

namespace StreamerBotLib.Models.Events
{
    public class ClipFoundEventArgs : EventArgs
    {
        public List<TwitchClip> ClipList { get; set; }
    }
}
