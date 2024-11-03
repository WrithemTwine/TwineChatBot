namespace StreamerBotLib.Enums
{
    /// <summary>
    /// This enum holds values for mapping the functional areas to user-selected settings for whether to emit data to 
    /// the debug output logfile.
    /// </summary>
    public enum DebugLogTypes
    {
        BlackjackGame,
        BotController,
        CommandSystem,
        CommonSystem,
        CurrencySystem,
        DataManager,
        DiscordBot,
        FormatData,
        GUIBotComs,
        GUIDataViews,
        GUIEvents,
        GUIHelpers,
        GUIMultiLive,
        GUIProcessWatcher,
        GUITabSizes,
        GUIThemes,
        GUITwitchTokenAuth,
        LocalizedMessages,
        ModerationSystem,
        OutputMsgParsing,
        OverlayBot,// specific to processing an overlay out to broadcast software URLs
        OverlaySystem, // specific to the overlay portion of how overlays are determined
        StatSystem,
        SystemController,
        ThreadManager,
        TwitchBots,
        TwitchBotUserSvc,
        TwitchChatBot,
        TwitchClipBot,
        TwitchFollowBot,
        TwitchLiveBot,
        TwitchMultiLiveBot,
        TwitchPubSubBot,
        TwitchTokenBot

    }
}
