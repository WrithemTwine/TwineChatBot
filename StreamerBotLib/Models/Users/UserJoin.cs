using System.Diagnostics;

namespace StreamerBotLib.Models.Users
{
    [DebuggerDisplay("ChatUser={ChatUser}, GameUserName={GameUserName}")]
    public record UserJoin
    {
        public bool Remove { get; set; } = false;
        public string GameUserName { get; set; }
        public string ChatUser { get; set; }

    }


}
