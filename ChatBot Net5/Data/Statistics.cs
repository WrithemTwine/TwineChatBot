using System;
using System.Collections.Generic;

namespace ChatBot_Net5.Data
{
    public class Statistics
    {
        private List<string> CurrUsers = new();
        private List<string> UniqueUserJoined = new();
        private List<string> ModUsers = new();
        private List<string> SubUsers = new();
        private List<string> VIPUsers = new();
        private bool _StreamOnline;
        private DataManager datamanager;
        internal DateTime defaultDate = DateTime.Parse("01/01/1900");
        private StreamStat CurrStream { get; set; } = new();

        public Statistics(DataManager dataManager)
        {
            datamanager = dataManager;
            _StreamOnline = false;
        }

        /// <summary>
        /// Adds user to the database by name, or updates existing user, and the time they joined the channel
        /// </summary>
        /// <param name="User">User's DisplayName</param>
        /// <param name="CurrTime">The current time the user joined</param>
        /// <returns></returns>
        public bool UserJoined(string User, DateTime CurrTime)
        {
            CurrUsers.Add(User);
            datamanager.UserJoined(User, CurrTime);

            if (_StreamOnline)
            {
                CurrStream.MaxUsers = Math.Max(CurrStream.MaxUsers, CurrUsers.Count);

                if (!UniqueUserJoined.Contains(User))
                {
                    UniqueUserJoined.Add(User);
                    return true;
                }
            }
            return false;
        }

        public void ModJoined(string User)
        {
            if (_StreamOnline && !ModUsers.Contains(User))
            {
                ModUsers.Add(User);
            }
        }

        public void SubJoined(string User)
        {
            if (_StreamOnline && !SubUsers.Contains(User))
            {
                SubUsers.Add(User);
            }
        }

        public void VIPJoined(string User)
        {
            if (_StreamOnline && !VIPUsers.Contains(User))
            {
                VIPUsers.Add(User);
            }
        }

        public void UserLeft(string User, DateTime CurrTime)
        {
            UpdateWatchTime(User);
            datamanager.UserLeft(User, CurrTime);
            CurrUsers.Remove(User);
        }

        /// <summary>
        /// Default to all users or a specific user to register "DateTime.Now" as the current watch date.
        /// </summary>
        /// <param name="User">User to update "Now" or null to update all users watch time.</param>
        public void UpdateWatchTime(string User = null)
        {
            if (_StreamOnline)
            {
                UpdateWatchTime(User, DateTime.Now);
            }
        }

        public void UpdateWatchTime(string User, DateTime Seen)
        {
            if (_StreamOnline)
            {
                if (User != null)
                {
                    datamanager.UpdateWatchTime(User, Seen);
                }
                else
                {
                    foreach (string s in CurrUsers)
                    {
                        datamanager.UpdateWatchTime(s, Seen);
                    }
                }
            }
        }

        public bool StartStreamOnline(DateTime Started)
        {
            CurrStream.Clear();
            CurrStream.StreamStart = Started;
            datamanager.AddStream(Started);

            return !datamanager.GetTodayStream(Started);
        }

        public void StreamOnline()
        {
            _StreamOnline = true;
        }

        public void StreamOffline(DateTime Stopped)
        {
            UpdateWatchTime();
            _StreamOnline = false;
            CurrStream.StreamEnd = Stopped;
            CurrStream.ModsPresent = ModUsers.Count;
            CurrStream.VIPsPresent = VIPUsers.Count;
            CurrStream.SubsPresent = SubUsers.Count;
            datamanager.PostStreamStat(CurrStream);

            ModUsers.Clear();
            SubUsers.Clear();
            VIPUsers.Clear();
        }

        #region Stream Stat Methods
        public void AddFollow() => CurrStream.NewFollows++;
        public void AddSub() => CurrStream.NewSubs++;
        public void AddGiftSubs(int Gifted=1) => CurrStream.GiftSubs += Gifted;
        public void AddBits(int BitCount) => CurrStream.Bits += BitCount;
        public void AddRaids() => CurrStream.Raids++;
        public void AddHosted() => CurrStream.Hosted++;
        public void AddUserBanned() => CurrStream.UsersBanned++;
        public void AddUserTimedOut() => CurrStream.UsersTimedOut++;
        public void AddTotalChats() => CurrStream.TotalChats++;
        public void AddCommands() => CurrStream.Commands++;
        public void AddAutoEvents() => CurrStream.AutoEvents++;
        public void AddAutoCommands() => CurrStream.AutoCommands++;
        public void AddDiscord() => CurrStream.DiscordMsgs++;
        public void AddClips() => CurrStream.ClipsMade++;
        public void AddChannelPtsCount() => CurrStream.ChannelPtCount++;
        public void AddChannelChallenge() => CurrStream.ChannelChallenge++;
        #endregion
    }

    internal class StreamStat
    {
        internal DateTime StreamStart { get; set; } = DateTime.Parse("1/1/1990");
        internal DateTime StreamEnd { get; set; } = DateTime.Parse("1/1/1990");
        internal int NewFollows { get; set; } = 0;
        internal int NewSubs { get; set; } = 0;
        internal int GiftSubs { get; set; } = 0;
        internal int Bits { get; set; } = 0;
        internal int Raids { get; set; } = 0;
        internal int Hosted { get; set; } = 0;
        internal int UsersBanned { get; set; } = 0;
        internal int UsersTimedOut { get; set; } = 0;
        internal int ModsPresent { get; set; } = 0;
        internal int SubsPresent { get; set; } = 0;
        internal int VIPsPresent { get; set; } = 0;
        internal int TotalChats { get; set; } = 0;
        internal int Commands { get; set; } = 0;
        internal int AutoEvents { get; set; } = 0;
        internal int AutoCommands { get; set; } = 0;
        internal int DiscordMsgs { get; set; } = 0;
        internal int ClipsMade { get; set; } = 0;
        internal int ChannelPtCount { get; set; } = 0;
        internal int ChannelChallenge { get; set; } = 0;
        internal int MaxUsers { get; set; } = 0;

        public void Clear()
        {
            StreamStart = DateTime.Parse("1/1/1990");
            StreamEnd = DateTime.Parse("1/1/1990");
            NewFollows = 0;
            NewSubs = 0;
            GiftSubs = 0;
            Bits = 0;
            Raids = 0;
            Hosted = 0;
            UsersBanned = 0;
            UsersTimedOut = 0;
            ModsPresent = 0;
            SubsPresent = 0;
            VIPsPresent = 0;
            TotalChats = 0;
            Commands = 0;
            AutoEvents = 0;
            AutoCommands = 0;
            DiscordMsgs = 0;
            ClipsMade = 0;
            ChannelPtCount = 0;
            ChannelChallenge = 0;
            MaxUsers = 0;
        }
    }
}
