﻿using StreamerBotLib.Data;
using StreamerBotLib.Enums;

using System;
using System.Collections.Generic;
using System.Data;

namespace StreamerBotLib.Interfaces
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
        public string GetEventRowData(ChannelEventActions rowcriteria, out bool Enabled, out short Multi);
        public List<Tuple<bool, Uri>> GetWebhooks(WebhooksKind webhooks);
        bool TestInRaidData(string user, DateTime time, string viewers, string gamename);
        bool TestOutRaidData(string HostedChannel, DateTime dateTime);
        List<DataSource.LearnMsgsRow> UpdateLearnedMsgs();
        List<object> GetRowsDataColumn(DataTable dataTable, DataColumn dataColumn);
        DataRow[] GetRows(DataTable dataTable, string Filter = null, string Sort = null);
        DataRow GetRow(DataTable dataTable, string Filter = null, string Sort = null);
        List<string> GetTableFields(DataTable dataTable);
        List<string> GetTableFields(string TableName);
        List<string> GetTableNames();
        List<Tuple<string, string>> GetGameCategories();
        List<string> GetCurrencyNames();
        bool CheckFollower(string User);
        bool CheckUser(string User);
        bool CheckFollower(string User, DateTime ToDateTime);
        bool CheckUser(string User, DateTime ToDateTime);
        List<object> GetRowsDataColumn(string dataTable, string dataColumn);
    }
}
