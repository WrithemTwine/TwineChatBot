using StreamerBotLib.Models;

using System;
using System.ComponentModel;

namespace StreamerBotLib.GUI
{
    public class GUIAppStats
    {
        public AppStat<string> Header { get; private set; } = new() { Name = "Property Name", Value = "Value" };
        public AppStat<int> Threads { get; private set; } = new() { Name = "Thread Count", Value = 0 };
        public AppStat<int> ClosedThreads { get; private set; } = new() { Name = "Closed Threads", Value = 0 };
        public AppStat<TimeSpan> Uptime { get; private set; } = new() { Name = "Bot Uptime", Value = new(0) };
    }
}
