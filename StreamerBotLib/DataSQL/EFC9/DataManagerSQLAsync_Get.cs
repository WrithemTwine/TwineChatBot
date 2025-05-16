using Microsoft.EntityFrameworkCore;

namespace StreamerBotLib.DataSQL.EFC9
{
    internal partial class DataManagerSQLAsync
    {

        internal string GetKey(string table)
        {
            using var context = BuildDataContext();
            var entityType = context.Model.FindEntityType($"StreamerBotLib.DataSQL.Models.{table}");
            return entityType?.FindPrimaryKey()?.GetName();
        }

    }
}
