using StreamerBotLib.Models.Interfaces;
using StreamerBotLib.Systems.Overlay.Enums;

namespace StreamerBotLib.DataSQL.TableMeta
{
    internal class OverlayTicker : IDatabaseTableMeta
    {
        public OverlayTickerItem TickerName { get => (OverlayTickerItem)Values["TickerName"]; set => Values["TickerName"] = value; }
        public System.String UserName { get => (System.String)Values["UserName"]; set => Values["UserName"] = value; }

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
              { "TickerName", typeof(OverlayTickerItem) },
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

