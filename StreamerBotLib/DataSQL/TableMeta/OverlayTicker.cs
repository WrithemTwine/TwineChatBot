using StreamerBotLib.Enums;
using StreamerBotLib.DataSQL.Models;
using StreamerBotLib.Interfaces;
using StreamerBotLib.Overlay.Enums;

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

);
        }
        public void CopyUpdates(Models.OverlayTicker modelData)
        {

        }
    }
}

