using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static StreamerBotLib.Data.DataSource;

namespace StreamerBotLib.Models
{
    public class CommandData
    {
        public string Usage { get; }
        public bool IsEnabled { get; }
        public short SendMsgCount { get; }
        public string Permission { get; }
        public bool AddMe { get; }
        public string Message { get; }
        public bool AllowParam { get; }
        public bool Lookupdata { get; }
        public int Top { get; }
        public string Action { get; }
        public string CmdName { get; }
        public string Table { get; }
        public string Key_field { get; }
        public string Data_field { get; }
        public string Sort { get; }

        public CommandData(CommandsRow row)
        {
            Usage = ColHelper(row.Usage);
            IsEnabled = ColHelper(row.IsEnabled);
            SendMsgCount = ColHelper(row.SendMsgCount);
            Permission = ColHelper(row.Permission);
            AddMe = ColHelper(row.AddMe);
            Message = ColHelper(row.Message);
            AllowParam = ColHelper(row.AllowParam);
            Lookupdata = ColHelper(row.lookupdata);
            Top = ColHelper(row.top);
            Action = ColHelper(row.action);
            CmdName = ColHelper(row.CmdName);
            Table = ColHelper(row.table);
            Key_field = ColHelper(row.key_field);
            Data_field = ColHelper(row.data_field);
            Sort = ColHelper(row.sort);
        }

        private static T ColHelper<T>(T Data)
        {
            object returndata = null;

            if (DBNull.Value.Equals(Data))
            {
                if (typeof(T) == typeof(string))
                {
                    returndata = "";
                }
                else if (typeof(T) == typeof(int) || typeof(T) == typeof(short))
                {
                    returndata = 0;
                }
                else if (typeof(T) == typeof(bool))
                {
                    returndata = false;
                }
            }
            else
            {
                returndata = Data;
            }

            return (T)Convert.ChangeType(returndata, typeof(T));
        }

    }
}
