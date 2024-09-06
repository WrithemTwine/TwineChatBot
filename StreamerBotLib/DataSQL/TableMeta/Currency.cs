using StreamerBotLib.Interfaces;

namespace StreamerBotLib.DataSQL.TableMeta
{
    internal class Currency : IDatabaseTableMeta
    {
        public System.String UserName => (System.String)Values["UserName"];
        public System.Double Value => (System.Double)Values["Value"];
        public System.String CurrencyName => (System.String)Values["CurrencyName"];

        public Dictionary<string, object> Values { get; }

        public string TableName { get; } = "Currency";

        public Currency(Models.Currency tableData)
        {
            Values = new()
            {
                 { "UserName", tableData.UserName },
                 { "Value", tableData.Value },
                 { "CurrencyName", tableData.CurrencyName }
            };
        }
        public Dictionary<string, Type> Meta => new()
        {
              { "UserName", typeof(System.String) },
              { "Value", typeof(System.Double) },
              { "CurrencyName", typeof(System.String) }
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

