using Microsoft.EntityFrameworkCore;

using StreamerBotLib.DataSQL.Models;

using StreamerBotLib.Models;
using StreamerBotLib.Models.Enums;
using StreamerBotLib.Models.Interfaces;
using StreamerBotLib.Static;
using StreamerBotLib.Systems;

using System.Globalization;

namespace StreamerBotLib.DataSQL.EFC9
{
    internal partial class DataManagerSQLAsync
    {
        #region Delete Records

        internal async Task DeleteDataRows(IEnumerable<object> entities, string TableName)
        {
            if (entities != null)
            {
                switch (TableName)
                {
                    case "BanReasons":
                        using (var context = BuildDataContext())
                        {
                            await context.Database.BeginTransactionAsync();
                            context.BanReasons.RemoveRange(entities.Cast<Models.BanReasons>());
                            await context.Database.CommitTransactionAsync();
                            await context.SaveChangesAsync(true);
                            await RefreshBanReasonsList(true);
                        }
                        break;
                    case "BanRules":
                        using (var context = BuildDataContext())
                        {
                            await context.Database.BeginTransactionAsync();
                            context.BanRules.RemoveRange(entities.Cast<Models.BanRules>());
                            await context.Database.CommitTransactionAsync();
                            await context.SaveChangesAsync(true);
                            await RefreshBanRulesList(true);
                        }
                        break;
                    case "CategoryList":
                        using (var context = BuildDataContext())
                        {
                            await context.Database.BeginTransactionAsync();
                            context.CategoryList.RemoveRange(entities.Cast<Models.CategoryList>());
                            await context.Database.CommitTransactionAsync();
                            await context.SaveChangesAsync(true);
                            await RefreshCategoryListList(true);
                        }
                        break;
                    case "ChannelEvents":
                        using (var context = BuildDataContext())
                        {
                            await context.Database.BeginTransactionAsync();
                            context.ChannelEvents.RemoveRange(entities.Cast<Models.ChannelEvents>());
                            await context.Database.CommitTransactionAsync();
                            await context.SaveChangesAsync(true);
                            await RefreshChannelEventsList(true);
                        }
                        break;
                    case "Clips":
                        using (var context = BuildDataContext())
                        {
                            await context.Database.BeginTransactionAsync();
                            context.Clips.RemoveRange(entities.Cast<Models.Clips>());
                            await context.Database.CommitTransactionAsync();
                            await context.SaveChangesAsync(true);
                            await RefreshClipsList(true);
                        }
                        break;
                    case "Commands":
                        using (var context = BuildDataContext())
                        {
                            await context.Database.BeginTransactionAsync();
                            context.Commands.RemoveRange(entities.Cast<Models.Commands>());
                            await context.Database.CommitTransactionAsync();
                            await context.SaveChangesAsync(true);
                            await RefreshCommandsList(true);
                        }
                        break;
                    case "CommandsUser":
                        using (var context = BuildDataContext())
                        {
                            await context.Database.BeginTransactionAsync();
                            context.CommandsUser.RemoveRange(entities.Cast<Models.CommandsUser>());
                            await context.Database.CommitTransactionAsync();
                            await context.SaveChangesAsync(true);
                            await RefreshCommandsUserList(true);
                        }
                        break;
                    case "Currency":
                        using (var context = BuildDataContext())
                        {
                            await context.Database.BeginTransactionAsync();
                            context.Currency.RemoveRange(entities.Cast<Models.Currency>());
                            await context.Database.CommitTransactionAsync();
                            await context.SaveChangesAsync(true);
                            await RefreshCurrencyList(true);
                        }
                        break;
                    case "CurrencyType":
                        using (var context = BuildDataContext())
                        {
                            await context.Database.BeginTransactionAsync();
                            context.CurrencyType.RemoveRange(entities.Cast<Models.CurrencyType>());
                            await context.Database.CommitTransactionAsync();
                            await context.SaveChangesAsync(true);
                            await RefreshCurrencyTypeList(true);
                        }
                        break;
                    case "CustomWelcome":
                        using (var context = BuildDataContext())
                        {
                            await context.Database.BeginTransactionAsync();
                            context.CustomWelcome.RemoveRange(entities.Cast<Models.CustomWelcome>());
                            await context.Database.CommitTransactionAsync();
                            await context.SaveChangesAsync(true);
                            await RefreshCustomWelcomeList(true);
                        }
                        break;
                    case "Followers":
                        using (var context = BuildDataContext())
                        {
                            await context.Database.BeginTransactionAsync();
                            context.Followers.RemoveRange(entities.Cast<Models.Followers>());
                            await context.Database.CommitTransactionAsync();
                            await context.SaveChangesAsync(true);
                            await RefreshFollowersList(true);
                        }
                        break;
                    case "GameDeadCounter":
                        using (var context = BuildDataContext())
                        {
                            await context.Database.BeginTransactionAsync();
                            context.GameDeadCounter.RemoveRange(entities.Cast<Models.GameDeadCounter>());
                            await context.Database.CommitTransactionAsync();
                            await context.SaveChangesAsync(true);
                            await RefreshGameDeadCounterList(true);
                        }
                        break;
                    case "GiveawayUserData":
                        using (var context = BuildDataContext())
                        {
                            await context.Database.BeginTransactionAsync();
                            context.GiveawayUserData.RemoveRange(entities.Cast<Models.GiveawayUserData>());
                            await context.Database.CommitTransactionAsync();
                            await context.SaveChangesAsync(true);
                            await RefreshGiveawayUserDataList(true);
                        }
                        break;
                    case "InRaidData":
                        using (var context = BuildDataContext())
                        {
                            await context.Database.BeginTransactionAsync();
                            context.InRaidData.RemoveRange(entities.Cast<Models.InRaidData>());
                            await context.Database.CommitTransactionAsync();
                            await context.SaveChangesAsync(true);
                            await RefreshInRaidDataList(true);
                        }
                        break;
                    case "LearnMsgs":
                        using (var context = BuildDataContext())
                        {
                            await context.Database.BeginTransactionAsync();
                            context.LearnMsgs.RemoveRange(entities.Cast<Models.LearnMsgs>());
                            await context.Database.CommitTransactionAsync();
                            await context.SaveChangesAsync(true);
                            await RefreshLearnMsgsList(true);
                        }
                        break;
                    case "ModeratorApprove":
                        using (var context = BuildDataContext())
                        {
                            await context.Database.BeginTransactionAsync();
                            context.ModeratorApprove.RemoveRange(entities.Cast<Models.ModeratorApprove>());
                            await context.Database.CommitTransactionAsync();
                            await context.SaveChangesAsync(true);
                            await RefreshModeratorApproveList(true);
                        }
                        break;
                    case "MultiChannels":
                        using (var context = BuildDataContext())
                        {
                            await context.Database.BeginTransactionAsync();
                            context.MultiChannels.RemoveRange(entities.Cast<Models.MultiChannels>());
                            await context.Database.CommitTransactionAsync();
                            await context.SaveChangesAsync(true);
                            await RefreshMultiChannelsList(true);
                            await RefreshMultiLiveStreamsList(true);
                            await RefreshMultiSummaryLiveStreamsList(true);
                        }
                        break;
                    case "MultiLiveStreams":
                        using (var context = BuildDataContext())
                        {
                            await context.Database.BeginTransactionAsync();
                            context.MultiLiveStreams.RemoveRange(entities.Cast<Models.MultiLiveStreams>());
                            await context.Database.CommitTransactionAsync();
                            await context.SaveChangesAsync(true);
                            await RefreshMultiLiveStreamsList(true);
                        }
                        break;
                    case "MultiSummaryLiveStreams":
                        using (var context = BuildDataContext())
                        {
                            await context.Database.BeginTransactionAsync();
                            context.MultiSummaryLiveStreams.RemoveRange(entities.Cast<Models.MultiSummaryLiveStreams>());
                            await context.Database.CommitTransactionAsync();
                            await context.SaveChangesAsync(true);
                            await RefreshMultiSummaryLiveStreamsList(true);
                        }
                        break;
                    case "MultiWebhooks":
                        using (var context = BuildDataContext())
                        {
                            await context.Database.BeginTransactionAsync();
                            context.MultiWebhooks.RemoveRange(entities.Cast<Models.MultiWebhooks>());
                            await context.Database.CommitTransactionAsync();
                            await context.SaveChangesAsync(true);
                            await RefreshMultiWebhooksList(true);
                        }
                        break;
                    case "OldFollowUsers":
                        using (var context = BuildDataContext())
                        {
                            await context.Database.BeginTransactionAsync();
                            context.OldFollowUsers.RemoveRange(entities.Cast<Models.OldFollowUsers>());
                            await context.Database.CommitTransactionAsync();
                            await context.SaveChangesAsync(true);
                            await RefreshOldFollowUsersList(true);
                        }
                        break;
                    case "OutRaidData":
                        using (var context = BuildDataContext())
                        {
                            await context.Database.BeginTransactionAsync();
                            context.OutRaidData.RemoveRange(entities.Cast<Models.OutRaidData>());
                            await context.Database.CommitTransactionAsync();
                            await context.SaveChangesAsync(true);
                            await RefreshOutRaidDataList(true);
                        }
                        break;
                    case "OverlayServices":
                        using (var context = BuildDataContext())
                        {
                            await context.Database.BeginTransactionAsync();
                            context.OverlayServices.RemoveRange(entities.Cast<Models.OverlayServices>());
                            await context.Database.CommitTransactionAsync();
                            await context.SaveChangesAsync(true);
                            await RefreshOverlayServicesList(true);
                        }
                        break;
                    case "OverlayTicker":
                        using (var context = BuildDataContext())
                        {
                            await context.Database.BeginTransactionAsync();
                            context.OverlayTicker.RemoveRange(entities.Cast<Models.OverlayTicker>());
                            await context.Database.CommitTransactionAsync();
                            await context.SaveChangesAsync(true);
                            await RefreshOverlayTickerList(true);
                        }
                        break;
                    case "Quotes":
                        using (var context = BuildDataContext())
                        {
                            await context.Database.BeginTransactionAsync();
                            context.Quotes.RemoveRange(entities.Cast<Models.Quotes>());
                            await context.Database.CommitTransactionAsync();
                            await context.SaveChangesAsync(true);
                            await RefreshQuotesList(true);
                        }
                        break;
                    case "ShoutOuts":
                        using (var context = BuildDataContext())
                        {
                            await context.Database.BeginTransactionAsync();
                            context.ShoutOuts.RemoveRange(entities.Cast<Models.ShoutOuts>());
                            await context.Database.CommitTransactionAsync();
                            await context.SaveChangesAsync(true);
                            await RefreshShoutOutsList(true);
                        }
                        break;
                    case "StreamStats":
                        using (var context = BuildDataContext())
                        {
                            await context.Database.BeginTransactionAsync();
                            context.StreamStats.RemoveRange(entities.Cast<Models.StreamStats>());
                            await context.Database.CommitTransactionAsync();
                            await context.SaveChangesAsync(true);
                            await RefreshStreamStatsList(true);
                        }
                        break;
                    case "Users":
                        using (var context = BuildDataContext())
                        {
                            await context.Database.BeginTransactionAsync();
                            context.Users.RemoveRange(entities.Cast<Models.Users>());
                            await context.Database.CommitTransactionAsync();
                            await context.SaveChangesAsync(true);
                            await RefreshUsersList(true);
                        }
                        break;
                    case "UserStats":
                        using (var context = BuildDataContext())
                        {
                            await context.Database.BeginTransactionAsync();
                            context.UserStats.RemoveRange(entities.Cast<Models.UserStats>());
                            await context.Database.CommitTransactionAsync();
                            await context.SaveChangesAsync(true);
                            await RefreshUserStatsList(true);
                        }
                        break;
                    case "Webhooks":
                        using (var context = BuildDataContext())
                        {
                            await context.Database.BeginTransactionAsync();
                            context.Webhooks.RemoveRange(entities.Cast<Models.Webhooks>());
                            await context.Database.CommitTransactionAsync();
                            await context.SaveChangesAsync(true);
                            await RefreshWebhooksList(true);
                        }
                        break;
                    default:
                        LogWriter.DebugLog("DeleteDataRows", DebugLogTypes.DataManager, $"Table '{TableName}' not found.");
                        break;
                }
            }
        }

