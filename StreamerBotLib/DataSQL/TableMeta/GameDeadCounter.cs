using StreamerBotLib.Interfaces;

namespace StreamerBotLib.DataSQL.TableMeta
{
    internal class GameDeadCounter : IDatabaseTableMeta
    {
        public System.String CategoryId => (System.String)Values["CategoryId"];
        public System.String Category => (System.String)Values["Category"];
        public System.Int32 Counter => (System.Int32)Values["Counter"];

        public Dictionary<string, object> Values { get; }

        public string TableName { get; } = "GameDeadCounter";

        public GameDeadCounter(Models.GameDeadCounter tableData)
        {
            Values = new()
            {
                 { "CategoryId", tableData.CategoryId },
                 { "Category", tableData.Category },
                 { "Counter", tableData.Counter }
            };
        }
        public Dictionary<string, Type> Meta => new()
        {
              { "CategoryId", typeof(System.String) },
              { "Category", typeof(System.String) },
              { "Counter", typeof(System.Int32) }
        };
        public object GetModelEntity()
        {
            return new Models.GameDeadCounter(

);
        }
        public void CopyUpdates(Models.GameDeadCounter modelData)
        {

        }
    }
}

