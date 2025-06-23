
namespace StreamerBotLib.Models.Events
{
    using StreamerBotLib.Models.Interfaces;
    public class UpdatedDataRowArgs(IDatabaseTableMeta DataRow) : EventArgs
    {
        public bool RowChanged { get; set; }
        public IDatabaseTableMeta UpdatedData => DataRow;
    }
}
