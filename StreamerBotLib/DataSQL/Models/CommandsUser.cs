using StreamerBotLib.Enums;

namespace StreamerBotLib.DataSQL.Models
{
    public class CommandsUser(string cmdName = null,
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
                          CommandSort sort = default) : Commands(cmdName,
                           addMe,
                           permission,
                           isEnabled,
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
                           sort)
    {
    }
}
