namespace StreamerBotLib.Overlay.Enums
{
    public enum OverlayTickerItem
    {
        /// <summary>
        /// The last user to follow the channel.
        /// </summary>
        LastFollower,

        /// <summary>
        /// The last user to subscribe.
        /// </summary>
        LastSubscriber,

        /// <summary>
        /// The last user to donate cash. Currently not in use - not connected
        /// </summary>
        LastDonation,

        /// <summary>
        /// The last user to donate bits.
        /// </summary>
        LastBits,

        /// <summary>
        /// The last incoming raid user.
        /// </summary>
        LastInRaid,

        /// <summary>
        /// The last viewer who gifted subscription(s)
        /// </summary>
        LastGiftSub
    }
}
