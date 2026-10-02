using StreamerBotLib.Models.Interfaces;

namespace StreamerBotLib.DataSQL.TableMeta
{
    internal class CommandPlatformMessages : IDatabaseTableMeta
    {
        public System.String CmdName { get => (string)Values["CmdName"]; set => Values["CmdName"] = value; }
        public StreamerBotLib.Models.Enums.Platform Platform { get => (StreamerBotLib.Models.Enums.Platform)Enum.Parse(typeof(StreamerBotLib.Models.Enums.Platform), Values["Platform"]?.ToString() ?? "0"); set => Values["Platform"] = value; }
        public System.String Message { get => (string)Values["Message"]; set => Values["Message"] = value; }

        public Dictionary<string, object> Values { get; }

        public string TableName { get; } = "CommandPlatformMessages";

        public CommandPlatformMessages(Models.CommandPlatformMessages tableData)
        {
            Values = new()
            {
                 { "CmdName", tableData.CmdName },
                 { "Platform", tableData.Platform },
                 { "Message", tableData.Message }
            };
        }

        public Dictionary<string, Type> Meta => new()
        {
              { "CmdName", typeof(System.String) },
              { "Platform", typeof(StreamerBotLib.Models.Enums.Platform) },
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