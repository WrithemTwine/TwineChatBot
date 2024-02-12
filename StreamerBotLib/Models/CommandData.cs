using static StreamerBotLib.Data.DataSource;

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

        public CommandData(CommandsRow row)
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
                    Lookupdata = row.lookupdata;
                    Top = row.top;
                    Action = row.action;
                    CmdName = row.CmdName;
                    Table = row.table;
                    Key_field = row.key_field;
                    Data_field = row.data_field;
                    Currency_field = row.currency_field;
                    Sort = row.sort;
                }
            }
        }
    }
}
