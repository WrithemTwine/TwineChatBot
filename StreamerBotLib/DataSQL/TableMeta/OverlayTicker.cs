using StreamerBotLib.Models.Interfaces;

namespace StreamerBotLib.DataSQL.TableMeta
{
    internal class OverlayTicker : IDatabaseTableMeta
    {
        public StreamerBotLib.Systems.Overlay.Enums.OverlayTickerItem TickerName { get => (StreamerBotLib.Systems.Overlay.Enums.OverlayTickerItem)Enum.Parse(typeof(StreamerBotLib.Systems.Overlay.Enums.OverlayTickerItem), Values["TickerName"]?.ToString() ?? "0"); set => Values["TickerName"] = value; }
        public System.String UserName { get => (string)Values["UserName"]; set => Values["UserName"] = value; }

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
              { "TickerName", typeof(StreamerBotLib.Systems.Overlay.Enums.OverlayTickerItem) },
              { "UserName", typeof(System.String) }
        };

        public object GetModelEntity()
        {
            return new Models.OverlayTicker(
            tickerName: TickerName,
            userName: UserName
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