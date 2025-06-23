using StreamerBotLib.Models.Interfaces;

namespace StreamerBotLib.Models.Events
{
    public class UpdatedDataRowArgs(IDatabaseTableMeta DataRow) : EventArgs
    {
        public bool RowChanged { get; set; }
        public IDatabaseTableMeta UpdatedData => DataRow;
    }
}
