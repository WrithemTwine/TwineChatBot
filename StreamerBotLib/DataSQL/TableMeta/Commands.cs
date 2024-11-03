using StreamerBotLib.Interfaces;

namespace StreamerBotLib.DataSQL.TableMeta
{
    internal class Commands : IDatabaseTableMeta
    {
        public System.String CmdName { get => (System.String)Values["CmdName"]; set => Values["CmdName"] = value; }
        public System.Boolean AddMe { get => (System.Boolean)Values["AddMe"]; set => Values["AddMe"] = value; }
        public StreamerBotLib.Enums.ViewerTypes Permission { get => (StreamerBotLib.Enums.ViewerTypes)Values["Permission"]; set => Values["Permission"] = value; }
        public System.Boolean IsEnabled { get => (System.Boolean)Values["IsEnabled"]; set => Values["IsEnabled"] = value; }
        public System.Boolean Announce { get => (System.Boolean)Values["Announce"]; set => Values["Announce"] = value; }
        public System.String Message { get => (System.String)Values["Message"]; set => Values["Message"] = value; }
        public System.Int32 RepeatTimer { get => (System.Int32)Values["RepeatTimer"]; set => Values["RepeatTimer"] = value; }
        public System.Int16 SendMsgCount { get => (System.Int16)Values["SendMsgCount"]; set => Values["SendMsgCount"] = value; }
        public ICollection<System.String> Category { get => (ICollection<System.String>)Values["Category"]; set => Values["Category"] = value; }
        public System.Boolean AllowParam { get => (System.Boolean)Values["AllowParam"]; set => Values["AllowParam"] = value; }
        public System.String Usage { get => (System.String)Values["Usage"]; set => Values["Usage"] = value; }
        public System.Boolean LookupData { get => (System.Boolean)Values["LookupData"]; set => Values["LookupData"] = value; }
        public System.String Table { get => (System.String)Values["Table"]; set => Values["Table"] = value; }
        public System.String KeyField { get => (System.String)Values["KeyField"]; set => Values["KeyField"] = value; }
        public System.String DataField { get => (System.String)Values["DataField"]; set => Values["DataField"] = value; }
        public System.String CurrencyField { get => (System.String)Values["CurrencyField"]; set => Values["CurrencyField"] = value; }
        public System.String Unit { get => (System.String)Values["Unit"]; set => Values["Unit"] = value; }
        public StreamerBotLib.Enums.CommandAction Action { get => (StreamerBotLib.Enums.CommandAction)Values["Action"]; set => Values["Action"] = value; }
        public System.Int32 Top { get => (System.Int32)Values["Top"]; set => Values["Top"] = value; }
        public StreamerBotLib.Enums.CommandSort Sort { get => (StreamerBotLib.Enums.CommandSort)Values["Sort"]; set => Values["Sort"] = value; }
        public System.Int32 Calls { get => (System.Int32)Values["Calls"]; set => Values["Calls"] = value; }

        public Dictionary<string, object> Values { get; }

        public string TableName { get; } = "Commands";

        public Commands(Models.Commands tableData)
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
                 { "Sort", tableData.Sort },
                 { "Calls", tableData.Calls }
            };
        }
        public Dictionary<string, Type> Meta => new()
        {
              { "CmdName", typeof(System.String) },
              { "AddMe", typeof(System.Boolean) },
              { "Permission", typeof(StreamerBotLib.Enums.ViewerTypes) },
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
              { "Action", typeof(StreamerBotLib.Enums.CommandAction) },
              { "Top", typeof(System.Int32) },
              { "Sort", typeof(StreamerBotLib.Enums.CommandSort) },
              { "Calls", typeof(System.Int32) }
        };
        public object GetModelEntity()
        {
            return new Models.Commands(
            cmdName: CmdName,
            addMe: AddMe,
            permission: Permission,
            isEnabled: IsEnabled,
            announce: Announce,
            message: Message,
            repeatTimer: Convert.ToInt32(RepeatTimer),
            sendMsgCount: Convert.ToInt16(SendMsgCount),
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
            top: Convert.ToInt32(Top),
            sort: Sort,
            calls: Convert.ToInt32(Calls)
        );
        }
        public void CopyUpdates(Models.Commands modelData)
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

            if (modelData.Calls != Calls)
            {
                modelData.Calls = Calls;
            }

        }
    }
}

