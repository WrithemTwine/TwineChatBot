using StreamerBotLib.Interfaces;

namespace StreamerBotLib.DataSQL.TableMeta
{
    internal class ShoutOuts : IDatabaseTableMeta
    {
        public System.String UserId => (System.String)Values["UserId"];
        public System.String UserName => (System.String)Values["UserName"];
        public StreamerBotLib.Enums.Platform Platform => (StreamerBotLib.Enums.Platform)Values["Platform"];

        public Dictionary<string, object> Values { get; }

        public string TableName { get; } = "ShoutOuts";

        public ShoutOuts(Models.ShoutOuts tableData)
        {
            Values = new()
            {
                 { "UserId", tableData.UserId },
                 { "UserName", tableData.UserName },
                 { "Platform", tableData.Platform }
            };
        }
        public Dictionary<string, Type> Meta => new()
        {
              { "UserId", typeof(System.String) },
              { "UserName", typeof(System.String) },
              { "Platform", typeof(StreamerBotLib.Enums.Platform) }
        };
        public object GetModelEntity()
        {
            return new Models.ShoutOuts(
                                          (System.String)Values["UserId"],
                                          (System.String)Values["UserName"],
                                          (StreamerBotLib.Enums.Platform)Values["Platform"]
);
        }
        public void CopyUpdates(Models.ShoutOuts modelData)
        {
            if (modelData.UserId != UserId)
            {
                modelData.UserId = UserId;
            }

            if (modelData.UserName != UserName)
            {
                modelData.UserName = UserName;
            }

            if (modelData.Platform != Platform)
            {
                modelData.Platform = Platform;
            }

        }
    }
}

