using StreamerBotLib.Models.Enums;
using StreamerBotLib.Static;

namespace StreamerBotLib.Models.Repeat
{
    internal class RepeatSerialMode : RepeatCommandMode
    {
        private int _currIdx = 0;

        private DateTime _NextRun;
        private DateTime _LastRun;

        private int _repeatSeconds;
        private TimeSpan _Interval;

        internal RepeatSerialMode() : base()
        {
            LogWriter.DebugLog(".ctor_RepeatSerialMode", DebugLogTypes.RepeatCommandSystem, "Build the sequential repeat mode object.");

            _repeatSeconds = OptionFlags.RepeatSerialTime;
            _Interval = new TimeSpan(0, 0, _repeatSeconds);
            _LastRun = DateTime.Now;
            _NextRun = _LastRun.Add(_Interval.Multiply(_dilutetime));

            ThreadManager.CreateThreadStart(".ctor_RepeatSerialMode", () => RepeatThread());
        }

        internal override void NotifyRepeatManager_StreamOnline()
        {
            if (OptionFlags.RepeatLiveReset)
            {
                LogWriter.DebugLog("NotifyRepeatManager_StreamOnline", DebugLogTypes.RepeatCommandSystem, "Recieved notice new stream is online, and resetting the repeat timer from now.");
                _NextRun = DateTime.Now.Add(_Interval.Multiply(_dilutetime));
            }
        }

        internal override void AddCommands(IEnumerable<Tuple<string, int, List<string>>> commands, double dilutetime)
        {
            LogWriter.DebugLog("AddCommands", DebugLogTypes.RepeatCommandSystem, $"Adjusting commands to the serial mode command list: {commands.Count()}, dilute time {dilutetime} ");
            base.UpdateDiluteTime(dilutetime);

            lock (RepeatCommands)
            {
                RepeatCommands.Clear();
                RepeatCommands.AddRange(OptionFlags.RepeatSerialSaveDataString.Cast<string>().Select(s => new TimerCommand(new(s, 0, ["All"]), dilutetime)));

                // reset index if the repeating list is shorter after the update, so it doesn't give index out of range
                if (_currIdx > RepeatCommands.Count)
                {
                    _currIdx = 0;
                }
            }
        }

        internal override void CheckRepeats()
        {
            LogWriter.DebugLog("CheckRepeats", DebugLogTypes.RepeatCommandSystem, "Reviewing if it's time for the sequential repeat command to activate.");
            lock (RepeatCommands)
            {
                // check if the user changed the repeat time, update the running times
                if (_repeatSeconds != OptionFlags.RepeatSerialTime)
                {
                    LogWriter.DebugLog("CheckRepeats", DebugLogTypes.RepeatCommandSystem, "The sequential repeat timeframe interval changed. Updating interval.");
                    _repeatSeconds = OptionFlags.RepeatSerialTime;
                    _Interval = new TimeSpan(0, 0, _repeatSeconds);
                }

                // adjust the next run if the repeat seconds or dilutetime changed - might cache this when dilutetime & repeat seconds remain unchanged, had unusual results during testing (no updated dilute time for smart-slowdown mode)
                _NextRun = _LastRun.Add(_Interval.Multiply(_dilutetime));
                if (DateTime.Now > _NextRun)
                {
                    LogWriter.DebugLog("CheckRepeats", DebugLogTypes.RepeatCommandSystem, $"Found it's time to run the next command, {RepeatCommands[_currIdx]}. Updating for the next run.");

                    // send the activated command
                    RepeatEvent(RepeatCommands[_currIdx].Command, Platform.Twitch);

                    // record the just activated command time as the last run to occur.
                    _LastRun = _NextRun;

                    // increment the index within the repeat command list
                    _currIdx = (_currIdx + 1) % RepeatCommands.Count;
                }
            }
        }
    }
}
