using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

using StreamerBotLib.DataSQL.Models;
using StreamerBotLib.Models;
using StreamerBotLib.Models.Enums;
using StreamerBotLib.Models.Interfaces;
using StreamerBotLib.Static;
using StreamerBotLib.Systems;

using System.Data;
using System.Globalization;

namespace StreamerBotLib.DataSQL.MultiContext
{
    internal partial class DataManagerSQLAsync
    {
        #region Post_Methods

        internal async Task PostDataGridGUIAddRow(IDatabaseTableMeta tableMeta)
        {
            using var context = BuildDataContext();

            try
            {
                // show all, but GUI access-controls will prevent these certain tables from having new rows (channel events doesn't need a new row for another event, except API driven and application will already update it)
                if (tableMeta.TableName == "BanReasons")
                {
                    await context.BanReasons.AddAsync((Models.BanReasons)tableMeta.GetModelEntity());
                    await context.SaveChangesAsync();
                    await RefreshBanReasonsList(true);
                }
                else if (tableMeta.TableName == "BanRules")
                {
                    await context.BanRules.AddAsync((Models.BanRules)tableMeta.GetModelEntity());
                    await context.SaveChangesAsync();
                    await RefreshBanRulesList(true);
                }
                else if (tableMeta.TableName == "CategoryList")
                {
                    await context.CategoryList.AddAsync((Models.CategoryList)tableMeta.GetModelEntity());
                    await context.SaveChangesAsync();
                    await RefreshCategoryListList(true);
                }
                else if (tableMeta.TableName == "ChannelEvents")
                {
                    await context.ChannelEvents.AddAsync((Models.ChannelEvents)tableMeta.GetModelEntity());
                    await context.SaveChangesAsync();
                    await RefreshChannelEventsList(true);
                }
                else if (tableMeta.TableName == "Clips")
                {
                    await context.Clips.AddAsync((Models.Clips)tableMeta.GetModelEntity());
                    await context.SaveChangesAsync();
                    await RefreshClipsList(true);
                }
                else if (tableMeta.TableName == "Commands")
                {
                    await context.Commands.AddAsync((Models.Commands)tableMeta.GetModelEntity());
                    await context.SaveChangesAsync();
                    await RefreshCommandsList(true);
                }
                else if (tableMeta.TableName == "CommandsUser")
                {
                    await context.CommandsUser.AddAsync((Models.CommandsUser)tableMeta.GetModelEntity());
                    await context.SaveChangesAsync();
                    await RefreshCommandsUserList(true);
                }
                else if (tableMeta.TableName == "Currency")
                {
                    await context.Currency.AddAsync((Models.Currency)tableMeta.GetModelEntity());
                    await context.SaveChangesAsync();
                    await RefreshCurrencyList(true);
                }
                else if (tableMeta.TableName == "CurrencyType")
                {
                    await context.CurrencyType.AddAsync((Models.CurrencyType)tableMeta.GetModelEntity());
                    await context.SaveChangesAsync();
                    await RefreshCurrencyTypeList(true);
                }
                else if (tableMeta.TableName == "CustomWelcome")
                {
                    await context.CustomWelcome.AddAsync((Models.CustomWelcome)tableMeta.GetModelEntity());
                    await context.SaveChangesAsync();
                    await RefreshCustomWelcomeList(true);
                }
                else if (tableMeta.TableName == "Followers")
                {
                    await context.Followers.AddAsync((Models.Followers)tableMeta.GetModelEntity());
                    await context.SaveChangesAsync();
                    await RefreshFollowersList(true);
                }
                else if (tableMeta.TableName == "GameDeadCounter")
                {
                    await context.GameDeadCounter.AddAsync((Models.GameDeadCounter)tableMeta.GetModelEntity());
                    await context.SaveChangesAsync();
                    await RefreshGameDeadCounterList(true);
                }
                else if (tableMeta.TableName == "GiveawayUserData")
                {
                    await context.GiveawayUserData.AddAsync((Models.GiveawayUserData)tableMeta.GetModelEntity());
                    await context.SaveChangesAsync();
                    await RefreshGiveawayUserDataList(true);
                }
                else if (tableMeta.TableName == "InRaidData")
                {
                    await context.InRaidData.AddAsync((Models.InRaidData)tableMeta.GetModelEntity());
                    await context.SaveChangesAsync();
                    await RefreshInRaidDataList(true);
                }
                else if (tableMeta.TableName == "LearnMsgs")
                {
                    await context.LearnMsgs.AddAsync((Models.LearnMsgs)tableMeta.GetModelEntity());
                    await context.SaveChangesAsync();
                    await RefreshLearnMsgsList(true);
                }
                else if (tableMeta.TableName == "ModeratorApprove")
                {
                    await context.ModeratorApprove.AddAsync((Models.ModeratorApprove)tableMeta.GetModelEntity());
                    await context.SaveChangesAsync();
                    await RefreshModeratorApproveList(true);
                }
                else if (tableMeta.TableName == "MultiChannels")
                {
                    await context.MultiChannels.AddAsync((Models.MultiChannels)tableMeta.GetModelEntity());
                    await context.SaveChangesAsync();
                    await RefreshMultiChannelsList(true);
                }
                else if (tableMeta.TableName == "MultiLiveStreams")
                {
                    await context.MultiLiveStreams.AddAsync((Models.MultiLiveStreams)tableMeta.GetModelEntity());
                    await context.SaveChangesAsync();
                    await RefreshMultiLiveStreamsList(true);
                }
                else if (tableMeta.TableName == "MultiSummaryLiveStreams")
                {
                    await context.MultiSummaryLiveStreams.AddAsync((Models.MultiSummaryLiveStreams)tableMeta.GetModelEntity());
                    await context.SaveChangesAsync();
                    await RefreshMultiSummaryLiveStreamsList(true);
                }
                else if (tableMeta.TableName == "MultiWebhooks")
                {
                    await context.MultiWebhooks.AddAsync((Models.MultiWebhooks)tableMeta.GetModelEntity());
                    await context.SaveChangesAsync();
                    await RefreshMultiWebhooksList(true);
                }
                else if (tableMeta.TableName == "OldFollowUsers")
                {
                    await context.OldFollowUsers.AddAsync((Models.OldFollowUsers)tableMeta.GetModelEntity());
                    await context.SaveChangesAsync();
                    await RefreshOldFollowUsersList(true);
                }
                else if (tableMeta.TableName == "OutRaidData")
                {
                    await context.OutRaidData.AddAsync((Models.OutRaidData)tableMeta.GetModelEntity());
                    await context.SaveChangesAsync();
                    await RefreshOutRaidDataList(true);
                }
                else if (tableMeta.TableName == "OverlayServices")
                {
                    await context.OverlayServices.AddAsync((Models.OverlayServices)tableMeta.GetModelEntity());
                    await context.SaveChangesAsync();
                    await RefreshOverlayServicesList(true);
                }
                else if (tableMeta.TableName == "OverlayTicker")
                {
                    await context.OverlayTicker.AddAsync((Models.OverlayTicker)tableMeta.GetModelEntity());
                    await context.SaveChangesAsync();
                    await RefreshOverlayTickerList(true);
                }
                else if (tableMeta.TableName == "Quotes")
                {
                    await context.Quotes.AddAsync((Models.Quotes)tableMeta.GetModelEntity());
                    await context.SaveChangesAsync();
                    await RefreshQuotesList(true);
                }
                else if (tableMeta.TableName == "ShoutOuts")
                {
                    await context.ShoutOuts.AddAsync((Models.ShoutOuts)tableMeta.GetModelEntity());
                    await context.SaveChangesAsync();
                    await RefreshShoutOutsList(true);
                }
                else if (tableMeta.TableName == "StreamStats")
                {
                    await context.StreamStats.AddAsync((Models.StreamStats)tableMeta.GetModelEntity());
                    await context.SaveChangesAsync();
                    await RefreshStreamStatsList(true);
                }
                else if (tableMeta.TableName == "Users")
                {
                    await context.Users.AddAsync((Models.Users)tableMeta.GetModelEntity());
                    await context.SaveChangesAsync();
                    await RefreshUsersList(true);
                }
                else if (tableMeta.TableName == "UserStats")
                {
                    await context.UserStats.AddAsync((Models.UserStats)tableMeta.GetModelEntity());
                    await context.SaveChangesAsync();
                    await RefreshUserStatsList(true);
                }
                else if (tableMeta.TableName == "Webhooks")
                {
                    await context.Webhooks.AddAsync((Models.Webhooks)tableMeta.GetModelEntity());
                    await context.SaveChangesAsync();
                    await RefreshWebhooksList(true);
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex, "PostDataGridGUIAddRow");
            }
        }

