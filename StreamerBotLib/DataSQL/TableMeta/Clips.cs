using StreamerBotLib.Enums;
using StreamerBotLib.DataSQL.Models;
using StreamerBotLib.Interfaces;
using StreamerBotLib.Overlay.Enums;

namespace StreamerBotLib.DataSQL.TableMeta
{
    internal class Clips : IDatabaseTableMeta
    {
        public System.String ClipId => (System.String)Values["ClipId"];
        public System.DateTime CreatedAt => (System.DateTime)Values["CreatedAt"];
        public System.String Title => (System.String)Values["Title"];
        public System.String CategoryId => (System.String)Values["CategoryId"];
        public System.String Language => (System.String)Values["Language"];
        public System.Single Duration => (System.Single)Values["Duration"];
        public System.String Url => (System.String)Values["Url"];

        public Dictionary<string, object> Values { get; }

        public string TableName { get; } = "Clips";

        public Clips(Models.Clips tableData)
        {
            Values = new()
            {
                 { "ClipId", tableData.ClipId },
                 { "CreatedAt", tableData.CreatedAt },
                 { "Title", tableData.Title },
                 { "CategoryId", tableData.CategoryId },
                 { "Language", tableData.Language },
                 { "Duration", tableData.Duration },
                 { "Url", tableData.Url }
            };
        }
        public Dictionary<string, Type> Meta => new()
        {
              { "ClipId", typeof(System.String) },
              { "CreatedAt", typeof(System.DateTime) },
              { "Title", typeof(System.String) },
              { "CategoryId", typeof(System.String) },
              { "Language", typeof(System.String) },
              { "Duration", typeof(System.Single) },
              { "Url", typeof(System.String) }
        };
        public object GetModelEntity()
        {
            return new Models.Clips(

);
        }
        public void CopyUpdates(Models.Clips modelData)
        {

        }
    }
}

