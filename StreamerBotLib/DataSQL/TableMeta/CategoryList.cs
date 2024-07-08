using StreamerBotLib.Interfaces;

namespace StreamerBotLib.DataSQL.TableMeta
{
    internal class CategoryList : IDatabaseTableMeta
    {
        public System.String CategoryId => (System.String)Values["CategoryId"];
        public System.String Category => (System.String)Values["Category"];
        public System.Int32 StreamCount => (System.Int32)Values["StreamCount"];

        public Dictionary<string, object> Values { get; }

        public string TableName { get; } = "CategoryList";

        public CategoryList(Models.CategoryList tableData)
        {
            Values = new()
            {
                 { "CategoryId", tableData.CategoryId },
                 { "Category", tableData.Category },
                 { "StreamCount", tableData.StreamCount }
            };
        }
        public Dictionary<string, Type> Meta => new()
        {
              { "CategoryId", typeof(System.String) },
              { "Category", typeof(System.String) },
              { "StreamCount", typeof(System.Int32) }
        };
        public object GetModelEntity()
        {
            return new Models.CategoryList(
                                          (System.String)Values["CategoryId"],
                                          (System.String)Values["Category"],
                                          (System.Int32)Values["StreamCount"]
);
        }
        public void CopyUpdates(Models.CategoryList modelData)
        {
            if (modelData.CategoryId != CategoryId)
            {
                modelData.CategoryId = CategoryId;
            }

            if (modelData.Category != Category)
            {
                modelData.Category = Category;
            }

            if (modelData.StreamCount != StreamCount)
            {
                modelData.StreamCount = StreamCount;
            }

        }
    }
}