        internal async Task<bool> PostCategory(CategoryData categoryData)
        {
            if (string.IsNullOrEmpty(categoryData.CategoryId) && string.IsNullOrEmpty(categoryData.CategoryName))
            {
                return false;
            }

            using var context = BuildDataContext();
            var categoryExists = await context.CategoryList
                .AnyAsync(CL => CL.Category == FormatData.AddEscapeFormat(categoryData.CategoryName) || CL.CategoryId == categoryData.CategoryId);

            if (!categoryExists)
            {
                await context.CategoryList.AddAsync(new(categoryId: categoryData.CategoryId, category: categoryData.CategoryName, streamCount: 0));
                await context.SaveChangesAsync();
                await RefreshCategoryListList(true);
                return true;
            }

            return false;
        }

        internal async Task PostCategoryStream(CategoryData categoryData, int StreamCount = 0)
        {
            using var context = BuildDataContext();
            await PostCategory(categoryData);

            if (OptionFlags.IsStreamOnline)
            {
                CategoryList category = await context.CategoryList
                                                     .Where(CL => (CL.Category == FormatData.AddEscapeFormat(categoryData.CategoryName)) || CL.CategoryId == categoryData.CategoryId)
                                                     .Select(CL => CL).FirstOrDefaultAsync();
                category.StreamCount++;
                await context.SaveChangesAsync();
                await RefreshCategoryListList(true);
            }
        }

