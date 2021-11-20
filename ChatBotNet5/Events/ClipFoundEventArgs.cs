﻿using System;
using System.Collections.Generic;

using TwitchLib.Api.Helix.Models.Clips.GetClips;

namespace ChatBot_Net5.Events
{
    public class ClipFoundEventArgs : EventArgs
    {
        public List<Clip> ClipList { get; set; }
    }
}
