using StreamerBotLib.Enums;

namespace StreamerBotLib.Models
{
    public class PlayingCard<T, S>
        where T : Enum
        where S : Enum
    {
        public T PlayingCardValue { get; set; }
        public S Suit { get; set; }
        public bool Used { get; set; }

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