        internal async Task<bool> PostClip(string ClipId, DateTime CreatedAt, decimal Duration, string GameId, string Language, string Title, string Url, string fromUserId, string fromUserName)
        {
            bool result;
            using var context = BuildDataContext();
            if (!await context.Clips
                  .Where(C => C.ClipId == ClipId)
                  .Select(C => C).AnyAsync())
            {
                await context.Clips.AddAsync(new(clipId: ClipId, createdAt: CreatedAt, title: Title, categoryId: GameId, language: Language, duration: (float)Duration, url: Url));
                await context.SaveChangesAsync();
                await RefreshClipsList(true);
                result = true;
            }
            result = false;

            return result;
        }

        internal async Task<string> PostCommand(string cmd, CommandParams Params)
        {
            string result;
            using var context = BuildDataContext();
            if (!await context.CommandsBase
                  .Where(Com => Com.CmdName == cmd)
                  .Select(Com => Com).AnyAsync())
            {
                await context.CommandsUser.AddAsync(new(cmdName: cmd, addMe: Params.AddMe, permission: Params.Permission,
                     isEnabled: Params.IsEnabled, message: Params.Message, repeatTimer: Params.Timer, sendMsgCount: Params.RepeatMsg, category: [Params.Category],
                     allowParam: Params.AllowParam, usage: Params.Usage, lookupData: Params.LookupData, table: Params.Table, keyField: GetKey(Params.Table).Result,
                     dataField: Params.Field, currencyField: Params.Currency, unit: Params.Unit, action: Params.Action, top: Params.Top, sort: Params.Sort));

                await context.SaveChangesAsync();
                await RefreshCommandsUserList(true);
                result = string.Format(CultureInfo.CurrentCulture, LocalizedMsgSystem.GetDefaultComMsg(DefaultCommand.addcommand), cmd);
            }
            result = string.Format(CultureInfo.CurrentCulture, LocalizedMsgSystem.GetVar(Msg.MsgAddCommandFailed), cmd);

            return result;
        }

