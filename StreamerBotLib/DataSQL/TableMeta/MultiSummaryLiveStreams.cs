
namespace StreamerBotLib.DataSQL.TableMeta
{
    using StreamerBotLib.Models.Enums;
    using StreamerBotLib.Models.Interfaces;
    internal class MultiSummaryLiveStreams : IDatabaseTableMeta
    {
        public System.Int32 StreamCount { get => (System.Int32)Values["StreamCount"]; set => Values["StreamCount"] = value; }
        public System.DateTime ThroughDate { get => (System.DateTime)Values["ThroughDate"]; set => Values["ThroughDate"] = value; }
        public System.String UserId { get => (System.String)Values["UserId"]; set => Values["UserId"] = value; }
        public Platform Platform { get => (Platform)Values["Platform"]; set => Values["Platform"] = value; }

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
              { "Platform", typeof(Platform) }
        };
        public object GetModelEntity()
        {
            return new Models.MultiSummaryLiveStreams(
            streamCount: Convert.ToInt32(StreamCount),
            throughDate: ThroughDate,
            userId: UserId,
            platform: Platform
        );
        }
        public void CopyUpdates(Models.MultiSummaryLiveStreams modelData)
        {
            if (modelData.StreamCount != StreamCount)
            {
                modelData.StreamCount = StreamCount;
            }

            if (modelData.ThroughDate != ThroughDate)
            {
                modelData.ThroughDate = ThroughDate;
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

