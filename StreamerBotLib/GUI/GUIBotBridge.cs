using StreamerBotLib.BotIOController;
using StreamerBotLib.Models.BotManagement;
using StreamerBotLib.Models.Enums;
using StreamerBotLib.Models.Events;
using StreamerBotLib.Static;

using System.Windows.Controls;

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

            Controller.BotStatusChanged += Controller_BotStatusChanged;

            AuthUiChanged += BotBridge_AuthUiChanged;
            BotRunningChanged += BotBridge_BotRunningChanged;
            BotFailed += BotBridge_BotFailed;
        }
 
        private void Controller_BotStatusChanged(object sender, BotStartStopEventArgs e)
        {
            if (e.Error == false)
            {
                BotRunningChanged?.Invoke(e.Platform, e.Bots, e.Started);
            }
            else
            {
                BotFailed?.Invoke(e.Platform, e.Bots, "Bot failed to start. Check the logs for more information.");
            }
        }

        public Task InitializeAsync()
        {
            return Task.Run(() =>
            {
                Controller.PerformBotOperation(botOperationTypes: BotOperationTypes.ReadyToAuthorize, bots: Bots.Default);
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

        private void BotBridge_AuthUiChanged()
        {
            throw new NotImplementedException();
        }


        private void BotBridge_BotRunningChanged(Platform arg1, Bots arg2, bool arg3)
        {
            ThreadManager.AddTaskToGUIDispatcher(() =>
            {
                BotRegistry botRegistry = ElementRegistry.FirstOrDefault(c => c.BotIdKey.IsSelected(platform: arg1, bots: arg2, botOperationType: BotOperationTypes.SingleBotActive, botStatusCategory: (arg3 ? BotStatusCategory.Start : BotStatusCategory.Stop)) && c.ElementType == typeof(RadioButton));


                if (botRegistry != default)
                {
                    if (botRegistry.FrameworkElement is RadioButton radioButton)
                    {
                        radioButton.IsChecked = true;
                    }
                }

            });
        }

        private void BotBridge_BotFailed(Platform arg1, Bots arg2, string arg3)
        {
            ThreadManager.AddTaskToGUIDispatcher(() =>
            {
                BotRegistry botRegistry = ElementRegistry.FirstOrDefault(c => c.BotIdKey.IsSelected(platform: arg1, bots: arg2, botOperationType: BotOperationTypes.SingleBotActive, botStatusCategory: BotStatusCategory.Error));

                if (botRegistry != default && botRegistry.FrameworkElement is Label label)
                {
                    label.Visibility = BotRegistry.SetVisibility(true, botRegistry.BotIdKey.LabelOperationVisibility);
                }

                BotBridge_BotRunningChanged(arg1, arg2, false); // set the bot stop radiobutton

            });
        }


    }
}
