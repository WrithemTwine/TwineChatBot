using StreamerBotLib.Models.Enums;
using StreamerBotLib.Models.Interfaces;

namespace StreamerBotLib.DataSQL.TableMeta
{
    public class CommandPlatformMessages : IDatabaseTableMeta
    {
        public System.String CmdName { get => (System.String)Values["CmdName"]; set => Values["CmdName"] = value; }
        public Platform Platform { get => (Platform)Values["Platform"]; set => Values["Platform"] = value; }
        public System.String Message { get => (System.String)Values["Message"]; set => Values["Message"] = value; }

        public string TableName => "CommandPlatformMessages";

        public Dictionary<string, object> Values { get; }

        public CommandPlatformMessages(Models.CommandPlatformMessages tableData)
        {
            Values = new Dictionary<string, object>
            {
                { "CmdName", tableData.CmdName },
                { "Platform", tableData.Platform },
                { "Message", tableData.Message }
            };
        }

        public Dictionary<string, Type> Meta => new()
        {
            { "CmdName", typeof(System.String) },
            { "Platform", typeof(Platform) },
            { "Message", typeof(System.String) }
        };

        public object GetModelEntity()
        {
            return new Models.CommandPlatformMessages(
                cmdName: CmdName,
                platform: Platform,
                message: Message
            );
        }

        public void CopyUpdates(Models.CommandPlatformMessages modelData)
        {
            if (modelData.CmdName != CmdName)
            {
                modelData.CmdName = CmdName;
            }

            if (modelData.Platform != Platform)
            {
                modelData.Platform = Platform;
            }

            if (modelData.Message != Message)
            {
                modelData.Message = Message;
            }
        }
    }
}
