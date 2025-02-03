using StreamerBotLib.Enums;
using StreamerBotLib.DataSQL.Models;
using StreamerBotLib.Interfaces;
using StreamerBotLib.Overlay.Enums;

namespace StreamerBotLib.DataSQL.TableMeta
{
    internal class CustomWelcome : IDatabaseTableMeta
    {
        public System.String Message { get => (System.String)Values["Message"]; set => Values["Message"] = value; }
        public System.String UserId { get => (System.String)Values["UserId"]; set => Values["UserId"] = value; }
        public StreamerBotLib.Enums.Platform Platform { get => (StreamerBotLib.Enums.Platform)Values["Platform"]; set => Values["Platform"] = value; }

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
              { "Platform", typeof(StreamerBotLib.Enums.Platform) }
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

