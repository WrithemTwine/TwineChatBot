
namespace StreamerBotLib.DataSQL.TableMeta
{
    using StreamerBotLib.Models.Interfaces;
    internal class Quotes : IDatabaseTableMeta
    {
        public System.Int32 Number { get => (System.Int32)Values["Number"]; set => Values["Number"] = value; }
        public System.String Quote { get => (System.String)Values["Quote"]; set => Values["Quote"] = value; }

        public Dictionary<string, object> Values { get; }

        public string TableName { get; } = "Quotes";

        public Quotes(Models.Quotes tableData)
        {
            Values = new()
            {
                 { "Number", tableData.Number },
                 { "Quote", tableData.Quote }
            };
        }
        public Dictionary<string, Type> Meta => new()
        {
              { "Number", typeof(System.Int32) },
              { "Quote", typeof(System.String) }
        };
        public object GetModelEntity()
        {
            return new Models.Quotes(
            number: Convert.ToInt32(Number),
            quote: Quote
        );
        }
        public void CopyUpdates(Models.Quotes modelData)
        {
            if (modelData.Number != Number)
            {
                modelData.Number = Number;
            }

            if (modelData.Quote != Quote)
            {
                modelData.Quote = Quote;
            }

        }
    }
}

