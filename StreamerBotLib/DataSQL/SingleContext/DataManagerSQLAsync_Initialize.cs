using Microsoft.EntityFrameworkCore.Storage;

using StreamerBotLib.DataSQL.Models;
using StreamerBotLib.Enums;
using StreamerBotLib.MLearning;
using StreamerBotLib.Models;
using StreamerBotLib.Static;
using StreamerBotLib.Systems;

using System.Data;

namespace StreamerBotLib.DataSQL.SingleContext
{
    internal partial class DataManagerSQLAsync
    {
        #region Construct default items

        /// <summary>
        /// Perform table setup procedures
        /// </summary>
        internal Task Initialize()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("Initialize", DebugLogTypes.DataManager, $"Initializing the database.");

               // using var context = BuildDataContext();

                await SetDefaultChannelEventsTable();  // check all default ChannelEvents names
                await SetDefaultCommandsTable(); // check all default Commands
                await SetLearnedMessages();
                //CleanCategories(context);

                OptionFlags.DataLoaded = true;
            });
        }

        /// <summary>
        /// Add default data to Channel Events table, to ensure the data is available to use in event messages.
        /// </summary>
        private async Task SetDefaultChannelEventsTable(IDbContextTransaction contextTransaction = null)
        {
            LogWriter.DebugLog("SetDefaultChannelEventsTable", DebugLogTypes.DataManager, $"Setting default channel events, adding any missing events.");

            Dictionary<ChannelEventActions, Tuple<string, string>> dictionary = new()
                {
                    {
                        ChannelEventActions.BeingHosted,
                        new(LocalizedMsgSystem.GetEventMsg(ChannelEventActions.BeingHosted, out _, out _), VariableParser.ConvertVars([MsgVars.user, MsgVars.autohost, MsgVars.viewers]))
                    },
                    {
                        ChannelEventActions.Bits,
                        new(LocalizedMsgSystem.GetEventMsg(ChannelEventActions.Bits, out _, out _), VariableParser.ConvertVars([MsgVars.user, MsgVars.bits]))
                    },
                    {
                        ChannelEventActions.CommunitySubs,
                        new(LocalizedMsgSystem.GetEventMsg(ChannelEventActions.CommunitySubs, out _, out _), VariableParser.ConvertVars([MsgVars.user, MsgVars.count, MsgVars.subplan]))
                    },
                    {
                        ChannelEventActions.NewFollow,
                        new(LocalizedMsgSystem.GetEventMsg(ChannelEventActions.NewFollow, out _, out _), VariableParser.ConvertVars([MsgVars.user]))
                    },
                    {
                        ChannelEventActions.GiftSub,
                        new(LocalizedMsgSystem.GetEventMsg(ChannelEventActions.GiftSub, out _, out _), VariableParser.ConvertVars([MsgVars.user, MsgVars.months, MsgVars.receiveuser, MsgVars.subplan, MsgVars.subplanname]))
                    },
                    {
                        ChannelEventActions.Live,
                        new(LocalizedMsgSystem.GetEventMsg(ChannelEventActions.Live, out _, out _), VariableParser.ConvertVars([MsgVars.user, MsgVars.category, MsgVars.title, MsgVars.url, MsgVars.everyone]))
                    },
                    {
                        ChannelEventActions.Raid,
                        new(LocalizedMsgSystem.GetEventMsg(ChannelEventActions.Raid, out _, out _), VariableParser.ConvertVars([MsgVars.user, MsgVars.viewers]))
                    },
                    {
                        ChannelEventActions.Resubscribe,
                        new(LocalizedMsgSystem.GetEventMsg(ChannelEventActions.Resubscribe, out _, out _), VariableParser.ConvertVars([MsgVars.user, MsgVars.months, MsgVars.submonths, MsgVars.subplan, MsgVars.subplanname, MsgVars.streak]))
                    },
                    {
                        ChannelEventActions.Subscribe,
                        new(LocalizedMsgSystem.GetEventMsg(ChannelEventActions.Subscribe, out _, out _), VariableParser.ConvertVars([MsgVars.user, MsgVars.submonths, MsgVars.subplan, MsgVars.subplanname]))
                    },
                    {
                        ChannelEventActions.UserJoined,
                        new(LocalizedMsgSystem.GetEventMsg(ChannelEventActions.UserJoined, out _, out _), VariableParser.ConvertVars([MsgVars.user]))
                    },
                    {
                        ChannelEventActions.ReturnUserJoined,
                        new(LocalizedMsgSystem.GetEventMsg(ChannelEventActions.ReturnUserJoined, out _, out _), VariableParser.ConvertVars([MsgVars.user]))
                    },
                    {
                        ChannelEventActions.SupporterJoined,
                        new(LocalizedMsgSystem.GetEventMsg(ChannelEventActions.SupporterJoined, out _, out _), VariableParser.ConvertVars([MsgVars.user]))
                    },
                    {
                        ChannelEventActions.BannedUser,
                        new(LocalizedMsgSystem.GetEventMsg(ChannelEventActions.BannedUser, out _, out _), VariableParser.ConvertVars([MsgVars.user]))
                    }
                };

            using var transaction = context.Database.BeginTransaction();
            context.ChannelEvents.AddRange(from CE in from E in dictionary.ExceptBy(context.ChannelEvents.Select(C => C.Name), E => E.Key)
                                                         let values = dictionary[E.Key]
                                                         select (E.Key, values)
                                              select new ChannelEvents(name: CE.Key, repeatMsg: 0, addMe: false, isEnabled: true, message: CE.values.Item1, commands: CE.values.Item2));
            await transaction.CommitAsync();
            context.SaveChanges(true);
        }

        /// <summary>
        /// Add all of the default commands to the table, ensure they are available
        /// </summary>
        private async Task SetDefaultCommandsTable(IDbContextTransaction contextTransaction = null)
        {
            LogWriter.DebugLog("SetDefaultCommandsTable", DebugLogTypes.DataManager, $"Setting up and checking default commands, adding missing commands.");

            //using var transaction = await context.Database.BeginTransactionAsync();

            if (!(from C in context.CategoryList where C.Category == LocalizedMsgSystem.GetVar(Msg.MsgAllCategory) select C).Any())
            {
                await context.CategoryList.AddAsync(new(categoryId: "0", category: LocalizedMsgSystem.GetVar(Msg.MsgAllCategory), streamCount: 0));
            }

            // dictionary with commands, messages, and parameters
            // command name     // msg   // params
            Dictionary<string, Tuple<string, string>> DefCommandsDictionary = [];

            // add each of the default commands with localized strings
            foreach (DefaultCommand com in Enum.GetValues(typeof(DefaultCommand)))
            {
                DefCommandsDictionary.Add(com.ToString(), new(LocalizedMsgSystem.GetDefaultComMsg(com), LocalizedMsgSystem.GetDefaultComParam(com)));
            }

            // add each of the social commands
            foreach (DefaultSocials social in Enum.GetValues(typeof(DefaultSocials)))
            {
                DefCommandsDictionary.Add(social.ToString(), new(DefaulSocialMsg, LocalizedMsgSystem.GetVar("Parameachsocial")));
            }

            if (context.CommandsBase.Any())
            {
                foreach (var C in from C in context.CommandsBase select C)
                {
                    DefCommandsDictionary.Remove(C.CmdName);
                }
            }

            await context.Commands.AddRangeAsync(from C in (from key in DefCommandsDictionary
                                                    let param = CommandParams.Parse(DefCommandsDictionary[key.Key].Item2)
                                                    select (key.Key, param))
                                         select new Commands(cmdName: C.Key,
                                                    addMe: false,
                                                    permission: C.param.Permission,
                                                    isEnabled: C.param.IsEnabled,
                                                    announce: false,
                                                    message: DefCommandsDictionary[C.Key].Item1,
                                                    repeatTimer: C.param.Timer,
                                                    sendMsgCount: C.param.RepeatMsg,
                                                    category: [string.IsNullOrEmpty(C.param.Category) ?
                                                                 LocalizedMsgSystem.GetVar(Msg.MsgAllCategory) :
                                                                 C.param.Category],
                                                    allowParam: C.param.AllowParam,
                                                    usage: C.param.Usage,
                                                    lookupData: C.param.LookupData,
                                                    table: C.param.Table,
                                                    keyField: !string.IsNullOrEmpty(C.param.Table) ? GetKey(C.param.Table).Result : "",
                                                    dataField: C.param.Field,
                                                    currencyField: C.param.Currency,
                                                    unit: C.param.Unit,
                                                    action: C.param.Action,
                                                    top: C.param.Top,
                                                    sort: C.param.Sort)
             );

            //await transaction.CommitAsync();

            await context.SaveChangesAsync(true);
        }
        private async Task SetLearnedMessages(IDbContextTransaction contextTransaction = null)
        {
            LogWriter.DebugLog("SetLearnedMessages", DebugLogTypes.DataManager, $"Machine learning, setting learned messages.");

            //using var transaction = await context.Database.BeginTransactionAsync();

            if (!context.LearnMsgs.Any())
            {
                await context.LearnMsgs.AddRangeAsync(from M in LearnedMessagesPrimer.PrimerList
                                                      select new LearnMsgs(msgType: M.MsgType, teachingMsg: M.Message));
            }

            if (!context.BanReasons.Any())
            {
                await context.BanReasons.AddRangeAsync(from B in LearnedMessagesPrimer.BanReasonList
                                                       select new Models.BanReasons(msgType: B.MsgType, banReason: B.Reason));
            }

            if (!context.BanRules.Any())
            {
                await context.BanRules.AddRangeAsync(from R in LearnedMessagesPrimer.BanViewerRulesList
                                                     select new BanRules(0, R.ViewerType, R.MsgType, R.ModAction, R.TimeoutSeconds));
            }

            //await transaction.CommitAsync();
            await context.SaveChangesAsync(true);
        }

        private async Task CleanCategories(IDbContextTransaction contextTransaction = null)
        {
            List<CategoryList> CatsToReplace = [];

            //using var transaction = await context.Database.BeginTransactionAsync();

            foreach (CategoryList C in context.CategoryList)
            {
                if (C.Category.Contains("''''"))
                {
                    CatsToReplace.Add(C);
                }
            }

            context.CategoryList.RemoveRange(CatsToReplace);
            CatsToReplace.ForEach((c) => c.Category = FormatData.AddEscapeFormat(c.Category));
            context.CategoryList.AddRange(CatsToReplace);

            foreach (var CU in context.CommandsBase)
            {
                if (CU.Category is null || CU.Category.Contains(""))
                {
                    CU.Category.Clear();
                    CU.Category.Add("All");
                }

                if (CU.Category.Count > 1)
                {
                    List<string> Temp = new(CU.Category);
                    CU.Category.Clear();

                    foreach (string s in Temp)
                    {
                        CU.Category.UniqueAdd(s.Trim());
                    }
                }

            }

            //await transaction.CommitAsync();
            await context.SaveChangesAsync(true);
        }

        #endregion
    }
}
