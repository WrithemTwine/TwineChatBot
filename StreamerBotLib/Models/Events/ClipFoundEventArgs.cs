using TwitchClip = TwitchLib.Api.Helix.Models.Clips.GetClips.Clip;

namespace StreamerBotLib.Models.Events
{
    public class ClipFoundEventArgs(bool allClips, List<TwitchClip> clipList) : EventArgs
    {
        public bool AllClips { get; set; } = allClips;

        public List<TwitchClip> ClipList { get; set; } = clipList;
    }
}
