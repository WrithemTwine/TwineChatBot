using StreamerBotLib.Enums;
using StreamerBotLib.DataSQL.Models;
using StreamerBotLib.Interfaces;
using StreamerBotLib.Overlay.Enums;

namespace StreamerBotLib.DataSQL.TableMeta
{
    internal class InRaidData : IDatabaseTableMeta
    {
        public System.Int32 ViewerCount => (System.Int32)Values["ViewerCount"];
        public System.DateTime RaidDate => (System.DateTime)Values["RaidDate"];
        public System.String Category => (System.String)Values["Category"];
        public System.String UserId => (System.String)Values["UserId"];
        public StreamerBotLib.Enums.Platform Platform => (StreamerBotLib.Enums.Platform)Values["Platform"];

        public Dictionary<string, object> Values { get; }

        public string TableName { get; } = "InRaidData";

        public InRaidData(Models.InRaidData tableData)
        {
            Values = new()
            {
                 { "ViewerCount", tableData.ViewerCount },
                 { "RaidDate", tableData.RaidDate },
                 { "Category", tableData.Category },
                 { "UserId", tableData.UserId },
                 { "Platform", tableData.Platform }
            };
        }
        public Dictionary<string, Type> Meta => new()
        {
              { "ViewerCount", typeof(System.Int32) },
              { "RaidDate", typeof(System.DateTime) },
              { "Category", typeof(System.String) },
              { "UserId", typeof(System.String) },
              { "Platform", typeof(StreamerBotLib.Enums.Platform) }
        };
        public object GetModelEntity()
        {
            return new Models.InRaidData(

);
        }
        public void CopyUpdates(Models.InRaidData modelData)
        {

        }
    }
}

