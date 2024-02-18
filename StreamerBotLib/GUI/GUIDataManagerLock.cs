namespace StreamerBotLib.GUI
{
    /// <summary>
    /// Only purpose is for multithreading safe operations
    /// </summary>
    public static class GUIDataManagerLock
    {
        /// <summary>
        /// Database lock object
        /// </summary>
        public readonly static string Lock = "true";
    }
}
