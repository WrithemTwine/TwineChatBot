namespace StreamerBotLib.Models.Enums
{
    // TwitchLib.Client.Enums.UserType

    public enum ChannelEventActions
    {
        BeingHosted,
        Bits,
        CommunitySubs,
        NewFollow,
        GiftSub,
        JoinChannel,
        LeaveChannel,
        Live,
        Raid,
        unused1,  // removed existing value, must maintain the order due to the Enum used in the EFC model, the Enum resolves to this list
        Respond,
        Resubscribe,
        Subscribe,
        UserJoined,
        ReturnUserJoined,
        SupporterJoined,
        BannedUser,
        AdSoon, // Twitch "Ad Start Soon" notification
        AdStart, // Twitch "Ad Started" notification
        AdEnd // Twitch "Ad Ended" notification
    }
}
