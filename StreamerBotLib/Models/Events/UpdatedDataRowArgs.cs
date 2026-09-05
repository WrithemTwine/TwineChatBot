using StreamerBotLib.Models.Interfaces;

namespace StreamerBotLib.Models.Events
{
    public class UpdatedDataRowArgs(IDatabaseTableMeta DataRow) : EventArgs
    {
        public IDatabaseTableMeta UpdatedData => DataRow;
    }
}
