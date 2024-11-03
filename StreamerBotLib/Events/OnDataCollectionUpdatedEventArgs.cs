namespace StreamerBotLib.Events
{
    public class OnDataCollectionUpdatedEventArgs(string TableName) : EventArgs
    {
        public string DatabaseModelName { get; set; } = TableName;
    }
}
