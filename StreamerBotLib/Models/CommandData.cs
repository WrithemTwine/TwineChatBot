using StreamerBotLib.Data;

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
            // TODO: check and account for null rows, handle checks
            if (row != null)
            {
                Usage = ColHelper<string>(row.Usage);
                IsEnabled = ColHelper<bool>(row.IsEnabled);
                SendMsgCount = ColHelper<short>(row.SendMsgCount);
                Permission = ColHelper<string>(row.Permission);
                AddMe = ColHelper<bool>(row.AddMe);
                Message = ColHelper<string>(row.Message);
                AllowParam = ColHelper<bool>(row.AllowParam);
                Lookupdata = ColHelper<bool>(row.lookupdata);
                Top = ColHelper<int>(row.top);
                Action = ColHelper<string>(row.action);
                CmdName = ColHelper<string>(row.CmdName);
                Table = ColHelper<string>(row.table);
                Key_field = ColHelper<string>(row.key_field);
                Data_field = ColHelper<string>(row.data_field);
                Sort = ColHelper<string>(row.sort);
            }
        }

        private static T ColHelper<T>(object Data)
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
