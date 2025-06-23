namespace StreamerBotLib.Models.Events
{
    public class OnBulkFollowersAddFinishedEventArgs(string lastFollowerUserName) : EventArgs
    {
        public string LastFollowerUserName { get; set; } = lastFollowerUserName;
    }
}
