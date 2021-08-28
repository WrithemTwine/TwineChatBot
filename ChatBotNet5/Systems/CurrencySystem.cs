﻿using ChatBot_Net5.Data;
using ChatBot_Net5.Static;

using System;
using System.Collections.Generic;
using System.Threading;

namespace ChatBot_Net5.Systems
{
    public class CurrencySystem
    {
        private const int SecondsDelay = 5000;

        private bool CurAccrualStarted;
        private bool WatchStarted;

        private DataManager Datamanager { get; set; }
        private readonly List<string> CurrUsers;

        public CurrencySystem(DataManager dataManager, List<string> CurrUserList)
        {
            Datamanager = dataManager;
            CurrUsers = CurrUserList;
        }

        internal void StartClock()
        {
            if (!CurAccrualStarted)
            {
                CurAccrualStarted = true;
                new Thread(new ThreadStart(() =>
                {
                    while (OptionFlags.IsStreamOnline && OptionFlags.TwitchCurrencyStart && OptionFlags.ManageUsers)
                    {
                        foreach (string U in CurrUsers)
                        {
                            Datamanager.UpdateCurrency(U, DateTime.Now.ToLocalTime());
                        }
                        // randomly extend the time delay up to 2times as long
                        Thread.Sleep(SecondsDelay * (1 + (DateTime.Now.Second / 60)));
                    }
                    CurAccrualStarted = false;
                })).Start();
            }
        }
        
        internal void MonitorWatchTime()
        {
            if (!WatchStarted)
            {
                WatchStarted = true;
                new Thread(new ThreadStart(() =>
                {
                    // watch time accruing only works when stream is online <- i.e. watched!
                    while (OptionFlags.IsStreamOnline && OptionFlags.ManageUsers)
                    {
                        lock (CurrUsers)
                        {
                            foreach (string U in CurrUsers)
                            {
                                Datamanager.UpdateWatchTime(U, DateTime.Now.ToLocalTime());
                            }
                        }
                        // randomly extend the time delay up to 2times as long
                        Thread.Sleep(SecondsDelay * (1 + (DateTime.Now.Second / 60)));
                    }
                    WatchStarted = false;
                })).Start();
            }
        }
    }
}
