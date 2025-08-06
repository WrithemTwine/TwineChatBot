using Microsoft.Extensions.Logging;

namespace StreamerBotLib.Static.Logger
{
    public class StreamLoggerConfiguration
    {
        public int EventId { get; set; }

        public Dictionary<LogLevel, bool> LogLevelEnabled { get; set; } = new()
        {
            [LogLevel.Debug] = true,
            [LogLevel.Information] = true,
            [LogLevel.Warning] = true,
            [LogLevel.Error] = true,
            [LogLevel.Critical] = true,
            [LogLevel.None] = true,
            [LogLevel.Trace] = true
        };
    }
}
