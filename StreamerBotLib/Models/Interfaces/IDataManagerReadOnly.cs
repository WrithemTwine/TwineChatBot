using StreamerBotLib.Models.Enums;

namespace StreamerBotLib.Models.Interfaces
{
    public interface IDataManagerReadOnly
    {
        event EventHandler UpdatedMonitoringChannels;
        bool GetCmdAnnounce(string CmdName);
        bool GetEventAnnounce(ChannelEventActions EventName);
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
        List<LearnMsgRecord> UpdateLearnedMsgs();
        List<string> GetTableFields(string TableName);
        List<string> GetTableNames();
        List<CategoryData> GetGameCategories();
        List<string> GetCurrencyNames();
        bool CheckFollower(string User);
        bool CheckUser(LiveUser User);
        bool CheckFollower(string User, DateTime ToDateTime);
        bool CheckUser(LiveUser User, DateTime ToDateTime);
        string GetUserId(LiveUser User);
        IEnumerable<string> GetKeys(string Table);
        int GetTimerCommandTime(string Cmd);
        bool CheckMultiChannelName(string UserName, Platform platform);
        List<string> GetMultiChannelIds(Platform platform);
        List<Tuple<WebhooksSource, Uri>> GetMultiWebHooks();
        bool CheckMultiStreamDate(string UserId, Platform platform, DateTime dateTime);
        LiveUser GetUser(string UserName);
        string GetCommandString();
        IEnumerable<string> GetCommandList(bool prefix = true);
    }
}
