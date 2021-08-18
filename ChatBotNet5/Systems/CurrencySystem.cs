using ChatBot_Net5.Data;
using ChatBot_Net5.Static;

using System;
using System.Collections.Generic;
using System.Threading;

namespace ChatBot_Net5.Systems
{
    public class CurrencySystem
    {
        private const int SecondsDelay = 5000;

        private bool WatchStarted = false;
        private bool CurAccrualStarted = false;

        private DataManager Datamanager { get; set; }

        public CurrencySystem(DataManager dataManager)
        {
            Datamanager = dataManager;
        }

        public void StartClock()
        {
            if (!WatchStarted)
            {
                WatchStarted = true;
                new Thread(new ThreadStart(() =>
                {
                    // watch time accruing only works when stream is online <- i.e. watched!
                    while (OptionFlags.IsStreamOnline)
                    {
                        Datamanager.UpdateWatchTime(DateTime.Now.ToLocalTime());
                        // randomly extend the time delay up to 2times as long
                        Thread.Sleep(SecondsDelay * (1 + (DateTime.Now.Second / 60)));
                    }
                    WatchStarted = false;
                })).Start();
            }

            if (!CurAccrualStarted)
            {
                CurAccrualStarted = true;
                new Thread(new ThreadStart(() =>
                {
                    while ((OptionFlags.IsStreamOnline && OptionFlags.TwitchCurrencyOnline) || !OptionFlags.TwitchCurrencyOnline)
                    {
                        Datamanager.UpdateCurrency(DateTime.Now.ToLocalTime());
                    // randomly extend the time delay up to 2times as long
                    Thread.Sleep(SecondsDelay * (1 + (DateTime.Now.Second / 60)));
                    }
                    CurAccrualStarted = false;
                })).Start();
            }

        }
    }
}
