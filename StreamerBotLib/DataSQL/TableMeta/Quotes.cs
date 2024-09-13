using StreamerBotLib.Interfaces;

namespace StreamerBotLib.DataSQL.TableMeta
{
    internal class Quotes : IDatabaseTableMeta
    {
        public System.Int32 Number => (System.Int32)Values["Number"];
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
              { "Number", typeof(System.Int32) },
              { "Quote", typeof(System.String) }
        };
        public object GetModelEntity()
        {
            return new Models.Quotes(

);
        }
        public void CopyUpdates(Models.Quotes modelData)
        {

        }
    }
}

