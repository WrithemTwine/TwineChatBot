
namespace StreamerBotLib.Models.Events
{
    using StreamerBotLib.Models;

    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Event arguments for summarizing multi live data.
    /// </summary>
    public class MultiLiveSummarizeEventArgs : EventArgs
    {
        /// <summary>
        /// The data to summarize. If null, the entire date dataset will be pulled from the database.
        /// </summary>
        [AllowNull]
        public ArchiveMultiStream Data { get; set; }

        /// <summary>
        /// The action to perform after the data is summarized.
        /// </summary>
        [NotNull]
        public Action CallbackAction { get; set; }
    }
}
