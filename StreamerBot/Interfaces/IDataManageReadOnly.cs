using StreamerBot.Data;
using StreamerBot.Enums;

using System;
using System.Collections.Generic;

namespace StreamerBot.Interfaces
{
    public interface IDataManageReadOnly
    {
        public bool CheckTable(string table);
        public bool CheckField(string table, string field);
        public bool CheckPermission(string cmd, ViewerTypes permission);
        public bool CheckShoutName(string UserName);
        public string GetKey(string Table);
        public string GetSocials();
        public string GetUsage(string command);
        public DataSource.CommandsRow GetCommand(string cmd);
        public List<Tuple<string, int, string[]>> GetTimerCommands();
        public Tuple<string, int, string[]> GetTimerCommand(string Cmd);
        public object GetRowData(DataRetrieve dataRetrieve, ChannelEventActions rowcriteria);
        public List<Tuple<bool, Uri>> GetWebhooks(WebhooksKind webhooks);
    }
}
