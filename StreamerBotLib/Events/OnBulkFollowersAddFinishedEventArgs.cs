namespace StreamerBotLib.Events
{
    public class OnBulkFollowersAddFinishedEventArgs(string lastFollowerUserName) : EventArgs
    {
        public string LastFollowerUserName { get; set; } = lastFollowerUserName;
    }
}
