using StreamerBotLib.Enums;
using StreamerBotLib.DataSQL.Models;
using StreamerBotLib.Interfaces;
using StreamerBotLib.Overlay.Enums;

namespace StreamerBotLib.DataSQL.TableMeta
{
    internal class Followers : IDatabaseTableMeta
    {
        public System.Boolean IsFollower { get => (System.Boolean)Values["IsFollower"]; set => Values["IsFollower"] = value; }
        public System.DateTime FollowedDate { get => (System.DateTime)Values["FollowedDate"]; set => Values["FollowedDate"] = value; }
        public System.DateTime StatusChangeDate { get => (System.DateTime)Values["StatusChangeDate"]; set => Values["StatusChangeDate"] = value; }
        public System.String Category { get => (System.String)Values["Category"]; set => Values["Category"] = value; }
        public System.DateTime AddDate { get => (System.DateTime)Values["AddDate"]; set => Values["AddDate"] = value; }
        public System.String UserId { get => (System.String)Values["UserId"]; set => Values["UserId"] = value; }
        public StreamerBotLib.Enums.Platform Platform { get => (StreamerBotLib.Enums.Platform)Values["Platform"]; set => Values["Platform"] = value; }

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
              { "Platform", typeof(StreamerBotLib.Enums.Platform) }
        };
        public object GetModelEntity()
        {
            return new Models.Followers(
            isFollower: IsFollower, 
            followedDate: FollowedDate, 
            statusChangeDate: StatusChangeDate, 
            category: Category, 
            addDate: AddDate, 
            userId: UserId, 
            platform: Platform
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

          if (modelData.Platform != Platform)
            {
                modelData.Platform = Platform;
            }

        }
    }
}

