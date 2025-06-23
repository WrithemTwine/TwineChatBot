using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;

namespace StreamerBotLib.Static.Logger
{
    public static class StreamLoggerExtenstions
    {
        public static ILoggingBuilder AddStreamLogger(
            this ILoggingBuilder builder)
        {
            builder.AddConfiguration();

            builder.Services.TryAddEnumerable(
                ServiceDescriptor.Singleton<ILoggerProvider, StreamLoggerProvider>());

            LoggerProviderOptions.RegisterProviderOptions
                <StreamLoggerConfiguration, StreamLoggerProvider>(builder.Services);

            return builder;
        }

        public static ILoggingBuilder AddStreamLogger(
            this ILoggingBuilder builder,
            Action<StreamLoggerConfiguration> configure)
        {
            builder.AddStreamLogger();
            builder.Services.Configure(configure);

            return builder;
        }
    }
}
