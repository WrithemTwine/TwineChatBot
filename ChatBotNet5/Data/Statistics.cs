using System;
using System.Collections.Generic;

namespace ChatBot_Net5.Data
{
    public class Statistics
    {
        private readonly List<string> CurrUsers = new();
        private readonly List<string> UniqueUserJoined = new();
        private readonly List<string> UniqueUserChat = new();
        private readonly List<string> ModUsers = new();
        private readonly List<string> SubUsers = new();
        private readonly List<string> VIPUsers = new();
        private readonly DataManager datamanager;
        private StreamStat CurrStream { get; set; } = new();

        public bool IsStreamOnline { get; private set; }

        public Statistics(DataManager dataManager)
        {
            datamanager = dataManager;
            IsStreamOnline = false;
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

            if (IsStreamOnline)
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

        public bool UserChat(string User)
        {
            if (IsStreamOnline)
            {
                if (!UniqueUserChat.Contains(User))
                {
                    UniqueUserChat.Add(User);
                    return true;
                }
            }
            return false;
        }

        public void ModJoined(string User)
        {
            if (IsStreamOnline && !ModUsers.Contains(User))
            {
                ModUsers.Add(User);
            }
        }

        public void SubJoined(string User)
        {
            if (IsStreamOnline && !SubUsers.Contains(User))
            {
                SubUsers.Add(User);
            }
        }

        public void VIPJoined(string User)
        {
            if (IsStreamOnline && !VIPUsers.Contains(User))
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
            if (IsStreamOnline)
            {
                UpdateWatchTime(User, DateTime.Now);
            }
        }

        public void UpdateWatchTime(string User, DateTime Seen)
        {
            if (IsStreamOnline)
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

        public bool StreamOnline(DateTime Started)
        {
            IsStreamOnline = true;
            CurrStream.StreamStart = Started.ToLocalTime();
            return datamanager.AddStream(CurrStream.StreamStart);
        }

        public void StreamOffline(DateTime Stopped)
        {
            UpdateWatchTime();
            IsStreamOnline = false;
            CurrStream.StreamEnd = Stopped.ToLocalTime();
            CurrStream.ModsPresent = ModUsers.Count;
            CurrStream.VIPsPresent = VIPUsers.Count;
            CurrStream.SubsPresent = SubUsers.Count;
            datamanager.PostStreamStat(CurrStream);
            CurrStream.Clear();

            ModUsers.Clear();
            SubUsers.Clear();
            VIPUsers.Clear();
            UniqueUserJoined.Clear();
            UniqueUserChat.Clear();
        }

        public DateTime GetCurrentStreamStart() => CurrStream.StreamStart;

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
}