        /// <summary>
        /// Post a new currency type to the database. Will add new currency records for existing users.
        /// </summary>
        /// <param name="currencyType"></param>
        internal async Task PostCurrencyType(Models.CurrencyType currencyType)
        {
            bool recordchange = false;
            using var context = BuildDataContext();

            if (!await context.CurrencyType
                                .Include(curr => curr.Currency)
                                .Where(CT => CT.CurrencyName == currencyType.CurrencyName)
                                .Select(CT => CT).AnyAsync())
            {
                var newCurrType = await context.CurrencyType.AddAsync(currencyType);
                recordchange = true;

                foreach (Users U in context.Users.Include(curr => curr.Currency))
                {
                    U.Currency.Add(new(U.UserId, U.Platform, 0, newCurrType.Entity.CurrencyName));
                }

                await context.SaveChangesAsync();
                await RefreshCurrencyTypeList(recordchange);
                await RefreshCurrencyList(recordchange);
            }
        }

        internal async Task PostCurrencyUpdate(LiveUser User, double value, string CurrencyName)
        {
#if DEBUG
            double oldValueCurrency = 0, newValueCurrency = -1;
#endif

            using var context = BuildDataContext();
            Currency currency = await context.Currency
                                                .Include(type => type.CurrencyType)
                                                .Where(C => (C.CurrencyName == CurrencyName && C.UserId == User.UserId))
                                                .Select(C => C).FirstOrDefaultAsync();
            if (currency != default)
            {
#if DEBUG
                oldValueCurrency = currency.Value;
#endif

                currency.Value = Math.Min(Math.Round(currency.Value + value, 2), currency.CurrencyType.MaxValue);

#if DEBUG
                newValueCurrency = currency.Value;
#endif
                await context.SaveChangesAsync();
                await RefreshCurrencyList();
            }

#if DEBUG
            Currency UpdatedCurrency = await context.Currency
                                        .Where(C => (C.CurrencyName == CurrencyName && C.UserId == User.UserId))
                                        .Select(C => C).FirstOrDefaultAsync();

            Currency GUIContextCurrency = await GUIContext.Currency
                                           .Where(C => (C.CurrencyName == CurrencyName && C.UserId == User.UserId))
                                           .Select(C => C).FirstOrDefaultAsync();

            LogWriter.DebugLog("PostCurrencyUpdate", DebugLogTypes.SpecialPurpose, $"OldValueCurrency: {oldValueCurrency}, NewValueCurrency: {newValueCurrency}");
            LogWriter.DebugLog("PostCurrencyUpdate", DebugLogTypes.SpecialPurpose, $"UpdatedCurrency.Value: {UpdatedCurrency?.Value}, GUIContextCurrency.Value: {GUIContextCurrency?.Value}");
#endif

        }

        internal async Task PostCurrencyUpdate(List<PlayGameUserWager<PlayingCardFrench, PlayingCardSuit>> Updates, string CurrencyName)
        {
            using var context = BuildDataContext();

            foreach (var Player in Updates)
            {
                foreach (Currency CurrUser in await context.Currency
                                                            .Include(type => type.CurrencyType)
                                                            .Where(C => C.CurrencyName == CurrencyName && C.UserId == Player.Player.UserId)
                                                            .Select(C => C)
                                                            .ToListAsync())
                {
                    CurrUser.Value = Math.Min(Math.Round(CurrUser.Value + Player.Payout, 2), CurrUser.CurrencyType.MaxValue);
                }
            }

            await context.SaveChangesAsync();
            await RefreshCurrencyList();
        }

        internal async Task<int> PostDeathCounterUpdate(string currCategory, bool Reset = false, int updateValue = 1)
        {
            bool recordchange = false;
            using var context = BuildDataContext();
            GameDeadCounter update = await context.GameDeadCounter
                                        .Include(category => category.CategoryList)
                                      .Where(G => G.Category == currCategory)
                                      .Select(G => G).FirstOrDefaultAsync();

            if (update != default)
            {
                if (Reset)
                {
                    update.Counter = updateValue;
                }
                else
                {
                    update.Counter += updateValue;
                }
            }
            else
            {
                update = context.GameDeadCounter.Add(new(category: currCategory)).Entity;
                recordchange = true;
            }
            await context.SaveChangesAsync();
            await RefreshGameDeadCounterList(recordchange);

            return update?.Counter ?? 0;
        }

