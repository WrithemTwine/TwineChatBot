using Microsoft.EntityFrameworkCore.Metadata;

using StreamerBotLib.DataSQL.Models;
using StreamerBotLib.Enums;
using StreamerBotLib.GUI;
using StreamerBotLib.Interfaces;
using StreamerBotLib.Models;
using StreamerBotLib.Static;
using StreamerBotLib.Systems;

using System.Data;
using System.Globalization;
using System.Reflection;

namespace StreamerBotLib.DataSQL
{
    public partial class DataManagerSQL : IDataManager, IDataManagerReadOnly, IDataManagerTestMethods
    {
        #region Post_Methods

        public void PostDataGridGUIAddRow(IDatabaseTableMeta tableMeta, SQLDBContext Refcontext = null)
        {
            lock (GUIDataManagerLock.Lock)
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();

                try
                {
                    // show all, but GUI access-controls will prevent these certain tables from having new rows (channel events doesn't need a new row for another event, except API driven and application will already update it)
                    if (tableMeta.TableName == "BanReasons")
                    {
                        context.BanReasons.Add((Models.BanReasons)tableMeta.GetModelEntity());
                        context.SaveChanges(true);
                        RefreshBanReasonsObservableCollection();
                    }
                    else if (tableMeta.TableName == "BanRules")
                    {
                        context.BanRules.Add((Models.BanRules)tableMeta.GetModelEntity());
                        context.SaveChanges(true);
                        RefreshBanRulesObservableCollection();
                    }
                    else if (tableMeta.TableName == "CategoryList")
                    {
                        context.CategoryList.Add((Models.CategoryList)tableMeta.GetModelEntity());
                        context.SaveChanges(true);
                        RefreshCategoryListObservableCollection();
                    }
                    else if (tableMeta.TableName == "ChannelEvents")
                    {
                        context.ChannelEvents.Add((Models.ChannelEvents)tableMeta.GetModelEntity());
                        context.SaveChanges(true);
                        RefreshChannelEventsObservableCollection();
                    }
                    else if (tableMeta.TableName == "Clips")
                    {
                        context.Clips.Add((Models.Clips)tableMeta.GetModelEntity());
                        context.SaveChanges(true);
                        RefreshClipsObservableCollection();
                    }
                    else if (tableMeta.TableName == "Commands")
                    {
                        context.Commands.Add((Models.Commands)tableMeta.GetModelEntity());
                        context.SaveChanges(true);
                        RefreshCommandsObservableCollection();
                    }
                    else if (tableMeta.TableName == "CommandsUser")
                    {
                        context.CommandsUser.Add((Models.CommandsUser)tableMeta.GetModelEntity());
                        context.SaveChanges(true);
                        RefreshCommandsUserObservableCollection();
                    }
                    else if (tableMeta.TableName == "Currency")
                    {
                        context.Currency.Add((Models.Currency)tableMeta.GetModelEntity());
                        context.SaveChanges(true);
                        RefreshCurrencyObservableCollection();
                    }
                    else if (tableMeta.TableName == "CurrencyType")
                    {
                        context.CurrencyType.Add((Models.CurrencyType)tableMeta.GetModelEntity());
                        context.SaveChanges(true);
                        RefreshCurrencyTypeObservableCollection();
                    }
                    else if (tableMeta.TableName == "CustomWelcome")
                    {
                        context.CustomWelcome.Add((Models.CustomWelcome)tableMeta.GetModelEntity());
                        context.SaveChanges(true);
                        RefreshCustomWelcomeObservableCollection();
                    }
                    else if (tableMeta.TableName == "Followers")
                    {
                        context.Followers.Add((Models.Followers)tableMeta.GetModelEntity());
                        context.SaveChanges(true);
                        RefreshFollowersObservableCollection();
                    }
                    else if (tableMeta.TableName == "GameDeadCounter")
                    {
                        context.GameDeadCounter.Add((Models.GameDeadCounter)tableMeta.GetModelEntity());
                        context.SaveChanges(true);
                        RefreshGameDeadCounterObservableCollection();
                    }
                    else if (tableMeta.TableName == "GiveawayUserData")
                    {
                        context.GiveawayUserData.Add((Models.GiveawayUserData)tableMeta.GetModelEntity());
                        context.SaveChanges(true);
                        RefreshGiveawayUserDataObservableCollection();
                    }
                    else if (tableMeta.TableName == "InRaidData")
                    {
                        context.InRaidData.Add((Models.InRaidData)tableMeta.GetModelEntity());
                        context.SaveChanges(true);
                        RefreshInRaidDataObservableCollection();
                    }
                    else if (tableMeta.TableName == "LearnMsgs")
                    {
                        context.LearnMsgs.Add((Models.LearnMsgs)tableMeta.GetModelEntity());
                        context.SaveChanges(true);
                        RefreshLearnMsgsObservableCollection();
                    }
                    else if (tableMeta.TableName == "ModeratorApprove")
                    {
                        context.ModeratorApprove.Add((Models.ModeratorApprove)tableMeta.GetModelEntity());
                        context.SaveChanges(true);
                        RefreshModeratorApproveObservableCollection();
                    }
                    else if (tableMeta.TableName == "MultiChannels")
                    {
                        context.MultiChannels.Add((Models.MultiChannels)tableMeta.GetModelEntity());
                        context.SaveChanges(true);
                        RefreshMultiChannelsObservableCollection();
                    }
                    else if (tableMeta.TableName == "MultiLiveStreams")
                    {
                        context.MultiLiveStreams.Add((Models.MultiLiveStreams)tableMeta.GetModelEntity());
                        context.SaveChanges(true);
                        RefreshMultiLiveStreamsObservableCollection();
                    }
                    else if (tableMeta.TableName == "MultiSummaryLiveStreams")
                    {
                        context.MultiSummaryLiveStreams.Add((Models.MultiSummaryLiveStreams)tableMeta.GetModelEntity());
                        context.SaveChanges(true);
                        RefreshMultiSummaryLiveStreamsObservableCollection();
                    }
                    else if (tableMeta.TableName == "MultiWebhooks")
                    {
                        context.MultiWebhooks.Add((Models.MultiWebhooks)tableMeta.GetModelEntity());
                        context.SaveChanges(true);
                        RefreshMultiWebhooksObservableCollection();
                    }
                    else if (tableMeta.TableName == "OldFollowUsers")
                    {
                        context.OldFollowUsers.Add((Models.OldFollowUsers)tableMeta.GetModelEntity());
                        context.SaveChanges(true);
                        RefreshOldFollowUsersObservableCollection();
                    }
                    else if (tableMeta.TableName == "OutRaidData")
                    {
                        context.OutRaidData.Add((Models.OutRaidData)tableMeta.GetModelEntity());
                        context.SaveChanges(true);
                        RefreshOutRaidDataObservableCollection();
                    }
                    else if (tableMeta.TableName == "OverlayServices")
                    {
                        context.OverlayServices.Add((Models.OverlayServices)tableMeta.GetModelEntity());
                        context.SaveChanges(true);
                        RefreshOverlayServicesObservableCollection();
                    }
                    else if (tableMeta.TableName == "OverlayTicker")
                    {
                        context.OverlayTicker.Add((Models.OverlayTicker)tableMeta.GetModelEntity());
                        context.SaveChanges(true);
                        RefreshOverlayTickerObservableCollection();
                    }
                    else if (tableMeta.TableName == "Quotes")
                    {
                        context.Quotes.Add((Models.Quotes)tableMeta.GetModelEntity());
                        context.SaveChanges(true);
                        RefreshQuotesObservableCollection();
                    }
                    else if (tableMeta.TableName == "ShoutOuts")
                    {
                        context.ShoutOuts.Add((Models.ShoutOuts)tableMeta.GetModelEntity());
                        context.SaveChanges(true);
                        RefreshShoutOutsObservableCollection();
                    }
                    else if (tableMeta.TableName == "StreamStats")
                    {
                        context.StreamStats.Add((Models.StreamStats)tableMeta.GetModelEntity());
                        context.SaveChanges(true);
                        RefreshStreamStatsObservableCollection();
                    }
                    else if (tableMeta.TableName == "Users")
                    {
                        context.Users.Add((Models.Users)tableMeta.GetModelEntity());
                        context.SaveChanges(true);
                        RefreshUsersObservableCollection();
                    }
                    else if (tableMeta.TableName == "UserStats")
                    {
                        context.UserStats.Add((Models.UserStats)tableMeta.GetModelEntity());
                        context.SaveChanges(true);
                        RefreshUserStatsObservableCollection();
                    }
                    else if (tableMeta.TableName == "Webhooks")
                    {
                        context.Webhooks.Add((Models.Webhooks)tableMeta.GetModelEntity());
                        context.SaveChanges(true);
                        RefreshWebhooksObservableCollection();
                    }
                }
                catch (Exception ex)
                {
                    LogWriter.LogException(ex, MethodBase.GetCurrentMethod().Name);
                }

                if (Refcontext == null) { ClearDataContext(context); }

            }
        }

