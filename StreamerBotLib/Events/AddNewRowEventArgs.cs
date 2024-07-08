using StreamerBotLib.Interfaces;

namespace StreamerBotLib.Events
{
    internal class AddNewRowEventArgs(IDatabaseTableMeta DataRow) : EventArgs
    {
        internal IDatabaseTableMeta NewRow => DataRow;
    }
}
