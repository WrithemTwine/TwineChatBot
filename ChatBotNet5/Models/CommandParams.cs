using System;
using System.Collections.Generic;

namespace ChatBot_Net5.Models
{
    internal class CommandParams
    {
        internal string Table { get; set; } = string.Empty;
        internal string Field { get; set; } = string.Empty;
        internal string Currency { get; set; } = string.Empty;
        internal string Unit { get; set; } = string.Empty;
        internal ViewerTypes Permission { get; set; } = ViewerTypes.Viewer;
        internal long Top { get; set; } = 0;
        internal string Sort { get; set; } = CommandSort.ASC.ToString();
        internal string Action { get; set; } = CommandAction.Get.ToString();
        internal bool AllowParam { get; set; } = false;
        internal bool LookupData { get; set; } = false;
        internal int Timer { get; set; } = 0;
        internal string Usage { get; set; } = "!<command>";
        internal string Message { get; set; } = string.Empty;
        internal bool AddMe { get; set; } = false;
        internal bool Empty { get; set; } = false;

        internal static CommandParams Parse(string ParamString)
        {
            List<string> list = new( ParamString.Split('-') );
            return Parse(list);
        }

        internal static CommandParams Parse(List<string> ParamList)
        {
            bool CheckList(string a) => ParamList.Exists((s) => s.StartsWith(a));

            CommandParams data = new();

            if (ParamList[0] == " ")
            {
                data.Empty = true;
            }
            else if (ParamList.Count > 0)
            {
                bool c = CheckList("c");
                bool f = CheckList("f");
                bool t = CheckList("t");
                bool u = CheckList("unit");

                if ((c || u) && !(f && t))
                {
                    throw new InvalidOperationException(f ? "Table not specified." : "Field not specified.");
                }

                if (f && !t)
                {
                    throw new InvalidOperationException("Table not specified.");
                }

                bool checkUsage = false;

                foreach (string param in ParamList)
                {
                    string[] keyvalue = param.Split(':');
                    string value = (keyvalue.Length > 1) ? keyvalue[1].Trim() : "";

                    switch (keyvalue[0])
                    {
                        case "t":
                            data.Table = value;
                            break;
                        case "f":
                            data.Field = value;
                            break;
                        case "c":
                            data.Currency = value;
                            break;
                        case "unit":
                            data.Unit = value;
                            break;
                        case "p":
                            data.Permission = (ViewerTypes)Enum.Parse(typeof(ViewerTypes), value);
                            break;
                        case "top":
                            data.Top = int.Parse(value);
                            break;
                        case "s":
                            data.Sort = Validate(value, typeof(CommandSort));
                            break;
                        case "a":
                            data.Action = Validate(value, typeof(CommandAction));
                            break;
                        case "param":
                            data.AllowParam = bool.Parse(value);
                            break;
                        case "timer":
                            data.Timer = int.Parse(value);
                            break;
                        case "use":
                            checkUsage = true;
                            data.Usage = value;
                            break;
                        case "m":
                            data.Message = keyvalue[0];
                            break;
                        case "addme":
                            data.AddMe = bool.Parse(value);
                            break;
                    }
                }

                if (!checkUsage)
                {
                    data.Usage = "!<command>" + (data.AllowParam ? " displayname" : "");
                }
            }

            data.LookupData = !(data.Table == string.Empty);

            return data;
        }

        private static string Validate(string v, Type type)
        {
            foreach (string enumvalue in type.GetEnumNames())
            {
                if (enumvalue == v)
                {
                    return enumvalue;
                }
            }

            throw new ArgumentException(string.Format("The supplied value {0} is not included within the acceptable list: {1}", v, type.GetEnumNames().ToString()));
        }



        //internal string DBParamsString()
        //{
        //    static string Combine(string key, string value) => key + ":" + value + " ";

        //    string param = " ";

        //    if (!Empty)
        //    {
        //        Dictionary<string, string> paramdictionary = new()
        //        {
        //            { "c", Currency },
        //            { "unit", Unit },
        //            { "top", Top.ToString() },
        //            { "s", Sort },
        //            { "a", Action }
        //        };

        //        foreach (string k in paramdictionary.Keys)
        //        {
        //            param += Combine(k, paramdictionary[k]);
        //        }
        //    }

        //    return param.Trim();
        //}
    }
}
