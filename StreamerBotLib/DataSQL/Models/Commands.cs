using Microsoft.EntityFrameworkCore;

using StreamerBotLib.Enums;
using StreamerBotLib.Models;

using System.Globalization;
using System.Windows.Data;

namespace StreamerBotLib.DataSQL.Models
{
    [PrimaryKey(nameof(CmdName))]
    [Index(nameof(CmdName), IsUnique = true)]
    public class Commands(string cmdName = null,
                          bool addMe = false,
                          ViewerTypes permission = default,
                          bool isEnabled = false,
                          string message = null,
                          int repeatTimer = 0,
                          short sendMsgCount = 0,
                          ICollection<string> category = null,
                          bool allowParam = false,
                          string usage = null,
                          bool lookupData = false,
                          string table = null,
                          string keyField = null,
                          string dataField = null,
                          string currencyField = null,
                          string unit = null,
                          CommandAction action = default,
                          int top = 0,
                          CommandSort sort = default) : EntityBase
    {
        public string CmdName { get; set; } = cmdName;
        public bool AddMe { get; set; } = addMe;
        public ViewerTypes Permission { get; set; } = permission;
        public bool IsEnabled { get; set; } = isEnabled;
        public string Message { get; set; } = message;
        public int RepeatTimer { get; set; } = repeatTimer;
        public short SendMsgCount { get; set; } = sendMsgCount;
        public ICollection<string> Category { get; set; } = category;
        public bool AllowParam { get; set; } = allowParam;
        public string Usage { get; set; } = usage;
        public bool LookupData { get; set; } = lookupData;
        public string Table { get; set; } = table;
        public string KeyField { get; set; } = keyField;
        public string DataField { get; set; } = dataField;
        public string CurrencyField { get; set; } = currencyField;
        public string Unit { get; set; } = unit;
        public CommandAction Action { get; set; } = action;
        public int Top { get; set; } = top;
        public CommandSort Sort { get; set; } = sort;


        public ICollection<CategoryList> CategoryList { get; } = new List<CategoryList>();

        public static Commands GetCommands(CommandData commandData)
        {
            return new(
                cmdName: commandData.CmdName,
                addMe: commandData.AddMe,
                permission: commandData.Permission,
                isEnabled: commandData.IsEnabled,
                message: commandData.Message,
                sendMsgCount: commandData.SendMsgCount,
                allowParam: commandData.AllowParam,
                usage: commandData.Usage,
                lookupData: commandData.Lookupdata,
                table: commandData.Table,
                keyField: commandData.Key_field,
                dataField: commandData.Data_field,
                currencyField: commandData.Currency_field,
                action: commandData.Action,
                top: commandData.Top,
                sort: commandData.Sort
                );
        }
    }

}
