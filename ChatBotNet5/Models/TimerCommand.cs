﻿using System;

namespace ChatBot_Net5.Models
{
    internal class TimerCommand : IComparable<TimerCommand>, IEquatable<TimerCommand>
    {
        internal string Command { get; set; }
        internal int RepeatTime { get; set; }
        internal DateTime NextRun { get; set; }

        internal TimerCommand( Tuple<string,int> ComRepeat )
        {
            Command = ComRepeat.Item1;
            RepeatTime = ComRepeat.Item2;
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
