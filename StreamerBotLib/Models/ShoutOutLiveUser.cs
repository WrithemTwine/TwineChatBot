using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace StreamerBotLib.Models
{
    /// <summary>
    /// Manages the ShoutOut requested for each LiveUser.
    /// 
    /// NewUserEntry: Different users can only be shoutout once every 2 minutes
    /// -LastShoutOut = null, NextShoutOut = null => first shoutout occurs asap
    /// 
    /// ExistingUserEntry: Same user can only be shoutout after at least every 60 minutes
    /// -LastShoutOut = value, NextShoutOut = null => no shoutout scheduled
    /// -LastShoutOUt = value, NextShoutOut = value => computed next shoutout to perform
    /// </summary>
    /// <param name="user">The user to shoutout.</param>
    [DebuggerDisplay("User = {User}")]
    public class ShoutOutLiveUser(LiveUser user) : IEquatable<ShoutOutLiveUser>, IComparable<ShoutOutLiveUser>
    {
        /// <summary>
        /// The previous shoutout performed for this user.
        /// </summary>
        public DateTime? LastShoutOut { get; set; } = null;
        /// <summary>
        /// Any scheduled upcoming shoutout.
        /// -Twitch limits shoutouts to once every 2 minutes for different users, the same user is limited to 1 shoutout every 60 minutes between shoutouts.
        /// </summary>
        public DateTime? NextShoutOut { get; set; } = null;
        /// <summary>
        /// Gets a value indicating whether at least one hour has passed since the last shout-out and no shout-out is
        /// currently scheduled.
        /// </summary>
        public bool HourSinceLastShoutOut => !(LastShoutOut == null && NextShoutOut == null) && (NextShoutOut == null) && (DateTime.Now - LastShoutOut.Value).TotalMinutes >= 60;
        /// <summary>
        /// The user information for the current shoutout entry.
        /// </summary>
        public LiveUser User { get; set; } = user;

        public int CompareTo(ShoutOutLiveUser other)
        {
            return User.CompareTo(other.User);
        }

        public bool Equals(ShoutOutLiveUser x, ShoutOutLiveUser y)
        {
            return x.User.Equals(y.User);
        }

        public bool Equals(ShoutOutLiveUser other)
        {
            return User.Equals(other.User);
        }

        public int GetHashCode([DisallowNull] ShoutOutLiveUser obj)
        {
            return obj.User.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ShoutOutLiveUser);
        }

        public override int GetHashCode()
        {
            return User.GetHashCode();
        }
    }
}
