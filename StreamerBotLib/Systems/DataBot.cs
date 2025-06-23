using StreamerBotLib.Models.Interfaces;

namespace StreamerBotLib.Systems
{
    public class DataBot
    {
        private static SystemsController SystemsController { get; set; } = new();

        public DataBot()
        {

        }

        public void InitializeDataManagerViews(IGUIDataManagerViews GUIDataManagerViews)
        {
            GUIDataManagerViews.SetDataManagerViews(SystemsController.DataManage);
            GUIDataManagerViews.SetSystemCollections();
        }
    }
}
