using StreamerBotLib.Models.Interfaces;

namespace StreamerBotLib.DataSQL.TableMeta
{
    internal class Quotes : IDatabaseTableMeta
    {
        public System.Int32 Number { get => Convert.ToInt32(Values["Number"]); set => Values["Number"] = value; }
        public System.String CategoryName { get => (string)Values["CategoryName"]; set => Values["CategoryName"] = value; }
        public System.DateTime QuoteDate { get => Convert.ToDateTime(Values["QuoteDate"]); set => Values["QuoteDate"] = value; }
        public System.String Quote { get => (string)Values["Quote"]; set => Values["Quote"] = value; }

        public Dictionary<string, object> Values { get; }

        public string TableName { get; } = "Quotes";

        public Quotes(Models.Quotes tableData)
        {
            Values = new()
            {
                 { "Number", tableData.Number },
                 { "CategoryName", tableData.CategoryName },
                 { "QuoteDate", tableData.QuoteDate },
                 { "Quote", tableData.Quote }
            };
        }

        public Dictionary<string, Type> Meta => new()
        {
              { "Number", typeof(System.Int32) },
              { "CategoryName", typeof(System.String) },
              { "QuoteDate", typeof(System.DateTime) },
              { "Quote", typeof(System.String) }
        };

        public object GetModelEntity()
        {
            return new Models.Quotes(
            number: Number,
            categoryName: CategoryName,
            quoteDate: QuoteDate,
            quote: Quote
            );
        }

        public void CopyUpdates(Models.Quotes modelData)
        {
            if (modelData.Number != Number)
            {
                modelData.Number = Number;
            }
            if (modelData.CategoryName != CategoryName)
            {
                modelData.CategoryName = CategoryName;
            }
            if (modelData.QuoteDate != QuoteDate)
            {
                modelData.QuoteDate = QuoteDate;
            }
            if (modelData.Quote != Quote)
            {
                modelData.Quote = Quote;
            }
        }
    }
}