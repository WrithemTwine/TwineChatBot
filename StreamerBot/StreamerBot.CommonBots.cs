using StreamerBotLib.Models.BotManagement;
using StreamerBotLib.Models.Enums;
using StreamerBotLib.Static;

using System.Windows;
using System.Windows.Controls;

namespace StreamerBot
{
    public partial class StreamerBotWindow
    {
        private readonly List<BotRegistry> botControlElements = [];


        private void InitializeBotBridge()
        {
            BotBridge = new(Controller, botControlElements);
            BotBridge.InitializeAsync();
        }


        private void RegisterElement(FrameworkElement frameworkElement, BotIdKey botIdKey)
        {
            botControlElements.UniqueAdd(new BotRegistry(frameworkElement, botIdKey));
        }

        private void FrameworkElementLoaded(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement frameworkElement)
            {
                if (frameworkElement.Tag is BotIdKey botIdKey && botIdKey != null)
                {
                    RegisterElement(frameworkElement, botIdKey);
                    frameworkElement.Loaded -= FrameworkElementLoaded;
                }
            }
        }

    }
}
