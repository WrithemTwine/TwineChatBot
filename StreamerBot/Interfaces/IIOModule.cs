using StreamerBot.Enum;

using System;

namespace StreamerBot.Interfaces
{
    public interface IIOModule
    {
        public Bots BotClientName { get; set; }

        /// <summary>
        /// Whether to display bot connection to channel.
        /// </summary>
        public static bool ShowConnectionMsg { get; set; }

        // Connect to the data provider, must have Stream Key and Token set to connect
        bool Connect();

        // Send data to the provider
        bool Send(string s);

        // Send whisper to the provider
        bool SendWhisper(string user, string s);

        // Receive Whisper data from the provider via the callback method
        bool ReceiveWhisper(Action<string> ReceiveWhisperCallback);

        // Start send receive operations
        bool StartBot();

        // Stop operations
        bool StopBot();

        bool RefreshSettings();

        // Procedures for full app exit
        bool ExitBot();
    }
}
