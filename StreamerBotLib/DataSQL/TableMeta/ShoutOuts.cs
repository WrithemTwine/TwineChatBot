using StreamerBotLib.Interfaces;

namespace StreamerBotLib.DataSQL.TableMeta
{
    internal class ShoutOuts : IDatabaseTableMeta
    {
        public System.String UserId => (System.String)Values["UserId"];
        public StreamerBotLib.Enums.Platform Platform => (StreamerBotLib.Enums.Platform)Values["Platform"];

        public Dictionary<string, object> Values { get; }

        public string TableName { get; } = "ShoutOuts";

        public ShoutOuts(Models.ShoutOuts tableData)
        {
            Values = new()
            {
                 { "UserId", tableData.UserId },
                 { "Platform", tableData.Platform }
            };
        }
        public Dictionary<string, Type> Meta => new()
        {
              { "UserId", typeof(System.String) },
              { "Platform", typeof(StreamerBotLib.Enums.Platform) }
        };
        public object GetModelEntity()
        {
            return new Models.ShoutOuts(

);
        }
        public void CopyUpdates(Models.ShoutOuts modelData)
        {

        }
    }
}

