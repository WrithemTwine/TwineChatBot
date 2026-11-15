using StreamerBotLib.Culture;
using StreamerBotLib.Models.Statistics;

using System.Collections.ObjectModel;

namespace StreamerBotLib.GUI
{
    public class GUIAppStats
    {
        public AppStat Threads { get; } = new() { Name = Msgs.appstatThread };
        public AppStat ClosedThreads { get; } = new() { Name = Msgs.appstatClosedThreads };
        public AppStat Uptime { get; } = new() { Name = Msgs.appstatUptime };

        /// <summary>Bind the ListView to this — add new stats here once.</summary>
        public List<AppStat> Items { get; }

        public GUIAppStats()
        {
            Threads.UpdateValue(0);
            ClosedThreads.UpdateValue(0);
            Uptime.UpdateValue(TimeSpan.Zero);

            Items =
            [
                Threads,
                ClosedThreads,
                Uptime
            ];
        }
    }
}
