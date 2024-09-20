using StreamerBotLib.Interfaces;

namespace StreamerBotLib.DataSQL.TableMeta
{
    internal class MultiLiveStreams : IDatabaseTableMeta
    {
        public System.DateTime LiveDate => (System.DateTime)Values["LiveDate"];
        public System.String UserId => (System.String)Values["UserId"];
        public StreamerBotLib.Enums.Platform Platform => (StreamerBotLib.Enums.Platform)Values["Platform"];

        public Dictionary<string, object> Values { get; }

        public string TableName { get; } = "MultiLiveStreams";

        public MultiLiveStreams(Models.MultiLiveStreams tableData)
        {
            Values = new()
            {
                 { "LiveDate", tableData.LiveDate },
                 { "UserId", tableData.UserId },
                 { "Platform", tableData.Platform }
            };
        }
        public Dictionary<string, Type> Meta => new()
        {
              { "LiveDate", typeof(System.DateTime) },
              { "UserId", typeof(System.String) },
              { "Platform", typeof(StreamerBotLib.Enums.Platform) }
        };
        public object GetModelEntity()
        {
            return new Models.MultiLiveStreams(

);
        }
        public void CopyUpdates(Models.MultiLiveStreams modelData)
        {

        }
    }
}

