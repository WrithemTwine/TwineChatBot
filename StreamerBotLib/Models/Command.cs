
namespace StreamerBotLib.Models
{
    using System.Diagnostics;
    [DebuggerDisplay("Parameter={Parameter}, Value={Value}")]
    public record Command
    {
        public string Parameter { get; init; } = default;
        public string Value { get; init; } = default;
    }
}
