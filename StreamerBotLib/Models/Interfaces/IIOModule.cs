using StreamerBotLib.Models.Enums;

namespace StreamerBotLib.Models.Interfaces
{
    public interface IIOModule
    {
        public Bots BotClientName { get; set; }

        /// <summary>
        /// Whether to display bot connection to channel.
        /// </summary>
        public static bool ShowConnectionMsg { get; set; }

        // Connect to the data provider, must have Stream Key and Token set to connect
        Task<bool> Connect();

        // Send data to the provider
        Task Send(string s, bool Announcement = false);

        // Send whisper to the provider
        bool SendWhisper(string user, string s);

        // Receive Whisper data from the provider via the callback method
        bool ReceiveWhisper(Action<string> ReceiveWhisperCallback);

        // Start send receive operations
        Task StartBot();

        // Stop operations
        Task StopBot();

        // Procedures for full app exit
        Task<bool> ExitBot();
    }
}
