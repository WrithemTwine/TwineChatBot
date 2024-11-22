using StreamerBotLib.DataSQL;
using StreamerBotLib.Enums;
using StreamerBotLib.Models;

using System.Collections.ObjectModel;

namespace StreamerBotLib.Interfaces
{
    public interface IDataManagerReadOnly
    {
        event EventHandler UpdatedMonitoringChannels;
        ObservableCollection<ArchiveMultiStream> CleanupList { get; }
        string MultiLiveStatusLog { get; }
        bool CheckField(string table, string field, SQLDBContext Refcontext = null);
        bool CheckPermission(string cmd, ViewerTypes permission, SQLDBContext Refcontext = null);
        bool CheckShoutName(string UserName, SQLDBContext Refcontext = null);
        string GetKey(string Table, SQLDBContext Refcontext = null);
        string GetSocials(SQLDBContext Refcontext = null);
        string GetUsage(string command, SQLDBContext Refcontext = null);
        CommandData GetCommand(string cmd, SQLDBContext Refcontext = null);
        Tuple<string, int, List<string>> GetTimerCommand(string Cmd, SQLDBContext Refcontext = null);
        List<Tuple<string, int, List<string>>> GetTimerCommands(SQLDBContext Refcontext = null);
        string GetEventRowData(ChannelEventActions rowcriteria, out bool Enabled, out short Multi, SQLDBContext Refcontext = null);
        List<Tuple<bool, Uri>> GetWebhooks(WebhooksSource webhooksSource, WebhooksKind webhooks, SQLDBContext Refcontext = null);
        List<LearnMsgRecord> UpdateLearnedMsgs(SQLDBContext Refcontext = null);
        List<string> GetTableFields(string TableName, SQLDBContext Refcontext = null);
        List<string> GetTableNames(SQLDBContext Refcontext = null);
        List<CategoryData> GetGameCategories(SQLDBContext Refcontext = null);
        List<string> GetCurrencyNames(SQLDBContext Refcontext = null);
        bool CheckFollower(string User, SQLDBContext Refcontext = null);
        bool CheckUser(LiveUser User, SQLDBContext Refcontext = null);
        bool CheckFollower(string User, DateTime ToDateTime, SQLDBContext Refcontext = null);
        bool CheckUser(LiveUser User, DateTime ToDateTime, SQLDBContext Refcontext = null);
        string GetUserId(LiveUser User, SQLDBContext Refcontext = null);
        IEnumerable<string> GetKeys(string Table, SQLDBContext Refcontext = null);
        int GetTimerCommandTime(string Cmd, SQLDBContext Refcontext = null);
        bool CheckMultiChannelName(string UserName, Platform platform, SQLDBContext Refcontext = null);
        List<string> GetMultiChannelIds(Platform platform, SQLDBContext Refcontext = null);
        List<Tuple<WebhooksSource, Uri>> GetMultiWebHooks(SQLDBContext Refcontext = null);
        bool CheckMultiStreamDate(string UserId, Platform platform, DateTime dateTime, SQLDBContext Refcontext = null);
        LiveUser GetUser(string UserName, SQLDBContext Refcontext = null);
        string GetCommandString(SQLDBContext Refcontext = null);
        IEnumerable<string> GetCommandList(bool prefix = true, SQLDBContext Refcontext = null);
        ObservableCollection<ArchiveMultiStream> GetCleanupList();
    }
}
