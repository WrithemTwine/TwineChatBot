namespace StreamerBotLib.Models.Enums
{
    /// <summary>
    /// This enum holds values for mapping the functional areas to user-selected settings for whether to emit data to 
    /// the debug output logfile.
    /// </summary>
    public enum DebugLogTypes
    {
#if DEBUG
        SpecialPurpose, // used for special purpose debugging, not included in the release builds
#endif

        BlackjackGame,
        BotController,
        CommandSystem,
        RepeatCommandSystem,
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
        ManageStreamViewers,
        ModerationSystem,
        OutputMsgParsing,
        OverlayBot,// specific to processing an overlay out to broadcast software URLs
        OverlaySystem, // specific to the overlay portion of how overlays are determined
        StatSystem,
        SystemController,
        ThreadManager,
        TwitchBots,
        TwitchHelixBot,
        TwitchClipBot,
        TwitchBotSendChat,
        TwitchBotEventSubBot,
        TwitchStreamerEventSubBot,
        TwitchEventSub,
        TwitchMultiLiveBot,
        TwitchTokenBot,
        TwitchStreamerNoScopesEventSubBot
    }
}
