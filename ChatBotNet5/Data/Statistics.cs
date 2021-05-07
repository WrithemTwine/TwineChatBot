using System;
using System.Collections.Generic;

namespace ChatBot_Net5.Data
{
    public class Statistics
    {
        private List<string> CurrUsers = new();
        private List<string> UniqueUserJoined = new();
        private List<string> UniqueUserChat = new();
        private List<string> ModUsers = new();
        private List<string> SubUsers = new();
        private List<string> VIPUsers = new();
        private bool _StreamOnline;
        private DataManager datamanager;
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

        public bool UserChat(string User)
        {
            if (_StreamOnline)
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

        public void StartStreamOnline(DateTime Started)
        {
            if (CurrStream.StreamStart == DateTime.Parse(StreamStat.DefaultTime))
            {
                CurrStream.StreamStart = Started.ToLocalTime();
                datamanager.AddStream(Started);
            }
        }

        public void StreamOnline() => _StreamOnline = true;

        public void StreamOffline(DateTime Stopped)
        {
            UpdateWatchTime();
            _StreamOnline = false;
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
