using StreamerBotLib.Interfaces;

namespace StreamerBotLib.DataSQL.TableMeta
{
    internal class CustomWelcome : IDatabaseTableMeta
    {
        public System.String Message => (System.String)Values["Message"];
        public System.String UserId => (System.String)Values["UserId"];
        public System.String UserName => (System.String)Values["UserName"];
        public StreamerBotLib.Enums.Platform Platform => (StreamerBotLib.Enums.Platform)Values["Platform"];

        public Dictionary<string, object> Values { get; }

        public string TableName { get; } = "CustomWelcome";

        public CustomWelcome(Models.CustomWelcome tableData)
        {
            Values = new()
            {
                 { "Message", tableData.Message },
                 { "UserId", tableData.UserId },
                 { "UserName", tableData.UserName },
                 { "Platform", tableData.Platform }
            };
        }
        public Dictionary<string, Type> Meta => new()
        {
              { "Message", typeof(System.String) },
              { "UserId", typeof(System.String) },
              { "UserName", typeof(System.String) },
              { "Platform", typeof(StreamerBotLib.Enums.Platform) }
        };
        public object GetModelEntity()
        {
            return new Models.CustomWelcome(
                                          (System.String)Values["Message"],
                                          (System.String)Values["UserId"],
                                          (System.String)Values["UserName"],
                                          (StreamerBotLib.Enums.Platform)Values["Platform"]
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

