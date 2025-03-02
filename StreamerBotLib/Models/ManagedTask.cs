using System.Diagnostics.CodeAnalysis;

namespace StreamerBotLib.Models
{
    internal class ManagedTask(string taskName, Task task) : IEqualityComparer<ManagedTask>
    {
        public string TaskName { get; set; } = taskName;
        public Task Task { get; set; } = task;

        public bool Equals(ManagedTask x, ManagedTask y)
        {
            return x.TaskName == y.TaskName;
        }

        public int GetHashCode([DisallowNull] ManagedTask obj)
        {
            return obj.TaskName.GetHashCode();
        }
    }
}