        internal async Task PostGiveawayData(string UserId, DateTime dateTime)
        {
            using var context = BuildDataContext();
            await context.GiveawayUserData.AddAsync(new(dateTime: dateTime, userId: UserId));
            await context.SaveChangesAsync();

            await RefreshGiveawayUserDataList(true);
        }

        internal async Task PostInRaidData(LiveUser user, DateTime time, int viewers, CategoryData gamename)
        {
            using var context = BuildDataContext();
            await PostNewUser(user, time);
            await PostCategory(gamename);
            await context.InRaidData.AddAsync(new(userId: user.UserId, raidDate: time, viewerCount: viewers, category: gamename.CategoryName, platform: user.Platform));
            await context.SaveChangesAsync();
            await RefreshUsersList();
            await RefreshCategoryListList(true);
            await RefreshInRaidDataList(true);
        }

        internal async Task PostLearnMsgsRow(string Message, MsgTypes MsgType)
        {
            using var context = BuildDataContext();
            if (!await context.LearnMsgs
                  .Where(M => M.TeachingMsg == Message)
                  .Select(M => M).AnyAsync())
            {
                await context.LearnMsgs.AddAsync(new(msgType: MsgType, teachingMsg: Message));
                LearnMsgChanged = true;
            }
            await context.SaveChangesAsync();
            await RefreshLearnMsgsList(LearnMsgChanged);
        }

        internal async Task<bool> PostMergeUserStats(string currUser, string sourceUser, Platform platform)
        {
            using var context = BuildDataContext();

            // Fetch user currencies and group by CurrencyName for efficient merging
            var userCurrencies = await context.Currency
                .Include(c => c.User)
                .Where(c => c.User.UserName == currUser)
                .ToListAsync();

            var sourceCurrencies = await context.Currency
                .Include(c => c.User)
                .Where(c => c.User.UserName == sourceUser)
                .ToListAsync();

            var currencyMap = userCurrencies.ToDictionary(c => c.CurrencyName);

            foreach (var sourceCurrency in sourceCurrencies)
            {
                if (currencyMap.TryGetValue(sourceCurrency.CurrencyName, out var userCurrency))
                {
                    userCurrency.Add(sourceCurrency);
                }
                else
                {
                    userCurrencies.Add(sourceCurrency);
                }
            }

            context.Currency.RemoveRange(sourceCurrencies);

            // Fetch user stats
            var currUserStats = await context.UserStats
                .Include(us => us.User)
                .FirstOrDefaultAsync(us => us.User.UserName == currUser && us.Platform == platform);

            var sourceUserStats = await context.UserStats
                .Include(us => us.User)
                .FirstOrDefaultAsync(us => us.User.UserName == sourceUser && us.Platform == platform);

            if (currUserStats != null && sourceUserStats != null)
            {
                currUserStats += sourceUserStats;
                context.UserStats.Remove(sourceUserStats);
                await context.SaveChangesAsync();

                await RefreshCurrencyList(true);
                await RefreshUsersList(true);

                return true;
            }

            await RefreshCurrencyList(true);
            await RefreshUsersList(true);

            return false;
        }

        internal async Task PostNewAutoShoutUser(string UserId, Platform platform)
        {
            bool recordchange = false;
            using var context = BuildDataContext();
            if (!await context.ShoutOuts
            .Where(SO => (SO.UserId == UserId && SO.Platform == platform))
            .Select(SO => SO).AnyAsync())
            {
                await context.ShoutOuts.AddAsync(new(userId: UserId, platform: platform));
                recordchange = true;
            }
            await context.SaveChangesAsync();
            await RefreshShoutOutsList(recordchange);
        }

