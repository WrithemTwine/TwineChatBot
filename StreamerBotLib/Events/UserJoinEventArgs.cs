using StreamerBotLib.Models;

namespace StreamerBotLib.Events
{
    public record UserJoinEventArgs : UserJoin
    {
        // without command prefix; default '!'
        public string Command { get; set; }
        public bool AddMe { get; set; } = false;
    }
}
