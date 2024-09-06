using StreamerBotLib.Enums;

namespace StreamerBotLib.DataSQL.Models
{
#if DEBUG_EFMODELS_NODEFAULTPARAM
    public class CommandsUser(string cmdName,
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
    public class CommandsUser(string cmdName = null,
                          bool addMe = false,
                          ViewerTypes permission = ViewerTypes.Viewer,
                          bool isEnabled = false,
                          bool announce = false,
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
                          CommandSort sort = default)
#endif
 : Commands(cmdName,
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
                           sort)
    {
 
    }
}
