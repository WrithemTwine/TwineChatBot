using StreamerBotLib.Models;
using StreamerBotLib.Models.Enums;
using StreamerBotLib.Models.Events;

using System.Windows.Threading;

namespace StreamerBotLib.Static
{
    public static class ThreadManager
    {
        private static readonly List<ThreadData> CurrThreads = [];
        private static int ClosedThreads;

        private static readonly List<int> PriorityList = [];
        private static readonly int PriorityGap = 20000;
        private static readonly Dictionary<ThreadExitPriority, int> PrioritySchedule = new()
        {
            [ThreadExitPriority.VeryHigh] = 0,
            [ThreadExitPriority.High] = 20000,
            [ThreadExitPriority.Normal] = 40000,
            [ThreadExitPriority.Low] = 60000,
            [ThreadExitPriority.VeryLow] = 80000
        };

        private static readonly Thread TrackThread;

        private static Dispatcher GUIDispatcher;

        /// <summary>
        /// Provides how many Threads this application creates during execution.
        /// </summary>
        public static event EventHandler<ThreadManagerCountArg> OnThreadCountUpdate;

        static ThreadManager()
        {
            ClosedThreads = 0;

            TrackThread = new Thread(new ThreadStart(ThreadManagerBegin)) { IsBackground = true };
            TrackThread.Start();
        }

        /// <summary>
        /// Provide the Application GUI Dispatcher to permit different code areas to 
        /// modify GUI thread objects.
        /// </summary>
        /// <param name="dispatcher">The main GUI Dispatcher</param>
        public static void SetGUIDispatcher(Dispatcher dispatcher)
        {
            GUIDispatcher = dispatcher;
        }

        /// <summary>
        /// Add a task on the GUI dispatcher and use "BeginInvoke". Does not maintain ongoing threads
        /// outside of the task.
        /// Use <see cref="CreateThread"/> or <see cref="CreateThreadStart"/> for ongoing threads
        /// and to specify priority for the thread stoping order.
        /// </summary>
        /// <param name="task">The task to add to the GUI dispatcher.</param>
        public static void AddTaskToGUIDispatcher(Action action)
        {
            GUIDispatcher.Invoke(() => action.Invoke());
        }

        /// <summary>
        /// Add a task on the GUI dispatcher and use "BeginInvoke". Does not maintain ongoing threads
        /// outside of the task.
        /// Use <see cref="CreateThread"/> or <see cref="CreateThreadStart"/> for ongoing threads
        /// and to specify priority for the thread stoping order.
        /// </summary>
        /// <param name="task">The task to add to the GUI dispatcher.</param>
        public static async Task AddTaskToGUIDispatcher(Task task)
        {
            try
            {
                await GUIDispatcher.BeginInvoke(task.RunSynchronously);
            }
            catch (Exception e)
            {
                LogWriter.LogException(e, "AddTaskToGUIDispatcher");
            }
        }
        public static void AddAsyncTaskToGUIDispatcher(string CallMethodName, Action action)
        {
            LogWriter.DebugLog(CallMethodName, DebugLogTypes.ThreadManager, $"ThreadManager called the GUI Dispatcher's Invoke, to start soon, on behalf of {CallMethodName}.");

            GUIDispatcher.Invoke(() => action.Invoke());
        }

        /// <summary>
        /// Perform and post the event.
        /// </summary>
        private static void PostUpdatedCount()
        {
            OnThreadCountUpdate?.Invoke(null, new() { AllThreadCount = CurrThreads.Count + ClosedThreads, ClosedThreadCount = ClosedThreads });
        }

        /// <summary>
        /// Thread for managing the Thread List, removing stopped threads.
        /// </summary>
        private static void ThreadManagerBegin()
        {
            bool Changed = false;

            while (OptionFlags.ActiveToken)
            {
                lock (CurrThreads)
                {
                    List<ThreadData> ToRemove = [];

                    foreach (ThreadData t in CurrThreads.Where(t => t.ThreadItem.ThreadState == ThreadState.Stopped))
                    {
                        ClosedThreads++;
                        ToRemove.Add(t);
                        Changed = true;

                        LogWriter.DebugLog(t.CallingSource, DebugLogTypes.ThreadManager, $"ThreadManager thread for {t.CallingSource} now closed/ended.");
                    }

                    foreach (ThreadData R in ToRemove)
                    {
                        CurrThreads.Remove(R);
                        PriorityList.Remove(R.ThreadPriority);
                    }
                }
                if (Changed)
                {
                    PostUpdatedCount();
                }

                Changed = false;
                Thread.Sleep(10);
            }

            Exit();
        }

        /// <summary>
        /// Creates a Thread with the provided Action, and maintained parameters to relativize the threads to other threads.
        /// </summary>
        /// <param name="action">The action to perform in the thread.</param>
        /// <param name="waitState">Whether to "Wait" or "Close" the Thread when application is exiting.</param>
        /// <param name="Priority">The relative order of the Thread priority, 1-Highest Priority, 2+ in descending priority; 0 is neutral priority. The Highest Priority threads are waited on first when exiting.</param>
        /// <returns>The created Thread.</returns>
        public static Thread CreateThread(string CallMethodName, Action action, ThreadWaitStates waitState = ThreadWaitStates.Close, ThreadExitPriority Priority = ThreadExitPriority.Normal)
        {
            ThreadData threadData = CreateThreadData(CallMethodName, action, waitState, Priority);
            LogWriter.DebugLog(CallMethodName, DebugLogTypes.ThreadManager, $"ThreadManager generating a thread for {CallMethodName}, to start soon.");
            return threadData.ThreadItem;
        }