        #endregion

        #region Post_Methods

        internal async Task PostDataGridGUIAddRow(IDatabaseTableMeta tableMeta)
        {
            using var context = BuildDataContext();
            await context.Database.BeginTransactionAsync();

            try
            {
                // show all, but GUI access-controls will prevent these certain tables from having new rows (channel events doesn't need a new row for another event, except API driven and application will already update it)
                if (tableMeta.TableName == "BanReasons")
                {
                    await context.BanReasons.AddAsync((Models.BanReasons)tableMeta.GetModelEntity());
                    await context.Database.CommitTransactionAsync();
                    await context.SaveChangesAsync(true);
                    await RefreshBanReasonsList(true);
                }
                else if (tableMeta.TableName == "BanRules")
                {
                    await context.BanRules.AddAsync((Models.BanRules)tableMeta.GetModelEntity());
                    await context.Database.CommitTransactionAsync();
                    await context.SaveChangesAsync(true);
                    await RefreshBanRulesList(true);
                }
                else if (tableMeta.TableName == "CategoryList")
                {
                    await context.CategoryList.AddAsync((Models.CategoryList)tableMeta.GetModelEntity());
                    await context.Database.CommitTransactionAsync();
                    await context.SaveChangesAsync(true);
                    await RefreshCategoryListList(true);
                }
                else if (tableMeta.TableName == "ChannelEvents")
                {
                    await context.ChannelEvents.AddAsync((Models.ChannelEvents)tableMeta.GetModelEntity());
                    await context.Database.CommitTransactionAsync();
                    await context.SaveChangesAsync(true);
                    await RefreshChannelEventsList(true);
                }
                else if (tableMeta.TableName == "Clips")
                {
                    await context.Clips.AddAsync((Models.Clips)tableMeta.GetModelEntity());
                    await context.Database.CommitTransactionAsync();
                    await context.SaveChangesAsync(true);
                    await RefreshClipsList(true);
                }
                else if (tableMeta.TableName == "Commands")
                {
                    await context.Commands.AddAsync((Models.Commands)tableMeta.GetModelEntity());
                    await context.Database.CommitTransactionAsync();
                    await context.SaveChangesAsync(true);
                    await RefreshCommandsList(true);
                }
                else if (tableMeta.TableName == "CommandsUser")
                {
                    await context.CommandsUser.AddAsync((Models.CommandsUser)tableMeta.GetModelEntity());
                    await context.Database.CommitTransactionAsync();
                    await context.SaveChangesAsync(true);
                    await RefreshCommandsUserList(true);
                }
                else if (tableMeta.TableName == "Currency")
                {
                    await context.Currency.AddAsync((Models.Currency)tableMeta.GetModelEntity());
                    await context.Database.CommitTransactionAsync();
                    await context.SaveChangesAsync(true);
                    await RefreshCurrencyList(true);
                }
                else if (tableMeta.TableName == "CurrencyType")
                {
                    await context.CurrencyType.AddAsync((Models.CurrencyType)tableMeta.GetModelEntity());
                    await context.Database.CommitTransactionAsync();
                    await context.SaveChangesAsync(true);
                    await RefreshCurrencyTypeList(true);
                }
                else if (tableMeta.TableName == "CustomWelcome")
                {
                    await context.CustomWelcome.AddAsync((Models.CustomWelcome)tableMeta.GetModelEntity());
                    await context.Database.CommitTransactionAsync();
                    await context.SaveChangesAsync(true);
                    await RefreshCustomWelcomeList(true);
                }
                else if (tableMeta.TableName == "Followers")
                {
                    await context.Followers.AddAsync((Models.Followers)tableMeta.GetModelEntity());
                    await context.Database.CommitTransactionAsync();
                    await context.SaveChangesAsync(true);
                    await RefreshFollowersList(true);
                }
                else if (tableMeta.TableName == "GameDeadCounter")
                {
                    await context.GameDeadCounter.AddAsync((Models.GameDeadCounter)tableMeta.GetModelEntity());
                    await context.Database.CommitTransactionAsync();
                    await context.SaveChangesAsync(true);
                    await RefreshGameDeadCounterList(true);
                }
                else if (tableMeta.TableName == "GiveawayUserData")
                {
                    await context.GiveawayUserData.AddAsync((Models.GiveawayUserData)tableMeta.GetModelEntity());
                    await context.Database.CommitTransactionAsync();
                    await context.SaveChangesAsync(true);
                    await RefreshGiveawayUserDataList(true);
                }
                else if (tableMeta.TableName == "InRaidData")
                {
                    await context.InRaidData.AddAsync((Models.InRaidData)tableMeta.GetModelEntity());
                    await context.Database.CommitTransactionAsync();
                    await context.SaveChangesAsync(true);
                    await RefreshInRaidDataList(true);
                }
                else if (tableMeta.TableName == "LearnMsgs")
                {
                    await context.LearnMsgs.AddAsync((Models.LearnMsgs)tableMeta.GetModelEntity());
                    await context.Database.CommitTransactionAsync();
                    await context.SaveChangesAsync(true);
                    await RefreshLearnMsgsList(true);
                }
                else if (tableMeta.TableName == "ModeratorApprove")
                {
                    await context.ModeratorApprove.AddAsync((Models.ModeratorApprove)tableMeta.GetModelEntity());
                    await context.Database.CommitTransactionAsync();
                    await context.SaveChangesAsync(true);
                    await RefreshModeratorApproveList(true);
                }
                else if (tableMeta.TableName == "MultiChannels")
                {
                    await context.MultiChannels.AddAsync((Models.MultiChannels)tableMeta.GetModelEntity());
                    await context.Database.CommitTransactionAsync();
                    await context.SaveChangesAsync(true);
                    await RefreshMultiChannelsList(true);
                }
                else if (tableMeta.TableName == "MultiLiveStreams")
                {
                    await context.MultiLiveStreams.AddAsync((Models.MultiLiveStreams)tableMeta.GetModelEntity());
                    await context.Database.CommitTransactionAsync();
                    await context.SaveChangesAsync(true);
                    await RefreshMultiLiveStreamsList(true);
                }
                else if (tableMeta.TableName == "MultiSummaryLiveStreams")
                {
                    await context.MultiSummaryLiveStreams.AddAsync((Models.MultiSummaryLiveStreams)tableMeta.GetModelEntity());
                    await context.Database.CommitTransactionAsync();
                    await context.SaveChangesAsync(true);
                    await RefreshMultiSummaryLiveStreamsList(true);
                }
                else if (tableMeta.TableName == "MultiWebhooks")
                {
                    await context.MultiWebhooks.AddAsync((Models.MultiWebhooks)tableMeta.GetModelEntity());
                    await context.Database.CommitTransactionAsync();
                    await context.SaveChangesAsync(true);
                    await RefreshMultiWebhooksList(true);
                }
                else if (tableMeta.TableName == "OldFollowUsers")
                {
                    await context.OldFollowUsers.AddAsync((Models.OldFollowUsers)tableMeta.GetModelEntity());
                    await context.Database.CommitTransactionAsync();
                    await context.SaveChangesAsync(true);
                    await RefreshOldFollowUsersList(true);
                }
                else if (tableMeta.TableName == "OutRaidData")
                {
                    await context.OutRaidData.AddAsync((Models.OutRaidData)tableMeta.GetModelEntity());
                    await context.Database.CommitTransactionAsync();
                    await context.SaveChangesAsync(true);
                    await RefreshOutRaidDataList(true);
                }
                else if (tableMeta.TableName == "OverlayServices")
                {
                    await context.OverlayServices.AddAsync((Models.OverlayServices)tableMeta.GetModelEntity());
                    await context.Database.CommitTransactionAsync();
                    await context.SaveChangesAsync(true);
                    await RefreshOverlayServicesList(true);
                }
                else if (tableMeta.TableName == "OverlayTicker")
                {
                    await context.OverlayTicker.AddAsync((Models.OverlayTicker)tableMeta.GetModelEntity());
                    await context.Database.CommitTransactionAsync();
                    await context.SaveChangesAsync(true);
                    await RefreshOverlayTickerList(true);
                }
                else if (tableMeta.TableName == "Quotes")
                {
                    await context.Quotes.AddAsync((Models.Quotes)tableMeta.GetModelEntity());
                    await context.Database.CommitTransactionAsync();
                    await context.SaveChangesAsync(true);
                    await RefreshQuotesList(true);
                }
                else if (tableMeta.TableName == "ShoutOuts")
                {
                    await context.ShoutOuts.AddAsync((Models.ShoutOuts)tableMeta.GetModelEntity());
                    await context.Database.CommitTransactionAsync();
                    await context.SaveChangesAsync(true);
                    await RefreshShoutOutsList(true);
                }
                else if (tableMeta.TableName == "StreamStats")
                {
                    await context.StreamStats.AddAsync((Models.StreamStats)tableMeta.GetModelEntity());
                    await context.Database.CommitTransactionAsync();
                    await context.SaveChangesAsync(true);
                    await RefreshStreamStatsList(true);
                }
                else if (tableMeta.TableName == "Users")
                {
                    await context.Users.AddAsync((Models.Users)tableMeta.GetModelEntity());
                    await context.Database.CommitTransactionAsync();
                    await context.SaveChangesAsync(true);
                    await RefreshUsersList(true);
                }
                else if (tableMeta.TableName == "UserStats")
                {
                    await context.UserStats.AddAsync((Models.UserStats)tableMeta.GetModelEntity());
                    await context.Database.CommitTransactionAsync();
                    await context.SaveChangesAsync(true);
                    await RefreshUserStatsList(true);
                }
                else if (tableMeta.TableName == "Webhooks")
                {
                    await context.Webhooks.AddAsync((Models.Webhooks)tableMeta.GetModelEntity());
                    await context.Database.CommitTransactionAsync();
                    await context.SaveChangesAsync(true);
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
                await context.Database.BeginTransactionAsync();
                await context.CategoryList.AddAsync(new(categoryId: categoryData.CategoryId, category: categoryData.CategoryName, streamCount: 0));
                await context.Database.CommitTransactionAsync();
                await context.SaveChangesAsync(true);
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
                await context.Database.BeginTransactionAsync();
                CategoryList category = await context.CategoryList
                                                     .Where(CL => (CL.Category == FormatData.AddEscapeFormat(categoryData.CategoryName))
                                                                    || CL.CategoryId == categoryData.CategoryId)
                                                     .Select(CL => CL).FirstOrDefaultAsync();
                category.StreamCount++;
                StreamStats currStream = await context.StreamStats
                          .Where(S => S.StreamStart == CurrStreamStart)
                          .Select(S => S)
                          .FirstOrDefaultAsync();
                if (currStream != default)
                {
                    currStream.Category.UniqueAdd(categoryData.CategoryName);
                }

                await context.Database.CommitTransactionAsync();
                await context.SaveChangesAsync(true);
                await RefreshCategoryListList(true);
                await RefreshStreamStatsList(true);
            }
        }

        internal async Task<bool> PostClip(string ClipId, DateTime CreatedAt, decimal Duration, string GameId, string Language, string Title, string Url, string fromUserId, string fromUserName, bool LastClip)
        {
            bool result;
            using var context = BuildDataContext();

            var found = await context.Clips
                  .Where(C => C.ClipId == ClipId)
                  .Select(C => C).AnyAsync();

            if (!found)
            {
                await context.Database.BeginTransactionAsync();
                await context.Clips.AddAsync(new(clipId: ClipId, createdAt: CreatedAt, title: Title, categoryId: GameId, language: Language, duration: (float)Duration, url: Url));
                await context.Database.CommitTransactionAsync();
                await context.SaveChangesAsync(true);
                result = true;
            }
            else
            {
                result = false;
            }

            if (LastClip)
            {
                ThreadManager.AddTaskToGUIDispatcher(async () =>
                {
                    await RefreshClipsList(true);
                });
            }

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
                await context.Database.BeginTransactionAsync();

                await context.CommandsUser.AddAsync(new(cmdName: cmd, addMe: Params.AddMe, permission: Params.Permission,
                     isEnabled: Params.IsEnabled, message: Params.Message, repeatTimer: Params.Timer, sendMsgCount: Params.RepeatMsg, category: [Params.Category],
                     allowParam: Params.AllowParam, usage: Params.Usage, lookupData: Params.LookupData, table: Params.Table, keyField: GetKey(Params.Table).Result,
                     dataField: Params.Field, currencyField: Params.Currency, unit: Params.Unit, action: Params.Action, top: Params.Top, sort: Params.Sort));

                await context.Database.CommitTransactionAsync();
                await context.SaveChangesAsync(true);
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
                await context.Database.BeginTransactionAsync();
                var newCurrType = await context.CurrencyType.AddAsync(currencyType);
                recordchange = true;

                foreach (Users U in context.Users.Include(curr => curr.Currency))
                {
                    U.Currency.Add(new(U.UserId, U.Platform, 0, newCurrType.Entity.CurrencyName));
                }

                await context.Database.CommitTransactionAsync();
                await context.SaveChangesAsync(true);
                await RefreshCurrencyTypeList(recordchange);
                await RefreshCurrencyList(recordchange);
            }
        }

