
namespace StreamerBotLib.Models.Interfaces
{
    using StreamerBotLib.Systems;

    public interface IGUIDataManagerViews
    {
        void SetDataManagerViews(DataBot dataBot, Action<bool, Action<IEnumerable<string>>> callback);
        void SetSystemCollections(ActionSystem actionSystem);
    }
}