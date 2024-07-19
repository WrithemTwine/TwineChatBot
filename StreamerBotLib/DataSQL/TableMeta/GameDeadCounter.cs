using StreamerBotLib.Enums;
using StreamerBotLib.DataSQL.Models;
using StreamerBotLib.Interfaces;
using StreamerBotLib.Overlay.Enums;

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
                                          (System.String)Values["CategoryId"], 
                                          (System.String)Values["Category"], 
                                          Convert.ToInt32(Values["Counter"])
);
        }
        public void CopyUpdates(Models.GameDeadCounter modelData)
        {
          if (modelData.CategoryId != CategoryId)
            {
                modelData.CategoryId = CategoryId;
            }

          if (modelData.Category != Category)
            {
                modelData.Category = Category;
            }

          if (modelData.Counter != Counter)
            {
                modelData.Counter = Counter;
            }

        }
    }
}

