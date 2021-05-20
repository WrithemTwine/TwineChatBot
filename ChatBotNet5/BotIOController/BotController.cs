//#if DEBUG
//#define LOGGING
//#endif


using ChatBot_Net5.BotIOController.Models;
using ChatBot_Net5.Clients;
using ChatBot_Net5.Data;
using ChatBot_Net5.Models;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;

namespace ChatBot_Net5.BotIOController
{

    // see "BotController_Events.cs for partial class implementation
    public sealed partial class BotController
    {
        #region logging actions
#if LOGGING
        readonly StreamWriter _TraceLogWriter = new StreamWriter("LogCalledEvents.txt", true);
#else
        readonly StreamWriter _TraceLogWriter = null;
#endif
        #endregion logging actions

        #region properties

        /// <summary>
        /// List of commands supported as codes within the text, replaced with an actual value.
        /// </summary>
        public CommandCollection CommandInfo { get; private set; } = new();

        /// <summary>
        /// Manages data storage with the data interface to the datagram xml.
        /// </summary>
        public DataManager DataManage { get; private set; } = new();
 
        /// <summary>
        /// Manages statistics as the chat bot runs.
        /// </summary>
        public Statistics Stats { get; private set; }

        #region Bot Services
        /// <summary>
        /// Collection of each attached chat bot.
        /// </summary>
        public Collection<IOModule> IOModuleList { get; private set; } = new();

        /// <summary>
        /// Specifically Twitch Lib chat bot.
        /// </summary>
        public IOModuleTwitch TwitchIO { get; private set; }

        public IOModuleTwitch_FollowerSvc TwitchFollower { get; private set; }

        public IOModuleTwitch_LiveMonitorSvc TwitchLiveMonitor { get; private set; }

        #endregion Bot Services

        #endregion properties

        /// <summary>
        /// Build a new BotController.
        /// </summary>
        public BotController()
        {
#if LOGGING
            _TraceLogWriter.AutoFlush = true;
#endif

            TwitchIO = new();
            TwitchFollower = new();
            TwitchLiveMonitor = new();
            IOModuleList.Add(TwitchIO);
            IOModuleList.Add(TwitchFollower);
            IOModuleList.Add(TwitchLiveMonitor);

            TwitchIO.OnBotStarted += TwitchIO_OnBotStarted;
            TwitchIO.OnBotStopped += TwitchIO_OnBotStopped;
            TwitchFollower.OnBotStarted += TwitchFollower_OnBotStarted;
            TwitchLiveMonitor.OnBotStarted += TwitchLiveMonitor_OnBotStarted;

            Stats = new(DataManage);

            OptionFlags.SetSettings();
        }



        /// <summary>
        /// Set all of the attached bots into a stopped state.
        /// </summary>
        /// <returns>True when successful.</returns>
        public bool StopBot()
        {
            foreach (IOModule i in IOModuleList)
            {
                i.StopBot();
            }
            return true;
        }

        /// <summary>
        /// Send a message to the channel via the background worker.
        /// it queues the tasks to provide messages in order.
        /// </summary>
        /// <param name="s">The string message to send.</param>
        public void Send(string s)
        {
            lock (Operations)
            {
                Operations.Enqueue(
                    new Task(() =>
                    {
                        foreach (IOModule i in IOModuleList)
                        {
                            i.Send( (OptionFlags.AddMeMsg ? "/me ": "") + s);
                        }
                    }
                ));
            }
        }

        /// <summary>
        /// Retrieve the names of the bot APIs within this controller
        /// </summary>
        /// <returns>The string names array of the bots within this controller.</returns>
        public string[] GetProviderNames()
        {
#if LOGGING
            MethodBase b = MethodBase.GetCurrentMethod();
            _TraceLogWriter?.WriteLine(DateTime.Now.ToString() + " Method Name: " + b.Name);
#endif

            List<string> Names = new();
            
            foreach (IOModule a in IOModuleList)
            {
                Names.Add(a.ChatClientName);
            }

            Names.Sort();

            return Names.ToArray();
        }

        /// <summary>
        /// Stop bot operations, any sent messages, and save data
        /// </summary>
        public void ExitSave()
        {
#if LOGGING
            MethodBase b = MethodBase.GetCurrentMethod();
            _TraceLogWriter?.WriteLine(DateTime.Now.ToString() + " Method Name: " + b.Name);
#endif

            StopBot();              // stop all the bot processes
            DataManage.SaveData();  // save data

#if LOGGING
            _TraceLogWriter.Close();
#endif
        }
    }
}
