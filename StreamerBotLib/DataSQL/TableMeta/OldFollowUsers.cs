using StreamerBotLib.Models.Interfaces;

namespace StreamerBotLib.DataSQL.TableMeta
{
    internal class OldFollowUsers : IDatabaseTableMeta
    {
        public System.String UserId { get => (string)Values["UserId"]; set => Values["UserId"] = value; }
        public StreamerBotLib.Models.Enums.Platform Platform { get => (StreamerBotLib.Models.Enums.Platform)Enum.Parse(typeof(StreamerBotLib.Models.Enums.Platform), Values["Platform"]?.ToString() ?? "0"); set => Values["Platform"] = value; }
        public System.String UserName { get => (string)Values["UserName"]; set => Values["UserName"] = value; }
        public System.Boolean IsFollower { get => Convert.ToBoolean(Values["IsFollower"]); set => Values["IsFollower"] = value; }
        public System.DateTime FollowedDate { get => Convert.ToDateTime(Values["FollowedDate"]); set => Values["FollowedDate"] = value; }
        public System.DateTime StatusChangeDate { get => Convert.ToDateTime(Values["StatusChangeDate"]); set => Values["StatusChangeDate"] = value; }
        public System.String Category { get => (string)Values["Category"]; set => Values["Category"] = value; }
        public System.DateTime AddDate { get => Convert.ToDateTime(Values["AddDate"]); set => Values["AddDate"] = value; }

        public Dictionary<string, object> Values { get; }

        public string TableName { get; } = "OldFollowUsers";

        public OldFollowUsers(Models.OldFollowUsers tableData)
        {
            Values = new()
            {
                 { "UserId", tableData.UserId },
                 { "Platform", tableData.Platform },
                 { "UserName", tableData.UserName },
                 { "IsFollower", tableData.IsFollower },
                 { "FollowedDate", tableData.FollowedDate },
                 { "StatusChangeDate", tableData.StatusChangeDate },
                 { "Category", tableData.Category },
                 { "AddDate", tableData.AddDate }
            };
        }

        public Dictionary<string, Type> Meta => new()
        {
              { "UserId", typeof(System.String) },
              { "Platform", typeof(StreamerBotLib.Models.Enums.Platform) },
              { "UserName", typeof(System.String) },
              { "IsFollower", typeof(System.Boolean) },
              { "FollowedDate", typeof(System.DateTime) },
              { "StatusChangeDate", typeof(System.DateTime) },
              { "Category", typeof(System.String) },
              { "AddDate", typeof(System.DateTime) }
        };

        public object GetModelEntity()
        {
            return new Models.OldFollowUsers(
            userId: UserId,
            platform: Platform,
            userName: UserName,
            isFollower: IsFollower,
            followedDate: FollowedDate,
            statusChangeDate: StatusChangeDate,
            category: Category,
            addDate: AddDate
            );
        }

        public void CopyUpdates(Models.OldFollowUsers modelData)
        {
            if (modelData.UserId != UserId)
            {
                modelData.UserId = UserId;
            }
            if (modelData.Platform != Platform)
            {
                modelData.Platform = Platform;
            }
            if (modelData.UserName != UserName)
            {
                modelData.UserName = UserName;
            }
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
        }
    }
}