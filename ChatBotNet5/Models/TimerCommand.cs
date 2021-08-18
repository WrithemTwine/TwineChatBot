using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ChatBot_Net5.Models
{
    [DebuggerDisplay("Command={Command}, RepeatTime={RepeatTime}, NextRun={NextRun}")]
    internal class TimerCommand : IComparable<TimerCommand>, IEquatable<TimerCommand>
    {
        /// <summary>
        /// The Command represented in this command repeater
        /// </summary>
        internal string Command { get; set; }

        /// <summary>
        /// The amount of seconds between each run.
        /// </summary>
        internal int RepeatTime { get; set; }

        /// <summary>
        /// The DateTime of the next repeat timer run for this event.
        /// </summary>
        internal DateTime NextRun { get; set; }

        /// <summary>
        /// The list of categories applying to this command, run command if category matches current stream or "All" category designation.
        /// </summary>
        internal List<string> CategoryList { get; } = new();

        internal TimerCommand(Tuple<string, int, string[]> ComRepeat, double TimeDilute)
        {
            Command = ComRepeat.Item1;
            RepeatTime = ComRepeat.Item2;
            CategoryList.AddRange(ComRepeat.Item3);
            UpdateTime(TimeDilute);
        }

        internal void UpdateTime(double TimeDilute) => NextRun = DateTime.Now.ToLocalTime().AddSeconds(RepeatTime*TimeDilute);

        internal bool CheckFireTime() => DateTime.Now.ToLocalTime() > NextRun;

        public int CompareTo(TimerCommand obj) => Command.CompareTo(obj.Command);

        public bool Equals(TimerCommand other) => Command == other.Command;

        public override bool Equals(object obj) => Equals(obj as TimerCommand);

        public override int GetHashCode() => (Command + RepeatTime.ToString()).GetHashCode();
    }
}
