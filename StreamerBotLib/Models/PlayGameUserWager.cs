using System;
using System.Collections.Generic;

namespace StreamerBotLib.Models
{
    /// <summary>
    /// Holds the data for each player in the game.
    /// </summary>
    internal class PlayGameUserWager<T, S>
        where T : Enum
        where S : Enum
    {
        /// <summary>
        /// The current player detail.
        /// </summary>
        internal LiveUser Player { get; set; }

        /// <summary>
        /// The player's wager.
        /// </summary>
        internal int Wager { get; set; }

        /// <summary>
        /// Indicates whether this player won their hand.
        /// </summary>
        internal bool Win { get; set; }

        /// <summary>
        /// The final payout for the player.
        /// </summary>
        internal double Payout { get; set; }

        /// <summary>
        /// The resulting message for the player's hand - whether they won or lost.
        /// </summary>
        internal string ResultMessage { get; set; }

        /// <summary>
        /// The cards in the player's hand.
        /// </summary>
        internal List<PlayingCard<T, S>> Cards { get; set; } = new();

        /// <summary>
        /// The numerical amount of the player's hand.
        /// </summary>
        internal int CardCount { get; set; }

        /// <summary>
        /// The visual representation of the player's cards in their hand.
        /// </summary>
        internal string CardItems { get; set; } = "";

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
