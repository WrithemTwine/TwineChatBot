using StreamerBotLib.Models.Interfaces;

namespace StreamerBotLib.DataSQL.TableMeta
{
    internal class CustomWelcome : IDatabaseTableMeta
    {
        public System.String Message { get => (string)Values["Message"]; set => Values["Message"] = value; }
        public System.String UserId { get => (string)Values["UserId"]; set => Values["UserId"] = value; }
        public StreamerBotLib.Models.Enums.Platform Platform { get => (StreamerBotLib.Models.Enums.Platform)Enum.Parse(typeof(StreamerBotLib.Models.Enums.Platform), Values["Platform"]?.ToString() ?? "0"); set => Values["Platform"] = value; }

        public Dictionary<string, object> Values { get; }

        public string TableName { get; } = "CustomWelcome";

        public CustomWelcome(Models.CustomWelcome tableData)
        {
            Values = new()
            {
                 { "Message", tableData.Message },
                 { "UserId", tableData.UserId },
                 { "Platform", tableData.Platform }
            };
        }

        public Dictionary<string, Type> Meta => new()
        {
              { "Message", typeof(System.String) },
              { "UserId", typeof(System.String) },
              { "Platform", typeof(StreamerBotLib.Models.Enums.Platform) }
        };

        public object GetModelEntity()
        {
            return new Models.CustomWelcome(
            message: Message,
            userId: UserId,
            platform: Platform
            );
        }

        public void CopyUpdates(Models.CustomWelcome modelData)
        {
            if (modelData.Message != Message)
            {
                modelData.Message = Message;
            }
            if (modelData.UserId != UserId)
            {
                modelData.UserId = UserId;
            }
            if (modelData.Platform != Platform)
            {
                modelData.Platform = Platform;
            }
        }
    }
}