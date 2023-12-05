using StreamerBotLib.Models;

using System;
using System.Collections.Generic;
using System.Linq;

namespace StreamerBotLib.Systems.CurrencyGames
{
    /// <summary>
    /// Generate Playing Cards
    /// </summary>
    /// <typeparam name="T">The enum of playing cards-card values, e.g. PlayingCardValue- A,1-10,J,Q,K</typeparam>
    /// <typeparam name="S">The enum defining the suits for the cards, e.g. PlayingCardSuit</typeparam>
    internal class PlayingCards<T, S>
        where T : Enum
        where S : Enum
    {
        /// <summary>
        /// Holds the deck of cards used in this instance.
        /// </summary>
        private List<PlayingCard<T, S>> playingCards = new();

        /// <summary>
        /// Create a deck of playing cards from the 'T' and 'S' enum types.
        /// </summary>
        /// <param name="NumberofDecks">Specify how many card decks should be generated for dealing cards.</param>
        internal PlayingCards(int NumberofDecks = 1)
        {
            List<PlayingCard<T, S>> temp = new();

            for (int i = 0; i < NumberofDecks; i++)
            {
                foreach (var CardValue in Enum.GetValues(typeof(T)))
                {
                    foreach (var SuitType in Enum.GetValues(typeof(S)))
                    {
                        temp.Add(new() { PlayingCardValue = (T)CardValue, Suit = (S)SuitType });
                    }
                }
            }
            ShuffleDeck(temp);
        }

        /// <summary>
        /// Shuffle the card deck, and reset the usage.
        /// </summary>
        /// <param name="temp">Supply the card stack to shuffle.</param>
        private void ShuffleDeck(List<PlayingCard<T, S>> temp)
        {
            playingCards.Clear();

            Random Shuffle = new();

            for (int s = 0; s < temp.Count; s++)
            {
                playingCards.Add(temp[Shuffle.Next(temp.Count)]);
            }

            ResetDeck();
        }

        /// <summary>
        /// Reset all of the cards in the deck to unused.
        /// </summary>
        internal void ResetDeck()
        {
            playingCards.ForEach((c) => c.Used = false);
        }

        /// <summary>
        /// Retrieve the first unused card from the list.
        /// </summary>
        /// <returns>The first unused playing card.</returns>
        internal PlayingCard<T, S> DealCard()
        {
            PlayingCard<T, S> card = null;

            foreach (PlayingCard<T, S> c in from PlayingCard<T, S> c in playingCards
                                            where !c.Used
                                            select c)
            {
                card = c;
                c.Used = true;
                break;
            }

            return card;
        }

        /// <summary>
        /// Reshuffle the deck of cards. Reset all usage to 'unused'.
        /// </summary>
        internal void ReShuffleDeck()
        {
            ShuffleDeck(new(playingCards));
        }
    }
}
