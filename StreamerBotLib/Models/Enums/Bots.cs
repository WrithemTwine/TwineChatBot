namespace StreamerBotLib.Models.Enums
{
    public enum Bots
    {
        Default, // default case for repeat timer, general commands with no bot origin
        MediaOverlayServer,
        DiscordWebhooks,
        TwitchClipBot,
        TwitchMultiBot,
        TwitchEventSubBot,
        TwitchEventSubStreamer,

        // EventSub subscription managers
        TwitchStreamerEventSubScopes,
        TwitchStreamerEventSubNoScopes,
        TwitchBotSendChatClient,
        // end

        TwitchTokenBot,
        TwitchHelixBot
    }
}
