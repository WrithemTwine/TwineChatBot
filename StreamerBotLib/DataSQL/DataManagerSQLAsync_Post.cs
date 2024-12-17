using Microsoft.EntityFrameworkCore.Metadata;

using StreamerBotLib.DataSQL.Models;
using StreamerBotLib.Enums;
using StreamerBotLib.Interfaces;
using StreamerBotLib.Models;
using StreamerBotLib.Static;
using StreamerBotLib.Systems;

using System.Data;
using System.Globalization;

namespace StreamerBotLib.DataSQL
{
    internal partial class DataManagerSQLAsync
    {
        #region Post_Methods

        internal Task PostDataGridGUIAddRow(IDatabaseTableMeta tableMeta, SQLDBContext Refcontext = null)
        {
            return Task.Run(async () =>
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();

                try
                {
                    // show all, but GUI access-controls will prevent these certain tables from having new rows (channel events doesn't need a new row for another event, except API driven and application will already update it)
                    if (tableMeta.TableName == "BanReasons")
                    {
                        await context.BanReasons.AddAsync((Models.BanReasons)tableMeta.GetModelEntity());
                        await context.SaveChangesAsync();
                        RefreshBanReasonsObservableCollection();
                    }
                    else if (tableMeta.TableName == "BanRules")
                    {
                        await context.BanRules.AddAsync((Models.BanRules)tableMeta.GetModelEntity());
                        await context.SaveChangesAsync();
                        RefreshBanRulesObservableCollection();
                    }
                    else if (tableMeta.TableName == "CategoryList")
                    {
                        await context.CategoryList.AddAsync((Models.CategoryList)tableMeta.GetModelEntity());
                        await context.SaveChangesAsync();
                        RefreshCategoryListObservableCollection();
                    }
                    else if (tableMeta.TableName == "ChannelEvents")
                    {
                        await context.ChannelEvents.AddAsync((Models.ChannelEvents)tableMeta.GetModelEntity());
                        await context.SaveChangesAsync();
                        RefreshChannelEventsObservableCollection();
                    }
                    else if (tableMeta.TableName == "Clips")
                    {
                        await context.Clips.AddAsync((Models.Clips)tableMeta.GetModelEntity());
                        await context.SaveChangesAsync();
                        RefreshClipsObservableCollection();
                    }
                    else if (tableMeta.TableName == "Commands")
                    {
                        await context.Commands.AddAsync((Models.Commands)tableMeta.GetModelEntity());
                        await context.SaveChangesAsync();
                        RefreshCommandsObservableCollection();
                    }
                    else if (tableMeta.TableName == "CommandsUser")
                    {
                        await context.CommandsUser.AddAsync((Models.CommandsUser)tableMeta.GetModelEntity());
                        await context.SaveChangesAsync();
                        RefreshCommandsUserObservableCollection();
                    }
                    else if (tableMeta.TableName == "Currency")
                    {
                        await context.Currency.AddAsync((Models.Currency)tableMeta.GetModelEntity());
                        await context.SaveChangesAsync();
                        RefreshCurrencyObservableCollection();
                    }
                    else if (tableMeta.TableName == "CurrencyType")
                    {
                        await context.CurrencyType.AddAsync((Models.CurrencyType)tableMeta.GetModelEntity());
                        await context.SaveChangesAsync();
                        RefreshCurrencyTypeObservableCollection();
                    }
                    else if (tableMeta.TableName == "CustomWelcome")
                    {
                        await context.CustomWelcome.AddAsync((Models.CustomWelcome)tableMeta.GetModelEntity());
                        await context.SaveChangesAsync();
                        RefreshCustomWelcomeObservableCollection();
                    }
                    else if (tableMeta.TableName == "Followers")
                    {
                        await context.Followers.AddAsync((Models.Followers)tableMeta.GetModelEntity());
                        await context.SaveChangesAsync();
                        RefreshFollowersObservableCollection();
                    }
                    else if (tableMeta.TableName == "GameDeadCounter")
                    {
                        await context.GameDeadCounter.AddAsync((Models.GameDeadCounter)tableMeta.GetModelEntity());
                        await context.SaveChangesAsync();
                        RefreshGameDeadCounterObservableCollection();
                    }
                    else if (tableMeta.TableName == "GiveawayUserData")
                    {
                        await context.GiveawayUserData.AddAsync((Models.GiveawayUserData)tableMeta.GetModelEntity());
                        await context.SaveChangesAsync();
                        RefreshGiveawayUserDataObservableCollection();
                    }
                    else if (tableMeta.TableName == "InRaidData")
                    {
                        await context.InRaidData.AddAsync((Models.InRaidData)tableMeta.GetModelEntity());
                        await context.SaveChangesAsync();
                        RefreshInRaidDataObservableCollection();
                    }
                    else if (tableMeta.TableName == "LearnMsgs")
                    {
                        await context.LearnMsgs.AddAsync((Models.LearnMsgs)tableMeta.GetModelEntity());
                        await context.SaveChangesAsync();
                        RefreshLearnMsgsObservableCollection();
                    }
                    else if (tableMeta.TableName == "ModeratorApprove")
                    {
                        await context.ModeratorApprove.AddAsync((Models.ModeratorApprove)tableMeta.GetModelEntity());
                        await context.SaveChangesAsync();
                        RefreshModeratorApproveObservableCollection();
                    }
                    else if (tableMeta.TableName == "MultiChannels")
                    {
                        await context.MultiChannels.AddAsync((Models.MultiChannels)tableMeta.GetModelEntity());
                        await context.SaveChangesAsync();
                        RefreshMultiChannelsObservableCollection();
                    }
                    else if (tableMeta.TableName == "MultiLiveStreams")
                    {
                        await context.MultiLiveStreams.AddAsync((Models.MultiLiveStreams)tableMeta.GetModelEntity());
                        await context.SaveChangesAsync();
                        RefreshMultiLiveStreamsObservableCollection();
                    }
                    else if (tableMeta.TableName == "MultiSummaryLiveStreams")
                    {
                        await context.MultiSummaryLiveStreams.AddAsync((Models.MultiSummaryLiveStreams)tableMeta.GetModelEntity());
                        await context.SaveChangesAsync();
                        RefreshMultiSummaryLiveStreamsObservableCollection();
                    }
                    else if (tableMeta.TableName == "MultiWebhooks")
                    {
                        await context.MultiWebhooks.AddAsync((Models.MultiWebhooks)tableMeta.GetModelEntity());
                        await context.SaveChangesAsync();
                        RefreshMultiWebhooksObservableCollection();
                    }
                    else if (tableMeta.TableName == "OldFollowUsers")
                    {
                        await context.OldFollowUsers.AddAsync((Models.OldFollowUsers)tableMeta.GetModelEntity());
                        await context.SaveChangesAsync();
                        RefreshOldFollowUsersObservableCollection();
                    }
                    else if (tableMeta.TableName == "OutRaidData")
                    {
                        context.OutRaidData.AddAsync((Models.OutRaidData)tableMeta.GetModelEntity());
                        await context.SaveChangesAsync();
                        RefreshOutRaidDataObservableCollection();
                    }
                    else if (tableMeta.TableName == "OverlayServices")
                    {
                        await context.OverlayServices.AddAsync((Models.OverlayServices)tableMeta.GetModelEntity());
                        await context.SaveChangesAsync();
                        RefreshOverlayServicesObservableCollection();
                    }
                    else if (tableMeta.TableName == "OverlayTicker")
                    {
                        await context.OverlayTicker.AddAsync((Models.OverlayTicker)tableMeta.GetModelEntity());
                        await context.SaveChangesAsync();
                        RefreshOverlayTickerObservableCollection();
                    }
                    else if (tableMeta.TableName == "Quotes")
                    {
                        await context.Quotes.AddAsync((Models.Quotes)tableMeta.GetModelEntity());
                        await context.SaveChangesAsync();
                        RefreshQuotesObservableCollection();
                    }
                    else if (tableMeta.TableName == "ShoutOuts")
                    {
                        await context.ShoutOuts.AddAsync((Models.ShoutOuts)tableMeta.GetModelEntity());
                        await context.SaveChangesAsync();
                        RefreshShoutOutsObservableCollection();
                    }
                    else if (tableMeta.TableName == "StreamStats")
                    {
                        await context.StreamStats.AddAsync((Models.StreamStats)tableMeta.GetModelEntity());
                        await context.SaveChangesAsync();
                        RefreshStreamStatsObservableCollection();
                    }
                    else if (tableMeta.TableName == "Users")
                    {
                        await context.Users.AddAsync((Models.Users)tableMeta.GetModelEntity());
                        await context.SaveChangesAsync();
                        RefreshUsersObservableCollection();
                    }
                    else if (tableMeta.TableName == "UserStats")
                    {
                        await context.UserStats.AddAsync((Models.UserStats)tableMeta.GetModelEntity());
                        await context.SaveChangesAsync();
                        RefreshUserStatsObservableCollection();
                    }
                    else if (tableMeta.TableName == "Webhooks")
                    {
                        await context.Webhooks.AddAsync((Models.Webhooks)tableMeta.GetModelEntity());
                        await context.SaveChangesAsync();
                        RefreshWebhooksObservableCollection();
                    }
                }
                catch (Exception ex)
                {
                    LogWriter.LogException(ex, "PostDataGridGUIAddRow");
                }

                if (Refcontext == null) { ClearDataContext(context); }

            });
        }

