using StreamerBotLib.Enums;
using StreamerBotLib.DataSQL.Models;
using StreamerBotLib.Interfaces;
using StreamerBotLib.Overlay.Enums;

namespace StreamerBotLib.DataSQL.TableMeta
{
    internal class MultiLiveStreams : IDatabaseTableMeta
    {
        public System.DateTime LiveDate => (System.DateTime)Values["LiveDate"];
        public System.String UserId => (System.String)Values["UserId"];
        public System.String UserName => (System.String)Values["UserName"];
        public StreamerBotLib.Enums.Platform Platform => (StreamerBotLib.Enums.Platform)Values["Platform"];

        public Dictionary<string, object> Values { get; }

        public string TableName { get; } = "MultiLiveStreams";

        public MultiLiveStreams(Models.MultiLiveStreams tableData)
        {
            Values = new()
            {
                 { "LiveDate", tableData.LiveDate },
                 { "UserId", tableData.UserId },
                 { "UserName", tableData.UserName },
                 { "Platform", tableData.Platform }
            };
        }
        public Dictionary<string, Type> Meta => new()
        {
              { "LiveDate", typeof(System.DateTime) },
              { "UserId", typeof(System.String) },
              { "UserName", typeof(System.String) },
              { "Platform", typeof(StreamerBotLib.Enums.Platform) }
        };
        public object GetModelEntity()
        {
            return new Models.MultiLiveStreams(
                                          (System.DateTime)Values["LiveDate"], 
                                          (System.String)Values["UserId"], 
                                          (System.String)Values["UserName"], 
                                          (StreamerBotLib.Enums.Platform)Values["Platform"]
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

