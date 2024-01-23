using Microsoft.EntityFrameworkCore;

using StreamerBotLib.Enums;

using System.ComponentModel.DataAnnotations.Schema;

namespace StreamerBotLib.DataSQL.Models
{
    [Index(nameof(CmdName), IsUnique = true)]
    public class Commands(uint id = 0,
                          string cmdName = null,
                          bool addMe = false,
                          ViewerTypes permission = default,
                          bool isEnabled = false,
                          string message = null,
                          uint repeatTimer = 0,
                          ushort sendMsgCount = 0,
                          ICollection<CategoryList> category = null,
                          bool allowParam = false,
                          string usage = null,
                          bool lookupData = false,
                          string table = null,
                          string keyField = null,
                          string dataField = null,
                          string currencyField = null,
                          string unit = null,
                          CommandAction action = default,
                          uint top = 0)
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public uint Id { get; set; } = id;
        public string CmdName { get; set; } = cmdName;
        public bool AddMe { get; set; } = addMe;
        public ViewerTypes Permission { get; set; } = permission;
        public bool IsEnabled { get; set; } = isEnabled;
        public string Message { get; set; } = message;
        public uint RepeatTimer { get; set; } = repeatTimer;
        public ushort SendMsgCount { get; set; } = sendMsgCount;
        public ICollection<CategoryList> Category { get; set; } = category;
        public bool AllowParam { get; set; } = allowParam;
        public string Usage { get; set; } = usage;
        public bool LookupData { get; set; } = lookupData;
        public string Table { get; set; } = table;
        public string KeyField { get; set; } = keyField;
        public string DataField { get; set; } = dataField;
        public string CurrencyField { get; set; } = currencyField;
        public string Unit { get; set; } = unit;
        public CommandAction Action { get; set; } = action;
        public uint Top { get; set; } = top;
        public DataSort Sort { get; set; }
    }
}