        internal Task<bool> PostCategory(CategoryData categoryData, SQLDBContext Refcontext = null)
        {
            return Task.Run(async () =>
            {
                bool found = false;
                if (string.IsNullOrEmpty(categoryData.CategoryId) && string.IsNullOrEmpty(categoryData.CategoryName))
                {
                    found = false;
                }
                else
                {
                    SQLDBContext context = Refcontext ?? BuildDataContext();
                    CategoryList categoryList = (from CL in context.CategoryList
                                                 where (CL.Category == FormatData.AddEscapeFormat(categoryData.CategoryName)) || CL.CategoryId == categoryData.CategoryId
                                                 select CL).FirstOrDefault();
                    if (categoryList == default)
                    {
                        await context.CategoryList.AddAsync(new(categoryId: categoryData.CategoryId, category: categoryData.CategoryName, streamCount: 0));
                        found = true;
                    }
                    await context.SaveChangesAsync();
                    RefreshCategoryListObservableCollection();
                    if (Refcontext == null) { ClearDataContext(context); }
                }
                return found;
            });
        }

        internal Task PostCategoryStream(CategoryData categoryData, int StreamCount = 0, SQLDBContext Refcontext = null)
        {
            return Task.Run(async () =>
             {
                 SQLDBContext context = Refcontext ?? BuildDataContext();
                 await PostCategory(categoryData, context);
                 CategoryList category = (from CL in context.CategoryList
                                          where (CL.Category == FormatData.AddEscapeFormat(categoryData.CategoryName)) || CL.CategoryId == categoryData.CategoryId
                                          select CL).FirstOrDefault();

                 if (OptionFlags.IsStreamOnline)
                 {
                     category.StreamCount++;
                 }

                 await context.SaveChangesAsync();
                 RefreshCategoryListObservableCollection();
                 if (Refcontext == null) { ClearDataContext(context); }
             });
        }

