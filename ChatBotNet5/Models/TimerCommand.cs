using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ChatBot_Net5.Models
{
    [DebuggerDisplay("Command={Command}, RepeatTime={RepeatTime}, NextRun={NextRun}")]
    internal class TimerCommand : IComparable<TimerCommand>, IEquatable<TimerCommand>
    {
        internal string Command { get; set; }
        internal int RepeatTime { get; set; }
        internal DateTime NextRun { get; set; }
        internal List<string> CategoryList { get; } = new();

        internal TimerCommand(Tuple<string, int, string[]> ComRepeat)
        {
            Command = ComRepeat.Item1;
            RepeatTime = ComRepeat.Item2;
            CategoryList.AddRange(ComRepeat.Item3);
            UpdateTime();
        }

        internal void UpdateTime() => NextRun = DateTime.Now.AddSeconds(RepeatTime);

        internal bool CheckFireTime() => DateTime.Now > NextRun;

        public int CompareTo(TimerCommand obj) => RepeatTime.CompareTo(obj.RepeatTime);

        public bool Equals(TimerCommand other) => Command == other.Command;

        public override bool Equals(object obj) => Equals(obj as TimerCommand);

        public override int GetHashCode() => (Command + RepeatTime.ToString()).GetHashCode();
    }
}
