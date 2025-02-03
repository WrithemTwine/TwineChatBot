using StreamerBotLib.Enums;
using StreamerBotLib.DataSQL.Models;
using StreamerBotLib.Interfaces;
using StreamerBotLib.Overlay.Enums;

namespace StreamerBotLib.DataSQL.TableMeta
{
    internal class GiveawayUserData : IDatabaseTableMeta
    {
        public System.DateTime DateTime { get => (System.DateTime)Values["DateTime"]; set => Values["DateTime"] = value; }
        public System.String UserId { get => (System.String)Values["UserId"]; set => Values["UserId"] = value; }
        public StreamerBotLib.Enums.Platform Platform { get => (StreamerBotLib.Enums.Platform)Values["Platform"]; set => Values["Platform"] = value; }

        public Dictionary<string, object> Values { get; }

        public string TableName { get; } = "GiveawayUserData";

        public GiveawayUserData(Models.GiveawayUserData tableData)
        {
            Values = new()
            {
                 { "DateTime", tableData.DateTime },
                 { "UserId", tableData.UserId },
                 { "Platform", tableData.Platform }
            };
        }
        public Dictionary<string, Type> Meta => new()
        {
              { "DateTime", typeof(System.DateTime) },
              { "UserId", typeof(System.String) },
              { "Platform", typeof(StreamerBotLib.Enums.Platform) }
        };
        public object GetModelEntity()
        {
            return new Models.GiveawayUserData(
            dateTime: DateTime, 
            userId: UserId, 
            platform: Platform
        );
        }
        public void CopyUpdates(Models.GiveawayUserData modelData)
        {
          if (modelData.DateTime != DateTime)
            {
                modelData.DateTime = DateTime;
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

