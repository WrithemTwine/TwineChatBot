using StreamerBotLib.DataSQL.Models;
using StreamerBotLib.Enums;

namespace StreamerBotLib.Models
{
    public record CommandData
    {
        public string Usage { get; }
        public bool IsEnabled { get; }
        public short SendMsgCount { get; }
        public ViewerTypes Permission { get; }
        public bool AddMe { get; }
        public string Message { get; }
        public bool AllowParam { get; }
        public bool Lookupdata { get; }
        public int Top { get; }
        public CommandAction Action { get; }
        public string CmdName { get; }
        public string Table { get; }
        public string Key_field { get; }
        public string Data_field { get; }
        public string Currency_field { get; }
        public CommandSort Sort { get; }

        public CommandData(Commands row)
        {
            lock (GUI.GUIDataManagerLock.Lock)
            {
                if (row != null)
                {
                    Usage = row.Usage;
                    IsEnabled = row.IsEnabled;
                    SendMsgCount = row.SendMsgCount;
                    Permission = row.Permission;
                    AddMe = row.AddMe;
                    Message = row.Message;
                    AllowParam = row.AllowParam;
                    Lookupdata = row.LookupData;
                    Top = row.Top;
                    Action = row.Action;
                    CmdName = row.CmdName;
                    Table = row.Table;
                    Key_field = row.KeyField;
                    Data_field = row.DataField;
                    Currency_field = row.CurrencyField;
                    Sort = row.Sort;
                }
            }
        }
    }
}
