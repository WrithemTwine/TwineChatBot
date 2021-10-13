using ChatBot_Net5.BotClients;
using ChatBot_Net5.Events;
using ChatBot_Net5.Static;
using ChatBot_Net5.Systems;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using TwitchLib.Api.Helix.Models.Clips.GetClips;
using TwitchLib.Api.Helix.Models.Users.GetUserFollows;

namespace ChatBot_Net5.BotIOController
{
    public sealed partial class BotController
    {
        #region Process Bot Operations

        private const int SendMsgDelay = 750;
        // 600ms between messages, permits about 100 messages max in 60 seconds == 1 minute
        // 759ms between messages, permits about 80 messages max in 60 seconds == 1 minute

        public event EventHandler<DownloadedFollowersEventArgs> OnCompletedDownloadFollowers;
        public event EventHandler<ClipFoundEventArgs> OnClipFound;

        private Queue<Task> Operations { get; set; } = new();   // an ordered list, enqueue into one end, dequeue from other end
        private Thread SendThread;  // the thread for sending messages back to the monitored Twitch channel

        private List<Follow> Follows { get; set; }
        private List<Clip> ClipList { get; set; }

        private bool StartClips { get; set; }

        private void StartThreads()
        {
            StartProcMsgThread();
        }

        /// <summary>
        /// Initialize a thread to process sending messages back to each chat bot and start the message processing thread.
        /// </summary>
        private void StartProcMsgThread()
        {
            SendThread = new(new ThreadStart(BeginProcMsgs));
            SendThread.Start();
        }

        /// <summary>
        /// Cycles through the 'Operations' queue and runs each task in order.
        /// </summary>
        private void BeginProcMsgs()
        {
            // TODO: set option to stop messages immediately, and wait until started again to send them
            // until the ProcessOps is false to stop operations, only run until the operations queue is empty
            while (OptionFlags.ProcessOps || Operations.Count > 0)
            {
                Task temp = null;
                lock (Operations)
                {
                    if (Operations.Count > 0)
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

                Thread.Sleep(SendMsgDelay);
            }
        }

        /// <summary>
        /// Retrieve all followers for the channel and add to the datatable.
        /// </summary>
        private void BeginAddFollowers()
        {
            if (OptionFlags.ManageFollowers && OptionFlags.TwitchAddFollowersStart && TwitchFollower.IsStarted)
            {
                new Thread(new ThreadStart(ProcessFollows)).Start();
            }
        }

        /// <summary>
        /// Process all of the followers from the reviewing channel
        /// </summary>
        private void ProcessFollows()
        {
            string ChannelName = TwitchBots.TwitchChannelName;

            Follows = TwitchFollower.GetAllFollowersAsync().Result;

            //const int count = 200;

            //Follow[] FollowArray = new Follow[count];

            //int x = 0;

            //while (x < Follows.Count)
            //{
            //    Follows.CopyTo(x, FollowArray, 0, Math.Min(Follows.Count - x, count));
            //    OnCompletedDownloadFollowers?.Invoke(this, new() { ChannelName = ChannelName, FollowList = FollowArray });
            //    x += count;
            //}

            OnCompletedDownloadFollowers?.Invoke(this, new() { ChannelName = ChannelName, FollowList = Follows });

        }

        private void BeginAddClips()
        {
            new Thread(new ThreadStart(ProcessClips)).Start();
        }

        private void ProcessClips()
        {
            StartClips = true;
            ClipList = TwitchClip.GetAllClipsAsync().Result;

            OnClipFound?.Invoke(this, new() { ClipList = ClipList });
            StartClips = false;
        }

        #endregion Process Bot Operations

    }
}
