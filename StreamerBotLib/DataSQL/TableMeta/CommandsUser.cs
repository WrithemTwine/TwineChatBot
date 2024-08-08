using StreamerBotLib.Interfaces;

namespace StreamerBotLib.DataSQL.TableMeta
{
    internal class CommandsUser : IDatabaseTableMeta
    {
        public System.String CmdName => (System.String)Values["CmdName"];
        public System.Boolean AddMe => (System.Boolean)Values["AddMe"];
        public StreamerBotLib.Enums.ViewerTypes Permission => (StreamerBotLib.Enums.ViewerTypes)Values["Permission"];
        public System.Boolean IsEnabled => (System.Boolean)Values["IsEnabled"];
        public System.String Message => (System.String)Values["Message"];
        public System.Int32 RepeatTimer => (System.Int32)Values["RepeatTimer"];
        public System.Int16 SendMsgCount => (System.Int16)Values["SendMsgCount"];
        public ICollection<System.String> Category => (ICollection<System.String>)Values["Category"];
        public System.Boolean AllowParam => (System.Boolean)Values["AllowParam"];
        public System.String Usage => (System.String)Values["Usage"];
        public System.Boolean LookupData => (System.Boolean)Values["LookupData"];
        public System.String Table => (System.String)Values["Table"];
        public System.String KeyField => (System.String)Values["KeyField"];
        public System.String DataField => (System.String)Values["DataField"];
        public System.String CurrencyField => (System.String)Values["CurrencyField"];
        public System.String Unit => (System.String)Values["Unit"];
        public StreamerBotLib.Enums.CommandAction Action => (StreamerBotLib.Enums.CommandAction)Values["Action"];
        public System.Int32 Top => (System.Int32)Values["Top"];
        public StreamerBotLib.Enums.CommandSort Sort => (StreamerBotLib.Enums.CommandSort)Values["Sort"];

        public Dictionary<string, object> Values { get; }

        public string TableName { get; } = "CommandsUser";

        public CommandsUser(Models.CommandsUser tableData)
        {
            Values = new()
            {
                 { "CmdName", tableData.CmdName },
                 { "AddMe", tableData.AddMe },
                 { "Permission", tableData.Permission },
                 { "IsEnabled", tableData.IsEnabled },
                 { "Message", tableData.Message },
                 { "RepeatTimer", tableData.RepeatTimer },
                 { "SendMsgCount", tableData.SendMsgCount },
                 { "Category", tableData.Category },
                 { "AllowParam", tableData.AllowParam },
                 { "Usage", tableData.Usage },
                 { "LookupData", tableData.LookupData },
                 { "Table", tableData.Table },
                 { "KeyField", tableData.KeyField },
                 { "DataField", tableData.DataField },
                 { "CurrencyField", tableData.CurrencyField },
                 { "Unit", tableData.Unit },
                 { "Action", tableData.Action },
                 { "Top", tableData.Top },
                 { "Sort", tableData.Sort }
            };
        }
        public Dictionary<string, Type> Meta => new()
        {
              { "CmdName", typeof(System.String) },
              { "AddMe", typeof(System.Boolean) },
              { "Permission", typeof(StreamerBotLib.Enums.ViewerTypes) },
              { "IsEnabled", typeof(System.Boolean) },
              { "Message", typeof(System.String) },
              { "RepeatTimer", typeof(System.Int32) },
              { "SendMsgCount", typeof(System.Int16) },
              { "Category", typeof(ICollection<System.String>) },
              { "AllowParam", typeof(System.Boolean) },
              { "Usage", typeof(System.String) },
              { "LookupData", typeof(System.Boolean) },
              { "Table", typeof(System.String) },
              { "KeyField", typeof(System.String) },
              { "DataField", typeof(System.String) },
              { "CurrencyField", typeof(System.String) },
              { "Unit", typeof(System.String) },
              { "Action", typeof(StreamerBotLib.Enums.CommandAction) },
              { "Top", typeof(System.Int32) },
              { "Sort", typeof(StreamerBotLib.Enums.CommandSort) }
        };
        public object GetModelEntity()
        {
            return new Models.CommandsUser(
                                          (System.String)Values["CmdName"],
                                          (System.Boolean)Values["AddMe"],
                                          (StreamerBotLib.Enums.ViewerTypes)Values["Permission"],
                                          (System.Boolean)Values["IsEnabled"],
                                          (System.String)Values["Message"],
                                          Convert.ToInt32(Values["RepeatTimer"]),
                                          Convert.ToInt16(Values["SendMsgCount"]),
                                          (ICollection<System.String>)Values["Category"],
                                          (System.Boolean)Values["AllowParam"],
                                          (System.String)Values["Usage"],
                                          (System.Boolean)Values["LookupData"],
                                          (System.String)Values["Table"],
                                          (System.String)Values["KeyField"],
                                          (System.String)Values["DataField"],
                                          (System.String)Values["CurrencyField"],
                                          (System.String)Values["Unit"],
                                          (StreamerBotLib.Enums.CommandAction)Values["Action"],
                                          Convert.ToInt32(Values["Top"]),
                                          (StreamerBotLib.Enums.CommandSort)Values["Sort"]
);
        }
        public void CopyUpdates(Models.CommandsUser modelData)
        {
            if (modelData.CmdName != CmdName)
            {
                modelData.CmdName = CmdName;
            }

            if (modelData.AddMe != AddMe)
            {
                modelData.AddMe = AddMe;
            }

            if (modelData.Permission != Permission)
            {
                modelData.Permission = Permission;
            }

            if (modelData.IsEnabled != IsEnabled)
            {
                modelData.IsEnabled = IsEnabled;
            }

            if (modelData.Message != Message)
            {
                modelData.Message = Message;
            }

            if (modelData.RepeatTimer != RepeatTimer)
            {
                modelData.RepeatTimer = RepeatTimer;
            }

            if (modelData.SendMsgCount != SendMsgCount)
            {
                modelData.SendMsgCount = SendMsgCount;
            }

            if (modelData.Category != Category)
            {
                modelData.Category = Category;
            }

            if (modelData.AllowParam != AllowParam)
            {
                modelData.AllowParam = AllowParam;
            }

            if (modelData.Usage != Usage)
            {
                modelData.Usage = Usage;
            }

            if (modelData.LookupData != LookupData)
            {
                modelData.LookupData = LookupData;
            }

            if (modelData.Table != Table)
            {
                modelData.Table = Table;
            }

            if (modelData.KeyField != KeyField)
            {
                modelData.KeyField = KeyField;
            }

            if (modelData.DataField != DataField)
            {
                modelData.DataField = DataField;
            }

            if (modelData.CurrencyField != CurrencyField)
            {
                modelData.CurrencyField = CurrencyField;
            }

            if (modelData.Unit != Unit)
            {
                modelData.Unit = Unit;
            }

            if (modelData.Action != Action)
            {
                modelData.Action = Action;
            }

            if (modelData.Top != Top)
            {
                modelData.Top = Top;
            }

            if (modelData.Sort != Sort)
            {
                modelData.Sort = Sort;
            }

        }
    }
}

