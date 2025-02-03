using StreamerBotLib.Enums;
using StreamerBotLib.DataSQL.Models;
using StreamerBotLib.Interfaces;
using StreamerBotLib.Overlay.Enums;

namespace StreamerBotLib.DataSQL.TableMeta
{
    internal class MultiLiveStreams : IDatabaseTableMeta
    {
        public System.DateTime LiveDate { get => (System.DateTime)Values["LiveDate"]; set => Values["LiveDate"] = value; }
        public System.String UserId { get => (System.String)Values["UserId"]; set => Values["UserId"] = value; }
        public StreamerBotLib.Enums.Platform Platform { get => (StreamerBotLib.Enums.Platform)Values["Platform"]; set => Values["Platform"] = value; }

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

