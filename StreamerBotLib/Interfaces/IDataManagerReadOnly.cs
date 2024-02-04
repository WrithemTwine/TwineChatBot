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
        bool CheckField(string table, string field);
        bool CheckPermission(string cmd, ViewerTypes permission);
        bool CheckShoutName(string UserName);
        string GetKey(string Table);
        string GetSocials();
        string GetUsage(string command);
        CommandData GetCommand(string cmd);
        Tuple<string, int, List<string>> GetTimerCommand(string Cmd);
        List<Tuple<string, int, List<string>>> GetTimerCommands();
        string GetEventRowData(ChannelEventActions rowcriteria, out bool Enabled, out short Multi);
        List<Tuple<bool, Uri>> GetWebhooks(WebhooksSource webhooksSource, WebhooksKind webhooks);
        bool TestInRaidData(string user, DateTime time, string viewers, string gamename);
        bool TestOutRaidData(string HostedChannel, DateTime dateTime);
        List<LearnMsgRecord> UpdateLearnedMsgs();
        List<string> GetTableFields(string TableName);
        List<string> GetTableNames();
        List<Tuple<string, string>> GetGameCategories();
        List<string> GetCurrencyNames();
        bool CheckFollower(string User);
        bool CheckUser(LiveUser User);
        bool CheckFollower(string User, DateTime ToDateTime);
        bool CheckUser(LiveUser User, DateTime ToDateTime);
        string GetUserId(LiveUser User);
        IEnumerable<string> GetKeys(string Table);
        IEnumerable<string> GetCommandList();
        string GetCommands();
        int GetTimerCommandTime(string Cmd);
        bool GetMultiChannelName(string UserName, Platform platform);
        List<string> GetMultiChannelNames(Platform platform);
        List<Tuple<WebhooksSource, Uri>> GetMultiWebHooks();
        bool CheckMultiStreamDate(string ChannelName, Platform platform, DateTime dateTime);
        LiveUser GetUser(string UserName);
    }
}
