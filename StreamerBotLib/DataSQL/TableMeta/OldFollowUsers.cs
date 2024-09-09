using StreamerBotLib.Enums;
using StreamerBotLib.DataSQL.Models;
using StreamerBotLib.Interfaces;
using StreamerBotLib.Overlay.Enums;

namespace StreamerBotLib.DataSQL.TableMeta
{
    internal class OldFollowUsers : IDatabaseTableMeta
    {
        public System.String UserId => (System.String)Values["UserId"];
        public StreamerBotLib.Enums.Platform Platform => (StreamerBotLib.Enums.Platform)Values["Platform"];
        public System.String UserName => (System.String)Values["UserName"];
        public System.Boolean IsFollower => (System.Boolean)Values["IsFollower"];
        public System.DateTime FollowedDate => (System.DateTime)Values["FollowedDate"];
        public System.DateTime StatusChangeDate => (System.DateTime)Values["StatusChangeDate"];
        public System.String Category => (System.String)Values["Category"];
        public System.DateTime AddDate => (System.DateTime)Values["AddDate"];

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
              { "Platform", typeof(StreamerBotLib.Enums.Platform) },
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

);
        }
        public void CopyUpdates(Models.OldFollowUsers modelData)
        {

        }
    }
}

