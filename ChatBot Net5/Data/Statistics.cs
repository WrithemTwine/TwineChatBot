using System;
using System.Collections.Generic;

namespace ChatBot_Net5.Data
{
    public class Statistics
    {
        private List<string> CurrUsers = new List<string>();
        private bool _StreamOnline;
        private DataManager datamanager;

        public Statistics(DataManager dataManager)
        {
            datamanager = dataManager;
            _StreamOnline = false;
        }

        public void UserJoined(string User, DateTime CurrTime)
        {
            CurrUsers.Add(User);
            datamanager.UserJoined(User, CurrTime);
        }

        public void UserLeft(string User, DateTime CurrTime)
        {
            UpdateWatchTime(User);
            CurrUsers.Remove(User);
            // datamanager.UserLeft(User, CurrTime);
        }

        public void UpdateWatchTime(string User=null)
        {
            if (_StreamOnline)
            {
                DateTime curr = DateTime.Now;
                if (User != null) { }
                else
                {
                    foreach (string s in CurrUsers)
                    {
                        datamanager.UpdateWatchTime(s, curr);
                    }
                }
            }
        }

        public void StreamOnline()
        {
            _StreamOnline = true;
        }

        public void StreamOffline()
        {
            _StreamOnline = false;
        }
    }
}
