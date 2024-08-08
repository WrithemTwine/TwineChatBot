using StreamerBotLib.Interfaces;

namespace StreamerBotLib.DataSQL.TableMeta
{
    internal class Clips : IDatabaseTableMeta
    {
        public System.Int32 ClipId => (System.Int32)Values["ClipId"];
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
              { "ClipId", typeof(System.Int32) },
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
                                          Convert.ToInt32(Values["ClipId"]),
                                          (System.DateTime)Values["CreatedAt"],
                                          (System.String)Values["Title"],
                                          (System.String)Values["CategoryId"],
                                          (System.String)Values["Language"],
                                          (System.Single)Values["Duration"],
                                          (System.String)Values["Url"]
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

            if (modelData.Duration != Duration)
            {
                modelData.Duration = Duration;
            }

            if (modelData.Url != Url)
            {
                modelData.Url = Url;
            }

        }
    }
}

