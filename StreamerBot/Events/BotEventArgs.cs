﻿using System;

namespace StreamerBot.Events
{
    public class BotEventArgs
    {
        public string MethodName { get; set; }
        public EventArgs e { get; set; }
    }
}