        internal async Task PostNewAutoShoutUser(LiveUser liveUser)
        {
            await PostNewAutoShoutUser(liveUser.UserId, liveUser.Platform);
        }

        //internal void PostNewData(TableMeta.TableMeta tableMeta)
        //{
        //    lock (GUIDataManagerLock.Lock)
        //    {
        //        SQLDBContext context = Refcontext ?? BuildDataContext();

        //        switch (tableMeta.CurrEntity.TableName)
        //        {
        //            case "BanReasons":
        //                context.BanReasons.Add((Models.BanReasons)tableMeta.DataEntity);
        //                break;
        //            case "BanRules":
        //                context.BanRules.Add((BanRules)tableMeta.DataEntity);
        //                break;
        //            case "CategoryList":
        //                context.CategoryList.Add((CategoryList)tableMeta.DataEntity);
        //                break;
        //            case "ChannelEvents":
        //                context.ChannelEvents.Add((ChannelEvents)tableMeta.DataEntity);
        //                break;
        //            case "Clips":
        //                context.Clips.Add((Clips)tableMeta.DataEntity);
        //                break;
        //            case "Commands":
        //                context.Commands.Add((Commands)tableMeta.DataEntity);
        //                break;
        //            case "CommandsBase":
        //                context.CommandsBase.Add((CommandsBase)tableMeta.DataEntity);
        //                break;
        //            case "CommandsUser":
        //                context.CommandsUser.Add((CommandsUser)tableMeta.DataEntity);
        //                break;
        //            case "Currency":
        //                context.Currency.Add((Currency)tableMeta.DataEntity);
        //                break;
        //            case "CurrencyType":
        //                context.CurrencyType.Add((Models.CurrencyType)tableMeta.DataEntity);
        //                break;
        //            case "CustomWelcome":
        //                context.CustomWelcome.Add((CustomWelcome)tableMeta.DataEntity);
        //                break;
        //            case "Followers":
        //                context.Followers.Add((Followers)tableMeta.DataEntity);
        //                break;
        //            case "GameDeadCounter":
        //                context.GameDeadCounter.Add((GameDeadCounter)tableMeta.DataEntity);
        //                break;
        //            case "GiveawayUserData":
        //                context.GiveawayUserData.Add((GiveawayUserData)tableMeta.DataEntity);
        //                break;
        //            case "InRaidData":
        //                context.InRaidData.Add((InRaidData)tableMeta.DataEntity);
        //                break;
        //            case "LearnMsgs":
        //                context.LearnMsgs.Add((LearnMsgs)tableMeta.DataEntity);
        //                break;
        //            case "ModeratorApprove":
        //                context.ModeratorApprove.Add((ModeratorApprove)tableMeta.DataEntity);
        //                break;
        //            case "MultiChannels":
        //                context.MultiChannels.Add((MultiChannels)tableMeta.DataEntity);
        //                break;
        //            case "MultiLiveStreams":
        //                context.MultiLiveStreams.Add((MultiLiveStreams)tableMeta.DataEntity);
        //                break;
        //            case "MultiSummaryLiveStreams":
        //                context.MultiSummaryLiveStreams.Add((MultiSummaryLiveStreams)tableMeta.DataEntity);
        //                break;
        //            case "MultiWebhooks":
        //                context.MultiWebhooks.Add((MultiWebhooks)tableMeta.DataEntity);
        //                break;
        //            case "OldFollowUsers":
        //                context.OldFollowUsers.Add((OldFollowUsers)tableMeta.DataEntity);
        //                break;
        //            case "OutRaidData":
        //                context.OutRaidData.Add((OutRaidData)tableMeta.DataEntity);
        //                break;
        //            case "OverlayServices":
        //                context.OverlayServices.Add((OverlayServices)tableMeta.DataEntity);
        //                break;
        //            case "OverlayTicker":
        //                context.OverlayTicker.Add((OverlayTicker)tableMeta.DataEntity);
        //                break;
        //            case "Quotes":
        //                context.Quotes.Add((Quotes)tableMeta.DataEntity);
        //                break;
        //            case "ShoutOuts":
        //                context.ShoutOuts.Add((ShoutOuts)tableMeta.DataEntity);
        //                break;
        //            case "StreamStats":
        //                context.StreamStats.Add((StreamStats)tableMeta.DataEntity);
        //                break;
        //            case "Users":
        //                context.Users.Add((Users)tableMeta.DataEntity);
        //                break;
        //            case "UserStats":
        //                context.UserStats.Add((UserStats)tableMeta.DataEntity);
        //                break;
        //            case "Webhooks":
        //                context.Webhooks.Add((Webhooks)tableMeta.DataEntity);
        //                break;
        //            case "WebhooksBase":
        //                context.WebhooksBase.Add((WebhooksBase)tableMeta.DataEntity);
        //                break;
        //        }

