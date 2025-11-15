using StreamerBotLib.Models.Enums;
using StreamerBotLib.Models.Interfaces;

namespace StreamerBotLib.DataSQL.TableMeta
{
    internal class ShoutOuts : IDatabaseTableMeta
    {
        public System.String UserId { get => (System.String)Values["UserId"]; set => Values["UserId"] = value; }
        public Platform Platform { get => (Platform)Values["Platform"]; set => Values["Platform"] = value; }

        public Dictionary<string, object> Values { get; }

        public string TableName { get; } = "ShoutOuts";

        public ShoutOuts(Models.ShoutOuts tableData)
        {
            Values = new()
            {
                 { "UserId", tableData.UserId },
                 { "Platform", tableData.Platform }
            };
        }
        public Dictionary<string, Type> Meta => new()
        {
              { "UserId", typeof(System.String) },
              { "Platform", typeof(Platform) }
        };
        public object GetModelEntity()
        {
            return new Models.ShoutOuts(
            userId: UserId,
            platform: Platform
        );
        }
        public void CopyUpdates(Models.ShoutOuts modelData)
        {
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

