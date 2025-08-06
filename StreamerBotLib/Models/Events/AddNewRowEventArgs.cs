using StreamerBotLib.Models.Interfaces;

namespace StreamerBotLib.Models.Events
{
    internal class AddNewRowEventArgs(IDatabaseTableMeta DataRow) : EventArgs
    {
        internal IDatabaseTableMeta NewRow => DataRow;
    }
}
