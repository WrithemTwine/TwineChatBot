namespace StreamerBotLib.Models.Events
{
    public class GetUserIdResponseEventArgs : BotPlatformEventArgs
    {
        public string UserId { get; set; }
    }
}
