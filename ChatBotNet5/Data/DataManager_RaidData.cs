using System;

namespace ChatBot_Net5.Data
{
    partial class DataManager
    {
        public void AddRaidData(string user, DateTime time, string viewers, string gamename)
        {
            _ = _DataSource.RaidData.AddRaidDataRow(user, viewers, time, gamename);
            NotifySaveData();
        }

        public void RemoveAllRaidData()
        {
            _DataSource.RaidData.Clear();
        }
    }
}
