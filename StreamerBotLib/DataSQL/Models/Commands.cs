using StreamerBotLib.Enums;
using StreamerBotLib.Models;

namespace StreamerBotLib.DataSQL.Models
{
#if DEBUG_EFMODELS_NODEFAULTPARAM
    public class Commands(string cmdName,
                          bool addMe,
                          ViewerTypes permission,
                          bool isEnabled,
                          bool announce,
                          string message,
                          int repeatTimer,
                          short sendMsgCount,
                          ICollection<string> category,
                          bool allowParam,
                          string usage,
                          bool lookupData,
                          string table,
                          string keyField,
                          string dataField,
                          string currencyField,
                          string unit,
                          CommandAction action,
                          int top,
                          CommandSort sort)
#else
    public class Commands(string cmdName = null,
                          bool addMe = false,
                          ViewerTypes permission = ViewerTypes.Viewer,
                          bool isEnabled = false,
                          bool announce = false,
                          string message = "",
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
                          CommandSort sort = default,
                          int calls = 0)
#endif
    : CommandsBase(cmdName,
                           addMe,
                           permission,
                           isEnabled,
                           announce,
                           message,
                           repeatTimer,
                           sendMsgCount,
                           category,
                           allowParam,
                           usage,
                           lookupData,
                           table,
                           keyField,
                           dataField,
                           currencyField,
                           unit,
                           action,
                           top,
                           sort,
                           calls)
    {
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
