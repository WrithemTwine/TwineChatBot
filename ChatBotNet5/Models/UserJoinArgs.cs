namespace ChatBot_Net5.Models
{
    public class UserJoinArgs : UserJoin
    {
        // without command prefix; default '!'
        public string Command { get; set; }
        public bool AddMe { get; set; } = false;
    }
}
