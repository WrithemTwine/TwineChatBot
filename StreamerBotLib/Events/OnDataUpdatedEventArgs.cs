namespace StreamerBotLib.Events
{
    public class OnDataUpdatedEventArgs : EventArgs
    {
        public List<string> UpdatedTables = new();
    }
}