        /// <summary>
        /// Helper method to use the parameters and create a ThreadData object, and adds to the internal collection.
        /// </summary>
        /// <param name="action">The action to perform in the thread.</param>
        /// <param name="waitState">Whether to "Wait" or "Close" the Thread when application is exiting.</param>
        /// <param name="Priority">The relative order of the Thread priority, 1-Highest Priority, 2+ in descending priority; 0 is neutral priority. The Highest Priority threads are waited on first when exiting.</param>
        /// <returns>A new ThreadData object data bundle.</returns>
        private static ThreadData CreateThreadData(string CallMethodName, Action action, ThreadWaitStates waitState = ThreadWaitStates.Close, ThreadExitPriority Priority = ThreadExitPriority.Normal)
        {
            ThreadData threadData = new() { ThreadItem = new Thread(new ThreadStart(action)), CloseState = waitState, ThreadPriority = GetThreadPriority(Priority), CallingSource = CallMethodName };
            ThreadAdd(threadData);
            return threadData;
        }

        /// <summary>
        /// Creates a Thread with the provided Action, and maintained parameters to relativize the threads to other threads; and starts the Thread execution.
        /// </summary>
        /// <param name="action">The action to perform in the thread.</param>
        /// <param name="waitState">Whether to "Wait" or "Close" the Thread when application is exiting.</param>
        /// <param name="Priority">The relative order of the Thread priority, 1-Highest Priority, 2+ in descending priority; 0 is neutral priority. The Highest Priority threads are waited on first when exiting.</param>
        public static void CreateThreadStart(string CallMethodName, Action action, ThreadWaitStates waitState = ThreadWaitStates.Close, ThreadExitPriority Priority = ThreadExitPriority.Normal)
        {
            ThreadData threadData = CreateThreadData(CallMethodName, action, waitState, Priority);
            LogWriter.DebugLog(CallMethodName, DebugLogTypes.ThreadManager, $"ThreadManager starting a thread for {CallMethodName}.");
            threadData.ThreadItem.Start();
        }

        //private static ThreadData CreateThreadData(Task task, ThreadWaitStates waitState = ThreadWaitStates.Close, ThreadExitPriority Priority = ThreadExitPriority.Normal)
        //{
        //    return CreateThreadData("CreateThreadData", () => task.Start(), waitState, Priority);
        //}

        /// <summary>
        /// Creates a Thread with the provided task, and maintained parameters to
        /// relativize the threads to other threads; and starts the Thread execution.
        /// </summary>
        /// <param name="task">The task to perform in the thread.</param>
        /// <param name="waitState">Whether to "Wait" or "Close" the Thread when 
        /// application is exiting.</param>
        /// <param name="Priority">The relative order of the Thread priority, 
        /// 1-Highest Priority, 2+ in descending priority; 0 is neutral priority. The 
        /// Highest Priority threads are waited on first when exiting.</param>
        public static void CreateThreadStart(Task task, ThreadWaitStates waitState = ThreadWaitStates.Close, ThreadExitPriority Priority = ThreadExitPriority.Normal)
        {
            CreateThreadStart("CreateThreadStart", () => task.Start(), waitState, Priority);
        }

        private static int GetThreadPriority(ThreadExitPriority threadExitPriority)
        {
            //    VeryHigh, High, Normal, Low, VeryLow

            int floor = PrioritySchedule[threadExitPriority];

            int idx = floor;

            while (!PriorityList.Contains(idx) && idx < floor + PriorityGap)
            {
                idx++;
            }

            PriorityList.Add(idx);

            return idx;
        }

        /// <summary>
        /// Insert a new Thread into the CurrThreads list based on ThreadData.ThreadPriority, as the closing process relies on threads concluding before other threads conclude.
        /// </summary>
        /// <param name="threadData"></param>
        private static void ThreadAdd(ThreadData threadData)
        {
            lock (CurrThreads)
            {
                if (threadData.ThreadPriority == 0)
                {
                    CurrThreads.Add(threadData);
                }
                else
                {
                    int add = CurrThreads.FindIndex(new((i) => i.ThreadPriority > threadData.ThreadPriority));

                    if (add == -1)
                    {
                        CurrThreads.Add(threadData);
                    }
                    else
                    {
                        CurrThreads.Insert(add, threadData);
                    }
                }
            }

            PostUpdatedCount();
        }
        private static void Exit()
        {
            lock (CurrThreads)
            {
                foreach (ThreadData t in CurrThreads)
                {
                    if ((t.ThreadItem.ThreadState & (ThreadState.Stopped | ThreadState.Unstarted)) == 0)
                    {
                        if (t.CloseState == ThreadWaitStates.Wait)
                        {
                            t.ThreadItem.Join();
                            LogWriter.DebugLog(t.CallingSource, DebugLogTypes.ThreadManager, $"ThreadManager Exit joined with {t.CallingSource} thread.");
                        }
                        else if (t.CloseState == ThreadWaitStates.Close)
                        {
                            LogWriter.DebugLog(t.CallingSource, DebugLogTypes.ThreadManager, $"ThreadManager already closed {t.CallingSource} thread.");
                            // t.ThreadItem.Interrupt();
                        }
                    }
                }
            }
            LogWriter.DebugLog("Exit", DebugLogTypes.ThreadManager, "Closed all threads started through ThreadManager.");
        }
    }
}
