
using System;
using System.Collections.Generic;

using TwitchLib.Api.Helix.Models.Clips.GetClips;

namespace StreamerBot.BotClients.Twitch.TwitchLib.Events.ClipService
{
    public class OnNewClipsDetectedArgs : EventArgs
    {
        public string Channel { get; set; }
        public List<Clip> Clips { get; set; }
    }
}
