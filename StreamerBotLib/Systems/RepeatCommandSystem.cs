using StreamerBotLib.Models.Enums;
using StreamerBotLib.Models.Repeat;
using StreamerBotLib.Static;

namespace StreamerBotLib.Systems
{
    public partial class ActionSystem
    {
        private static bool ElapsedThread;

        private readonly RepeatManager RepeatManager;

        public void ActivateRepeatTimers()
        {
            LogWriter.DebugLog("ActivateRepeatTimers", DebugLogTypes.SystemController, "Activating repeat timers.");
            StartElapsedTimerThread();
        }

        public void ResetRepeatTimerMode()
        {
            LogWriter.DebugLog("ResetRepeatTimerMode", DebugLogTypes.SystemController, "Resetting repeat timer mode.");
            StopElapsedTimerThread();
            StartElapsedTimerThread();
        }

        private void RepeatManager_OnRepeatCheckStopped(object sender, EventArgs e)
        { // in case the repeat manager stops without this side calling for stop, reset flag to allow start again
            ElapsedThread = false;
        }

        /// <summary>
        /// Call to register a command edit occurred.
        /// </summary>
        public void UpdateCommandsChanged()
        {
            LogWriter.DebugLog("UpdateCommandsChanged", DebugLogTypes.RepeatCommandSystem, "Updating changed commands.");
            RepeatManager.UpdateCommands();
        }

        public void UpdateCategory()
        {
            LogWriter.DebugLog("UpdateCategory", DebugLogTypes.RepeatCommandSystem, "Updating the current category.");
            RepeatManager.UpdateCategory(Category);
        }

        public void NotifyRepeatManager_StreamOnline()
        {
            LogWriter.DebugLog("NotifyRepeatManager_StreamOnline", DebugLogTypes.RepeatCommandSystem, "Sending notification to the repeat manager.");
            RepeatManager?.NotifyRepeatManager_StreamOnline();
        }

        /// <summary>
        /// Starts up the repeat timer thread.
        /// </summary>
        public void StartElapsedTimerThread()
        {
            // don't start another thread if the current is still active
            if (!ElapsedThread && ((OptionFlags.RepeatWhenLive && OptionFlags.IsStreamOnline) || !OptionFlags.RepeatWhenLive))
            {
                LogWriter.DebugLog("StartElapsedTimerThread", DebugLogTypes.RepeatCommandSystem, "Starting the repeat manager.");
                RepeatManager.Start();
                RepeatManager.OnRepeatEventOccured += ProcessCommands_OnRepeatEventOccured;

                ElapsedThread = true;
            }
        }

        /// <summary>
        /// Stops repeat commands thread; for bot shutdown purposes.
        /// </summary>
        public void StopElapsedTimerThread()
        {
            LogWriter.DebugLog("StopElapsedTimerThread", DebugLogTypes.RepeatCommandSystem, "Stopping the repeat manager.");
            RepeatManager.Stop();
            ElapsedThread = false;
        }
    }
}
