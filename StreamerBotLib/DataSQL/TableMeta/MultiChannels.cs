using StreamerBotLib.Models.Interfaces;

namespace StreamerBotLib.DataSQL.TableMeta
{
    internal class MultiChannels : IDatabaseTableMeta
    {
        public System.String UserName { get => (string)Values["UserName"]; set => Values["UserName"] = value; }
        public System.String UserId { get => (string)Values["UserId"]; set => Values["UserId"] = value; }
        public StreamerBotLib.Models.Enums.Platform Platform { get => (StreamerBotLib.Models.Enums.Platform)Enum.Parse(typeof(StreamerBotLib.Models.Enums.Platform), Values["Platform"]?.ToString() ?? "0"); set => Values["Platform"] = value; }

        public Dictionary<string, object> Values { get; }

        public string TableName { get; } = "MultiChannels";

        public MultiChannels(Models.MultiChannels tableData)
        {
            Values = new()
            {
                 { "UserName", tableData.UserName },
                 { "UserId", tableData.UserId },
                 { "Platform", tableData.Platform }
            };
        }

        public Dictionary<string, Type> Meta => new()
        {
              { "UserName", typeof(System.String) },
              { "UserId", typeof(System.String) },
              { "Platform", typeof(StreamerBotLib.Models.Enums.Platform) }
        };

        public object GetModelEntity()
        {
            return new Models.MultiChannels(
            userName: UserName,
            userId: UserId,
            platform: Platform
            );
        }

        public void CopyUpdates(Models.MultiChannels modelData)
        {
            if (modelData.UserName != UserName)
            {
                modelData.UserName = UserName;
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