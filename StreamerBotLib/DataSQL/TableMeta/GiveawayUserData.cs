using StreamerBotLib.Interfaces;

namespace StreamerBotLib.DataSQL.TableMeta
{
    internal class GiveawayUserData : IDatabaseTableMeta
    {
        public System.DateTime DateTime => (System.DateTime)Values["DateTime"];
        public System.String UserId => (System.String)Values["UserId"];
        public StreamerBotLib.Enums.Platform Platform => (StreamerBotLib.Enums.Platform)Values["Platform"];

        public Dictionary<string, object> Values { get; }

        public string TableName { get; } = "GiveawayUserData";

        public GiveawayUserData(Models.GiveawayUserData tableData)
        {
            Values = new()
            {
                 { "DateTime", tableData.DateTime },
                 { "UserId", tableData.UserId },
                 { "Platform", tableData.Platform }
            };
        }
        public Dictionary<string, Type> Meta => new()
        {
              { "DateTime", typeof(System.DateTime) },
              { "UserId", typeof(System.String) },
              { "Platform", typeof(StreamerBotLib.Enums.Platform) }
        };
        public object GetModelEntity()
        {
            return new Models.GiveawayUserData(

);
        }
        public void CopyUpdates(Models.GiveawayUserData modelData)
        {

        }
    }
}

