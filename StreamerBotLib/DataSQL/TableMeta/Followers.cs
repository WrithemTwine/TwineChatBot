using StreamerBotLib.Interfaces;

namespace StreamerBotLib.DataSQL.TableMeta
{
    internal class Followers : IDatabaseTableMeta
    {
        public System.Boolean IsFollower => (System.Boolean)Values["IsFollower"];
        public System.DateTime FollowedDate => (System.DateTime)Values["FollowedDate"];
        public System.DateTime StatusChangeDate => (System.DateTime)Values["StatusChangeDate"];
        public System.String Category => (System.String)Values["Category"];
        public System.DateTime AddDate => (System.DateTime)Values["AddDate"];
        public System.String UserId => (System.String)Values["UserId"];
        public System.String UserName => (System.String)Values["UserName"];
        public StreamerBotLib.Enums.Platform Platform => (StreamerBotLib.Enums.Platform)Values["Platform"];

        public Dictionary<string, object> Values { get; }

        public string TableName { get; } = "Followers";

        public Followers(Models.Followers tableData)
        {
            Values = new()
            {
                 { "IsFollower", tableData.IsFollower },
                 { "FollowedDate", tableData.FollowedDate },
                 { "StatusChangeDate", tableData.StatusChangeDate },
                 { "Category", tableData.Category },
                 { "AddDate", tableData.AddDate },
                 { "UserId", tableData.UserId },
                 { "UserName", tableData.UserName },
                 { "Platform", tableData.Platform }
            };
        }
        public Dictionary<string, Type> Meta => new()
        {
              { "IsFollower", typeof(System.Boolean) },
              { "FollowedDate", typeof(System.DateTime) },
              { "StatusChangeDate", typeof(System.DateTime) },
              { "Category", typeof(System.String) },
              { "AddDate", typeof(System.DateTime) },
              { "UserId", typeof(System.String) },
              { "UserName", typeof(System.String) },
              { "Platform", typeof(StreamerBotLib.Enums.Platform) }
        };
        public object GetModelEntity()
        {
            return new Models.Followers(
                                          (System.Boolean)Values["IsFollower"],
                                          (System.DateTime)Values["FollowedDate"],
                                          (System.DateTime)Values["StatusChangeDate"],
                                          (System.String)Values["Category"],
                                          (System.DateTime)Values["AddDate"],
                                          (System.String)Values["UserId"],
                                          (System.String)Values["UserName"],
                                          (StreamerBotLib.Enums.Platform)Values["Platform"]
);
        }
        public void CopyUpdates(Models.Followers modelData)
        {
            if (modelData.IsFollower != IsFollower)
            {
                modelData.IsFollower = IsFollower;
            }

            if (modelData.FollowedDate != FollowedDate)
            {
                modelData.FollowedDate = FollowedDate;
            }

            if (modelData.StatusChangeDate != StatusChangeDate)
            {
                modelData.StatusChangeDate = StatusChangeDate;
            }

            if (modelData.Category != Category)
            {
                modelData.Category = Category;
            }

            if (modelData.AddDate != AddDate)
            {
                modelData.AddDate = AddDate;
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

