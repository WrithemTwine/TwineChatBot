using StreamerBotLib.Interfaces;

namespace StreamerBotLib.Events
{
    public class UpdatedDataRowArgs(IDatabaseTableMeta DataRow) : EventArgs
    {
        public bool RowChanged { get; set; }
        public IDatabaseTableMeta UpdatedData => DataRow;
    }
}
