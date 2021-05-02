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
        internal int Top { get; set; } = 0;
        internal string Sort { get; set; } = "ASC";
        internal string Action { get; set; } = "Get";
        internal bool AllowUser { get; set; } = false;
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

                    switch (keyvalue[0])
                    {
                        case "t":
                            data.Table = keyvalue[1];
                            break;
                        case "f":
                            data.Field = keyvalue[1];
                            break;
                        case "c":
                            data.Currency = keyvalue[1];
                            break;
                        case "unit":
                            data.Unit = keyvalue[1];
                            break;
                        case "p":
                            data.Permission = (ViewerTypes)Enum.Parse(typeof(ViewerTypes), keyvalue[1]);
                            break;
                        case "top":
                            data.Top = int.Parse(keyvalue[1]);
                            break;
                        case "s":
                            data.Sort = keyvalue[1];
                            break;
                        case "a":
                            data.Action = keyvalue[1];
                            break;
                        case "u":
                            data.AllowUser = bool.Parse(keyvalue[1]);
                            break;
                        case "timer":
                            data.Timer = int.Parse(keyvalue[1]);
                            break;
                        case "usage":
                            checkUsage = true;
                            data.Usage = keyvalue[1];
                            break;
                        case "m":
                            data.Message = keyvalue[0];
                            break;
                        case "addme":
                            data.AddMe = bool.Parse(keyvalue[1]);
                            break;
                    }
                }

                if (!checkUsage)
                {
                    data.Usage = "!<command>" + (data.AllowUser ? " displayname" : "");
                }
            }

            return data;
        }

        internal string DBParamsString()
        {
            static string Combine(string key, string value) => key + ":" + value + " ";

            string param = " ";

            if (!Empty)
            {
                Dictionary<string, string> paramdictionary = new()
                {
                    { "c", Currency },
                    { "unit", Unit },
                    { "top", Top.ToString() },
                    { "s", Sort },
                    { "a", Action }
                };

                foreach (string k in paramdictionary.Keys)
                {
                    param += Combine(k, paramdictionary[k]);
                }
            }

            return param.Trim();
        }
    }
}
