namespace StreamerBotLib.DataSQL.SingleContext
{
    using StreamerBotLib.Models;

    internal partial class DataManagerSQLAsync
    {
        #region Test Method Verification

        /// <summary>
        /// Provides check for test code; checks if there's a record for the provided channel, date, viewer count, and game name.
        /// </summary>
        /// <param name="user">The user bringing in the raid.</param>
        /// <param name="time">The time the user raided.</param>
        /// <param name="viewers">The viewers they brought.</param>
        /// <param name="gamename">The category the raiding stream had at the raid time.</param>
        /// <returns><code>true: when record is found.</code>
        /// <code>false: when record is not found.</code></returns>
        internal Task<bool> TestInRaidData(string userId, DateTime time, int viewers, string gamename)
        {
            return Task.Run(() =>
            {
                //// using var context = BuildDataContext();
                var result = (from I in context.InRaidData
                              where (I.UserId == userId && I.RaidDate == time && I.ViewerCount == viewers && I.Category == gamename)
                              select I).Any();

                return result;
            });
        }

        /// <summary>
        /// Provides check for test code; checks if there's a record for the provided channel and date.
        /// </summary>
        /// <param name="HostedChannel">The channel raided.</param>
        /// <param name="dateTime">The date & time of the raid.</param>
        /// <returns><code>true: when record is found.</code>
        /// <code>false: when record is not found.</code></returns>
        internal Task<bool> TestOutRaidData(string HostedChannel, DateTime dateTime)
        {
            return Task.Run(() =>
            {
                //// using var context = BuildDataContext();
                var result = (from O in context.OutRaidData
                              where (O.ChannelRaided == HostedChannel && O.RaidDate == dateTime)
                              select O).Any();

                return result;
            });
        }

        internal Task<List<LiveUser>> TestGetRandomUsers(int count)
        {
            return Task.Run(() =>
            {
                //// using var context = BuildDataContext();
                var Users = (from U in context.Users select U).ToList();

                Random random = new();
                List<LiveUser> result = [];

                for (int x = 0; x < count; x++)
                {
                    Models.Users row = Users[random.Next(Users.Count)];
                    result.Add(new(row.UserName, row.Platform, row.UserId));
                }

                return result;
            });
        }

        #endregion
    }
}
