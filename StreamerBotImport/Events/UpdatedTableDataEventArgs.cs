namespace StreamerBotImport.Events
{
    internal class UpdatedTableDataEventArgs(int tableCount, int rowCount) : EventArgs
    {
        public int TableCount { get; set; } = tableCount;
        public int RowCount { get; set; } = rowCount;
    }
}
