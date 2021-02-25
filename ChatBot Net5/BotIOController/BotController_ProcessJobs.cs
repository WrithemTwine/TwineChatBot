using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ChatBot_Net5.BotIOController
{
    public sealed partial class BotController
    {
        #region Process Bot Operations
        private bool ProcessOps { get; set; }   // whether to process ops or not
        private bool ExitApp { get; set; } = false; // 

        private Queue<Task> Operations { get; set; } = new Queue<Task>();   // an ordered list, enqueue into one end, dequeue from other end
        private Thread SendThread;  // the thread for sending messages back to the monitored Twitch channel

        /// <summary>
        /// Initialize a thread to process sending messages back to Twitch.
        /// </summary>
        private void SetThread()
        {
            SendThread = new Thread(new ThreadStart(ProcMsgs));
        }

        private void ProcMsgs()
        {
            // until the ProcessOps is false to stop operations, only run until the operations queue is empty
            while (ProcessOps || Operations.Count > 0) 
            {
                Task temp = null;
                lock (Operations)
                {
                    if(Operations.Count>0)
                    {
                        temp = Operations.Dequeue(); // get a task from the queue
                    }
                }

                if (temp != null)
                {
                    temp.Start();   // begin, wait, and dispose the task; let it process
                    temp.Wait();
                    temp.Dispose();
                }

                Thread.Sleep(600); // sleep 600ms between messages, permits about 100 messages max in 60 seconds == 1 minute

                // pauses thread if user stops the bot, could be existing messages still need processing
                while(!ProcessOps && !ExitApp)  // sleep the thread if the app isn't ready to exit, otherwise this sleeper stops and will conclude this method
                {
                    Thread.Sleep(5000); // sleeps every 5 seconds, checks if to emerge back to processing messages
                }
            }
        }

        //private readonly BackgroundWorker bgworker = new BackgroundWorker();

        //private void Bgworker_DoWork(object sender, DoWorkEventArgs e)
        //{
        //    while (ProcessOps || Operations.Count > 0)
        //    {
        //        Task temp;
        //        lock (Operations)
        //        {
        //            temp = Operations.Dequeue();
        //        }
        //        temp.Start();
        //        Thread.Sleep(600); // sleep 600ms between messages, permits about 100 messages max in 60 seconds == 1 minute
        //    }

        //}

        //public void Dispose()
        //{
        //    bgworker.Dispose();
        //}



        #endregion Process Bot Operations

    }
}
