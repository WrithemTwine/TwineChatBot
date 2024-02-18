namespace StreamerBotLib.Events
{
    public class MultiLiveGetChannelsEventArgs : EventArgs
    {
        public Action<List<string>> Callback { get; set; }
    }
}
