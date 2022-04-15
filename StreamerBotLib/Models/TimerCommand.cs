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
        /// The list of categories applying to this command, run command if category matches current stream or "All" category designation.
        /// </summary>
        public List<string> CategoryList { get; } = new();

        public TimerCommand(Tuple<string, int, string[]> ComRepeat, double TimeDilute)
        {
            Command = ComRepeat.Item1.ToLower();
            RepeatTime = ComRepeat.Item2;
            CategoryList.AddRange(ComRepeat.Item3);
            UpdateTime(TimeDilute);
        }

        /// <summary>
        /// Change the time to run through direct time specification. The provided <paramref name="NewRepeatTime"/> seconds are added or subtracted from the existing timer adjusted by the <paramref name="TimeDilute"/> factor. The repeat timer seconds is replaced with the <paramref name="NewRepeatTime"/> seconds.
        /// </summary>
        /// <param name="NewRepeatTime">The number of seconds to repeat the timer commands. Used to adjust current repeat time.</param>
        /// <param name="TimeDilute">The time factor to dilute the current amount of repeat time.</param>
        public void ModifyTime(int NewRepeatTime, double TimeDilute)
        {
            if (NewRepeatTime < RepeatTime)
            {
                NextRun = NextRun.AddSeconds((NewRepeatTime - RepeatTime) * TimeDilute);
            }
            else if (NewRepeatTime > RepeatTime)
            {
                NextRun = NextRun.AddSeconds((RepeatTime - NewRepeatTime) * TimeDilute);
            }
            RepeatTime = NewRepeatTime;
        }

        private Random random = new();

        public void SetNow() => NextRun = DateTime.Now.AddSeconds(random.Next(60,1250)).ToLocalTime();

        public void UpdateTime(double TimeDilute) => NextRun = DateTime.Now.ToLocalTime().AddSeconds(RepeatTime * TimeDilute);

        public bool CheckFireTime() => DateTime.Now.ToLocalTime() > NextRun;

        public int CompareTo(TimerCommand obj) => Command.CompareTo(obj.Command);

        public bool Equals(TimerCommand other) => Command == other.Command;

        public override bool Equals(object obj) => Equals(obj as TimerCommand);

        public override int GetHashCode() => (Command + RepeatTime.ToString()).GetHashCode();
    }
}
