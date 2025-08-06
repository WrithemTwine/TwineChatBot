using StreamerBotLib.Systems;

namespace StreamerBotLib.Models.Interfaces
{
    public interface IGUIDataManagerViews
    {
        void SetDataManagerViews(DataBot dataBot, Action<bool, Action<IEnumerable<string>>> callback);
        void SetSystemCollections(ActionSystem actionSystem);
    }
}