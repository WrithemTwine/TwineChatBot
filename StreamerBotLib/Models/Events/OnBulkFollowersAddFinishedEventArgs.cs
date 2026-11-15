namespace StreamerBotLib.Models.Events
{
    public class OnBulkFollowersAddFinishedEventArgs(string lastFollowerUserName) : BotPlatformEventArgs
    {
        public string LastFollowerUserName { get; set; } = lastFollowerUserName;
    }
}
