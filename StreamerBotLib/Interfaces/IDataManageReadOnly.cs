using StreamerBotLib.Enums;
using StreamerBotLib.Models;

using System;
using System.Collections.Generic;

namespace StreamerBotLib.Interfaces
{
    public interface IDataManageReadOnly
    {
        public bool CheckField(string table, string field);
        public bool CheckPermission(string cmd, ViewerTypes permission);
        public bool CheckShoutName(string UserName);
        public string GetKey(string Table);
        public string GetSocials();
        public string GetUsage(string command);
        public CommandData GetCommand(string cmd);
        public List<Tuple<string, int, string[]>> GetTimerCommands();
        public Tuple<string, int, string[]> GetTimerCommand(string Cmd);
        public string GetEventRowData(ChannelEventActions rowcriteria, out bool Enabled, out short Multi);
        public List<Tuple<bool, Uri>> GetWebhooks(WebhooksKind webhooks);
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
        List<object> GetRowsDataColumn(string dataTable, string dataColumn);
        string GetUserId(LiveUser User);
        IEnumerable<string> GetKeys(string Table);
        IEnumerable<string> GetCommandList();
        string GetCommands();

    //  public bool CheckTable(string table);
    //  List<object> GetRowsDataColumn(DataTable dataTable, DataColumn dataColumn);
    //  DataRow[] GetRows(DataTable dataTable, string Filter = null, string Sort = null);
    //  DataRow GetRow(DataTable dataTable, string Filter = null, string Sort = null);
    //  List<string> GetTableFields(DataTable dataTable);

    }
}
