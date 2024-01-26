using StreamerBotLib.Enums;

using System.Diagnostics;

namespace StreamerBotLib.Models
{
    /// <summary>
    /// Data specifying details of a user joined to the live stream channel.
    /// </summary>
    [DebuggerDisplay("UserId,UserName,Source = {UserId},{UserName},{ Source}")]
    public sealed record LiveUser : IComparable<LiveUser>, IEquatable<LiveUser>
    {
        /// <summary>
        /// The user's UserName.
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// The source of the Bot with the registered user.
        /// </summary>
        public Platform Source { get; set; }
        /// <summary>
        /// The userName userId of the user name, per the Bot platform
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Constructs the object.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="botSource">The bot source of user.</param>
        /// <param name="userType">The type of the user.</param>
        public LiveUser(string userName, Platform botSource, string userId = "")
        {
            UserName = userName;
            Source = botSource;
            UserId = userId;
        }

        /// <summary>
        /// Determines if the provided object is equal to another object.
        /// </summary>
        /// <param name="other">The object to compare.</param>
        /// <returns>True if the objects contain identical values.</returns>
        public bool Equals(LiveUser other)
        {
            return other != null & UserName == other.UserName && Source == other.Source && UserId == other.UserId;
        }

        public int GetHashCode(object Obj)
        {
            return (Obj as LiveUser).GetHashCode();
        }

        /// <summary>
        /// Compares the UserNames between two objects.
        /// </summary>
        /// <param name="other">The object with the UserName to compare.</param>
        /// <returns>The result of comparing two strings.</returns>
        public int CompareTo(LiveUser other)
        {
            return UserName.CompareTo(other.UserName);
        }

        public override int GetHashCode()
        {
            return string.GetHashCode(ToString());
        }

        public override string ToString()
        {
            return $"{UserName}{Source}{UserId}";
        }

        public static bool operator <(LiveUser left, LiveUser right)
        {
            return left is null ? right is not null : left.CompareTo(right) < 0;
        }

        public static bool operator <=(LiveUser left, LiveUser right)
        {
            return left is null || left.CompareTo(right) <= 0;
        }

        public static bool operator >(LiveUser left, LiveUser right)
        {
            return left is not null && left.CompareTo(right) > 0;
        }

        public static bool operator >=(LiveUser left, LiveUser right)
        {
            return left is null ? right is null : left.CompareTo(right) >= 0;
        }
    }
}
