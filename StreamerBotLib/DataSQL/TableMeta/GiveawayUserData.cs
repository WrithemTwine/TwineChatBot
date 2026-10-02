using StreamerBotLib.Models.Interfaces;

namespace StreamerBotLib.DataSQL.TableMeta
{
    internal class GiveawayUserData : IDatabaseTableMeta
    {
        public System.DateTime DateTime { get => Convert.ToDateTime(Values["DateTime"]); set => Values["DateTime"] = value; }
        public System.String UserId { get => (string)Values["UserId"]; set => Values["UserId"] = value; }
        public StreamerBotLib.Models.Enums.Platform Platform { get => (StreamerBotLib.Models.Enums.Platform)Enum.Parse(typeof(StreamerBotLib.Models.Enums.Platform), Values["Platform"]?.ToString() ?? "0"); set => Values["Platform"] = value; }

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
              { "Platform", typeof(StreamerBotLib.Models.Enums.Platform) }
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