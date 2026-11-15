using System.Diagnostics;

namespace StreamerBotLib.Models.Data
{
    [DebuggerDisplay("Parameter={Parameter}, Value={Value}")]
    public record Command
    {
        public string Parameter { get; init; } = default;
        public string Value { get; init; } = default;
    }
}
