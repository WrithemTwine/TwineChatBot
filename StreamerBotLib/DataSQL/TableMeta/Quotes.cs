using StreamerBotLib.Enums;
using StreamerBotLib.DataSQL.Models;
using StreamerBotLib.Interfaces;
using StreamerBotLib.Overlay.Enums;

namespace StreamerBotLib.DataSQL.TableMeta
{
    internal class Quotes : IDatabaseTableMeta
    {
        public System.Int16 Number => (System.Int16)Values["Number"];
        public System.String Quote => (System.String)Values["Quote"];

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
              { "Number", typeof(System.Int16) },
              { "Quote", typeof(System.String) }
        };
        public object GetModelEntity()
        {
            return new Models.Quotes(
                                          Convert.ToInt16(Values["Number"]), 
                                          (System.String)Values["Quote"]
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

