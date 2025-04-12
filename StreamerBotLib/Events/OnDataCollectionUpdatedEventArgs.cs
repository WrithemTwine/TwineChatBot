namespace StreamerBotLib.Events
{
    public class OnDataCollectionUpdatedEventArgs(string TableName, bool RecordCountChange) : EventArgs
    {
        /// <summary>
        /// The name of the updated table.
        /// </summary>
        public string DatabaseModelName { get; set; } = TableName;

        /// <summary>
        /// A flag indicating if the record count has changed.
        /// </summary>
        public bool RecordCountChange { get; set; } = RecordCountChange;
    }
}
