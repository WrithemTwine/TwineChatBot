
using ChatBot_Net5.BotIOController.Models;
using ChatBot_Net5.Clients;
using ChatBot_Net5.Data;
using ChatBot_Net5.Models;

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Documents;

using TwitchLib.Client.Models;

namespace ChatBot_Net5.BotIOController
{
    // see "BotController_Events.cs for partial class implementation
    public sealed partial class BotController
    {
        #region properties

        public CommandCollection CommandInfo { get; private set; } = new CommandCollection();
        public DataManager DataManage { get; private set; } = new DataManager();
 
        public Statistics Stats { get; private set; }

        #region Bot Services
        public Collection<IOModule> IOModuleList { get; private set; } = new Collection<IOModule>();
        public IOModuleTwitch TwitchIO { get; private set; }
        #endregion Bot Services

        #endregion properties

        public BotController()
        {
            ProcessOps = false;

            SetThread();
            TwitchIO = new IOModuleTwitch();
            IOModuleList.Add(TwitchIO);

            Stats = new Statistics(DataManage);
        }


        public bool StartBot()
        {
            ProcessOps = true; // required as true to spin the "SendThread" while loop, so it doesn't conclude early

            if (SendThread.ThreadState == System.Threading.ThreadState.Unstarted) // start only an unstarted thread, don't attempt to restart the thread - it's sleeping
            {
                SendThread.Start();
            }

            foreach (IOModule i in IOModuleList)
            {
                i.Connect();

                // perform loading steps
                RegisterHandlers();

                i.StartBot();
            }

            return true;
        }


        public bool StopBot()
        {
            ProcessOps = false;

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
                            i.Send(s);
                        }
                    }
                ));
            }
        }

        public string[] GetProviderNames()
        {
            List<string> Names = new List<string>();
            
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
            ProcessOps = false;     // stop processing operations
            ExitApp = true;         // will cause the sleeping thread to drop out of action process
            SendThread.Join();      // wait for thread to finish processing messages
            StopBot();              // stop all the bot processes
            DataManage.ExitSave();  // save data
        }
    }
}
