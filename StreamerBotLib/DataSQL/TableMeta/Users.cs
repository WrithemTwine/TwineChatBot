using StreamerBotLib.Enums;
using StreamerBotLib.DataSQL.Models;
using StreamerBotLib.Interfaces;
using StreamerBotLib.Overlay.Enums;

namespace StreamerBotLib.DataSQL.TableMeta
{
    internal class Users : IDatabaseTableMeta
    {
        public System.DateTime FirstDateSeen => (System.DateTime)Values["FirstDateSeen"];
        public System.DateTime CurrLoginDate => (System.DateTime)Values["CurrLoginDate"];
        public System.DateTime LastDateSeen => (System.DateTime)Values["LastDateSeen"];
        public System.String UserId => (System.String)Values["UserId"];
        public System.String UserName => (System.String)Values["UserName"];
        public StreamerBotLib.Enums.Platform Platform => (StreamerBotLib.Enums.Platform)Values["Platform"];

        public Dictionary<string, object> Values { get; }

        public string TableName { get; } = "Users";

        public Users(Models.Users tableData)
        {
            Values = new()
            {
                 { "FirstDateSeen", tableData.FirstDateSeen },
                 { "CurrLoginDate", tableData.CurrLoginDate },
                 { "LastDateSeen", tableData.LastDateSeen },
                 { "UserId", tableData.UserId },
                 { "UserName", tableData.UserName },
                 { "Platform", tableData.Platform }
            };
        }
        public Dictionary<string, Type> Meta => new()
        {
              { "FirstDateSeen", typeof(System.DateTime) },
              { "CurrLoginDate", typeof(System.DateTime) },
              { "LastDateSeen", typeof(System.DateTime) },
              { "UserId", typeof(System.String) },
              { "UserName", typeof(System.String) },
              { "Platform", typeof(StreamerBotLib.Enums.Platform) }
        };
        public object GetModelEntity()
        {
            return new Models.Users(

);
        }
        public void CopyUpdates(Models.Users modelData)
        {

        }
    }
}

