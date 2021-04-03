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
        internal int Top { get; set; } = 1;
        internal string Sort { get; set; } = string.Empty;
        internal string Action { get; set; } = "Get";
        internal bool AllowUser { get; set; } = false;
        internal int Timer { get; set; } = 0;
        internal string Usage { get; set; } = "!<command>";
        internal string Message { get; set; } = string.Empty;

        internal static CommandParams Parse(string ParamString)
        {
            List<string> list = new( ParamString.Split(' ') );
            return Parse(list);
        }

        internal static CommandParams Parse(List<string> ParamList)
        {
            bool CheckList(string a) => ParamList.Exists((s) => s.StartsWith(a));

            CommandParams data = new();

            if (ParamList.Count > 0)
            {
                bool c = CheckList("-c");
                bool f = CheckList("-f");
                bool t = CheckList("-t");
                bool u = CheckList("-unit");

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
                        case "-t":
                            data.Table = keyvalue[1];
                            break;
                        case "-f":
                            data.Field = keyvalue[1];
                            break;
                        case "-c":
                            data.Currency = keyvalue[1];
                            break;
                        case "-unit":
                            data.Unit = keyvalue[1];
                            break;
                        case "-p":
                            data.Permission = (ViewerTypes)Enum.Parse(typeof(ViewerTypes), keyvalue[1]);
                            break;
                        case "-top":
                            data.Top = int.Parse(keyvalue[1]);
                            break;
                        case "-s":
                            data.Sort = keyvalue[1];
                            break;
                        case "-a":
                            data.Action = keyvalue[1];
                            break;
                        case "-u":
                            data.AllowUser = bool.Parse(keyvalue[1]);
                            break;
                        case "-timer":
                            data.Timer = int.Parse(keyvalue[1]);
                            break;
                        case "-usage":
                            checkUsage = true;
                            data.Usage = keyvalue[1];
                            break;
                        default:
                            data.Message = keyvalue[0];
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
            string Combine(string key, string value) => key + ":" + value + " ";

            string param = "";

            Dictionary<string, string> paramdictionary = new()
            {
                { "-t", Table },
                { "-f", Field },
                { "-c", Currency },
                { "-unit", Unit },
                //{ "-p", Permission.ToString() },
                { "-top", Top.ToString() },
                { "-s", Sort },
                { "-a", Action },
                //{ "-u", AllowUser.ToString() },
                //{ "-timer", Timer.ToString() }
            };

            foreach(string k in paramdictionary.Keys)
            {
                if(paramdictionary[k] != string.Empty && paramdictionary[k] != ViewerTypes.Viewer.ToString() && paramdictionary[k] != "Get" && paramdictionary[k] != "0")
                {
                    param += Combine(k, paramdictionary[k]);
                }
            }

            return param.Trim();
        }
    }
}
