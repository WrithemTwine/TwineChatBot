using StreamerBotLib.Models.Interfaces;

namespace StreamerBotLib.DataSQL.TableMeta
{
    internal class MultiLiveStreams : IDatabaseTableMeta
    {
        public System.DateTime LiveDate { get => Convert.ToDateTime(Values["LiveDate"]); set => Values["LiveDate"] = value; }
        public System.String UserId { get => (string)Values["UserId"]; set => Values["UserId"] = value; }
        public StreamerBotLib.Models.Enums.Platform Platform { get => (StreamerBotLib.Models.Enums.Platform)Enum.Parse(typeof(StreamerBotLib.Models.Enums.Platform), Values["Platform"]?.ToString() ?? "0"); set => Values["Platform"] = value; }

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
              { "Platform", typeof(StreamerBotLib.Models.Enums.Platform) }
        };

        public object GetModelEntity()
        {
            return new Models.MultiLiveStreams(
            liveDate: LiveDate,
            userId: UserId,
            platform: Platform
            );
        }

        public void CopyUpdates(Models.MultiLiveStreams modelData)
        {
            if (modelData.LiveDate != LiveDate)
            {
                modelData.LiveDate = LiveDate;
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