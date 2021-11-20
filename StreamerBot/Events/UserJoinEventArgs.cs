using StreamerBot.Models;

namespace StreamerBot.Events
{
    public class UserJoinEventArgs : UserJoin
    {
        // without command prefix; default '!'
        public string Command { get; set; }
        public bool AddMe { get; set; } = false;
    }
}
