using StreamerBotLib.Enums;

using System;

namespace StreamerBotLib.Models
{
    public class LiveUser : IEquatable<LiveUser>
    {
        public string UserName { get; set; }
        public Bots Source { get; set; }

        public LiveUser(string User, Bots botSource)
        {
            UserName = User;
            Source = botSource;
        }

        public bool Equals(LiveUser other)
        {
            return UserName == other.UserName && Source == other.Source;
        }
    }
}
