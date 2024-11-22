namespace StreamerBotLib.Models
{
    /// <summary>
    /// Holds the data for each player in the game.
    /// </summary>
    public class PlayGameUserWager<T, S>
        where T : Enum
        where S : Enum
    {
        /// <summary>
        /// The current player detail.
        /// </summary>
        public LiveUser Player { get; set; }

        /// <summary>
        /// The player's wager.
        /// </summary>
        public int Wager { get; set; }

        /// <summary>
        /// Indicates whether this player won their hand.
        /// </summary>
        public bool Win { get; set; }

        /// <summary>
        /// The final payout for the player.
        /// </summary>
        public double Payout { get; set; }

        /// <summary>
        /// The resulting message for the player's hand - whether they won or lost.
        /// </summary>
        public string ResultMessage { get; set; }

        /// <summary>
        /// The cards in the player's hand.
        /// </summary>
        public List<PlayingCard<T, S>> Cards { get; set; } = new();

        /// <summary>
        /// The numerical amount of the player's hand.
        /// </summary>
        public int CardCount { get; set; }

        /// <summary>
        /// The visual representation of the player's cards in their hand.
        /// </summary>
        public string CardItems { get; set; } = "";

        /// <summary>
        /// Assign the data.
        /// </summary>
        /// <param name="player">The current player.</param>
        /// <param name="wager">The player's wager.</param>
        public PlayGameUserWager(LiveUser player, int wager)
        {
            Player = player;
            Wager = wager;
        }
    }
}
