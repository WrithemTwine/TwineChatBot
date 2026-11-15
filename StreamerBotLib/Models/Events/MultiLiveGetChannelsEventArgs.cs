namespace StreamerBotLib.Models.Events
{
    public class MultiLiveGetChannelsEventArgs : BotPlatformEventArgs
    {
        public Action<List<string>> Callback { get; set; }
    }
}
