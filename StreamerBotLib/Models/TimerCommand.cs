using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace StreamerBotLib.Models
{
    [DebuggerDisplay("Command={Command}, RepeatTime={RepeatTime}, NextRun={NextRun}")]
    public class TimerCommand : IComparable<TimerCommand>, IEquatable<TimerCommand>
    {
        /// <summary>
        /// The Command represented in this command repeater
        /// </summary>
        public string Command { get; set; }

        /// <summary>
        /// The amount of seconds between each run.
        /// </summary>
        public int RepeatTime { get; set; }

        /// <summary>
        /// The DateTime of the next repeat timer run for this event.
        /// </summary>
        public DateTime NextRun { get; set; }

        /// <summary>
        /// The DateTime of the last repeat timer run for this event. Initially set to DateTime.Now.
        /// </summary>
        public DateTime LastRun { get; set; }

        /// <summary>
        /// The list of categories applying to this command, run command if category matches current stream or "All" category designation.
        /// </summary>
        public List<string> CategoryList { get; } = new();

        public TimerCommand(Tuple<string, int, string[]> ComRepeat, double TimeDilute)
        {
            Command = ComRepeat.Item1.ToLower();
            RepeatTime = ComRepeat.Item2;
            CategoryList.AddRange(ComRepeat.Item3);
            LastRun = DateTime.Now;
            UpdateTime(TimeDilute);
        }

        /// <summary>
        /// Change the time to run through direct time specification. The provided <paramref name="NewRepeatTime"/> seconds are added or subtracted from the existing timer adjusted by the <paramref name="TimeDilute"/> factor. The repeat timer seconds is replaced with the <paramref name="NewRepeatTime"/> seconds.
        /// </summary>
        /// <param name="NewRepeatTime">The number of seconds to repeat the timer commands. Used to adjust current repeat time.</param>
        /// <param name="TimeDilute">The time factor to dilute the current amount of repeat time.</param>
        public void ModifyTime(int NewRepeatTime, double TimeDilute)
        {
            NextRun = LastRun.AddSeconds(NewRepeatTime * TimeDilute);
            RepeatTime = NewRepeatTime;
        }

        private readonly Random random = new();

        /// <summary>
        /// Update the NextRun time to a random time between the next 30 seconds 
        /// to 5 minutes. So many updated commands don't run all at once.
        /// </summary>
        public void SetNow()
        {
            NextRun = DateTime.Now.AddSeconds(random.Next(30, 300));
            LastRun = NextRun;
        }

        /// <summary>
        /// Recompute the NextRun based on the LastRun plus (repeat_time * TimeDilute) seconds.
        /// </summary>
        /// <param name="TimeDilute">A factor to multiply to the repeat timer seconds to extend the NextRun time.</param>
        public void UpdateTime(double TimeDilute)
        {
            NextRun = LastRun.AddSeconds(RepeatTime * TimeDilute);
        }

        /// <summary>
        /// Checks if the NextRun time is before the Now time. When Now is after NextRun time, the 
        /// LastRun is reset to Now.
        /// </summary>
        /// <returns>Whether Now is after the NextRun time.</returns>
        public bool CheckFireTime()
        {
            bool result = DateTime.Now > NextRun;
            if (result)
            {
                LastRun = DateTime.Now;
            }
            return result;
        }

        public int CompareTo(TimerCommand obj)
        {
            return this.Command.CompareTo(obj.Command);
        }

        public bool Equals(TimerCommand other)
        {
            return this.Command == other.Command;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as TimerCommand);
        }

        public override int GetHashCode()
        {
            return (this.Command + RepeatTime.ToString()).GetHashCode();
        }
    }
}
