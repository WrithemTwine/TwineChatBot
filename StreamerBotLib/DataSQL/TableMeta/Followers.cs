using StreamerBotLib.Enums;
using StreamerBotLib.DataSQL.Models;
using StreamerBotLib.Interfaces;
using StreamerBotLib.Overlay.Enums;

namespace StreamerBotLib.DataSQL.TableMeta
{
    internal class Followers : IDatabaseTableMeta
    {
        public System.String UserName => (System.String)Values["UserName"];
        public System.Boolean IsFollower => (System.Boolean)Values["IsFollower"];
        public System.DateTime FollowedDate => (System.DateTime)Values["FollowedDate"];
        public System.DateTime StatusChangeDate => (System.DateTime)Values["StatusChangeDate"];
        public System.String Category => (System.String)Values["Category"];
        public System.DateTime AddDate => (System.DateTime)Values["AddDate"];
        public System.String UserId => (System.String)Values["UserId"];
        public StreamerBotLib.Enums.Platform Platform => (StreamerBotLib.Enums.Platform)Values["Platform"];

        public Dictionary<string, object> Values { get; }

        public string TableName { get; } = "Followers";

        public Followers(Models.Followers tableData)
        {
            Values = new()
            {
                 { "UserName", tableData.UserName },
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
              { "UserName", typeof(System.String) },
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

);
        }
        public void CopyUpdates(Models.Followers modelData)
        {

        }
    }
}

