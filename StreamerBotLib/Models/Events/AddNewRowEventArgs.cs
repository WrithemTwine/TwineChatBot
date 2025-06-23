
namespace StreamerBotLib.Models.Events
{
    using StreamerBotLib.Models.Interfaces;

    internal class AddNewRowEventArgs(IDatabaseTableMeta DataRow) : EventArgs
    {
        internal IDatabaseTableMeta NewRow => DataRow;
    }
}
