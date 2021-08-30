using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ChatBot_Net5.Models
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
            Command = ComRepeat.Item1;
            RepeatTime = ComRepeat.Item2;
            CategoryList.AddRange(ComRepeat.Item3);
            UpdateTime(TimeDilute);
        }

        public void UpdateTime(double TimeDilute) => NextRun = DateTime.Now.ToLocalTime().AddSeconds(RepeatTime*TimeDilute);

        public bool CheckFireTime() => DateTime.Now.ToLocalTime() > NextRun;

        public int CompareTo(TimerCommand obj) => Command.CompareTo(obj.Command);

        public bool Equals(TimerCommand other) => Command == other.Command;

        public override bool Equals(object obj) => Equals(obj as TimerCommand);

        public override int GetHashCode() => (Command + RepeatTime.ToString()).GetHashCode();
    }
}
