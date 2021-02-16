using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ChatBot_Net5.BotIOController
{
    public sealed partial class BotController
    {
        #region Process Bot Operations
        private bool ProcessOps { get; set; }

        private Queue<Task> Operations { get; set; } = new Queue<Task>();
        private Thread SendThread;

        private void SetThread()
        {
            SendThread = new Thread(new ThreadStart(ProcMsgs));
            SendThread.SetApartmentState(ApartmentState.STA);
        }

        private void ProcMsgs()
        {
            while (ProcessOps || Operations.Count > 0)
            {
                Task temp = null;
                lock (Operations)
                {
                    if(Operations.Count>0)
                    {
                        temp = Operations.Dequeue(); 

                    }
                }

                if (temp != null)
                {
                    temp.Start();
                    temp.Wait();
                    temp.Dispose();
                }

                Thread.Sleep(600); // sleep 600ms between messages, permits about 100 messages max in 60 seconds == 1 minute
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
