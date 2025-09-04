using StreamerBotLib.Systems;

namespace StreamerBotLib.Models.Interfaces
{
    public interface IGUIDataManagerViews
    {
        void SetDataManagerViews(DataBot dataBot, Action<bool, Action<IEnumerable<string>>> GetCommands, Action<bool, Action<IEnumerable<string>>> GetCommandsNoParams);
        void SetSystemCollections(ActionSystem actionSystem);
    }
}