        internal Task<bool> PostClip(string ClipId, DateTime CreatedAt, decimal Duration, string GameId, string Language, string Title, string Url, string fromUserId, string fromUserName, SQLDBContext Refcontext = null)
        {
            return Task.Run(async () =>
            {
                bool result;
                SQLDBContext context = Refcontext ?? BuildDataContext();
                if (!(from C in context.Clips
                      where (C.ClipId == ClipId)
                      select C).Any())
                {
                    await context.Clips.AddAsync(new(clipId: ClipId, createdAt: CreatedAt, title: Title, categoryId: GameId, language: Language, duration: (float)Duration, url: Url));
                    await context.SaveChangesAsync();
                    RefreshClipsObservableCollection();
                    result = true;
                }
                result = false;
                if (Refcontext == null) { ClearDataContext(context); }
                return result;
            });
        }

        internal Task<string> PostCommand(string cmd, CommandParams Params, SQLDBContext Refcontext = null)
        {
            return Task.Run(async () =>
            {
                string result;
                SQLDBContext context = Refcontext ?? BuildDataContext();
                if (!(from Com in context.CommandsBase
                      where Com.CmdName == cmd
                      select Com).Any())
                {
                    await context.CommandsUser.AddAsync(new(cmdName: cmd, addMe: Params.AddMe, permission: Params.Permission,
                         isEnabled: Params.IsEnabled, message: Params.Message, repeatTimer: Params.Timer, sendMsgCount: Params.RepeatMsg, category: [Params.Category],
                         allowParam: Params.AllowParam, usage: Params.Usage, lookupData: Params.LookupData, table: Params.Table, keyField: GetKey(Params.Table).Result,
                         dataField: Params.Field, currencyField: Params.Currency, unit: Params.Unit, action: Params.Action, top: Params.Top, sort: Params.Sort));

                    await context.SaveChangesAsync();
                    RefreshCommandsUserObservableCollection();
                    result = string.Format(CultureInfo.CurrentCulture, LocalizedMsgSystem.GetDefaultComMsg(DefaultCommand.addcommand), cmd);
                }
                result = string.Format(CultureInfo.CurrentCulture, LocalizedMsgSystem.GetVar(Msg.MsgAddCommandFailed), cmd);
                if (Refcontext == null) { ClearDataContext(context); }
                return result;
            });
        }

