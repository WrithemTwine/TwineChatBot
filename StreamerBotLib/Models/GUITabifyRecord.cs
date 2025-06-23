namespace StreamerBotLib.Models
{
    using System.Windows.Controls;

    public record GUITabifyRecord
    {
        public Grid SourceGrid { get; set; }
        public TabControl TargetTabControl { get; set; }

    }
}
