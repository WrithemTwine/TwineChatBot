using System.Reflection;
using System.Xml.Linq;

namespace StreamerBotLib.DataSQL.AccessPolicy
{
    public class Table
    {
        public string Name { get; init; }
        public MenuAccess MenuAccess { get; init; }
        public List<Columns> Columns { get; init; } = [];

        public Table(XElement tableNode)
        {
            Name = tableNode.Attribute("Name")?.Value;

            var menuAccessNode = tableNode.Element("MenuAccess");
            MenuAccess = new MenuAccess
            {
                AddRow = bool.Parse(menuAccessNode?.Element("AddRow")?.Value),
                EditRow = bool.Parse(menuAccessNode?.Element("EditRow")?.Value),
                DeleteRow = bool.Parse(menuAccessNode?.Element("DeleteRow")?.Value),
                AutoShout = bool.Parse(menuAccessNode?.Element("AutoShout")?.Value),
                MonitorLive = bool.Parse(menuAccessNode?.Element("MonitorLive")?.Value),
                EnableItems = bool.Parse(menuAccessNode?.Element("EnableItems")?.Value),
                DisableItems = bool.Parse(menuAccessNode?.Element("DisableItems")?.Value)
            };
            foreach (var columnNode in tableNode.Elements("Columns"))
            {
                Columns.Add(new Columns
                {
                    Name = columnNode.Element("Name")?.Value,
                    IsNewReadOnly = bool.Parse(columnNode.Element("IsNewReadOnly")?.Value),
                    IsEditReadOnly = bool.Parse(columnNode.Element("IsEditReadOnly")?.Value),
                    IsDataGridReadOnly = bool.Parse(columnNode.Element("IsDataGridReadOnly")?.Value)
                });
            }
        }
    }

    public class MenuAccess
    {
        public bool AddRow { get; init; }
        public bool EditRow { get; init; }
        public bool DeleteRow { get; init; }
        public bool AutoShout { get; init; }
        public bool MonitorLive { get; init; }
        public bool EnableItems { get; init; }
        public bool DisableItems { get; init; }

        public object this[string name]
        {
            get
            {
                return (from PropertyInfo propertyInfo in GetType().GetProperties()
                        where propertyInfo.Name == name
                        select propertyInfo).FirstOrDefault()?.GetValue(this, null);
            }
        }
    }

    public class Columns
    {
        public string Name { get; init; }
        public bool IsNewReadOnly { get; init; }
        public bool IsEditReadOnly { get; init; }
        public bool IsDataGridReadOnly { get; init; }
    }
}
