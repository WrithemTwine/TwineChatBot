using StreamerBotLib.Enums;

namespace StreamerBotLib.Models
{
    internal class PlayingCard<T, S>
        where T : Enum
        where S : Enum
    {
        internal T PlayingCardValue { get; set; }
        internal S Suit { get; set; }
        internal bool Used { get; set; }

        public override string ToString()
        {
            static string ConvertSuit(S suit)
            {
                if (typeof(S) == typeof(PlayingCardSuit))
                {
                    return suit switch
                    {
                        PlayingCardSuit.Diamond => "\u2666",
                        PlayingCardSuit.Spade => "\u2660",
                        PlayingCardSuit.Club => "\u2663",
                        PlayingCardSuit.Heart => "\u2665",
                        _ => ""
                    };
                }
                else
                {
                    return "";
                }
            }

            return $"{PlayingCardValue}{ConvertSuit(Suit)}";
        }
    }
}
