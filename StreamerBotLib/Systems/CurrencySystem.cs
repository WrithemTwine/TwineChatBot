using StreamerBotLib.Models;
using StreamerBotLib.Static;

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Linq;

namespace StreamerBotLib.Systems
{
    public class CurrencySystem : SystemsBase
    {
        private bool CurAccrualStarted;
        private bool WatchStarted;

        public CurrencySystem()
        {
        }

        public void StartCurrencyClock()
        {
            if (!CurAccrualStarted)
            {
                CurAccrualStarted = true;

                try
                {
                    ThreadManager.CreateThreadStart(() =>
                    {
                        while (OptionFlags.IsStreamOnline && OptionFlags.TwitchCurrencyStart && OptionFlags.ManageUsers)
                        {
                            lock (CurrUsers)
                            {
                                DataManage.UpdateCurrency(new(from LiveUser U in CurrUsers
                                                              select U.UserName), DateTime.Now.ToLocalTime());
                            }
                            // randomly extend the time delay up to 2times as long
                            Thread.Sleep(SecondsDelay * (1 + (DateTime.Now.Second / 60)));
                        }
                        CurAccrualStarted = false;
                    });
                }
                catch (ThreadInterruptedException ex)
                {
                    LogWriter.LogException(ex, MethodBase.GetCurrentMethod().Name);
                }
            }
        }

        public void MonitorWatchTime()
        {
            if (!WatchStarted)
            {
                WatchStarted = true;
                try
                {
                    ThreadManager.CreateThreadStart(() =>
                    {
                        // watch time accruing only works when stream is online <- i.e. watched!
                        while (OptionFlags.IsStreamOnline && OptionFlags.ManageUsers)
                        {
                            lock (CurrUsers)
                            {
                                DataManage.UpdateWatchTime(new(from LiveUser U in CurrUsers
                                                               select U.UserName), DateTime.Now.ToLocalTime());
                            }
                            // randomly extend the time delay up to 2times as long
                            Thread.Sleep(SecondsDelay * (1 + (DateTime.Now.Second / 60)));
                        }
                        WatchStarted = false;
                    });
                }
                catch (ThreadInterruptedException ex)
                {
                    LogWriter.LogException(ex, MethodBase.GetCurrentMethod().Name);
                }
            }
        }
    }
}
