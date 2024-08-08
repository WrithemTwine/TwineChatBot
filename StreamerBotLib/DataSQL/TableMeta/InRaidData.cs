using StreamerBotLib.Interfaces;

namespace StreamerBotLib.DataSQL.TableMeta
{
    internal class InRaidData : IDatabaseTableMeta
    {
        public System.Int32 ViewerCount => (System.Int32)Values["ViewerCount"];
        public System.DateTime RaidDate => (System.DateTime)Values["RaidDate"];
        public System.String Category => (System.String)Values["Category"];
        public System.String UserId => (System.String)Values["UserId"];
        public System.String UserName => (System.String)Values["UserName"];
        public StreamerBotLib.Enums.Platform Platform => (StreamerBotLib.Enums.Platform)Values["Platform"];

        public Dictionary<string, object> Values { get; }

        public string TableName { get; } = "InRaidData";

        public InRaidData(Models.InRaidData tableData)
        {
            Values = new()
            {
                 { "ViewerCount", tableData.ViewerCount },
                 { "RaidDate", tableData.RaidDate },
                 { "Category", tableData.Category },
                 { "UserId", tableData.UserId },
                 { "UserName", tableData.UserName },
                 { "Platform", tableData.Platform }
            };
        }
        public Dictionary<string, Type> Meta => new()
        {
              { "ViewerCount", typeof(System.Int32) },
              { "RaidDate", typeof(System.DateTime) },
              { "Category", typeof(System.String) },
              { "UserId", typeof(System.String) },
              { "UserName", typeof(System.String) },
              { "Platform", typeof(StreamerBotLib.Enums.Platform) }
        };
        public object GetModelEntity()
        {
            return new Models.InRaidData(
                                          Convert.ToInt32(Values["ViewerCount"]),
                                          (System.DateTime)Values["RaidDate"],
                                          (System.String)Values["Category"],
                                          (System.String)Values["UserId"],
                                          (System.String)Values["UserName"],
                                          (StreamerBotLib.Enums.Platform)Values["Platform"]
);
        }
        public void CopyUpdates(Models.InRaidData modelData)
        {
            if (modelData.ViewerCount != ViewerCount)
            {
                modelData.ViewerCount = ViewerCount;
            }

            if (modelData.RaidDate != RaidDate)
            {
                modelData.RaidDate = RaidDate;
            }

            if (modelData.Category != Category)
            {
                modelData.Category = Category;
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

