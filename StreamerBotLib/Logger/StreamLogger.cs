using Microsoft.Extensions.Logging;

using StreamerBotLib.Static;

namespace StreamerBotLib.Logger
{
    internal class StreamLogger(string name, Func<StreamLoggerConfiguration> getCurrentConfig) : ILogger
    {
        public IDisposable BeginScope<TState>(TState state) where TState : notnull => default!;

        public event EventHandler<OnWriteLineEventArgs> OnWriteLine;

        public bool IsEnabled(LogLevel logLevel) => getCurrentConfig().LogLevelEnabled.ContainsKey(logLevel);

        public void Log<TState>(LogLevel logLevel,
                                EventId eventId,
                                TState state,
                                Exception exception,
                                Func<TState, Exception, string> formatter)
        {
            //if (!IsEnabled(logLevel))
            //{
            //    return;
            //}
            //OnWriteLine?.Invoke(this, new($"{DateTime.Now.ToLocalTime()} [{eventId.Id,2}: {logLevel,-12}]"));
            ThreadManager.CreateThreadStart("Log", () =>
            {
                OnWriteLine?.Invoke(this, new($"{name[(name.LastIndexOf('.') + 1)..]} - {formatter(state, exception)}"));
            });
        }
    }
}
