using ChatBot_Net5.BotClients;
using ChatBot_Net5.Data;
using ChatBot_Net5.Models;
using ChatBot_Net5.Static;
using ChatBot_Net5.Systems;

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

namespace ChatBot_Net5.BotIOController
{
    // see "BotController_*.cs for partial class implementation
    public partial class BotController
    {
        #region properties

        /// <summary>
        /// List of commands supported as codes within the text, replaced with an actual value.
        /// </summary>
        public MsgVarHelp CommandInfo { get; private set; } = new();

        /// <summary>
        /// Manages data storage with the data interface to the datagram xml.
        /// </summary>
        public DataManager DataManage { get; private set; } = new();
 
        /// <summary>
        /// Manages statistics as the chat bot runs.
        /// </summary>
        public StatisticsSystem Stats { get; private set; }

        #region Bot Services
        /// <summary>
        /// Collection of each attached chat bot.
        /// </summary>
        public Collection<IOModule> IOModuleList { get; private set; } = new();

        /// <summary>
        /// Specifically Twitch Lib chat bot.
        /// </summary>
        public TwitchBotChatClient TwitchIO { get; private set; }

        public TwitchBotFollowerSvc TwitchFollower { get; private set; }

        public TwitchBotLiveMonitorSvc TwitchLiveMonitor { get; private set; }

        public TwitchBotClipSvc TwitchClip { get; private set; }

        #endregion Bot Services

        #endregion properties

        /// <summary>
        /// Build a new BotController.
        /// </summary>
        public BotController()
        {
            TwitchIO = new();
            TwitchFollower = new();
            TwitchLiveMonitor = new();
            TwitchClip = new();

            IOModuleList.Add(TwitchIO);
            IOModuleList.Add(TwitchFollower);
            IOModuleList.Add(TwitchLiveMonitor);
            IOModuleList.Add(TwitchClip);

            TwitchIO.OnBotStarted += TwitchIO_OnBotStarted;
            TwitchIO.OnBotStopped += TwitchIO_OnBotStopped;
            TwitchFollower.OnBotStarted += TwitchFollower_OnBotStarted;
            TwitchFollower.OnBotStopped += TwitchFollower_OnBotStopped;
            TwitchLiveMonitor.OnBotStarted += TwitchLiveMonitor_OnBotStarted;
            TwitchLiveMonitor.OnBotStopped += TwitchLiveMonitor_OnBotStopped;
            TwitchClip.OnBotStarted += TwitchClip_OnBotStarted;
            TwitchClip.OnBotStopped += TwitchClip_OnBotStopped;

            Stats = new(DataManage);

            OptionFlags.SetSettings();
        }

        /// <summary>
        /// Set all of the attached bots into a stopped state.
        /// </summary>
        /// <returns>True when successful.</returns>
        public bool ExitAllBots()
        {
            foreach (IOModule i in IOModuleList)
            {
                i.ExitBot();
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
                            i.Send( (OptionFlags.MsgAddMe ? "/me ": "") + s);
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
            List<Enum.Bots> Names = new();

            foreach (IOModule a in IOModuleList)
            {
                Names.Add(a.BotClientName);
            }

            Names.Sort();

            return Names.ConvertAll((e) => e.ToString()).ToArray();
        }

        /// <summary>
        /// Stop bot operations, any sent messages, and save data
        /// </summary>
        public void ExitSave()
        {
            ExitAllBots();              // stop all the bot processes
            DataManage.SaveData();  // save data
        }
    }
}
