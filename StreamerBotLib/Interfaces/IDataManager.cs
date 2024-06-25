﻿using StreamerBotLib.DataSQL.Models;
using StreamerBotLib.Enums;
using StreamerBotLib.Events;
using StreamerBotLib.Models;
using StreamerBotLib.Overlay.Enums;
using StreamerBotLib.Overlay.Models;

using System.Collections.ObjectModel;
using System.Data;

namespace StreamerBotLib.Interfaces
{
    public interface IDataManager : IDataManagerReadOnly
    {
        event EventHandler<OnBulkFollowersAddFinishedEventArgs> OnBulkFollowersAddFinished;
        event EventHandler<OnDataCollectionUpdatedEventArgs> OnDataCollectionUpdated;

        bool CheckCurrency(LiveUser User, double value, string CurrencyName);
        new bool CheckField(string table, string field);
        new bool CheckFollower(string User);
        new bool CheckFollower(string User, DateTime ToDateTime);
        Tuple<string, string> CheckModApprovalRule(ModActionType modActionType, string ModAction);
        bool CheckMultiStreams(DateTime streamStart);
        new bool CheckPermission(string cmd, ViewerTypes permission);
        new bool CheckShoutName(string UserId);
        bool CheckStreamTime(DateTime CurrTime);
        new bool CheckUser(LiveUser User);
        new bool CheckUser(LiveUser User, DateTime ToDateTime);
        string CheckWelcomeUser(string User);
        void ClearAllCurrencyValues();
        void ClearUsersNotFollowers();
        void ClearWatchTime();
        void DeleteDataRows(IEnumerable<DataRow> dataRows);
        string EditCommand(string cmd, List<string> Arglist);
        Tuple<ModActions, Enums.BanReasons, int> FindRemedy(ViewerTypes viewerTypes, MsgTypes msgTypes);
        ObservableCollection<DataSQL.Models.BanReasons> GetBanReasonsLocalObservable();
        ObservableCollection<BanRules> GetBanRulesLocalObservable();
        ObservableCollection<CategoryList> GetCategoryListLocalObservable();
        ObservableCollection<ChannelEvents> GetChannelEventsLocalObservable();
        ObservableCollection<Clips> GetClipsLocalObservable();
        new CommandData GetCommand(string cmd);
        new IEnumerable<string> GetCommandList();
        new string GetCommands();
        ObservableCollection<Commands> GetCommandsLocalObservable();
        ObservableCollection<CommandsUser> GetCommandsUserLocalObservable();
        ObservableCollection<Currency> GetCurrencyLocalObservable();
        new List<string> GetCurrencyNames();
        ObservableCollection<DataSQL.Models.CurrencyType> GetCurrencyTypeLocalObservable();
        ObservableCollection<CustomWelcome> GetCustomWelcomeLocalObservable();
        int GetDeathCounter(string currCategory);
        new string GetEventRowData(ChannelEventActions rowcriteria, out bool Enabled, out short Multi);
        int GetFollowerCount();
        ObservableCollection<Followers> GetFollowersLocalObservable();
        new List<Tuple<string, string>> GetGameCategories();
        ObservableCollection<GameDeadCounter> GetGameDeadCounterLocalObservable();
        ObservableCollection<GiveawayUserData> GetGiveawayUserDataLocalObservable();
        ObservableCollection<InRaidData> GetInRaidDataLocalObservable();
        new string GetKey(string Table);
        new IEnumerable<string> GetKeys(string Table);
        ObservableCollection<LearnMsgs> GetLearnMsgsLocalObservable();
        ObservableCollection<ModeratorApprove> GetModeratorApproveLocalObservable();
        ObservableCollection<MultiChannels> GetMultiChannelsLocalObservable();
        ObservableCollection<MultiLiveStreams> GetMultiLiveStreamsLocalObservable();
        ObservableCollection<MultiMsgEndPoints> GetMultiMsgEndPointsLocalObservable();
        ObservableCollection<MultiSummaryLiveStreams> GetMultiSummaryLiveStreamsLocalObservable();
        string GetNewestFollower();
        ObservableCollection<OutRaidData> GetOutRaidDataLocalObservable();
        Dictionary<string, List<string>> GetOverlayActions();
        List<OverlayActionType> GetOverlayActions(OverlayTypes overlayType, string overlayAction, string username);
        ObservableCollection<OverlayServices> GetOverlayServicesLocalObservable();
        ObservableCollection<OverlayTicker> GetOverlayTickerLocalObservable();
        string GetQuote(int QuoteNum);
        int GetQuoteCount();
        ObservableCollection<Quotes> GetQuotesLocalObservable();
        ObservableCollection<ShoutOuts> GetShoutOutsLocalObservable();
        List<string> GetSocialComs();
        new string GetSocials();
        StreamStat GetStreamData(DateTime dateTime);
        ObservableCollection<StreamStats> GetStreamStatsLocalObservable();
        new List<string> GetTableFields(string TableName);
        new List<string> GetTableNames();
        List<TickerItem> GetTickerItems();
        new Tuple<string, int, List<string>> GetTimerCommand(string Cmd);
        new List<Tuple<string, int, List<string>>> GetTimerCommands();
        new int GetTimerCommandTime(string Cmd);
        new string GetUsage(string command);
        new string GetUserId(LiveUser User);
        ObservableCollection<Users> GetUsersLocalObservable();
        ObservableCollection<UserStats> GetUserStatsLocalObservable();
        new List<Tuple<bool, Uri>> GetWebhooks(WebhooksSource webhooksSource, WebhooksKind webhooks);
        ObservableCollection<Webhooks> GetWebhooksLocalObservable();
        void Initialize();
        object[] PerformQuery(Commands row, int Top = 0);
        object PerformQuery(Commands row, string ParamValue);
        bool PostCategory(string CategoryId, string newCategory);
        bool PostClip(int ClipId, DateTime CreatedAt, decimal Duration, string GameId, string Language, string Title, string Url);
        string PostCommand(string cmd, CommandParams Params);
        void PostCurrencyUpdate(LiveUser User, double value, string CurrencyName);
        int PostDeathCounterUpdate(string currCategory, bool Reset = false, int updateValue = 1);
        bool PostFollower(Follow follow);
        void PostGiveawayData(string DisplayName, DateTime dateTime);
        void PostInRaidData(string user, DateTime time, int viewers, string gamename, Platform platform);
        void PostLearnMsgsRow(string Message, MsgTypes MsgType);
        bool PostMergeUserStats(string CurrUser, string SourceUser, Platform platform);
        void PostMonitorChannel(IEnumerable<LiveUser> liveUsers);
        bool PostMultiStreamDate(string userid, string username, Platform platform, DateTime onDate);
        void PostNewAutoShoutUser(string UserName, string UserId, Platform platform);
        void PostOutgoingRaid(string HostedChannel, DateTime dateTime);
        int PostQuote(string Text);
        bool PostStream(DateTime StreamStart);
        void PostStreamStat(StreamStat streamStat);
        void PostUserCustomWelcome(LiveUser User, string WelcomeMsg);
        void RemoveAllFollowers();
        void RemoveAllGiveawayData();
        void RemoveAllInRaidData();
        void RemoveAllOutRaidData();
        void RemoveAllOverlayTickerData();
        void RemoveAllStreamStats();
        void RemoveAllUsers();
        bool RemoveCommand(string command);
        bool RemoveQuote(int QuoteNum);
        void SetBuiltInCommandsEnabled(bool Enabled);
        void SetIsEnabled(IEnumerable<DataRow> dataRows, bool IsEnabled = false);
        void SetSystemEventsEnabled(bool Enabled);
        void SetUserDefinedCommandsEnabled(bool Enabled);
        void SetWebhooksEnabled(bool Enabled);
        void StartBulkFollowers();
        void SummarizeStreamData();
        void SummarizeStreamData(ArchiveMultiStream archiveRecord);
        new bool TestInRaidData(string user, DateTime time, string viewers, string gamename);
        new bool TestOutRaidData(string HostedChannel, DateTime dateTime);
        void UpdateCurrency(List<string> Users, DateTime dateTime);
        void UpdateFollowers(IEnumerable<Follow> follows);
        new List<LearnMsgRecord> UpdateLearnedMsgs();
        void UpdateOverlayTicker(OverlayTickerItem item, string name);
        void UpdateWatchTime(List<LiveUser> Users, DateTime CurrTime);
        void UpdateWatchTime(LiveUser User, DateTime CurrTime);
        void UserJoined(LiveUser User, DateTime NowSeen);
        void UserJoined(IEnumerable<LiveUser> Users, DateTime NowSeen);
        void UserLeft(LiveUser User, DateTime LastSeen);

    }
}