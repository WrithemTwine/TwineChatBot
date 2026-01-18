using TwitchLib.Api.Helix.Models.Clips.GetClips;

namespace StreamerBotLib.BotClients.Twitch.TwitchLib.Events.ClipService
{
    public class OnNewClipsDetectedArgs : EventArgs
    {
        public bool AllClips { get; set; }
        public string Channel { get; set; }
        public List<Clip> Clips { get; set; }
    }
}
