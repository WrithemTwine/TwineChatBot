
namespace StreamerBotLib.Systems
{

    using StreamerBotLib.Models.Interfaces;

    public class DataBot
    {
        private static ActionSystem SystemAction { get; set; } = new();

        public DataBot()
        {

        }

        public void InitializeDataManagerViews(IGUIDataManagerViews GUIDataManagerViews)
        {
            GUIDataManagerViews.SetDataManagerViews(ActionSystem.DataManage);
            GUIDataManagerViews.SetSystemCollections();
        }
    }
}
