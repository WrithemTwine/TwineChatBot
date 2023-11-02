namespace StreamerBotLib.Enums
{
    public enum PlayingCardOptions
    {
        /// <summary>
        /// Represents once the cards are dealt in a single round, the full deck is restored for the next deal.
        /// </summary>
        SingleDeal,
        /// <summary>
        /// Represents cards are dealt from the same stack until used completely, then reshuffled.
        /// </summary>
        MultiDeal
    }
}
