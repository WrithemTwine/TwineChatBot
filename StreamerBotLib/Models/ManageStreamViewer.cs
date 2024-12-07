namespace StreamerBotLib.Models
{
    /// <summary>
    /// A user's current interaction with the channel. Notation on joined the channel, first chat in channel in current stream,
    /// and whether user is in the current stream.
    /// </summary>
    /// <param name="liveUser">Contains the user details; username, platform, and userId.</param>
    /// <param name="joinedChannel">True/False - user first joined the channel, per new stream</param>
    /// <param name="firstChatMessage">True/False - user first chat in the channel, per new stream</param>
    /// <param name="inStreamNow">True/False - user is currently in the channel for 'current active' 
    /// purposes in calculating user stats</param>
    internal class ManageStreamViewer(
        LiveUser liveUser,
        bool evaluateCurrentCheck = false,
        bool firstjoinedChannel = false,
        bool firstChatMessage = false,
        bool inStreamNow = false) : IEquatable<ManageStreamViewer>
    {
        /// <summary>
        /// Identification for the current user
        /// </summary>
        public LiveUser LiveUser { get; set; } = liveUser;
        /// <summary>
        /// Used to indicate user is in the current user check
        /// </summary>
        public bool EvaluateCurrentCheck { get; set; } = evaluateCurrentCheck;
        /// <summary>
        /// Indicates user has first joined the channel
        /// </summary>
        public bool FirstJoinedChannel { get; set; } = firstjoinedChannel;
        /// <summary>
        /// Indicates user has first chatted in the channel
        /// </summary>
        public bool FirstChatMessage { get; set; } = firstChatMessage;
        /// <summary>
        /// Indicates user is currently in the stream now
        /// </summary>
        public bool InStreamNow { get; set; } = inStreamNow;

        public bool Equals(ManageStreamViewer other)
        {
            return LiveUser.Equals(other.LiveUser);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ManageStreamViewer);
        }

        public override int GetHashCode()
        {
            return LiveUser.GetHashCode();
        }
    }
}
