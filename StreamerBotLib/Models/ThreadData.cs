using StreamerBotLib.Enums;

using System.Threading;

namespace StreamerBotLib.Models
{
    public class ThreadData
    {
        /// <summary>
        /// The current managed thread.
        /// </summary>
        public Thread ThreadItem;

        /// <summary>
        /// When stopping the Thread, determine if thread should be aborted or waited.
        /// </summary>
        public ThreadWaitStates CloseState;

        /// <summary>
        /// Priority for ordering the threads; 1-5 is high-priority; 100-120 is low priority; 0 is neutral priority.
        /// </summary>
        public int ThreadPriority;
    }
}
