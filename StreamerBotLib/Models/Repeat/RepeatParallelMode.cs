using StreamerBotLib.Static;

namespace StreamerBotLib.Models.Repeat
{
    internal class RepeatParallelMode : RepeatCommandMode
    {

        internal RepeatParallelMode(double diluteTime = 1)
        {
            _dilutetime = diluteTime;
        }

        internal override void AddCommands(IEnumerable<Tuple<string, int, List<string>>> commands, double dilutetime)
        {
            LogWriter.DebugLog("AddCommands", Enums.DebugLogTypes.RepeatCommandSystem, "Updating any changed commands.");

            _dilutetime = dilutetime;

            lock (RepeatCommands)
            {
                // remove commands no longer in the list, maintain timing of other commands
                var Remove = RepeatCommands.Where(r => !commands.Where(c => c.Item1 == r.Command).Any()).Select(r => r);
                LogWriter.DebugLog("AddCommands", Enums.DebugLogTypes.RepeatCommandSystem, $"Removing {Remove.Count()} commands.");
                RepeatCommands.RemoveAll(r => Remove.Contains(r));
                // add new commands newly added, not already in the repeat command list
                var Add = commands.SkipWhile(c => RepeatCommands.Where(r => r.Command == c.Item1).Any()).Select(c => c);
                LogWriter.DebugLog("AddCommands", Enums.DebugLogTypes.RepeatCommandSystem, $"Adding {Add.Count()} commands.");
                RepeatCommands.AddRange(Add.Select(c => new TimerCommand(c, dilutetime)));

                var Same = RepeatCommands.IntersectBy(commands.Select(c => c.Item1), R => R.Command);
                LogWriter.DebugLog("AddCommands", Enums.DebugLogTypes.RepeatCommandSystem, $"Updating {Same.Count()} commands.");
                foreach (var com in Same)
                {
                    com.ModifyTime(commands.Where(n => n.Item1 == com.Command).Select(n => n.Item2).FirstOrDefault(), dilutetime);
                }
            }
        }

        internal override void UpdateDiluteTime(double diluteTime)
        {
            LogWriter.DebugLog("UpdateDiluteTime", Enums.DebugLogTypes.RepeatCommandSystem, $"Updating the new dilute time factor {diluteTime} to each command.");

            base.UpdateDiluteTime(diluteTime);

            // check if update is necessary
            if (_olddilutetime != _dilutetime)
            {
                foreach (var command in RepeatCommands)
                {
                    command.UpdateTime(diluteTime);
                }
            }
        }

        internal override void NotifyRepeatManager_StreamOnline()
        {
            if (OptionFlags.RepeatLiveReset)
            {
                LogWriter.DebugLog("NotifyRepeatManager_StreamOnline", Enums.DebugLogTypes.RepeatCommandSystem, "Resetting each command timer with new stream online.");
                DateTime now = DateTime.Now;
                foreach (var command in RepeatCommands)
                {
                    if (OptionFlags.RepeatLiveResetShow)
                    {
                        command.SetNow();
                    }
                    else
                    {
                        command.ResetLive(now, _dilutetime);
                    }
                }
            }
        }

        internal override void CheckRepeats()
        {
            LogWriter.DebugLog("CheckRepeats", Enums.DebugLogTypes.RepeatCommandSystem, "Checking each command if ready to activate.");
            lock (RepeatCommands)
            {
                foreach (var command in RepeatCommands)
                {
                    if (command.CategoryList.Any(CategoryList.Contains) && command.CheckFireTime(_dilutetime))
                    {
                        LogWriter.DebugLog("CheckRepeats", Enums.DebugLogTypes.RepeatCommandSystem, $"Command {command.Command} ready to activate.");

                        RepeatEvent(command.Command, Enums.Platform.Twitch);
                    }
                }
            }
        }
    }
}
