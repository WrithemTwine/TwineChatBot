//#if DEBUG
//#define LOGGING
//#endif

namespace ChatBot_Net5.BotIOController
{
    internal static class ThreadFlags
    {
        internal static bool ProcessOps { get; set; } = false;  // whether to process ops or not
    }
}
