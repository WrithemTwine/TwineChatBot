using System.Windows.Controls;

namespace StreamerBotLib.Models.Utiliy
{
    public record GUITabifyRecord
    {
        public Grid SourceGrid { get; set; }
        public TabControl TargetTabControl { get; set; }

    }
}
