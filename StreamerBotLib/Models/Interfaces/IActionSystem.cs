using StreamerBotLib.Models.Enums;
using StreamerBotLib.Models.Events;
using StreamerBotLib.Systems.Overlay.Enums;

namespace StreamerBotLib.Models.Interfaces
{
    public interface IActionSystem
    {
        // Events
        //event EventHandler<PostChannelMessageEventArgs> PostChannelMessage;
        //event EventHandler<BanUserRequestEventArgs> BanUserRequest;
        //event EventHandler<TwitchShoutOutUsersEventArgs> TwitchShoutOutUser;

        // Core methods
        void NotifyBotStart();
        void NotifyBotStop();
        void Exit();

        // Stream events
        void StreamOnline(DateTime startedAt, Action<bool> callback);
        void SetCategory(CategoryData categoryData);
        void StreamOffline(DateTime currTime);

        // Overlay
        void SetNewOverlayEventHandler(EventHandler<NewOverlayEventArgs> overlayHandler, EventHandler<UpdatedTickerItemsEventArgs> tickerHandler);
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

        void AddNewAutoShoutUser(string userId, Platform platform);
        void StartBulkFollowers();
        void UpdateFollowers(List<Follow> follows);
        void StopBulkFollowers();
        void PostOutgoingRaid(string hostedChannel, DateTime currTime);


        // Channel events
        void ProcessCommand(CmdMessage commandmsg, Platform source);

        // Timers
        void ActivateRepeatTimers();

        // Follower management
        void AddNewFollowers(List<Follow> follows);

        // Approval
        void PostApproval(string message, Task action);

        // Overlay ticker
        void AddNewOverlayTickerItem(OverlayTickerItem item, string value);

        // Incoming/Outgoing Raids
        void PostIncomingRaid(LiveUser user, DateTime raidTime, int viewerCount, CategoryData category);

        // Misc
        void CheckMultiStreamDate(string userId, Platform platform, DateTime currTime, Action<bool> callback);

        void ManageDatabase();
        void ClearWatchTime();
        void ClearAllCurrenciesValues();
        void ClearUsersNonFollowers();
        void SetSystemEventsEnabled(bool enabled);
        void SetBuiltInCommandsEnabled(bool enabled);
        void SetUserDefinedCommandsEnabled(bool enabled);
        void SetDiscordWebhooksEnabled(bool enabled);
        void PostMultiLiveLog(string message);
        void GetMultiWebHooks(Action<IEnumerable<Tuple<WebhooksSource, Uri>>> callback);
        void GetApprovalRule(ModActionType type, string rewardTitle, Action<Tuple<string, string>> callback);
        void GetDiscordWebhooks(WebhooksKind webhooksKind, Action<IEnumerable<Tuple<bool, Uri>>> callback);
        void GetEventAnnounce(ChannelEventActions channelEventActions, Action<bool> callback);
        void CheckForOverlayEvent(OverlayTypes overlayType, string eventAction, LiveUser user, string UserMsg = null);
        void PostMultiStreamDate(LiveUser User, DateTime currTime, Action<bool> callback);
        void DeleteDataRows(IEnumerable<object> dataRows, string TableName);
        void GUISaveDataGridEdits(bool CommandUpdate, string TableName);
    }
}
