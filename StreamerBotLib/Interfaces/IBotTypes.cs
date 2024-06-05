using StreamerBotLib.Events;

namespace StreamerBotLib.Interfaces
{
    /// <summary>
    /// Specifies the base methods to manage each bot cluster.
    /// </summary>
    public interface IBotTypes
    {
        public event EventHandler<BotEventArgs> BotEvent;

        /// <summary>
        /// Send output string through bot.
        /// </summary>
        /// <param name="s">The string to send.</param>
        public void Send(string s);

        /// <summary>
        /// Stops all bots.
        /// </summary>
        public void StopBots();

        /// <summary>
        /// Invokes the bots to retrieve all followers for the platform.
        /// </summary>
        public void GetAllFollowers();

        /// <summary>
        /// Gets user Ids for the bot and monitored channel usernames.
        /// </summary>
        public void SetIds();

        /// <summary>
        /// Specifically a method to call when a stream is online or offline, to start or stop different bot services - based
        /// on user choices.
        /// </summary>
        /// <param name="Start">True to start bots, False to stop bots - based on user selection in the GUI.</param>
        public void ManageStreamOnlineOfflineStatus(bool Start);
        void GetAllFollowers(bool OverrideUpdateFollowers = false);
    }
}
