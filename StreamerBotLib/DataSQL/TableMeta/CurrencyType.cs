using StreamerBotLib.Enums;
using StreamerBotLib.DataSQL.Models;
using StreamerBotLib.Interfaces;
using StreamerBotLib.Overlay.Enums;

namespace StreamerBotLib.DataSQL.TableMeta
{
    internal class CurrencyType : IDatabaseTableMeta
    {
        public System.Double AccrueAmt => (System.Double)Values["AccrueAmt"];
        public System.Int32 Seconds => (System.Int32)Values["Seconds"];
        public System.Int32 MaxValue => (System.Int32)Values["MaxValue"];
        public System.String CurrencyName => (System.String)Values["CurrencyName"];

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

);
        }
        public void CopyUpdates(Models.CurrencyType modelData)
        {

        }
    }
}