        internal async Task PostCurrencyUpdate(LiveUser User, double value, string CurrencyName)
        {
            using var context = BuildDataContext();
            Currency currency = await context.Currency
                                                .Include(type => type.CurrencyType)
                                                .Where(C => (C.CurrencyName == CurrencyName && C.UserId == User.UserId))
                                                .Select(C => C).FirstOrDefaultAsync();
            if (currency != default)
            {
                await context.Database.BeginTransactionAsync();
                currency.Value = Math.Min(Math.Round(currency.Value + value, 2), currency.CurrencyType.MaxValue);
                await context.Database.CommitTransactionAsync();
                await context.SaveChangesAsync(true);
                await RefreshCurrencyList();
            }

        }

        internal async Task PostCurrencyUpdate(List<PlayGameUserWager<PlayingCardFrench, PlayingCardSuit>> Updates, string CurrencyName)
        {
            using var context = BuildDataContext();
            await context.Database.BeginTransactionAsync();
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

            await context.Database.CommitTransactionAsync();
            await context.SaveChangesAsync(true);
            await RefreshCurrencyList();
        }

        internal async Task<int> PostDeathCounterUpdate(string currCategory, bool Reset = false, int updateValue = 1)
        {
            bool recordchange = false;
            using var context = BuildDataContext();
            await context.Database.BeginTransactionAsync();
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
            await context.Database.CommitTransactionAsync();
            await context.SaveChangesAsync(true);
            await RefreshGameDeadCounterList(recordchange);

            return update?.Counter ?? 0;
        }

