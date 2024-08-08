using StreamerBotLib.Interfaces;

namespace StreamerBotLib.DataSQL.TableMeta
{
    internal class OverlayTicker : IDatabaseTableMeta
    {
        public StreamerBotLib.Overlay.Enums.OverlayTickerItem TickerName => (StreamerBotLib.Overlay.Enums.OverlayTickerItem)Values["TickerName"];
        public System.String UserName => (System.String)Values["UserName"];

        public Dictionary<string, object> Values { get; }

        public string TableName { get; } = "OverlayTicker";

        public OverlayTicker(Models.OverlayTicker tableData)
        {
            Values = new()
            {
                 { "TickerName", tableData.TickerName },
                 { "UserName", tableData.UserName }
            };
        }
        public Dictionary<string, Type> Meta => new()
        {
              { "TickerName", typeof(StreamerBotLib.Overlay.Enums.OverlayTickerItem) },
              { "UserName", typeof(System.String) }
        };
        public object GetModelEntity()
        {
            return new Models.OverlayTicker(
                                          (StreamerBotLib.Overlay.Enums.OverlayTickerItem)Values["TickerName"],
                                          (System.String)Values["UserName"]
);
        }
        public void CopyUpdates(Models.OverlayTicker modelData)
        {
            if (modelData.TickerName != TickerName)
            {
                modelData.TickerName = TickerName;
            }

            if (modelData.UserName != UserName)
            {
                modelData.UserName = UserName;
            }

        }
    }
}

