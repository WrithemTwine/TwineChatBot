using ChatBot_Net5.Models;

using System;
using System.Reflection;

using static ChatBot_Net5.Data.DataSource;
using System.Linq;

namespace ChatBot_Net5.Data
{
    public partial class DataManager
    {

        #region Stream Statistics
        private StreamStatsRow CurrStreamStatRow;

        public StreamStatsRow[] GetAllStreamData()
        {
            return (StreamStatsRow[])_DataSource.StreamStats.Select();
        }

        private StreamStatsRow GetAllStreamData(DateTime dateTime)
        {
            lock (_DataSource.StreamStats)
            {
                foreach (StreamStatsRow streamStatsRow in from StreamStatsRow streamStatsRow in GetAllStreamData()
                                                          where streamStatsRow.StreamStart == dateTime
                                                          select streamStatsRow)
                {
                    return streamStatsRow;
                }
            }
            return null;
        }

        public StreamStat GetStreamData(DateTime dateTime)
        {
            StreamStatsRow streamStatsRow = GetAllStreamData(dateTime);
            StreamStat streamStat = new();

            if (streamStatsRow != null)
            {
                // can't use a simple method to duplicate this because "ref" can't be used with boxing
                foreach (PropertyInfo property in streamStat.GetType().GetProperties())
                {
                    // use properties from 'StreamStat' since StreamStatRow has additional properties
                    property.SetValue(streamStat, streamStatsRow.GetType().GetProperty(property.Name).GetValue(streamStatsRow));
                }
            }

            return streamStat;
        }

        public bool CheckMultiStreams(DateTime dateTime)
        {
            int x = 0;
            foreach (StreamStatsRow row in GetAllStreamData())
            {
                if (row.StreamStart.ToShortDateString() == dateTime.ToShortDateString())
                {
                    x++;
                }
            }

            return x > 1;
        }

        public bool AddStream(DateTime StreamStart)
        {
            bool returnvalue;

            if (CheckStreamTime(StreamStart))
            {
                returnvalue = false;
            }
            else
            {
                if (StreamStart != DateTime.MinValue.ToLocalTime())
                {
                    CurrStreamStart = StreamStart;

                    lock (_DataSource.StreamStats)
                    {
                        _DataSource.StreamStats.AddStreamStatsRow(StreamStart, StreamStart, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
                        SaveData();
                        OnPropertyChanged(nameof(StreamStats));
                        returnvalue = true;
                    }
                } else
                {
                    returnvalue = false;
                }
            }

            return returnvalue;
        }

        public void PostStreamStat(ref StreamStat streamStat)
        {
            lock (_DataSource.StreamStats)
            {
                CurrStreamStatRow = GetAllStreamData(streamStat.StreamStart);

                if (CurrStreamStatRow == null)
                {
                    _DataSource.StreamStats.AddStreamStatsRow(streamStat.StreamStart, streamStat.StreamEnd, streamStat.NewFollows, streamStat.NewSubscribers, streamStat.GiftSubs, streamStat.Bits, streamStat.Raids, streamStat.Hosted, streamStat.UsersBanned, streamStat.UsersTimedOut, streamStat.ModeratorsPresent, streamStat.SubsPresent, streamStat.VIPsPresent, streamStat.TotalChats, streamStat.Commands, streamStat.AutomatedEvents, streamStat.AutomatedCommands, streamStat.DiscordMsgs, streamStat.ClipsMade, streamStat.ChannelPtCount, streamStat.ChannelChallenge, streamStat.MaxUsers);
                }
                else
                {
                    // can't use a simple method to duplicate this because "ref" can't be used with boxing

                    foreach (PropertyInfo srcprop in CurrStreamStatRow.GetType().GetProperties())
                    {
                        bool found = false;
                        foreach (var _ in from PropertyInfo trgtprop in typeof(StreamStat).GetProperties()
                                          where trgtprop.Name == srcprop.Name
                                          select new { })
                        {
                            found = true;
                        }

                        if (found)
                        {
                            // use properties from 'StreamStat' since StreamStatRow has additional properties
                            srcprop.SetValue(CurrStreamStatRow, streamStat.GetType().GetProperty(srcprop.Name).GetValue(streamStat));
                        }
                    }
                }
            }
            SaveData();
            OnPropertyChanged(nameof(StreamStats));
        }

        /// <summary>
        /// Find if stream data already exists for the current stream
        /// </summary>
        /// <param name="CurrTime">The time to check</param>
        /// <returns><code>true</code>: the stream already has a data entry; <code>false</code>: the stream has no data entry</returns>
        public bool CheckStreamTime(DateTime CurrTime)
        {
            return GetAllStreamData(CurrTime) != null;
        }

        /// <summary>
        /// Remove all stream stats, to satisfy a user option selection to not track stats
        /// </summary>
        public void RemoveAllStreamStats()
        {
            lock (_DataSource.StreamStats)
            {
                _DataSource.StreamStats.Clear();
            }
            OnPropertyChanged(nameof(StreamStats));
        }

        #endregion

    }
}
