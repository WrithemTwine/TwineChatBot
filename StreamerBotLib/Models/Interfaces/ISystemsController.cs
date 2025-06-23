namespace StreamerBotLib.Models.Interfaces
{
    using StreamerBotLib.Models;
    using StreamerBotLib.Models.Enums;
    using StreamerBotLib.Models.Events;
    using StreamerBotLib.Systems.Overlay.Enums;

    using System;
    using System.Collections.Generic;

    public interface ISystemsController
    {
        // Events
        event EventHandler<PostChannelMessageEventArgs> PostChannelMessage;
        event EventHandler<BanUserRequestEventArgs> BanUserRequest;
        event EventHandler<TwitchShoutOutUsersEventArgs> TwitchShoutOutUser;

        // Core methods
        void NotifyBotStart();
        void NotifyBotStop();
        void Exit();

        // Stream events
        bool StreamOnline(DateTime startedAt);
        void SetCategory(CategoryData categoryData);
        void StreamOffline(DateTime currTime);

        // Overlay
        void SetNewOverlayEventHandler(Action<object, OverlayActionType> overlayHandler, Action<object, UpdatedTickerItemsEventArgs> tickerHandler);
        void CheckForOverlayEvent(OverlayTypes overlayType, ChannelEventActions eventAction, LiveUser user, string UserMsg = null);
        void ClipHelper(List<Clip> clips);

        // Stats
        void UpdatedStat(params StreamStatType[] statTypes);
        void UpdatedStat(StreamStatType statType, int value);

        // User/Chat
        void UserJoined(List<LiveUser> users);
        void UserLeft(LiveUser user);
        void MessageReceived(CmdMessage msg, LiveUser user);
        void ManageGiveaway(LiveUser user);
        void BeginGiveaway();
        void EndGiveaway();
        void PostGiveawayResult();
        void UserCheered(LiveUser user, int bits);

        // Channel events
        void ProcessCommand(CmdMessage commandmsg, Platform source);

        // Timers
        void ActivateRepeatTimers();

        // Follower management
        void AddNewFollowers(List<Follow> follows);

        // Approval
        void PostApproval(string message, Action action);

        // Overlay ticker
        void AddNewOverlayTickerItem(OverlayTickerItem item, string value);

        // Incoming/Outgoing Raids
        void PostIncomingRaid(LiveUser user, DateTime raidTime, int viewerCount, CategoryData category);

        // Misc
        bool CheckMultiStreamDate(string userId, Platform platform, DateTime currTime);
    }

    public static class SystemsController
    {
        // Static methods used in BotController
        public static void ManageDatabase() { }
        public static void ClearWatchTime() { }
        public static void ClearAllCurrenciesValues() { }
        public static void ClearUsersNonFollowers() { }
        public static void SetSystemEventsEnabled(bool enabled) { }
        public static void SetBuiltInCommandsEnabled(bool enabled) { }
        public static void SetUserDefinedCommandsEnabled(bool enabled) { }
        public static void SetDiscordWebhooksEnabled(bool enabled) { }
        public static void AddNewAutoShoutUser(string userId, Platform platform) { }
        public static void StartBulkFollowers() { }
        public static void UpdateFollowers(List<Follow> follows) { }
        public static void StopBulkFollowers() { }
        public static void SetCategory(CategoryData categoryData) { }
        public static void StreamOffline(DateTime currTime) { }
        public static void PostOutgoingRaid(string hostedChannel, DateTime currTime) { }
        public static void AddNewOverlayTickerItem(OverlayTickerItem item, string value) { }
        public static void PostMultiLiveLog(string message) { }
        public static IEnumerable<Tuple<WebhooksSource, Uri>> GetMultiWebHooks() => Array.Empty<Tuple<WebhooksSource, Uri>>();
        public static DataManage DataManage { get; }
        public static bool CheckMultiStreamDate(string userId, Platform platform, DateTime currTime) => false;
        public static void Exit() { }
        public static Tuple<string, string> GetApprovalRule(ModActionType type, string rewardTitle) => null;
    }
}
