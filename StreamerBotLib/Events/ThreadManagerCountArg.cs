namespace StreamerBotLib.Events
{
    public class ThreadManagerCountArg : EventArgs
    {
        public int AllThreadCount { get; set; }
        public int ClosedThreadCount { get; set; }
    }
}
