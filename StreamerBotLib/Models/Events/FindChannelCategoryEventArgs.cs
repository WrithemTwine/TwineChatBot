namespace StreamerBotLib.Models.Events
{
    public class FindChannelCategoryEventArgs : EventArgs
    {
        public string GameName { get; set; }
        public string GameId { get; set; }
    }
}
