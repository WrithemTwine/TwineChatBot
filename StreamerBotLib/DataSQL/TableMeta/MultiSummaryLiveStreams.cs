using StreamerBotLib.Enums;
using StreamerBotLib.DataSQL.Models;
using StreamerBotLib.Interfaces;
using StreamerBotLib.Overlay.Enums;

namespace StreamerBotLib.DataSQL.TableMeta
{
    internal class MultiSummaryLiveStreams : IDatabaseTableMeta
    {
        public System.Int32 StreamCount => (System.Int32)Values["StreamCount"];
        public System.DateTime ThroughDate => (System.DateTime)Values["ThroughDate"];
        public System.String UserId => (System.String)Values["UserId"];
        public StreamerBotLib.Enums.Platform Platform => (StreamerBotLib.Enums.Platform)Values["Platform"];

        public Dictionary<string, object> Values { get; }

        public string TableName { get; } = "MultiSummaryLiveStreams";

        public MultiSummaryLiveStreams(Models.MultiSummaryLiveStreams tableData)
        {
            Values = new()
            {
                 { "StreamCount", tableData.StreamCount },
                 { "ThroughDate", tableData.ThroughDate },
                 { "UserId", tableData.UserId },
                 { "Platform", tableData.Platform }
            };
        }
        public Dictionary<string, Type> Meta => new()
        {
              { "StreamCount", typeof(System.Int32) },
              { "ThroughDate", typeof(System.DateTime) },
              { "UserId", typeof(System.String) },
              { "Platform", typeof(StreamerBotLib.Enums.Platform) }
        };
        public object GetModelEntity()
        {
            return new Models.MultiSummaryLiveStreams(

);
        }
        public void CopyUpdates(Models.MultiSummaryLiveStreams modelData)
        {

        }
    }
}