        //            context.SaveChangesAsync();
        //        if (Refcontext == null) { ClearDataContext(context); }
        //    }
        //}

        internal async Task PostOutgoingRaid(string HostedChannel, DateTime dateTime)
        {
            using var context = BuildDataContext();
            await context.OutRaidData.AddAsync(new(channelRaided: HostedChannel, raidDate: dateTime));
            await context.SaveChangesAsync();
            await RefreshOutRaidDataList(true);
        }

        internal async Task<int> PostQuote(string Text)
        {
            using var context = BuildDataContext();

            // Fetch all existing quote numbers
            var existingNumbers = await context.Quotes.Select(q => q.Number).ToListAsync();

            // Find the smallest missing number
            int openNum = Enumerable.Range(1, existingNumbers.Count > 0 ? existingNumbers.Max() + 1 : 1)
                                    .Except(existingNumbers)
                                    .First();

            // Add the new quote
            await context.Quotes.AddAsync(new(number: openNum, quote: Text));
            await context.SaveChangesAsync();

            // Refresh the quotes list
            await RefreshQuotesList(true);

            return openNum;
        }

        /// <summary>
        /// Starts a new Stream record, if it doesn't currently exist.
        /// </summary>
        /// <param name="StreamStart">The time of stream start.</param>
        /// <returns><code>true: for posting a new stream start;</code> <code>false: when a stream start date row already exists</code></returns>
        internal async Task<bool> PostStream(DateTime StreamStart, string Category)
        {
            using var context = BuildDataContext();
            bool addstream = !await context.StreamStats.Where(S => S.StreamStart == StreamStart).Select(S => S).AnyAsync();
            if (addstream)
            {
                await context.StreamStats.AddAsync(new(streamStart: StreamStart, streamEnd: StreamStart));
                await context.SaveChangesAsync();
                await RefreshStreamStatsList(true);
            }

            CurrStreamStart = StreamStart;

            return addstream;
        }

        internal async Task PostStreamStat(StreamStat streamStat)
        {
            using var context = BuildDataContext();
            StreamStats currStream = await context.StreamStats
                                      .Where(S => S.StreamStart == streamStat.StreamStart)
                                      .Select(S => S)
                                      .FirstOrDefaultAsync();
            if (currStream != default)
            {
                currStream.Update(streamStat);
                LogWriter.DebugLog("PostStreamStat", DebugLogTypes.DataManager, $"Updated stream stats for stream started {streamStat.StreamStart}.");
                await context.SaveChangesAsync();
                await RefreshStreamStatsList();
            }
        }

        /// <summary>
        /// Add a custom welcome message for a specific user. Does not edit existing welcome message.
        /// </summary>
        /// <param name="User">The user to add the custom welcome message.</param>
        /// <param name="WelcomeMsg">The message for the user.</param>
        internal async Task PostUserCustomWelcome(LiveUser User, string WelcomeMsg)
        {
            using var context = BuildDataContext();
            if (!await context.CustomWelcome.Where(W => W.UserId == User.UserId).Select(W => W).AnyAsync())
            {
                await context.CustomWelcome.AddAsync(new(userId: User.UserId, platform: User.Platform, message: WelcomeMsg));
                await context.SaveChangesAsync();
                await RefreshCustomWelcomeList(true);
            }
        }

        #endregion Post_Methods

    }
}
