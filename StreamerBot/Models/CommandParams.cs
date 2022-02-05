using StreamerBot.Enums;
using StreamerBot.Systems;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Linq;

namespace StreamerBot.Models
{
    [DebuggerDisplay("Table={Table}, Field={Field}")]
    public class CommandParams
    {
        public string Table { get; set; } = string.Empty;
        public string Field { get; set; } = string.Empty;
        public string Currency { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public ViewerTypes Permission { get; set; } = ViewerTypes.Viewer;
        public int Top { get; set; } = 0;
        public string Sort { get; set; } = CommandSort.ASC.ToString();
        public string Action { get; set; } = CommandAction.Get.ToString();
        public bool AllowParam { get; set; } = false;
        public bool LookupData { get; set; } = false;
        public int Timer { get; set; } = 0;
        public short RepeatMsg { get; set; } = 0;
        public string Usage { get; set; } = "!<command>";
        public string Message { get; set; } = string.Empty;
        public bool IsEnabled { get; set; } = false;
        public bool AddMe { get; set; } = false;
        public bool Empty { get; set; } = false;
        public string Category { get; set; } = LocalizedMsgSystem.GetVar(Msg.MsgAllCateogry);

        public static CommandParams Parse(string ParamString) => Parse(new List<string>(new Regex(@"(^-)|( -)").Split(ParamString)));

        public static CommandParams Parse(List<string> ParamList)
        {
            bool CheckList(string a) => ParamList.Exists((s) => s.StartsWith(a));

            CommandParams data = new();
            ParamList.RemoveAll((s) => s.StartsWith(" -") || s == string.Empty);

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
                    throw new InvalidOperationException(LocalizedMsgSystem.GetVar(f ? ChatBotExceptions.ExceptionInvalidOperationTable : ChatBotExceptions.ExceptionInvalidOperationField));
                }

                if (f && !t)
                {
                    throw new InvalidOperationException(LocalizedMsgSystem.GetVar(ChatBotExceptions.ExceptionInvalidOperationTable));
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
                            data.Permission = (ViewerTypes)System.Enum.Parse(typeof(ViewerTypes), value);
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
                        case "r":
                            data.RepeatMsg = short.Parse(value);
                            break;
                        case "use":
                            checkUsage = true;
                            data.Usage = value;
                            break;
                        case "m":
                            data.Message = keyvalue[0];
                            break;
                        case "category":
                            data.Category = value;
                            break;
                        case "addme":
                            data.AddMe = bool.Parse(value);
                            break;
                        case "e":
                            data.IsEnabled = bool.Parse(value);
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

        public static Dictionary<string,string> ParseEditCommandParams(List<string> arglist)
        {
            Dictionary<string, string> edit = new();
            foreach (var (keyvalue, value) in from string param in arglist
                                              let keyvalue = param.Split(':')
                                              let value = (keyvalue.Length > 1) ? keyvalue[1].Trim() : ""
                                              select (keyvalue, value))
            {
                switch (keyvalue[0])
                {
                    case "t":
                        edit.Add("table", value);
                        break;
                    case "f":
                        edit.Add("data_field", value);
                        break;
                    case "c":
                        edit.Add("currency_field", value);
                        break;
                    case "unit":
                        edit.Add("unit", value);
                        break;
                    case "p":
                        edit.Add("Permission", (string)System.Enum.Parse(typeof(ViewerTypes), value));
                        break;
                    case "top":
                        edit.Add("top", value);
                        break;
                    case "s":
                        edit.Add("sort", Validate(value, typeof(CommandSort)));
                        break;
                    case "a":
                        edit.Add("action", Validate(value, typeof(CommandAction)));
                        break;
                    case "param":
                        edit.Add("AllowParam", value);
                        break;
                    case "timer":
                        edit.Add("RepeatTimer", value);
                        break;
                    case "r":
                        edit.Add("SendMsgCount", value);
                        break;
                    case "use":
                        edit.Add("Usage", value);
                        break;
                    case "m":
                        edit.Add("Message", value);
                        break;
                    case "category":
                        edit.Add("Category", value);
                        break;
                    case "addme":
                        edit.Add("AddMe", value);
                        break;
                    case "e":
                        edit.Add("IsEnabled", value);
                        break;
                    default:
                        break;
                }
            }

            return edit;
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

            throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, LocalizedMsgSystem.GetVar(ChatBotExceptions.ExceptionArgument), v, type.GetEnumNames().ToString()));
        }



        //public string DBParamsString()
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
