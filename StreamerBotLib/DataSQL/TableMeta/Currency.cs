using StreamerBotLib.Interfaces;

namespace StreamerBotLib.DataSQL.TableMeta
{
    internal class Currency : IDatabaseTableMeta
    {
        public System.Double Value { get => (System.Double)Values["Value"]; set => Values["Value"] = value; }
        public System.String CurrencyName { get => (System.String)Values["CurrencyName"]; set => Values["CurrencyName"] = value; }
        public System.String UserId { get => (System.String)Values["UserId"]; set => Values["UserId"] = value; }
        public StreamerBotLib.Enums.Platform Platform { get => (StreamerBotLib.Enums.Platform)Values["Platform"]; set => Values["Platform"] = value; }

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
            value: Value,
            currencyName: CurrencyName,
            userId: UserId,
            platform: Platform
        );
        }
        public void CopyUpdates(Models.Currency modelData)
        {
            if (modelData.Value != Value)
            {
                modelData.Value = Value;
            }

            if (modelData.CurrencyName != CurrencyName)
            {
                modelData.CurrencyName = CurrencyName;
            }

            if (modelData.UserId != UserId)
            {
                modelData.UserId = UserId;
            }

            if (modelData.Platform != Platform)
            {
                modelData.Platform = Platform;
            }

        }
    }
}

