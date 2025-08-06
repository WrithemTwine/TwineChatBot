using StreamerBotLib.Models.Enums;
using StreamerBotLib.Models.Interfaces;

namespace StreamerBotLib.DataSQL.TableMeta
{
    internal class OldFollowUsers : IDatabaseTableMeta
    {
        public System.String UserId { get => (System.String)Values["UserId"]; set => Values["UserId"] = value; }
        public Platform Platform { get => (Platform)Values["Platform"]; set => Values["Platform"] = value; }
        public System.String UserName { get => (System.String)Values["UserName"]; set => Values["UserName"] = value; }
        public System.Boolean IsFollower { get => (System.Boolean)Values["IsFollower"]; set => Values["IsFollower"] = value; }
        public System.DateTime FollowedDate { get => (System.DateTime)Values["FollowedDate"]; set => Values["FollowedDate"] = value; }
        public System.DateTime StatusChangeDate { get => (System.DateTime)Values["StatusChangeDate"]; set => Values["StatusChangeDate"] = value; }
        public System.String Category { get => (System.String)Values["Category"]; set => Values["Category"] = value; }
        public System.DateTime AddDate { get => (System.DateTime)Values["AddDate"]; set => Values["AddDate"] = value; }

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
              { "Platform", typeof(Platform) },
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

