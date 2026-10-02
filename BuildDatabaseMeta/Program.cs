using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace BuildDatabaseMeta
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string ModelPath = @"C:\Source\ChatBotApp\StreamerBotLib\DataSQL\Models";
            string MetaPath = @"C:\Source\ChatBotApp\StreamerBotLib\DataSQL\TableMeta";

            // get the list of 'files==class names' to use and filter for unneeded types
            var FileNames = from f in Directory.GetFiles(ModelPath)
                            let name = Path.GetFileNameWithoutExtension(f)
                            where !name.StartsWith('0')
                            select name;

            string baseMetaPath = Path.Combine(MetaPath, "TableMeta.cs");

            List<string> newEntities = [];
            List<string> existingEntities = [];
            List<string> getEntities = [];

            foreach (string name in FileNames)
            {
                Type table = Type.GetType(
                    $"StreamerBotLib.DataSQL.Models.{name}, {Assembly.GetAssembly(typeof(StreamerBotLib.DataSQL.Models.UserBase))}");

                if (table is null || name.Contains("Base"))
                    continue;

                List<string> props = [];
                List<string> values = [];
                List<string> entity = [];
                List<string> param = [];
                List<string> copyparam = [];

                // ----- build the three dynamic chains for TableMeta.cs -----
                newEntities.Add(
                    $$"""
                    if (Entity == typeof(Models.{{name}}))
                                {
                                    CurrEntity = new {{name}}(new Models.{{name}}());
                                }
                    """);

                existingEntities.Add(
                    $$"""
                    if (Entity.GetType() == typeof(Models.{{name}}))
                                {
                                    CurrEntity = new {{name}}((Models.{{name}})Entity);
                                }
                    """);

                getEntities.Add(
                    $$"""
                    if (DataEntity.GetType() == typeof(Models.{{name}}))
                                {
                                    (({{name}})Update).CopyUpdates((Models.{{name}})DataEntity);
                                    return DataEntity;
                                }
                    """);

                // ----- per-property generation -----
                foreach (PropertyInfo p in table.GetProperties())
                {
                    // 1. Always skip these
                    if (p.Name is "Item" or "DataSource" or "Commandtype")
                        continue;

                    Type propType = p.PropertyType;
                    Type underlying = Nullable.GetUnderlyingType(propType) ?? propType;

                    // Helper to decide if a type is a real entity
                    static bool IsEntityType(Type t)
                    {
                        Type u = Nullable.GetUnderlyingType(t) ?? t;
                        return u.Namespace != null
                               && u.Namespace.Contains("Models")
                               && !u.IsEnum
                               && u != typeof(string)
                               && !u.IsValueType;
                    }

                    // 2. Detect navigations
                    bool isReferenceNavigation = IsEntityType(underlying);

                    bool isCollectionNavigation = propType.IsGenericType
                                                  && propType.GetGenericTypeDefinition() == typeof(ICollection<>)
                                                  && IsEntityType(propType.GenericTypeArguments[0]);

                    if (isReferenceNavigation || isCollectionNavigation)
                        continue;   // ← this now kills both UserStats and InRaidDataList

                    // 3. Extra safety: property name matches another table
                    if (FileNames.Contains(p.Name))
                        continue;

                    if (p.Name.Contains("Calls"))
                        continue;

                    // ----- from here on it is safe to generate the property -----
                    string PropName = propType.IsGenericType && propType.Name.Contains("ICollection")
                        ? $"ICollection<{propType.GenericTypeArguments[0].FullName}>"
                        : propType.FullName!;

                    props.Add($"              {{ \"{p.Name}\", typeof({PropName}) }}");
                    values.Add($"                 {{ \"{p.Name}\", tableData.{p.Name} }}");

                    string getter = GetTypedGetter(p.Name, propType, PropName);
                    param.Add($"        public {PropName} {p.Name} {{ get => {getter}; set => Values[\"{p.Name}\"] = value; }}");

                    // Skip identity / computed columns for constructor + CopyUpdates
                    if (p.Name is "Id" or "Commandtype" or "DataSource")
                        continue;
                    if (p.Name == "Duration" && name == "StreamStats")
                        continue;

                    copyparam.Add(
                        $$"""
                  if (modelData.{{p.Name}} != {{p.Name}})
                    {
                        modelData.{{p.Name}} = {{p.Name}};
                    }
        """);

                    string argName = char.ToLowerInvariant(p.Name[0]) + p.Name[1..];
                    entity.Add($"            {argName}: {p.Name}");
                }

                WriteFile(
                    Path.Combine(MetaPath, name + ".cs"),
                    name,
                    string.Join(",\r\n", props),
                    string.Join(",\r\n", values),
                    string.Join(",\r\n", entity),
                    string.Join("\r\n", param),
                    string.Join("\r\n", copyparam));
            }

            // Join with "else" so the generated code becomes a proper if / else if chain
            WriteMetaFile(
                baseMetaPath,
                string.Join("\r\n            else ", newEntities),
                string.Join("\r\n            else ", existingEntities),
                string.Join("\r\n            else ", getEntities));
        }

        // ------------------------------------------------------------------
        // Safe getter that tolerates Values[...] containing a string
        // ------------------------------------------------------------------
        static string GetTypedGetter(string propName, Type type, string cleanTypeName)
        {
            Type underlying = Nullable.GetUnderlyingType(type) ?? type;

            if (underlying == typeof(string))
                return $"(string)Values[\"{propName}\"]";

            if (underlying == typeof(int))
                return $"Convert.ToInt32(Values[\"{propName}\"])";

            if (underlying == typeof(short))
                return $"Convert.ToInt16(Values[\"{propName}\"])";

            if (underlying == typeof(long))
                return $"Convert.ToInt64(Values[\"{propName}\"])";

            if (underlying == typeof(bool))
                return $"Convert.ToBoolean(Values[\"{propName}\"])";

            if (underlying == typeof(DateTime))
                return $"Convert.ToDateTime(Values[\"{propName}\"])";

            if (underlying == typeof(Uri))
                return $"Values[\"{propName}\"] is Uri u ? u : new Uri(Values[\"{propName}\"]?.ToString() ?? \"\")";

            if (underlying.IsEnum)
                return $"({cleanTypeName})Enum.Parse(typeof({underlying.FullName}), Values[\"{propName}\"]?.ToString() ?? \"0\")";

            // Important: use the clean name for generics / everything else
            return $"({cleanTypeName})Values[\"{propName}\"]";
        }

        // ------------------------------------------------------------------
        // Generates one *Meta.cs file per model
        // ------------------------------------------------------------------
        static void WriteFile(string filename, string classname,
                              string data, string values, string entity,
                              string param, string copyparam)
        {
            using StreamWriter sw = new(filename, false);

            sw.Write(
$$"""
using StreamerBotLib.Models.Enums;
using StreamerBotLib.Models.Interfaces;

namespace StreamerBotLib.DataSQL.TableMeta
{
    internal class {{classname}} : IDatabaseTableMeta
    {
{{param}}

        public Dictionary<string, object> Values { get; }

        public string TableName { get; } = "{{classname}}";

        public {{classname}}(Models.{{classname}} tableData)
        {
            Values = new()
            {
{{values}}
            };
        }

        public Dictionary<string, Type> Meta => new()
        {
{{data}}
        };

        public object GetModelEntity()
        {
            return new Models.{{classname}}(
{{entity}}
            );
        }

        public void CopyUpdates(Models.{{classname}} modelData)
        {
{{copyparam}}
        }
    }
}
""");
            sw.Close();
        }

        // ------------------------------------------------------------------
        // Generates the big TableMeta.cs with the injected if/else chains
        // ------------------------------------------------------------------
        static void WriteMetaFile(string filename,
                                  string NewEntity,
                                  string ExistingEntity,
                                  string GetEntity)
        {
            using StreamWriter sw = new(filename);

            sw.Write(
$$"""
using StreamerBotLib.DataSQL.AccessPolicy;
using StreamerBotLib.Models.Enums;
using StreamerBotLib.Models.Interfaces;

namespace StreamerBotLib.DataSQL.TableMeta
{
    public class TableMeta
    {
        internal IDatabaseTableMeta CurrEntity;

        public object DataEntity { get; private set; }

        public class EntityData(string name, TableMeta tableMeta, bool IsNew)
        {
            private TableMeta _tableMeta = tableMeta;
            public string Name { get; } = name;
            public bool IsReadOnly { get; } = IsNew
                ? PermissionDigest.GetColumnPermissions(tableMeta.CurrEntity.TableName, name).IsNewReadOnly
                : PermissionDigest.GetColumnPermissions(tableMeta.CurrEntity.TableName, name).IsEditReadOnly;
            public bool IsEnabled => !IsReadOnly;
            public Type ColType => _tableMeta.CurrEntity.Meta[Name];
            public object Value
            {
                get => _tableMeta.CurrEntity.Values[Name];
                set => _tableMeta.CurrEntity.Values[Name] = value;
            }
            public PopupEditTableDataType TableDataType => _tableMeta.CheckColumn(Name);
        }

        public List<EntityData> BindingList { get; } = [];

        public TableMeta SetNewEntity(Type Entity)
        {
            {{NewEntity}}

            SetBindingList(true);
            return this;
        }

        public TableMeta SetExistingEntity(object Entity)
        {
            DataEntity = Entity;

            {{ExistingEntity}}

            SetBindingList(false);
            return this;
        }

        private void SetBindingList(bool IsNew) =>
            BindingList.AddRange(
                from K in CurrEntity.Values.Keys
                where K is not "Id"
                select new EntityData(K, this, IsNew));

        public object GetEditedEntity() => GetUpdatedEntity(CurrEntity);

        private object GetUpdatedEntity(IDatabaseTableMeta Update)
        {
            {{GetEntity}}
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Maps a column name (and optionally its Type) to the correct UI element kind.
        /// </summary>
        public PopupEditTableDataType CheckColumn(string columnName)
        {
            // ----- 1. Exact name matches (highest priority) -----
            switch (columnName)
            {
                // Booleans
                case "IsFollower":
                case "AddMe":
                case "IsEnabled":
                case "AllowParam":
                case "AddEveryone":
                case "LookupData":
                case "UseChatMsg":
                case "Announce":
                    return PopupEditTableDataType.Bool;

                // File paths
                case "MediaFile":
                case "ImageFile":
                    return PopupEditTableDataType.FilePath;

                // Category multi-select
                case "Category":
                    return PopupEditTableDataType.Category;

                // Table + dependent fields
                case "Table":
                    return PopupEditTableDataType.Table;

                case "KeyField":
                case "DataField":
                case "CurrencyField":
                    return PopupEditTableDataType.TableField;

                // Overlay cascade
                case "OverlayAction":
                    return PopupEditTableDataType.OverlayAction;

                // ModeratorApprove cascades
                case "ModActionType":
                case "ModPerformType":
                    return PopupEditTableDataType.ModActionType;

                case "ModActionName":
                case "ModPerformAction":
                    return PopupEditTableDataType.ModPerformName;

                // Common date columns
                case "FollowedDate":
                case "FirstDateSeen":
                case "CurrLoginDate":
                case "LastDateSeen":
                case "CreatedAt":
                case "DateTime":
                case "StreamStart":
                case "StreamEnd":
                case "StatusChangeDate":
                case "AddDate":
                    return PopupEditTableDataType.DateTime;
            }

            // ----- 2. Fall back to Type information -----
            return DetermineElementTypeByType(CurrEntity.Meta[columnName]);
        }

        private static PopupEditTableDataType DetermineElementTypeByType(Type columnType)
        {
            if (columnType == null)
                return PopupEditTableDataType.Text;

            if (columnType.IsEnum || (columnType.FullName?.Contains("Enums") ?? false))
                return PopupEditTableDataType.Enum;

            if (columnType == typeof(bool) || columnType == typeof(bool?))
                return PopupEditTableDataType.Bool;

            if (columnType == typeof(DateTime) || columnType == typeof(DateTime?))
                return PopupEditTableDataType.DateTime;

            // Everything else (string, int, long, short, TimeSpan, Uri, etc.)
            return PopupEditTableDataType.Text;
        }
    }
}
""");
            sw.Close();
        }
    }
}