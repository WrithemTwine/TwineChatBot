using StreamerBotLib.DataSQL;

namespace StreamerBotLib.Events
{
    public class OnDataCollectionUpdatedEventArgs(string TableName, object TableData = null) : EventArgs
    {
        public string DatabaseModelName { get; set; } = TableName;
        public object TableData { get; set; } = TableData;
    }
}
