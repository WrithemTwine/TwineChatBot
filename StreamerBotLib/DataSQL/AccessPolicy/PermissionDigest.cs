using System.Reflection;
using System.Xml.Linq;

namespace StreamerBotLib.DataSQL.AccessPolicy
{
    public class PermissionDigest
    {
        public static List<Table> TablePermissions { get; set; } = [];

        public PermissionDigest()
        {
            var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("StreamerBotLib.DataSQL.AccessPolicy.PermissionDigest.xml");
            XElement Digest = XElement.Load(stream);

            TablePermissions.AddRange(Digest.Elements("Table").Select(e => new Table(e)));
        }

        public static Table GetTablePermissions(string tableName)
        {
            return TablePermissions.FirstOrDefault(t => t.Name == tableName);
        }

        public static Column GetColumnPermissions(string tableName, string columnName)
        {
            if (columnName == null) return null;
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
