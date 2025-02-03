using StreamerBotLib.Enums;
using StreamerBotLib.DataSQL.Models;
using StreamerBotLib.Interfaces;
using StreamerBotLib.Overlay.Enums;

namespace StreamerBotLib.DataSQL.TableMeta
{
    internal class Clips : IDatabaseTableMeta
    {
        public System.String ClipId { get => (System.String)Values["ClipId"]; set => Values["ClipId"] = value; }
        public System.DateTime CreatedAt { get => (System.DateTime)Values["CreatedAt"]; set => Values["CreatedAt"] = value; }
        public System.String Title { get => (System.String)Values["Title"]; set => Values["Title"] = value; }
        public System.String CategoryId { get => (System.String)Values["CategoryId"]; set => Values["CategoryId"] = value; }
        public System.String Language { get => (System.String)Values["Language"]; set => Values["Language"] = value; }
        public System.Single Duration { get => (System.Single)Values["Duration"]; set => Values["Duration"] = value; }
        public System.String Url { get => (System.String)Values["Url"]; set => Values["Url"] = value; }

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
            clipId: ClipId, 
            createdAt: CreatedAt, 
            title: Title, 
            categoryId: CategoryId, 
            language: Language, 
            url: Url
        );
        }
        public void CopyUpdates(Models.Clips modelData)
        {
          if (modelData.ClipId != ClipId)
            {
                modelData.ClipId = ClipId;
            }

          if (modelData.CreatedAt != CreatedAt)
            {
                modelData.CreatedAt = CreatedAt;
            }

          if (modelData.Title != Title)
            {
                modelData.Title = Title;
            }

          if (modelData.CategoryId != CategoryId)
            {
                modelData.CategoryId = CategoryId;
            }

          if (modelData.Language != Language)
            {
                modelData.Language = Language;
            }

          if (modelData.Url != Url)
            {
                modelData.Url = Url;
            }

        }
    }
}