        public bool PostCategory(CategoryData categoryData, SQLDBContext Refcontext = null)
        {
            bool found = false;
            if (string.IsNullOrEmpty(categoryData.CategoryId) && string.IsNullOrEmpty(categoryData.CategoryName))
            {
                found = false;
            }
            else
            {
                lock (GUIDataManagerLock.Lock)
                {
                    SQLDBContext context = Refcontext ?? BuildDataContext();
                    CategoryList categoryList = (from CL in context.CategoryList
                                                 where (CL.Category == FormatData.AddEscapeFormat(categoryData.CategoryName)) || CL.CategoryId == categoryData.CategoryId
                                                 select CL).FirstOrDefault();
                    if (categoryList == default)
                    {
                        context.CategoryList.Add(new(categoryId: categoryData.CategoryId, category: categoryData.CategoryName, streamCount: 0));
                        found = true;
                    }
                    context.SaveChanges(true);
                    RefreshCategoryListObservableCollection();
                    if (Refcontext == null) { ClearDataContext(context); }
                }
            }
            return found;
        }

        public void PostCategoryStream(CategoryData categoryData, int StreamCount = 0, SQLDBContext Refcontext = null)
        {
            PostCategory(categoryData);
            lock (GUIDataManagerLock.Lock)
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();
                CategoryList category = (from CL in context.CategoryList
                                         where (CL.Category == FormatData.AddEscapeFormat(categoryData.CategoryName)) || CL.CategoryId == categoryData.CategoryId
                                         select CL).FirstOrDefault();

                if (OptionFlags.IsStreamOnline)
                {
                    category.StreamCount++;
                }

                context.SaveChanges(true);
                RefreshCategoryListObservableCollection();
                if (Refcontext == null) { ClearDataContext(context); }
            }

        }

        public bool PostClip(string ClipId, DateTime CreatedAt, decimal Duration, string GameId, string Language, string Title, string Url, string fromUserId, string fromUserName, SQLDBContext Refcontext = null)
        {
            lock (GUIDataManagerLock.Lock)
            {
                bool result;
                SQLDBContext context = Refcontext ?? BuildDataContext();
                if (!(from C in context.Clips
                      where (C.ClipId == ClipId)
                      select C).Any())
                {
                    context.Clips.Add(new(clipId: ClipId, createdAt: CreatedAt, title: Title, categoryId: GameId, language: Language, duration: (float)Duration, url: Url));
                    context.SaveChanges(true);
                    RefreshClipsObservableCollection();
                    result = true;
                }
                result = false;
                if (Refcontext == null) { ClearDataContext(context); }
                return result;
            }
        }

        public string PostCommand(string cmd, CommandParams Params, SQLDBContext Refcontext = null)
        {
            lock (GUIDataManagerLock.Lock)
            {
                string result;
                SQLDBContext context = Refcontext ?? BuildDataContext();
                if (!(from Com in context.CommandsBase
                      where Com.CmdName == cmd
                      select Com).Any())
                {
                    context.CommandsUser.Add(new(cmdName: cmd, addMe: Params.AddMe, permission: Params.Permission,
                    isEnabled: Params.IsEnabled, message: Params.Message, repeatTimer: Params.Timer, sendMsgCount: Params.RepeatMsg, category: [Params.Category],
                    allowParam: Params.AllowParam, usage: Params.Usage, lookupData: Params.LookupData, table: Params.Table, keyField: GetKey(Params.Table),
                    dataField: Params.Field, currencyField: Params.Currency, unit: Params.Unit, action: Params.Action, top: Params.Top, sort: Params.Sort));

                    context.SaveChanges(true);
                    RefreshCommandsUserObservableCollection();
                    result = string.Format(CultureInfo.CurrentCulture, LocalizedMsgSystem.GetDefaultComMsg(DefaultCommand.addcommand), cmd);
                }
                result = string.Format(CultureInfo.CurrentCulture, LocalizedMsgSystem.GetVar(Msg.MsgAddCommandFailed), cmd);
                if (Refcontext == null) { ClearDataContext(context); }
                return result;
            }
        }

        /// <summary>
        /// Post a new currency type to the database. Will add new currency records for existing users.
        /// </summary>
        /// <param name="currencyType"></param>
        public void PostCurrencyType(Models.CurrencyType currencyType, SQLDBContext Refcontext = null)
        {
            lock (GUIDataManagerLock.Lock)
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();

                if (!(from CT in context.CurrencyType where CT.CurrencyName == currencyType.CurrencyName select CT).Any())
                {
                    context.CurrencyType.Add(currencyType);

                    List<Models.CurrencyType> types = new(context.CurrencyType);

                    foreach (Users U in context.Users)
                    {
                        foreach (Models.CurrencyType t in types)
                        {
                            if (!(from C in context.Currency where (C.User == U && C.CurrencyName == t.CurrencyName) select C).Any())
                            {
                                context.Currency.Add(new(userId: U.UserId, value: 0, currencyName: t.CurrencyName));
                            }
                        }
                    }
                }

                context.SaveChanges(true);
                RefreshCurrencyTypeObservableCollection();
                RefreshCurrencyObservableCollection();
                if (Refcontext == null) { ClearDataContext(context); }
            }
        }

        public void PostCurrencyUpdate(LiveUser User, double value, string CurrencyName, SQLDBContext Refcontext = null)
        {
            lock (GUIDataManagerLock.Lock)
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();
                Currency currency = (from C in context.Currency
                                     where (C.CurrencyName == CurrencyName && C.UserId == User.UserId)
                                     select C).FirstOrDefault();
                if (currency != default)
                {
                    currency.Value = Math.Min(Math.Round(currency.Value + value, 2), currency.CurrencyType.MaxValue);
                }
                context.SaveChanges(true);
                RefreshCurrencyObservableCollection();
                if (Refcontext == null) { ClearDataContext(context); }
            }
        }

        public void PostCurrencyUpdate(List<PlayGameUserWager<PlayingCardFrench, PlayingCardSuit>> Updates, string CurrencyName, SQLDBContext Refcontext = null)
        {
            lock (GUIDataManagerLock.Lock)
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

                context.SaveChanges(true);
                RefreshCurrencyObservableCollection();
                if (Refcontext == null) { ClearDataContext(context); }
            }
        }

        public int PostDeathCounterUpdate(string currCategory, bool Reset = false, int updateValue = 1, SQLDBContext Refcontext = null)
        {
            lock (GUIDataManagerLock.Lock)
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
                context.SaveChanges(true);
                RefreshGameDeadCounterObservableCollection();
                if (Refcontext == null) { ClearDataContext(context); }
                return update?.Counter ?? 0;
            }
        }

        public void PostGiveawayData(string UserId, DateTime dateTime, SQLDBContext Refcontext = null)
        {
            lock (GUIDataManagerLock.Lock)
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();
                context.GiveawayUserData.Add(new(dateTime: dateTime, userId: UserId));
                context.SaveChanges(true);

                RefreshGiveawayUserDataObservableCollection();
                if (Refcontext == null) { ClearDataContext(context); }
            }
        }

        public void PostInRaidData(LiveUser user, DateTime time, int viewers, CategoryData gamename, SQLDBContext Refcontext = null)
        {
            lock (GUIDataManagerLock.Lock)
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();
                PostNewUser(user, time, context);
                PostCategory(gamename, context);
                context.InRaidData.Add(new(userId: user.UserId, raidDate: time, viewerCount: viewers, category: gamename.CategoryName, platform: user.Platform));
                context.SaveChanges(true);
                RefreshInRaidDataObservableCollection();
                if (Refcontext == null) { ClearDataContext(context); }
            }
        }

        public void PostLearnMsgsRow(string Message, MsgTypes MsgType, SQLDBContext Refcontext = null)
        {
            lock (GUIDataManagerLock.Lock)
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();
                if (!(from M in context.LearnMsgs
                      where M.TeachingMsg == Message
                      select M).Any())
                {
                    context.LearnMsgs.Add(new(msgType: MsgType, teachingMsg: Message));
                    LearnMsgChanged = true;
                }
                context.SaveChanges(true);
                RefreshLearnMsgsObservableCollection();
                if (Refcontext == null) { ClearDataContext(context); }
            }
        }

        public bool PostMergeUserStats(string CurrUser, string SourceUser, Platform platform, SQLDBContext Refcontext = null)
        {
            lock (GUIDataManagerLock.Lock)
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
                    context.SaveChanges(true);

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
            }
        }

        public void PostNewAutoShoutUser(string UserId, Platform platform, SQLDBContext Refcontext = null)
        {
            lock (GUIDataManagerLock.Lock)
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();
                if (!(from SO in context.ShoutOuts where (SO.UserId == UserId && SO.Platform == platform) select SO).Any())
                {
                    context.ShoutOuts.Add(new(userId: UserId, platform: platform));
                }
                context.SaveChanges(true);
                RefreshShoutOutsObservableCollection();
                if (Refcontext == null) { ClearDataContext(context); }
            }
        }

        public void PostNewAutoShoutUser(LiveUser liveUser, SQLDBContext Refcontext = null)
        {
            PostNewAutoShoutUser(liveUser.UserId, liveUser.Platform, Refcontext);
        }

        //public void PostNewData(TableMeta.TableMeta tableMeta, SQLDBContext Refcontext = null)
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

        //            context.SaveChanges(true);
        //        if (Refcontext == null) { ClearDataContext(context); }
        //    }
        //}

        public void PostOutgoingRaid(string HostedChannel, DateTime dateTime, SQLDBContext Refcontext = null)
        {
            lock (GUIDataManagerLock.Lock)
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();
                context.OutRaidData.Add(new(channelRaided: HostedChannel, raidDate: dateTime));
                context.SaveChanges(true);
                RefreshOutRaidDataObservableCollection();
                if (Refcontext == null) { ClearDataContext(context); }
            }
        }

        public int PostQuote(string Text, SQLDBContext Refcontext = null)
        {
            lock (GUIDataManagerLock.Lock)
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();
                List<Quotes> quotes = new(from Q in context.Quotes select Q);
                int opennum = (from Q in context.Quotes select Q.Number)
                    .IntersectBy(Enumerable.Range(1, quotes.Count > 0 ? quotes.Max((f) => f.Number) : 1), q => q).Min();

                context.Quotes.Add(new(number: opennum, quote: Text));
                context.SaveChanges(true);
                RefreshQuotesObservableCollection();
                if (Refcontext == null) { ClearDataContext(context); }
                return opennum;
            }
        }

        /// <summary>
        /// Starts a new Stream record, if it doesn't currently exist.
        /// </summary>
        /// <param name="StreamStart">The time of stream start.</param>
        /// <returns><code>true: for posting a new stream start;</code> <code>false: when a stream start date row already exists</code></returns>
        public bool PostStream(DateTime StreamStart, string Category, SQLDBContext Refcontext = null)
        {
            lock (GUIDataManagerLock.Lock)
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();
                bool addstream = !(from S in context.StreamStats where S.StreamStart == StreamStart select S).Any();
                if (addstream)
                {
                    context.StreamStats.Add(new(streamStart: StreamStart, streamEnd: StreamStart));
                    context.SaveChanges(true);
                    RefreshStreamStatsObservableCollection();
                }

                if (Refcontext == null) { ClearDataContext(context); }
                return addstream;
            }
        }

        public void PostStreamStat(StreamStat streamStat, SQLDBContext Refcontext = null)
        {
            lock (GUIDataManagerLock.Lock)
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();
                StreamStats currStream = (from S in context.StreamStats where S.StreamStart == streamStat.StreamStart select S).FirstOrDefault();
                if (currStream != default)
                {
                    currStream.Update(streamStat);
                    context.SaveChanges(true);
                    RefreshStreamStatsObservableCollection();
                }
                if (Refcontext == null) { ClearDataContext(context); }
            }
        }

        /// <summary>
        /// Add a custom welcome message for a specific user. Does not edit existing welcome message.
        /// </summary>
        /// <param name="User">The user to add the custom welcome message.</param>
        /// <param name="WelcomeMsg">The message for the user.</param>
        public void PostUserCustomWelcome(LiveUser User, string WelcomeMsg, SQLDBContext Refcontext = null)
        {
            lock (GUIDataManagerLock.Lock)
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();
                if (!(from W in context.CustomWelcome where W.UserId == User.UserId select W).Any())
                {
                    context.CustomWelcome.Add(new(userId: User.UserId, platform: User.Platform, message: WelcomeMsg));
                    context.SaveChanges(true);
                    RefreshCustomWelcomeObservableCollection();
                    if (Refcontext == null) { ClearDataContext(context); }
                }
            }
        }

        #endregion Post_Methods

    }
}
