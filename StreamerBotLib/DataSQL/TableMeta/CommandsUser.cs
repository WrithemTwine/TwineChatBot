using StreamerBotLib.Models.Interfaces;

namespace StreamerBotLib.DataSQL.TableMeta
{
    internal class CommandsUser : IDatabaseTableMeta
    {
        public System.String CmdName { get => (string)Values["CmdName"]; set => Values["CmdName"] = value; }
        public System.Boolean AddMe { get => Convert.ToBoolean(Values["AddMe"]); set => Values["AddMe"] = value; }
        public StreamerBotLib.Models.Enums.ViewerTypes Permission { get => (StreamerBotLib.Models.Enums.ViewerTypes)Enum.Parse(typeof(StreamerBotLib.Models.Enums.ViewerTypes), Values["Permission"]?.ToString() ?? "0"); set => Values["Permission"] = value; }
        public System.Boolean IsEnabled { get => Convert.ToBoolean(Values["IsEnabled"]); set => Values["IsEnabled"] = value; }
        public System.Boolean Announce { get => Convert.ToBoolean(Values["Announce"]); set => Values["Announce"] = value; }
        public System.String Message { get => (string)Values["Message"]; set => Values["Message"] = value; }
        public System.Int32 RepeatTimer { get => Convert.ToInt32(Values["RepeatTimer"]); set => Values["RepeatTimer"] = value; }
        public System.Int16 SendMsgCount { get => Convert.ToInt16(Values["SendMsgCount"]); set => Values["SendMsgCount"] = value; }
        public ICollection<System.String> Category { get => (ICollection<System.String>)Values["Category"]; set => Values["Category"] = value; }
        public System.Boolean AllowParam { get => Convert.ToBoolean(Values["AllowParam"]); set => Values["AllowParam"] = value; }
        public System.String Usage { get => (string)Values["Usage"]; set => Values["Usage"] = value; }
        public System.Boolean LookupData { get => Convert.ToBoolean(Values["LookupData"]); set => Values["LookupData"] = value; }
        public System.String Table { get => (string)Values["Table"]; set => Values["Table"] = value; }
        public System.String KeyField { get => (string)Values["KeyField"]; set => Values["KeyField"] = value; }
        public System.String DataField { get => (string)Values["DataField"]; set => Values["DataField"] = value; }
        public System.String CurrencyField { get => (string)Values["CurrencyField"]; set => Values["CurrencyField"] = value; }
        public System.String Unit { get => (string)Values["Unit"]; set => Values["Unit"] = value; }
        public StreamerBotLib.Models.Enums.CommandAction Action { get => (StreamerBotLib.Models.Enums.CommandAction)Enum.Parse(typeof(StreamerBotLib.Models.Enums.CommandAction), Values["Action"]?.ToString() ?? "0"); set => Values["Action"] = value; }
        public System.Int32 Top { get => Convert.ToInt32(Values["Top"]); set => Values["Top"] = value; }
        public StreamerBotLib.Models.Enums.CommandSort Sort { get => (StreamerBotLib.Models.Enums.CommandSort)Enum.Parse(typeof(StreamerBotLib.Models.Enums.CommandSort), Values["Sort"]?.ToString() ?? "0"); set => Values["Sort"] = value; }

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
                 { "Announce", tableData.Announce },
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
              { "Permission", typeof(StreamerBotLib.Models.Enums.ViewerTypes) },
              { "IsEnabled", typeof(System.Boolean) },
              { "Announce", typeof(System.Boolean) },
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
              { "Action", typeof(StreamerBotLib.Models.Enums.CommandAction) },
              { "Top", typeof(System.Int32) },
              { "Sort", typeof(StreamerBotLib.Models.Enums.CommandSort) }
        };

        public object GetModelEntity()
        {
            return new Models.CommandsUser(
            cmdName: CmdName,
            addMe: AddMe,
            permission: Permission,
            isEnabled: IsEnabled,
            announce: Announce,
            message: Message,
            repeatTimer: RepeatTimer,
            sendMsgCount: SendMsgCount,
            category: Category,
            allowParam: AllowParam,
            usage: Usage,
            lookupData: LookupData,
            table: Table,
            keyField: KeyField,
            dataField: DataField,
            currencyField: CurrencyField,
            unit: Unit,
            action: Action,
            top: Top,
            sort: Sort
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
            if (modelData.Announce != Announce)
            {
                modelData.Announce = Announce;
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