        internal async Task PostGiveawayData(string UserId, DateTime dateTime)
        {
            using var context = BuildDataContext();
            await context.Database.BeginTransactionAsync();
            await context.GiveawayUserData.AddAsync(new(dateTime: dateTime, userId: UserId));
            await context.Database.CommitTransactionAsync();
            await context.SaveChangesAsync(true);

            await RefreshGiveawayUserDataList(true);
        }

        internal async Task PostInRaidData(LiveUser user, DateTime time, int viewers, CategoryData gamename)
        {
            await PostCategory(gamename);
            using var context = BuildDataContext();
            await context.Database.BeginTransactionAsync();
            await PostNewUser(context, user, time);
            await context.InRaidData.AddAsync(new(userId: user.UserId, raidDate: time, viewerCount: viewers, category: gamename.CategoryName, platform: user.Platform));
            await context.Database.CommitTransactionAsync();
            await context.SaveChangesAsync(true);
            await RefreshUsersList();
            await RefreshCategoryListList(true);
            await RefreshInRaidDataList(true);
        }

        internal async Task PostLearnMsgsRow(string Message, MsgTypes MsgType)
        {
            using var context = BuildDataContext();
            await context.Database.BeginTransactionAsync();
            if (!await context.LearnMsgs
                  .Where(M => M.TeachingMsg == Message)
                  .Select(M => M).AnyAsync())
            {
                await context.LearnMsgs.AddAsync(new(msgType: MsgType, teachingMsg: Message));
                LearnMsgChanged = true;
            }
            await context.Database.CommitTransactionAsync();
            await context.SaveChangesAsync(true);
            await RefreshLearnMsgsList(LearnMsgChanged);
        }

