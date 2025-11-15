using StreamerBotLib.Models.Interfaces;

namespace StreamerBotLib.DataSQL.TableMeta
{
    internal class CategoryList : IDatabaseTableMeta
    {
        public System.String CategoryId { get => (System.String)Values["CategoryId"]; set => Values["CategoryId"] = value; }
        public System.String Category { get => (System.String)Values["Category"]; set => Values["Category"] = value; }
        public System.Int32 StreamCount { get => (System.Int32)Values["StreamCount"]; set => Values["StreamCount"] = value; }

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
            categoryId: CategoryId,
            category: Category,
            streamCount: Convert.ToInt32(StreamCount)
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

