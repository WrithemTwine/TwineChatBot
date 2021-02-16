
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

        public CommandCollection CommandInfo { get; set; } = new CommandCollection();
        public DataManager DataManage { get; private set; } = new DataManager();


        #region User Join
        public ObservableCollection<UserJoin> JoinCollection { get; set; } = new ObservableCollection<UserJoin>();

        #endregion User Join

        #region ChatBot Module List
        public Collection<IOModule> IOModuleList { get; private set; } = new Collection<IOModule>();
        public IOModuleTwitch TwitchIO { get; private set; }
        #endregion ChatBot Module List

        #region Chatbox data
        
        private void AddChatString(ChatMessage s)
        {
            Paragraph p = new Paragraph();
            p.ElementStart.InsertTextInRun(s.Message);

            ChatData.Blocks.Add(p);
        }

        public FlowDocument ChatData { get; private set; } = new FlowDocument();

        #endregion Chatbox data

        #endregion properties

        public BotController()
        {
            ProcessOps = false;

            SetThread();
            TwitchIO = new IOModuleTwitch();
            IOModuleList.Add(TwitchIO);
        }

        public bool StartBot()
        {
            ProcessOps = true;

            if (SendThread.ThreadState == System.Threading.ThreadState.Stopped || SendThread.ThreadState == System.Threading.ThreadState.Unstarted)
            {
                SendThread.Start();
            }

            foreach (IOModule i in IOModuleList)
            {
                i.StartBot();
                if (i.GetType() == typeof(IOModuleTwitch))
                {
                    DataManage.UpdateFollowers(TwitchIO.ChannelName, TwitchIO.FollowerService.KnownFollowers);
                }
            }
            
            // perform loading steps
            RegisterHandlers();
            LoadCommands();

            return true;
        }

        public bool StopBot()
        {
            ProcessOps = false;

            //while (bgworker.IsBusy) { }
            SendThread.Join();

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

        public void ExitSave()
        {
            DataManage.ExitSave();
            TwitchIO.StopBot();
        }
    }
}
