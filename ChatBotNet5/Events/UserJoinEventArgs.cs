using ChatBot_Net5.Models;

namespace ChatBot_Net5.Events
{
    public class UserJoinEventArgs : UserJoin
    {
        // without command prefix; default '!'
        public string Command { get; set; }
        public bool AddMe { get; set; } = false;
    }
}
