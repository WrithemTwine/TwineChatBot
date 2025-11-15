using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using System.Collections.Concurrent;

namespace StreamerBotLib.Static.Logger
{
    internal class StreamLoggerProvider : ILoggerProvider
    {
        private readonly IDisposable _onChangeToken;
        private StreamLoggerConfiguration _currentConfig;
        private readonly ConcurrentDictionary<string, StreamLogger> _loggers =
            new(StringComparer.OrdinalIgnoreCase);

        public static event EventHandler<OnWriteLineEventArgs> OnWriteLine;

        public StreamLoggerProvider(IOptionsMonitor<StreamLoggerConfiguration> config)
        {
            _currentConfig = config.CurrentValue;
            _onChangeToken = config.OnChange(updatedConfig => _currentConfig = updatedConfig);
        }

        public ILogger CreateLogger(string categoryName = default)
        {
            StreamLogger newLog = _loggers.GetOrAdd(categoryName, name => new StreamLogger(name, GetCurrentConfig));
            newLog.OnWriteLine += OnWriteLine;
            return newLog;
        }

        private StreamLoggerConfiguration GetCurrentConfig() => _currentConfig;

        public void Dispose()
        {
            _loggers.Clear();
            _onChangeToken?.Dispose();
        }
    }
}
