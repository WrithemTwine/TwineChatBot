using System.Diagnostics.CodeAnalysis;

namespace StreamerBotLib.Models
{
    internal class ManagedAction(string taskName, Action action) : IEqualityComparer<ManagedAction>
    {
        public string TaskName { get; set; } = taskName;
        public Action Action { get; set; } = action;

        public bool Equals(ManagedAction x, ManagedAction y)
        {
            return x.TaskName == y.TaskName;
        }

        public int GetHashCode([DisallowNull] ManagedAction obj)
        {
            return obj.TaskName.GetHashCode();
        }
    }
}
