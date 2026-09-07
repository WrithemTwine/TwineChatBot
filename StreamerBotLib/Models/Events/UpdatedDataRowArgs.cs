using StreamerBotLib.DataSQL.TableMeta;

namespace StreamerBotLib.Models.Events
{
    public class UpdatedDataRowArgs(TableMeta DataRow) : EventArgs
    {
        public TableMeta UpdatedData => DataRow;
    }
}
