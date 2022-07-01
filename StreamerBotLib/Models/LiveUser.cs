using StreamerBotLib.Enums;

using System;
using System.Diagnostics;

namespace StreamerBotLib.Models
{
    /// <summary>
    /// Data specifying details of a user joined to the live stream channel.
    /// </summary>
    [DebuggerDisplay( "UserId,UserName,Source = {UserId},{UserName},{ Source}" )]
    public class LiveUser : IEquatable<LiveUser>, IComparable<LiveUser>
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
        /// The User Id of the user name, per the Bot platform
        /// </summary>
        public string UserId { get; set; }
        
        /// <summary>
        /// Constructs the object.
        /// </summary>
        /// <param name="User">Name of the user.</param>
        /// <param name="botSource">The bot source of user.</param>
        /// <param name="userType">The type of the user.</param>
        public LiveUser(string User, Platform botSource)
        {
            UserName = User;
            Source = botSource;
        }

        /// <summary>
        /// Determines if the provided object is equal to another object.
        /// </summary>
        /// <param name="other">The object to compare.</param>
        /// <returns>True if the objects contain identical values.</returns>
        public bool Equals(LiveUser other)
        {
            return UserName == other.UserName && Source == other.Source;
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
    }
}