        internal async Task<bool> PostMergeUserStats(string currUser, string sourceUser, Platform platform)
        {
            bool result = false;
            using var context = BuildDataContext();
            await context.Database.BeginTransactionAsync();
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
                result = true;
            }

            await context.Database.CommitTransactionAsync();
            await context.SaveChangesAsync(true);
            await RefreshCurrencyList(true);
            await RefreshUsersList(true);

            return result;
        }

        internal async Task PostNewAutoShoutUser(string UserId, Platform platform)
        {
            bool recordchange = false;
            using var context = BuildDataContext();
            await context.Database.BeginTransactionAsync();
            if (!await context.ShoutOuts
            .Where(SO => (SO.UserId == UserId && SO.Platform == platform))
            .Select(SO => SO).AnyAsync())
            {
                await context.ShoutOuts.AddAsync(new(userId: UserId, platform: platform));
                recordchange = true;
            }
            await context.Database.CommitTransactionAsync();
            await context.SaveChangesAsync(true);
            await RefreshShoutOutsList(recordchange);
        }

        internal async Task PostNewAutoShoutUser(LiveUser liveUser)
        {
            await PostNewAutoShoutUser(liveUser.UserId, liveUser.Platform);
        }

        internal async Task PostOutgoingRaid(string HostedChannel, DateTime dateTime)
        {
            using var context = BuildDataContext();
            await context.Database.BeginTransactionAsync();
            await context.OutRaidData.AddAsync(new(channelRaided: HostedChannel, raidDate: dateTime));
            await context.Database.CommitTransactionAsync();
            await context.SaveChangesAsync(true);
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

            await context.Database.BeginTransactionAsync();
            // Add the new quote
            await context.Quotes.AddAsync(new(number: openNum, quote: Text));
            await context.Database.CommitTransactionAsync();
            await context.SaveChangesAsync(true);

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
            bool addstream = !(await context.StreamStats.Where(S => S.StreamStart == StreamStart).Select(S => S).AnyAsync());
            if (addstream)
            {
                await context.Database.BeginTransactionAsync();
                await context.StreamStats.AddAsync(new(streamStart: StreamStart, streamEnd: StreamStart, category: [Category]));
                await context.Database.CommitTransactionAsync();
                await context.SaveChangesAsync(true);
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
                await context.Database.BeginTransactionAsync();
                currStream.Update(streamStat);
                LogWriter.DebugLog("PostStreamStat", DebugLogTypes.DataManager, $"Updated stream stats for stream started {streamStat.StreamStart}.");
                await context.Database.CommitTransactionAsync();
                await context.SaveChangesAsync(true);
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
                await context.Database.BeginTransactionAsync();
                await context.CustomWelcome.AddAsync(new(userId: User.UserId, platform: User.Platform, message: WelcomeMsg));
                await context.Database.CommitTransactionAsync();
                await context.SaveChangesAsync(true);
                await RefreshCustomWelcomeList(true);
            }
        }

        #endregion Post_Methods

    }
}
