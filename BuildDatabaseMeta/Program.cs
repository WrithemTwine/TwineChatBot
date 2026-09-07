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
            string ModelPath = "C:\\Source\\ChatBotApp\\StreamerBotLib\\DataSQL\\Models";
            string MetaPath = "C:\\Source\\ChatBotApp\\StreamerBotLib\\DataSQL\\TableMeta";

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
                Type table = Type.GetType($"StreamerBotLib.DataSQL.Models.{name}, {Assembly.GetAssembly(typeof(StreamerBotLib.DataSQL.Models.UserBase))}");

                List<string> props = [];
                List<string> values = [];
                List<string> entity = [];
                List<string> param = [];
                List<string> copyparam = [];
                List<string> getNew = [];
                List<string> copyEntity = [];

                if (!name.Contains("Base"))
                {
                    newEntities.Add($"if (Entity == typeof(Models.{name}))\r\n" +
                       $"            {{\r\n" +
                       $"                CurrEntity = new {name}(new Models.{name}());\r\n" +
                       $"            }}\r\n"
                    );

                    existingEntities.Add($"if (Entity.GetType() == typeof(Models.{name}))\r\n" +
                        $"            {{\r\n" +
                        $"                CurrEntity = new {name}((Models.{name})Entity);\r\n" +
                        $"            }}\r\n"
                    );

                    getEntities.Add($"if (DataEntity.GetType() == typeof(Models.{name}))\r\n" +
                        $"            {{\r\n" +
                        $"                (({name})Update).CopyUpdates((Models.{name})DataEntity);\r\n" +
                        $"                return DataEntity;\r\n" +
                        $"            }}\r\n"
                    );

                    foreach (PropertyInfo p in table.GetProperties())
                    {
                        if (p.Name != "Item" && p.Name != "DataSource" && p.Name != "Commandtype" && ((!FileNames.Contains(p.Name) && !p.PropertyType.ToString().Contains(".Models.")) || p.Name == "UserStats"))
                        {
                            string PropName = p.PropertyType.Name.Contains("ICollection") ? $"ICollection<{p.PropertyType.GenericTypeArguments[0].FullName}>" : p.PropertyType.FullName;

                            if (!FileNames.Contains(p.Name) && !p.Name.Contains("Calls"))
                            {
                                props.Add($"              {{ \"{p.Name}\", typeof({PropName}) }}");
                                values.Add($"                 {{ \"{p.Name}\", tableData.{p.Name} }}");

                                string newEntity = (PropName.Contains("Int") || PropName.Contains("int")) ? $"Convert.To{PropName.Replace("System.", "")}({p.Name})" : $"{p.Name}";

                                if (p.Name != "Duration" && p.Name != "Id" && p.Name != "Commandtype" && p.Name != "DataSource") // ignore "Duration" column as it is computed
                                {
                                    copyparam.Add($"          if (modelData.{p.Name} != {p.Name})\r\n" +
                                            $"            {{\r\n" +
                                            $"                modelData.{p.Name} = {p.Name};\r\n" +
                                            $"            }}\r\n");
                                    entity.Add($"            {p.Name.ToLower()[0]}{p.Name[1..]}: {newEntity}");
                                }
                                param.Add($"        public {PropName} {p.Name} {{ get => ({PropName})Values[\"{p.Name}\"]; set => Values[\"{p.Name}\"] = value; }}");
                            }
                        }
                    }

                    WriteFile(Path.Combine(MetaPath, name + ".cs"), name, string.Join(",\r\n", props), string.Join(",\r\n", values), string.Join(", \r\n", entity), string.Join("\r\n", param), string.Join("\r\n", copyparam));
                }
            }
            WriteMetaFile(baseMetaPath, string.Join("            else ", newEntities), string.Join("            else ", existingEntities), string.Join("            else ", getEntities));
        }

        static void WriteFile(string filename, string classname, string data, string values, string entity, string param, string copyparam)
        {
            StreamWriter streamWriter = new(filename, false);
            streamWriter.Write(
            $"namespace StreamerBotLib.DataSQL.TableMeta\r\n" +
            $"{{\r\n" +
               $"using StreamerBotLib.Enums;\r\n" +
            $"using StreamerBotLib.DataSQL.Models;\r\n" +
            $"using StreamerBotLib.Interfaces;\r\n" +
            $"using StreamerBotLib.Overlay.Enums;\r\n" +
            $"\r\n" +
         $"    internal class {classname} : IDatabaseTableMeta\r\n" +
            $"    {{\r\n" +
            $"{param}\r\n" +
            $"\r\n" +
            $"        public Dictionary<string, object> Values {{ get; }}\r\n" +
            $"\r\n" +
            $"        public string TableName {{ get; }} = \"{classname}\";\r\n" +
            $"\r\n" +
            $"        public {classname}(Models.{classname} tableData)\r\n" +
            $"        {{\r\n" +
            $"            Values = new()\r\n" +
            $"            {{\r\n" +
            $"{values}\r\n" +
            $"            }};\r\n" +
            $"        }}\r\n" +
            $"" +
            $"        public Dictionary<string, Type> Meta => new()\r\n" +
            $"        {{\r\n" +
            $"{data}\r\n" +
            $"        }};\r\n" +
            $"" +
            $"        public object GetModelEntity()\r\n" +
            $"        {{\r\n" +
            $"            return new Models.{classname}(\r\n" +
            $"{entity}\r\n" +
            $"        );\r\n" +
            $"        }}\r\n" +
            $"" +
            $"        public void CopyUpdates(Models.{classname} modelData)\r\n" +
            $"        {{\r\n" +
            $"{copyparam}\r\n" +
            $"        }}\r\n" +
            $"" +
            $"    }}\r\n" +
            $"}}\r\n" +
            $"\r\n");
            streamWriter.Close();
        }

        static void WriteMetaFile(string filename, string NewEntity, string ExistingEntity, string GetEntity)
        {
            StreamWriter streamWriter = new(filename);
            streamWriter.Write(
                $"namespace StreamerBotLib.DataSQL.TableMeta\r\n" +
                $"{{\r\n" +
                $"using StreamerBotLib.Interfaces;\r\n" +
                $"\r\n" +
                $"    public class TableMeta\r\n" +
                $"    {{\r\n" +
                $"        internal IDatabaseTableMeta CurrEntity;\r\n" +
                $"\r\n" +
                $"        public object DataEntity {{ get; private set; }}\r\n\r\n" +
                $"\r\n" +
                $"        public TableMeta SetNewEntity(Type Entity)\r\n" +
                $"        {{\r\n" +
                $"            {NewEntity}" +
                $"            \r\n" +
                $"            return this;\r\n" +
                $"        }}\r\n" +
                $"\r\n" +
                $"        public TableMeta SetExistingEntity(object Entity)\r\n" +
                $"        {{\r\n" +
                $"            DataEntity = Entity;\r\n" +
                $"\r\n" +
                $"            {ExistingEntity}" +
                $"            \r\n" +
                $"            return this;\r\n" +
                $"        }}\r\n" +
                $"\r\n" +
                $"        public object GetEditedEntity()" +
                $"        {{\r\n" +
                $"            return GetUpdatedEntity(CurrEntity);\r\n" +
                $"        }}\r\n" +
                $"\r\n" +
                $"        private object GetUpdatedEntity(IDatabaseTableMeta Update)\r\n" +
                $"        {{\r\n" +
                $"            {GetEntity}" +
                $"            else return null;\r\n" +
                $"        }}\r\n" +
                $"\r\n" +
                $"        /// <summary>\r\n" +
                $"          /// Maps a column name (and optionally its Type) to the correct UI element kind.\r\n" +
                $"        /// </summary>\r\n" +
                $"        public PopupEditTableDataType CheckColumn(string columnName)\r\n" +
                $"        {{\r\n" +
                $"            // ----- 1. Exact name matches (highest priority) -----\r\n" +
                $"            switch (columnName)\r\n" +
                $"            {{\r\n" +
                $"                // Booleans\r\n" +
                $"                case \"IsFollower\":\r\n" +
                $"                case \"AddMe\":\r\n" +
                $"                case \"IsEnabled\":\r\n" +
                $"                case \"AllowParam\":\r\n" +
                $"                case \"AddEveryone\":\r\n" +
                $"                case \"LookupData\":\r\n" +
                $"                case \"UseChatMsg\":\r\n" +
                $"                case \"Announce\":\r\n" +
                $"                    return PopupEditTableDataType.Bool;\r\n" +
                $"\r\n" +
                $"                // File paths\r\n" +
                $"                case \"MediaFile\":\r\n" +
                $"                case \"ImageFile\":\r\n" +
                $"                    return PopupEditTableDataType.FilePath;\r\n" +
                $"\r\n" +
                $"                // Category multi-select\r\n" +
                $"                case \"Category\":\r\n" +
                $"                    return PopupEditTableDataType.Category;\r\n" +
                $"\r\n" +
                $"                // Table + dependent fields\r\n" +
                $"                case \"Table\":\r\n" +
                $"                    return PopupEditTableDataType.Table;\r\n" +
                $"\r\n" +
                $"                case \"KeyField\":\r\n" +
                $"                case \"DataField\":\r\n" +
                $"                case \"CurrencyField\":\r\n" +
                $"                    return PopupEditTableDataType.TableField;\r\n" +
                $"\r\n" +
                $"                // Overlay cascade\r\n" +
                $"                case \"OverlayAction\":\r\n" +
                $"                    return PopupEditTableDataType.OverlayAction;\r\n" +
                $"\r\n" +
                $"                // ModeratorApprove cascades\r\n" +
                $"                case \"ModActionName\":\r\n" +
                $"                case \"ModPerformAction\":\r\n" +
                $"                    return PopupEditTableDataType.ModActionList;\r\n" +
                $"\r\n" +
                $"                // Common date columns\r\n" +
                $"                case \"FollowedDate\":\r\n" +
                $"                case \"FirstDateSeen\":\r\n" +
                $"                case \"CurrLoginDate\":\r\n" +
                $"                case \"LastDateSeen\":\r\n" +
                $"                case \"CreatedAt\":\r\n" +
                $"                case \"DateTime\":\r\n" +
                $"                case \"StreamStart\":\r\n" +
                $"                case \"StreamEnd\":\r\n" +
                $"                case \"StatusChangeDate\":\r\n" +
                $"                case \"AddDate\":\r\n" +
                $"                    return PopupEditTableDataType.DateTime;\r\n" +
                $"            }}\r\n" +
                $"\r\n" +
                $"            // ----- 2. Fall back to Type information -----\r\n" +
                $"            return DetermineElementTypeByType(CurrEntity.Meta[columnName]);\r\n" +
                $"        }}\r\n" +
                $"\r\n" +
                $"        private static PopupEditTableDataType DetermineElementTypeByType(Type columnType)\r\n" +
                $"        {{\r\n" +
                $"            if (columnType == null)\r\n" +
                $"                return PopupEditTableDataType.Text;\r\n" +
                $"\r\n            if (columnType.IsEnum || (columnType.FullName?.Contains(\"Enums\") ?? false))\r\n" +
                $"                return PopupEditTableDataType.Enum;\r\n" +
                $"\r\n            if (columnType == typeof(bool) || columnType == typeof(bool?))\r\n" +
                $"                return PopupEditTableDataType.Bool;\r\n" +
                $"\r\n" +
                $"            if (columnType == typeof(DateTime) || columnType == typeof(DateTime?))\r\n" +
                $"                return PopupEditTableDataType.DateTime;\r\n" +
                $"\r\n" +
                $"            // Everything else (string, int, long, short, TimeSpan, Uri, etc.)\r\n" +
                $"            return PopupEditTableDataType.Text;\r\n" +
                $"        }}" +
                $"    }}\r\n" +
                $"}}\r\n" +
                $"\r\n"
                );
            streamWriter.Close();
        }
    }
}