        /// <summary>
        /// Post a new currency type to the database. Will add new currency records for existing users.
        /// </summary>
        /// <param name="currencyType"></param>
        internal Task PostCurrencyType(Models.CurrencyType currencyType, SQLDBContext Refcontext = null)
        {
            return Task.Run(async () =>
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();

                if (!(from CT in context.CurrencyType where CT.CurrencyName == currencyType.CurrencyName select CT).Any())
                {
                    await context.CurrencyType.AddAsync(currencyType);

                    List<Models.CurrencyType> types = new(context.CurrencyType);

                    foreach (Users U in context.Users)
                    {
                        foreach (Models.CurrencyType t in types)
                        {
                            if (!(from C in context.Currency where (C.User == U && C.CurrencyName == t.CurrencyName) select C).Any())
                            {
                                await context.Currency.AddAsync(new(userId: U.UserId, value: 0, currencyName: t.CurrencyName));
                            }
                        }
                    }
                }

                await context.SaveChangesAsync();
                RefreshCurrencyTypeObservableCollection();
                RefreshCurrencyObservableCollection();
                if (Refcontext == null) { ClearDataContext(context); }
            });
        }

        internal Task PostCurrencyUpdate(LiveUser User, double value, string CurrencyName, SQLDBContext Refcontext = null)
        {
            return Task.Run(async () =>
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();
                Currency currency = (from C in context.Currency
                                     where (C.CurrencyName == CurrencyName && C.UserId == User.UserId)
                                     select C).FirstOrDefault();
                if (currency != default)
                {
                    currency.Value = Math.Min(Math.Round(currency.Value + value, 2), currency.CurrencyType.MaxValue);
                }
                await context.SaveChangesAsync();
                RefreshCurrencyObservableCollection();
                if (Refcontext == null) { ClearDataContext(context); }
            });
        }

        internal Task PostCurrencyUpdate(List<PlayGameUserWager<PlayingCardFrench, PlayingCardSuit>> Updates, string CurrencyName, SQLDBContext Refcontext = null)
        {
            return Task.Run(async () =>
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();

                foreach (var Player in Updates)
                {
                    foreach (Currency CurrUser in (from C in context.Currency
                                                   where C.CurrencyName == CurrencyName && C.UserId == Player.Player.UserId
                                                   select C))
                    {
                        CurrUser.Value = Math.Min(Math.Round(CurrUser.Value + Player.Payout, 2), CurrUser.CurrencyType.MaxValue);
                    }
                }

                context.SaveChangesAsync();
                RefreshCurrencyObservableCollection();
                if (Refcontext == null) { ClearDataContext(context); }
            });
        }

        internal Task<int> PostDeathCounterUpdate(string currCategory, bool Reset = false, int updateValue = 1, SQLDBContext Refcontext = null)
        {
            return Task.Run(async () =>
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();
                GameDeadCounter update = (from G in context.GameDeadCounter where G.Category == currCategory select G).FirstOrDefault();
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
                }
                await context.SaveChangesAsync();
                RefreshGameDeadCounterObservableCollection();
                if (Refcontext == null) { ClearDataContext(context); }
                return update?.Counter ?? 0;
            });
        }

        internal Task PostGiveawayData(string UserId, DateTime dateTime, SQLDBContext Refcontext = null)
        {
            return Task.Run(async () =>
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();
                await context.GiveawayUserData.AddAsync(new(dateTime: dateTime, userId: UserId));
                await context.SaveChangesAsync();

                RefreshGiveawayUserDataObservableCollection();
                if (Refcontext == null) { ClearDataContext(context); }
            });
        }

        internal Task PostInRaidData(LiveUser user, DateTime time, int viewers, CategoryData gamename, SQLDBContext Refcontext = null)
        {
            return Task.Run(async () =>
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();
                await PostNewUser(user, time, context);
                await PostCategory(gamename, context);
                await context.InRaidData.AddAsync(new(userId: user.UserId, raidDate: time, viewerCount: viewers, category: gamename.CategoryName, platform: user.Platform));
                await context.SaveChangesAsync();
                RefreshUsersObservableCollection();
                RefreshCategoryListObservableCollection();
                RefreshInRaidDataObservableCollection();
                if (Refcontext == null) { ClearDataContext(context); }
            });
        }

        internal Task PostLearnMsgsRow(string Message, MsgTypes MsgType, SQLDBContext Refcontext = null)
        {
            return Task.Run(async () =>
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();
                if (!(from M in context.LearnMsgs
                      where M.TeachingMsg == Message
                      select M).Any())
                {
                    await context.LearnMsgs.AddAsync(new(msgType: MsgType, teachingMsg: Message));
                    LearnMsgChanged = true;
                }
                await context.SaveChangesAsync();
                RefreshLearnMsgsObservableCollection();
                if (Refcontext == null) { ClearDataContext(context); }
            });
        }

        internal Task<bool> PostMergeUserStats(string CurrUser, string SourceUser, Platform platform, SQLDBContext Refcontext = null)
        {
            return Task.Run(async () =>
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();
                IEnumerable<Currency> userCurrency = from uCu in context.Currency where uCu.User.UserName == CurrUser select uCu;
                IEnumerable<Currency> srcCurrency = from sCu in context.Currency where sCu.User.UserName == SourceUser select sCu;

                foreach ((Currency UC, Currency SC) in (from U in userCurrency
                                                        from S in srcCurrency
                                                        where U.CurrencyName == S.CurrencyName
                                                        select (U, S)))
                {
                    UC.Add(SC);
                }
                context.Currency.RemoveRange(srcCurrency);

                UserStats currUserstat = (from Cu in context.UserStats where (Cu.User.UserName == CurrUser && Cu.Platform == platform) select Cu).FirstOrDefault();
                UserStats sourceUser = (from Su in context.UserStats where (Su.User.UserName == SourceUser && Su.Platform == platform) select Su).FirstOrDefault();

                bool result;
                if (currUserstat != default && sourceUser != default)
                {
                    currUserstat += sourceUser;

                    context.UserStats.Remove(sourceUser);
                    await context.SaveChangesAsync();

                    result = true;
                }
                else
                {
                    result = false;
                }

                RefreshCurrencyObservableCollection();
                RefreshUsersObservableCollection();

                if (Refcontext == null) { ClearDataContext(context); }
                return result;
            });
        }

        internal Task PostNewAutoShoutUser(string UserId, Platform platform, SQLDBContext Refcontext = null)
        {
            return Task.Run(async () =>
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();
                if (!(from SO in context.ShoutOuts where (SO.UserId == UserId && SO.Platform == platform) select SO).Any())
                {
                    await context.ShoutOuts.AddAsync(new(userId: UserId, platform: platform));
                }
                await context.SaveChangesAsync();
                RefreshShoutOutsObservableCollection();
                if (Refcontext == null) { ClearDataContext(context); }
            });
        }

        internal void PostNewAutoShoutUser(LiveUser liveUser, SQLDBContext Refcontext = null)
        {
            PostNewAutoShoutUser(liveUser.UserId, liveUser.Platform, Refcontext);
        }

        //internal void PostNewData(TableMeta.TableMeta tableMeta, SQLDBContext Refcontext = null)
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

        internal Task PostOutgoingRaid(string HostedChannel, DateTime dateTime, SQLDBContext Refcontext = null)
        {
            return Task.Run(async () =>
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();
                await context.OutRaidData.AddAsync(new(channelRaided: HostedChannel, raidDate: dateTime));
                await context.SaveChangesAsync();
                RefreshOutRaidDataObservableCollection();
                if (Refcontext == null) { ClearDataContext(context); }
            });
        }

        internal Task<int> PostQuote(string Text, SQLDBContext Refcontext = null)
        {
            return Task.Run(async () =>
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();
                List<Quotes> quotes = new(from Q in context.Quotes select Q);
                int opennum = (from Q in context.Quotes select Q.Number)
                    .IntersectBy(Enumerable.Range(1, quotes.Count > 0 ? quotes.Max((f) => f.Number) : 1), q => q).Min();

                await context.Quotes.AddAsync(new(number: opennum, quote: Text));
                await context.SaveChangesAsync();
                RefreshQuotesObservableCollection();
                if (Refcontext == null) { ClearDataContext(context); }
                return opennum;
            });
        }

        /// <summary>
        /// Starts a new Stream record, if it doesn't currently exist.
        /// </summary>
        /// <param name="StreamStart">The time of stream start.</param>
        /// <returns><code>true: for posting a new stream start;</code> <code>false: when a stream start date row already exists</code></returns>
        internal Task<bool> PostStream(DateTime StreamStart, string Category, SQLDBContext Refcontext = null)
        {
            return Task.Run(async () =>
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();
                bool addstream = !(from S in context.StreamStats where S.StreamStart == StreamStart select S).Any();
                if (addstream)
                {
                    await context.StreamStats.AddAsync(new(streamStart: StreamStart, streamEnd: StreamStart));
                    await context.SaveChangesAsync();
                    RefreshStreamStatsObservableCollection();
                }

                if (Refcontext == null) { ClearDataContext(context); }
                return addstream;
            });
        }

        internal Task PostStreamStat(StreamStat streamStat, SQLDBContext Refcontext = null)
        {
            return Task.Run(async () =>
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();
                StreamStats currStream = (from S in context.StreamStats
                                          where S.StreamStart == streamStat.StreamStart
                                          select S).FirstOrDefault();
                if (currStream != default)
                {
                    currStream.Update(streamStat);
                    LogWriter.DebugLog("PostStreamStat", DebugLogTypes.DataManager, $"Updated stream stats for stream started {streamStat.StreamStart}.");
                    await context.SaveChangesAsync();
                    RefreshStreamStatsObservableCollection();
                }
                if (Refcontext == null) { ClearDataContext(context); }
            });
        }

        /// <summary>
        /// Add a custom welcome message for a specific user. Does not edit existing welcome message.
        /// </summary>
        /// <param name="User">The user to add the custom welcome message.</param>
        /// <param name="WelcomeMsg">The message for the user.</param>
        internal Task PostUserCustomWelcome(LiveUser User, string WelcomeMsg, SQLDBContext Refcontext = null)
        {
            return Task.Run(async () =>
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();
                if (!(from W in context.CustomWelcome where W.UserId == User.UserId select W).Any())
                {
                    await context.CustomWelcome.AddAsync(new(userId: User.UserId, platform: User.Platform, message: WelcomeMsg));
                    await context.SaveChangesAsync();
                    RefreshCustomWelcomeObservableCollection();
                    if (Refcontext == null) { ClearDataContext(context); }
                }
            });
        }

        #endregion Post_Methods

    }
}
