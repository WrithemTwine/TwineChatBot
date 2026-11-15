using System.Windows;

namespace StreamerBotLib.Models.BotManagement
{
    public sealed record BotRegistry(FrameworkElement FrameworkElement, BotIdKey BotIdKey) : IEquatable<BotRegistry>
    {
        public FrameworkElement FrameworkElement { get; set; } = FrameworkElement;
        public Type ElementType => FrameworkElement.GetType();
        public BotIdKey BotIdKey { get; set; } = BotIdKey;

        public bool Equals(BotRegistry other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return FrameworkElement == other.FrameworkElement && BotIdKey == other.BotIdKey;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(FrameworkElement, BotIdKey);
        }
    }
}
