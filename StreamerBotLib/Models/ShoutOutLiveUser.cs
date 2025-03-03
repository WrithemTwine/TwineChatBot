using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    public class ShoutOutLiveUser(LiveUser user) : IEqualityComparer<ShoutOutLiveUser>
    {
        public DateTime? LastShoutOut { get; set; } = null;
        public DateTime? NextShoutOut { get; set; } = null;
        public LiveUser User { get; set; } = user;

        public bool Equals(ShoutOutLiveUser x, ShoutOutLiveUser y)
        {
            return x.Equals(y);
        }

        public int GetHashCode([DisallowNull] ShoutOutLiveUser obj)
        {
            return obj.User.GetHashCode();
        }
    }
}
