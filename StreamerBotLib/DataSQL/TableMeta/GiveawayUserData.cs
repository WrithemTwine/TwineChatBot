using StreamerBotLib.Enums;
using StreamerBotLib.DataSQL.Models;
using StreamerBotLib.Interfaces;
using StreamerBotLib.Overlay.Enums;

namespace StreamerBotLib.DataSQL.TableMeta
{
    internal class GiveawayUserData : IDatabaseTableMeta
    {
        public System.DateTime DateTime => (System.DateTime)Values["DateTime"];
        public System.String UserId => (System.String)Values["UserId"];
        public System.String UserName => (System.String)Values["UserName"];
        public StreamerBotLib.Enums.Platform Platform => (StreamerBotLib.Enums.Platform)Values["Platform"];

        public Dictionary<string, object> Values { get; }

        public string TableName { get; } = "GiveawayUserData";

        public GiveawayUserData(Models.GiveawayUserData tableData)
        {
            Values = new()
            {
                 { "DateTime", tableData.DateTime },
                 { "UserId", tableData.UserId },
                 { "UserName", tableData.UserName },
                 { "Platform", tableData.Platform }
            };
        }
        public Dictionary<string, Type> Meta => new()
        {
              { "DateTime", typeof(System.DateTime) },
              { "UserId", typeof(System.String) },
              { "UserName", typeof(System.String) },
              { "Platform", typeof(StreamerBotLib.Enums.Platform) }
        };
        public object GetModelEntity()
        {
            return new Models.GiveawayUserData(
                                          (System.DateTime)Values["DateTime"], 
                                          (System.String)Values["UserId"], 
                                          (System.String)Values["UserName"], 
                                          (StreamerBotLib.Enums.Platform)Values["Platform"]
);
        }
        public void CopyUpdates(Models.GiveawayUserData modelData)
        {
          if (modelData.DateTime != DateTime)
            {
                modelData.DateTime = DateTime;
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

