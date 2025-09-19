using StreamerBotLib.Models.Enums;
using StreamerBotLib.Models.Events;
using StreamerBotLib.Static;
using StreamerBotLib.Systems;

namespace StreamerBotLib.Models.Repeat
{
    internal abstract class RepeatCommandMode
    {
        internal event EventHandler<RepeatCommandFoundEventArgs> OnRepeatEventOccured;
        internal static double _dilutetime = 1.0;
        internal static double _olddilutetime = 1.0;
        private const int TaskDelay = 5000;

        internal static List<string> CategoryList { get; } = [];
        internal List<TimerCommand> RepeatCommands { get; }

        protected RepeatCommandMode()
        {
            LogWriter.DebugLog(".ctor_RepeatCommandMode", DebugLogTypes.RepeatCommandSystem, "Initializing repeat command list.");
            RepeatCommands = [];
        }

        virtual internal void AddCommands(IEnumerable<Tuple<string, int, List<string>>> commands, double dilutetime) { }

        virtual internal void NotifyRepeatManager_StreamOnline() { }

        virtual internal void UpdateDiluteTime(double diluteTime)
        {
            LogWriter.DebugLog("UpdateDiluteTime", DebugLogTypes.RepeatCommandSystem, $"Update the dilute time, old dilute time {_olddilutetime}, new dilute time {diluteTime}");
            _olddilutetime = _dilutetime;
            _dilutetime = diluteTime;
        }

        internal static void UpdateCategory(string CategoryName)
        {
            LogWriter.DebugLog("UpdateCategory", DebugLogTypes.RepeatCommandSystem, $"Updating the current category, to manage if the command should perform. Current category {CategoryName}");
            CategoryList.Clear();
            CategoryList.Add(LocalizedMsgSystem.GetVar(Msg.MsgAllCategory));
            CategoryList.Add(CategoryName);
        }

        protected void RepeatThread()
        {
            LogWriter.DebugLog("RepeatThread", DebugLogTypes.RepeatCommandSystem, "Starting the thread to check for when to repeat commands.");
            while (OptionFlags.ActiveToken && OptionFlags.RepeatTimerCommands
                   && (OptionFlags.RepeatWhenLive && OptionFlags.IsStreamOnline || !OptionFlags.RepeatWhenLive))
            {
                CheckRepeats();
                Task.Delay(TaskDelay).Wait();
            }
        }

        /// <summary>
        /// Check Repeat Commands if ready to run and emit the event.
        /// </summary>
        virtual internal void CheckRepeats() { }

        protected void RepeatEvent(string command, Platform platform)
        {
            LogWriter.DebugLog("RepeatEvent", DebugLogTypes.RepeatCommandSystem, $"Repeat command ready to perform, {command}.");
            OnRepeatEventOccured?.Invoke(this, new() { Command = command, platform = platform });
        }

    }
}
