using System;
using System.Collections.Generic;

namespace ChatBot_Net5.Data
{
    public class Statistics
    {
        private List<string> CurrUsers = new List<string>();
        private bool _StreamOnline;
        private DataManager datamanager;
        internal DateTime defaultDate = DateTime.Parse("01/01/1900");


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
            datamanager.UserLeft(User, CurrTime);
            CurrUsers.Remove(User);
        }

        /// <summary>
        /// Default to all users or a specific user to register "DateTime.Now" as the current watch date.
        /// </summary>
        /// <param name="User">User to update "Now" or null to update all users watch time.</param>
        public void UpdateWatchTime(string User=null)
        {
            if (_StreamOnline)
            {
                UpdateWatchTime(User, DateTime.Now);
            }
        }

        public void UpdateWatchTime(string User, DateTime Seen)
        {
            if (_StreamOnline)
            {
                if (User != null) 
                {
                    datamanager.UpdateWatchTime(User, Seen);
                }
                else
                {
                    foreach (string s in CurrUsers)
                    {
                        datamanager.UpdateWatchTime(s, Seen);
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
            UpdateWatchTime();
            _StreamOnline = false;
        }
    }
}
