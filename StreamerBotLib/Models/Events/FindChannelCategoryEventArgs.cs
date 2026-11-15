namespace StreamerBotLib.Models.Events
{
    public class FindChannelCategoryEventArgs : BotPlatformEventArgs
    {
        public string GameName { get; set; }
        public string GameId { get; set; }
    }
}
