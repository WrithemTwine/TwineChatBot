
namespace StreamerBotLib.Models.Events
{
    using TwitchClip = TwitchLib.Api.Helix.Models.Clips.GetClips.Clip;

    public class ClipFoundEventArgs : EventArgs
    {
        public List<TwitchClip> ClipList { get; set; }
    }
}
