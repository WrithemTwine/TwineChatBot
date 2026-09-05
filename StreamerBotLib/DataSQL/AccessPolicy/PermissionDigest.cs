using System.Xml.Linq;

namespace StreamerBotLib.DataSQL.AccessPolicy
{
    public static class PermissionDigest
    {
        private static readonly XElement Digest;

        public static List<Table> TablePermissions { get; set; } = [];

        static PermissionDigest()
        {
            Digest = XElement.Load("PermissionDigest.xml");
            TablePermissions.AddRange(Digest.Elements("Table").Select(tableNode => new Table(tableNode)));
        }

        public static Table GetTablePermissions(string tableName)
        {
            return TablePermissions.FirstOrDefault(t => t.Name == tableName);
        }

        public static Columns GetColumnPermissions(string tableName, string columnName)
        {
            var table = GetTablePermissions(tableName);
            return table?.Columns.FirstOrDefault(c => c.Name == columnName);
        }

        public static MenuAccess GetTableMenuAccess(string tableName)
        {
            var table = GetTablePermissions(tableName);
            return table?.MenuAccess;
        }
    }
}
