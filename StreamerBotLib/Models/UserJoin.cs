using System;
using System.Diagnostics;

namespace StreamerBotLib.Models
{
    [DebuggerDisplay("ChatUser={ChatUser}, GameUserName={GameUserName}")]
    public class UserJoin : IEquatable<UserJoin>
    {
        public bool Remove { get; set; } = false;
        public string GameUserName { get; set; }
        public string ChatUser { get; set; }

        public bool Equals(UserJoin other)
        {
            return ChatUser == other.ChatUser;
        }
    }


}
