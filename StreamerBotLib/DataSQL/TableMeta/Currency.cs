using StreamerBotLib.Interfaces;

namespace StreamerBotLib.DataSQL.TableMeta
{
    internal class Currency : IDatabaseTableMeta
    {
        public System.Double Value => (System.Double)Values["Value"];
        public System.String CurrencyName => (System.String)Values["CurrencyName"];
        public System.String UserId => (System.String)Values["UserId"];
        public StreamerBotLib.Enums.Platform Platform => (StreamerBotLib.Enums.Platform)Values["Platform"];

        public Dictionary<string, object> Values { get; }

        public string TableName { get; } = "Currency";

        public Currency(Models.Currency tableData)
        {
            Values = new()
            {
                 { "Value", tableData.Value },
                 { "CurrencyName", tableData.CurrencyName },
                 { "UserId", tableData.UserId },
                 { "Platform", tableData.Platform }
            };
        }
        public Dictionary<string, Type> Meta => new()
        {
              { "Value", typeof(System.Double) },
              { "CurrencyName", typeof(System.String) },
              { "UserId", typeof(System.String) },
              { "Platform", typeof(StreamerBotLib.Enums.Platform) }
        };
        public object GetModelEntity()
        {
            return new Models.Currency(

);
        }
        public void CopyUpdates(Models.Currency modelData)
        {

        }
    }
}

