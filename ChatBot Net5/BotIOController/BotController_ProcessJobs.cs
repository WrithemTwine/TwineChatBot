﻿using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ChatBot_Net5.BotIOController
{
    public sealed partial class BotController
    {
        #region Process Bot Operations
        private bool ProcessOps { get; set; } = false;  // whether to process ops or not

        private Queue<Task> Operations { get; set; } = new Queue<Task>();   // an ordered list, enqueue into one end, dequeue from other end
        private Thread SendThread;  // the thread for sending messages back to the monitored Twitch channel

        /// <summary>
        /// Initialize a thread to process sending messages back to each chat bot and start the message processing thread.
        /// </summary>
        private void SetThread()
        {
            SendThread = new Thread(new ThreadStart(ProcMsgs));
            SendThread.Start();
        }

        /// <summary>
        /// Cycles through the 'Operations' queue and runs each task in order.
        /// </summary>
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
                    temp.Start();   // begin, wait, and dispose the task; let it process in sequence before the next message
                    temp.Wait();
                    temp.Dispose();
                }

                Thread.Sleep(600); // sleep 600ms between messages, permits about 100 messages max in 60 seconds == 1 minute
            }
        }

        #endregion Process Bot Operations

    }
}