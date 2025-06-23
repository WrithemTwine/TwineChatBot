
namespace StreamerBotLib.DataSQL.TableMeta
{
    using StreamerBotLib.Models.Interfaces;
    internal class CurrencyType : IDatabaseTableMeta
    {
        public System.Double AccrueAmt { get => (System.Double)Values["AccrueAmt"]; set => Values["AccrueAmt"] = value; }
        public System.Int32 Seconds { get => (System.Int32)Values["Seconds"]; set => Values["Seconds"] = value; }
        public System.Int32 MaxValue { get => (System.Int32)Values["MaxValue"]; set => Values["MaxValue"] = value; }
        public System.String CurrencyName { get => (System.String)Values["CurrencyName"]; set => Values["CurrencyName"] = value; }

        public Dictionary<string, object> Values { get; }

        public string TableName { get; } = "CurrencyType";

        public CurrencyType(Models.CurrencyType tableData)
        {
            Values = new()
            {
                 { "AccrueAmt", tableData.AccrueAmt },
                 { "Seconds", tableData.Seconds },
                 { "MaxValue", tableData.MaxValue },
                 { "CurrencyName", tableData.CurrencyName }
            };
        }
        public Dictionary<string, Type> Meta => new()
        {
              { "AccrueAmt", typeof(System.Double) },
              { "Seconds", typeof(System.Int32) },
              { "MaxValue", typeof(System.Int32) },
              { "CurrencyName", typeof(System.String) }
        };
        public object GetModelEntity()
        {
            return new Models.CurrencyType(
            accrueAmt: AccrueAmt,
            seconds: Convert.ToInt32(Seconds),
            maxValue: Convert.ToInt32(MaxValue),
            currencyName: CurrencyName
        );
        }
        public void CopyUpdates(Models.CurrencyType modelData)
        {
            if (modelData.AccrueAmt != AccrueAmt)
            {
                modelData.AccrueAmt = AccrueAmt;
            }

            if (modelData.Seconds != Seconds)
            {
                modelData.Seconds = Seconds;
            }

            if (modelData.MaxValue != MaxValue)
            {
                modelData.MaxValue = MaxValue;
            }

            if (modelData.CurrencyName != CurrencyName)
            {
                modelData.CurrencyName = CurrencyName;
            }

        }
    }
}

