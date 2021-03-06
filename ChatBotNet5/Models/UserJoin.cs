﻿using System.Diagnostics;

namespace ChatBot_Net5.Models
{
    [DebuggerDisplay("ChatUser={ChatUser}, GameUserName={GameUserName}")]
    public class UserJoin
    {
        public bool Remove { get; set; } = false;
        public string GameUserName { get; set; }
        public string ChatUser { get; set; }
    }
}
