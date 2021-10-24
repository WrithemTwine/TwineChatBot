using StreamerBot.Enum;
using StreamerBot.Static;
using StreamerBot.Systems;

using System;
using System.Collections.Generic;

namespace StreamerBot.Data
{
    public partial class DataManager
    {

        #region Regular Channel Events
        /// <summary>
        /// Add default data to Channel Events table, to ensure the data is available to use in event messages.
        /// </summary>
        private void SetDefaultChannelEventsTable()
        {
            bool CheckName(string criteria)
            {
                return _DataSource.ChannelEvents.FindByName(criteria) == null;
            }

            Dictionary<ChannelEventActions, Tuple<string, string>> dictionary = new()
            {
                {
                    ChannelEventActions.BeingHosted,
                    new(LocalizedMsgSystem.GetEventMsg(ChannelEventActions.BeingHosted, out _), VariableParser.ConvertVars(new[] { MsgVars.user, MsgVars.autohost, MsgVars.viewers }))
                },
                {
                    ChannelEventActions.Bits,
                    new(LocalizedMsgSystem.GetEventMsg(ChannelEventActions.Bits, out _), VariableParser.ConvertVars(new[] { MsgVars.user, MsgVars.bits }))
                },
                {
                    ChannelEventActions.CommunitySubs,
                    new(LocalizedMsgSystem.GetEventMsg(ChannelEventActions.CommunitySubs, out _), VariableParser.ConvertVars(new[] { MsgVars.user, MsgVars.count, MsgVars.subplan }))
                },
                {
                    ChannelEventActions.NewFollow,
                    new(LocalizedMsgSystem.GetEventMsg(ChannelEventActions.NewFollow, out _), VariableParser.ConvertVars(new[] { MsgVars.user }))
                },
                {
                    ChannelEventActions.GiftSub,
                    new(LocalizedMsgSystem.GetEventMsg(ChannelEventActions.GiftSub, out _), VariableParser.ConvertVars(new[] { MsgVars.user, MsgVars.months, MsgVars.receiveuser, MsgVars.subplan, MsgVars.subplanname }))
                },
                {
                    ChannelEventActions.Live,
                    new(LocalizedMsgSystem.GetEventMsg(ChannelEventActions.Live, out _), VariableParser.ConvertVars(new[] { MsgVars.user, MsgVars.category, MsgVars.title, MsgVars.url, MsgVars.everyone }))
                },
                {
                    ChannelEventActions.Raid,
                    new(LocalizedMsgSystem.GetEventMsg(ChannelEventActions.Raid, out _), VariableParser.ConvertVars(new[] { MsgVars.user, MsgVars.viewers }))
                },
                {
                    ChannelEventActions.Resubscribe,
                    new(LocalizedMsgSystem.GetEventMsg(ChannelEventActions.Resubscribe, out _), VariableParser.ConvertVars(new[] { MsgVars.user, MsgVars.months, MsgVars.submonths, MsgVars.subplan, MsgVars.subplanname, MsgVars.streak }))
                },
                {
                    ChannelEventActions.Subscribe,
                    new(LocalizedMsgSystem.GetEventMsg(ChannelEventActions.Subscribe, out _), VariableParser.ConvertVars(new[] { MsgVars.user, MsgVars.submonths, MsgVars.subplan, MsgVars.subplanname }))
                },
                {
                    ChannelEventActions.UserJoined,
                    new(LocalizedMsgSystem.GetEventMsg(ChannelEventActions.UserJoined, out _), VariableParser.ConvertVars(new[] { MsgVars.user }))
                },
                {
                    ChannelEventActions.ReturnUserJoined,
                    new(LocalizedMsgSystem.GetEventMsg(ChannelEventActions.ReturnUserJoined, out _), VariableParser.ConvertVars(new[] { MsgVars.user }))
                },
                {
                    ChannelEventActions.SupporterJoined,
                    new(LocalizedMsgSystem.GetEventMsg(ChannelEventActions.SupporterJoined, out _), VariableParser.ConvertVars(new[] { MsgVars.user }))
                }
            };
            lock (_DataSource)
            {
                foreach (ChannelEventActions command in System.Enum.GetValues(typeof(ChannelEventActions)))
                {
                    // consider only the values in the dictionary, check if data is already defined in the data table
                    if (dictionary.ContainsKey(command) && CheckName(command.ToString()))
                    {   // extract the default data from the dictionary and add to the data table
                        Tuple<string, string> values = dictionary[command];

                        _DataSource.ChannelEvents.AddChannelEventsRow(command.ToString(), false, true, values.Item1, values.Item2);

                    }
                }

                _DataSource.ChannelEvents.AcceptChanges();
            }
        }
        #endregion Regular Channel Events

    }
}
