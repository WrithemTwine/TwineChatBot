namespace StreamerBotLib.Models
{
    public class CategoryData(string categoryId, string categoryName) : IEquatable<CategoryData>
    {
        public string CategoryId { get; set; } = categoryId;
        public string CategoryName { get; set; } = categoryName;

        public bool Equals(CategoryData categoryData)
        {
            return CategoryId == categoryData.CategoryId && CategoryName == categoryData.CategoryName;
        }

        public int HashCode()
        {
            return (CategoryId+CategoryName).GetHashCode();
        }
    };
}
