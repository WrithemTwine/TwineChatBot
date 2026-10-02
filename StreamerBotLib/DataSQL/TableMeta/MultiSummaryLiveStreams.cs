using StreamerBotLib.Models.Interfaces;

namespace StreamerBotLib.DataSQL.TableMeta
{
    internal class MultiSummaryLiveStreams : IDatabaseTableMeta
    {
        public System.Int32 StreamCount { get => Convert.ToInt32(Values["StreamCount"]); set => Values["StreamCount"] = value; }
        public System.DateTime ThroughDate { get => Convert.ToDateTime(Values["ThroughDate"]); set => Values["ThroughDate"] = value; }
        public System.String UserId { get => (string)Values["UserId"]; set => Values["UserId"] = value; }
        public StreamerBotLib.Models.Enums.Platform Platform { get => (StreamerBotLib.Models.Enums.Platform)Enum.Parse(typeof(StreamerBotLib.Models.Enums.Platform), Values["Platform"]?.ToString() ?? "0"); set => Values["Platform"] = value; }

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
              { "Platform", typeof(StreamerBotLib.Models.Enums.Platform) }
        };

        public object GetModelEntity()
        {
            return new Models.MultiSummaryLiveStreams(
            streamCount: StreamCount,
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