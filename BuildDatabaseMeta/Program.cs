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
                        if (p.Name != "Item" && !FileNames.Contains(p.Name) && !p.PropertyType.ToString().Contains(".Models."))
                        {
                            string PropName = p.PropertyType.Name.Contains("ICollection") ? $"ICollection<{p.PropertyType.GenericTypeArguments[0].FullName}>" : p.PropertyType.FullName;

                            if (!FileNames.Contains(p.Name))
                            {
                                props.Add($"              {{ \"{p.Name}\", typeof({PropName}) }}");
                                values.Add($"                 {{ \"{p.Name}\", tableData.{p.Name} }}");
                                entity.Add($"                                          ({PropName})Values[\"{p.Name}\"]");
                                param.Add($"        public {PropName} {p.Name} => ({PropName})Values[\"{p.Name}\"];");
                                copyparam.Add($"          if (modelData.{p.Name} != {p.Name})\r\n" +
                                                $"            {{\r\n" +
                                                $"                modelData.{p.Name} = {p.Name};\r\n" +
                                                $"            }}\r\n");
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
            $"using StreamerBotLib.Enums;\r\n" +
            $"using StreamerBotLib.DataSQL.Models;\r\n" +
            $"using StreamerBotLib.Interfaces;\r\n" +
            $"using StreamerBotLib.Overlay.Enums;\r\n" +
            $"\r\n" +
            $"namespace StreamerBotLib.DataSQL.TableMeta\r\n" +
            $"{{\r\n" +
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
            $");\r\n" +
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
                $"using StreamerBotLib.Interfaces;\r\n" +
                $"\r\n" +
                $"namespace StreamerBotLib.DataSQL.TableMeta\r\n" +
                $"{{\r\n" +
                $"    public class TableMeta\r\n" +
                $"    {{\r\n" +
                $"        internal IDatabaseTableMeta CurrEntity;\r\n" +
                $"\r\n" +
                $"        private object DataEntity;\r\n" +
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
                $"        public object GetUpdatedEntity(IDatabaseTableMeta Update)\r\n" +
                $"        {{\r\n" +
                $"            {GetEntity}" +
                $"            else return null;\r\n" +
                $"        }}\r\n" +
                $"    }}\r\n" +
                $"}}\r\n" +
                $"\r\n"
                );
            streamWriter.Close();
        }
    }
}
