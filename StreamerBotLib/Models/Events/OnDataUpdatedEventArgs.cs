namespace StreamerBotLib.Models.Events
{
    public class OnDataUpdatedEventArgs : EventArgs
    {
        public List<string> UpdatedTables = [];
    }
}
