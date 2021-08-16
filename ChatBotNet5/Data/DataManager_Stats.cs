using ChatBot_Net5.Models;

using System;

namespace ChatBot_Net5.Data
{
    public partial class DataManager
    {

        #region Stream Statistics
        private DataSource.StreamStatsRow CurrStreamStatRow;

        internal DataSource.StreamStatsRow[] GetAllStreamData()
        {
            lock (_DataSource.StreamStats)
            {
                return (DataSource.StreamStatsRow[])_DataSource.StreamStats.Select();
            }
        }

        internal DataSource.StreamStatsRow GetAllStreamData(DateTime dateTime)
        {
            foreach (DataSource.StreamStatsRow streamStatsRow in GetAllStreamData())
            {
                if (streamStatsRow.StreamStart == dateTime)
                {
                    return streamStatsRow;
                }
            }

            return null;
        }

        internal bool CheckMultiStreams(DateTime dateTime)
        {
            int x = 0;
            foreach (DataSource.StreamStatsRow row in GetAllStreamData())
            {
                if (row.StreamStart.ToShortDateString() == dateTime.ToShortDateString())
                {
                    x++;
                }
            }

            return x > 1;
        }

        internal bool AddStream(DateTime StreamStart)
        {
            if (CheckStreamTime(StreamStart))
            {
                return false;
            }
            lock (_DataSource.StreamStats)
            {
                _DataSource.StreamStats.AddStreamStatsRow(StreamStart, StreamStart, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
                SaveData();

                //CurrStreamStatRow = GetAllStreamData(StreamStart);
                OnPropertyChanged(nameof(StreamStats));

                return true;
            }
        }

        internal void PostStreamStat(StreamStat streamStat)
        {
            // TODO: consider regularly posting stream stats in case bot crashes and loses current stream stats up to the crash
            lock (_DataSource.StreamStats)
            {
                CurrStreamStatRow = GetAllStreamData(streamStat.StreamStart);

                if (CurrStreamStatRow == null)
                {
                    _DataSource.StreamStats.AddStreamStatsRow(streamStat.StreamStart, streamStat.StreamEnd, streamStat.NewFollows, streamStat.NewSubs, streamStat.GiftSubs, streamStat.Bits, streamStat.Raids, streamStat.Hosted, streamStat.UsersBanned, streamStat.UsersTimedOut, streamStat.ModsPresent, streamStat.SubsPresent, streamStat.VIPsPresent, streamStat.TotalChats, streamStat.Commands, streamStat.AutoEvents, streamStat.AutoCommands, streamStat.DiscordMsgs, streamStat.ClipsMade, streamStat.ChannelPtCount, streamStat.ChannelChallenge, streamStat.MaxUsers);
                }
                else
                {
                    CurrStreamStatRow.StreamStart = streamStat.StreamStart;
                    CurrStreamStatRow.StreamEnd = streamStat.StreamEnd;
                    CurrStreamStatRow.NewFollows = streamStat.NewFollows;
                    CurrStreamStatRow.NewSubscribers = streamStat.NewSubs;
                    CurrStreamStatRow.GiftSubs = streamStat.GiftSubs;
                    CurrStreamStatRow.Bits = streamStat.Bits;
                    CurrStreamStatRow.Raids = streamStat.Raids;
                    CurrStreamStatRow.Hosted = streamStat.Hosted;
                    CurrStreamStatRow.UsersBanned = streamStat.UsersBanned;
                    CurrStreamStatRow.UsersTimedOut = streamStat.UsersTimedOut;
                    CurrStreamStatRow.ModeratorsPresent = streamStat.ModsPresent;
                    CurrStreamStatRow.SubsPresent = streamStat.SubsPresent;
                    CurrStreamStatRow.VIPsPresent = streamStat.VIPsPresent;
                    CurrStreamStatRow.TotalChats = streamStat.TotalChats;
                    CurrStreamStatRow.Commands = streamStat.Commands;
                    CurrStreamStatRow.AutomatedEvents = streamStat.AutoEvents;
                    CurrStreamStatRow.AutomatedCommands = streamStat.AutoCommands;
                    CurrStreamStatRow.DiscordMsgs = streamStat.DiscordMsgs;
                    CurrStreamStatRow.ClipsMade = streamStat.ClipsMade;
                    CurrStreamStatRow.ChannelPtCount = streamStat.ChannelPtCount;
                    CurrStreamStatRow.ChannelChallenge = streamStat.ChannelChallenge;
                    CurrStreamStatRow.MaxUsers = streamStat.MaxUsers;
                }
                SaveData();
            }
            OnPropertyChanged(nameof(StreamStats));
        }

        internal bool CheckStreamTime(DateTime CurrTime)
        {
            return GetAllStreamData(CurrTime) != null;
        }

        internal void RemoveAllStreamStats()
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
