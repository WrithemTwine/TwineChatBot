using StreamerBotLib.BotIOController;
using StreamerBotLib.Models.BotManagement;
using StreamerBotLib.Models.Enums;

namespace StreamerBotLib.GUI
{
    public class GUIBotBridge
    {
        public BotController Controller { get; }
        public List<BotRegistry> ElementRegistry { get; }

        public GUIBotBridge(BotController controller, List<BotRegistry> registry)
        {
            Controller = controller;
            ElementRegistry = registry;
        }

        public Task InitializeAsync()
        {
            return Task.Run(() =>
            {

            });
        }

        public Task RefreshAuthStateAsync(PlatformID p)
        {
            return Task.Run(() =>
            {

            });
        }

        public void StartBot(BotIdKey key)
        {
            Controller.StartBot(p: key.Platform, key: key);
        }

        public void StopBot(BotIdKey key)
        {
            Controller.StopBot(p: key.Platform, key: key);
        }

        public void StartAll(Platform p = Platform.Default)
        {
            Controller.StartBot(p: p);
        }

        public void StopAll(Platform p = Platform.Default)
        {
            Controller.StopBot(p: p);
        }

        public void StartAuto()
        {
            Controller.StartAutoBots();
        }

        public event Action AuthUiChanged;
        public event Action<Platform, Bots, bool> BotRunningChanged;
        public event Action<Platform, Bots, string> BotFailed;
    }
}
