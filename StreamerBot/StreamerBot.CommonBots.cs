using StreamerBotLib.BotClients;
using StreamerBotLib.Models.BotManagement;
using StreamerBotLib.Models.Enums;
using StreamerBotLib.Models.Events;
using StreamerBotLib.Static;
using StreamerBotLib.Systems.Overlay;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace StreamerBot
{
    public partial class StreamerBotWindow
    {
        private readonly List<BotRegistry> botControlElements = [];

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
