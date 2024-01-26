using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

using StreamerBotLib.DataSQL.Models;
using StreamerBotLib.Enums;
using StreamerBotLib.GUI;
using StreamerBotLib.Interfaces;
using StreamerBotLib.Models;
using StreamerBotLib.Overlay.Enums;
using StreamerBotLib.Overlay.Models;
using StreamerBotLib.Static;
using StreamerBotLib.Systems;

using System.Collections.Concurrent;
using System.Data;
using System.Globalization;

namespace StreamerBotLib.DataSQL
{
    public class DataManagerSQL(IDbContextFactory<SQLDBContext> dbContextFactory) : IDataManager
    {
        /// <summary>
        /// always true to begin one learning cycle
        /// </summary>
        private bool LearnMsgChanged = true;

        private readonly ConcurrentQueue<Follow> followsQueue = new();

        private readonly string DefaulSocialMsg = LocalizedMsgSystem.GetVar(Msg.MsgDefaultSocialMsg);
        private DateTime CurrStreamStart { get; set; } = default;

        private readonly IDbContextFactory<SQLDBContext> dbContextFactory = dbContextFactory;

        public bool CheckCurrency(LiveUser User, double value, string CurrencyName)
        {
            lock (GUIDataManagerLock.Lock)
            {
                using var context = dbContextFactory.CreateDbContext();
                return (from C in context.Currency
                        where (C.UserName == User.UserName && C.CurrencyName == CurrencyName)
                        select C.Value).FirstOrDefault() >= value;
            }
        }

        public bool CheckField(string table, string field)
        {
            lock (GUIDataManagerLock.Lock)
            {
                using var context = dbContextFactory.CreateDbContext();
                return (context.Model.FindEntityType(table).FindProperty(field) != null);
            }
        }

        public bool CheckFollower(string User)
        {
            lock (GUIDataManagerLock.Lock)
            {
                return CheckFollower(User, default);
            }
        }

        public bool CheckFollower(string User, DateTime ToDateTime)
        {
            lock (GUIDataManagerLock.Lock)
            {
                using var context = dbContextFactory.CreateDbContext();
                return (from f in context.Followers
                        where (f.IsFollower && f.UserName == User && (ToDateTime == default || f.FollowedDate < ToDateTime))
                        select f).Any();
            }
        }

        public Tuple<string, string> CheckModApprovalRule(ModActionType modActionType, string ModAction)
        {
            lock (GUIDataManagerLock.Lock)
            {
                using var context = dbContextFactory.CreateDbContext();
                return (from M in context.ModeratorApprove
                        where (M.ModActionType == modActionType && M.ModActionName == ModAction)
                        select new Tuple<string, string>(
                            !string.IsNullOrEmpty(M.ModPerformType.ToString()) ? M.ModPerformType.ToString() : M.ModActionType.ToString(),
                            !string.IsNullOrEmpty(M.ModPerformAction) ? M.ModPerformAction : M.ModActionName
                            )).FirstOrDefault();
            }
        }

        /// <summary>
        /// Determines if there are multiple streams based on the same start date.
        /// </summary>
        /// <param name="streamStart">The stream start date and time to check.</param>
        /// <returns><code>true</code> if there are multiple streams
        /// <code>false</code> if there is no more than one stream.</returns>
        public bool CheckMultiStreams(DateTime streamStart)
        {
            lock (GUIDataManagerLock.Lock)
            {
                using var context = dbContextFactory.CreateDbContext();
                return (from s in context.StreamStats
                        where (s.StreamStart == streamStart)
                        select s).Count() > 1;
            }
        }

        /// <summary>
        /// Determine if the user invoking the command has permission to access the command.
        /// </summary>
        /// <param name="cmd">The command to verify the permission.</param>
        /// <param name="permission">The supplied permission to check.</param>
        /// <returns><code>true</code> - the permission is allowed to the command. <code>false</code> - the command permission is not allowed.</returns>
        public bool CheckPermission(string cmd, ViewerTypes permission)
        {
            lock (GUIDataManagerLock.Lock)
            {
                using var context = dbContextFactory.CreateDbContext();
                return (from c in context.Commands
                        where c.CmdName == cmd
                        select c).FirstOrDefault().Permission > permission;
            }
        }

        /// <summary>
        /// Verify if the provided UserName is within the ShoutOut table.
        /// </summary>
        /// <param name="UserName">The UserName to shoutout.</param>
        /// <returns>true if in the ShoutOut table.</returns>
        public bool CheckShoutName(string UserId)
        {
            lock (GUIDataManagerLock.Lock)
            {
                using var context = dbContextFactory.CreateDbContext();
                return (from s in context.ShoutOuts
                        where s.UserId == UserId
                        select s).Any();
            }
        }

        /// <summary>
        /// Find if stream data already exists for the current stream
        /// </summary>
        /// <param name="CurrTime">The time to check</param>
        /// <returns><code>true</code>: the stream already has a data entry; <code>false</code>: the stream has no data entry</returns>
        public bool CheckStreamTime(DateTime CurrTime)
        {
            lock (GUIDataManagerLock.Lock)
            {
                using var context = dbContextFactory.CreateDbContext();
                return (from s in context.StreamStats
                        where s.StreamStart == CurrTime
                        select s).Any();
            }
        }

        /// <summary>
        /// Check to see if the <paramref name="User"/> has been in the channel prior to DateTime.MaxValue.
        /// </summary>
        /// <param name="User">The user to check in the database.</param>
        /// <returns><code>true</code> if the <paramref name="User"/> has arrived anytime, <code>false</code> otherwise.</returns>
        public bool CheckUser(LiveUser User)
        {
            lock (GUIDataManagerLock.Lock)
            {
                return CheckUser(User, default);
            }
        }

        /// <summary>
        /// Check if the <paramref name="User"/> has visited the channel prior to <paramref name="ToDateTime"/>, identified as either DateTime.Now.ToLocalTime() or the current start of the stream.
        /// </summary>
        /// <param name="User">The user to verify.</param>
        /// <param name="ToDateTime">Specify the date to check if the user arrived to the channel prior to this date and time.</param>
        /// <returns><c>True</c> if the <paramref name="User"/> has been in channel before <paramref name="ToDateTime"/>, <c>false</c> otherwise.</returns>
        public bool CheckUser(LiveUser User, DateTime ToDateTime)
        {
            lock (GUIDataManagerLock.Lock)
            {
                using var context = dbContextFactory.CreateDbContext();
                return (from s in context.Users
                        where (ToDateTime == default || s.FirstDateSeen < ToDateTime) && s.UserName == User.UserName && s.Platform == User.Source
                        select s).Any();
            }
        }

        /// <summary>
        /// Check the CustomWelcome table for the user and provide the message.
        /// </summary>
        /// <param name="User">The user to check for a welcome message.</param>
        /// <returns>The welcome message if user is available, or empty string if not found.</returns>
        public string CheckWelcomeUser(string User)
        {
            lock (GUIDataManagerLock.Lock)
            {
                using var context = dbContextFactory.CreateDbContext();
                return (from s in context.CustomWelcome
                        where s.UserName == User
                        select s.Message).FirstOrDefault() ?? "";
            }
        }

        public void ClearAllCurrencyValues()
        {
            lock (GUIDataManagerLock.Lock)
            {
                using var context = dbContextFactory.CreateDbContext();
                foreach (Currency c in from u in context.Currency
                                       select u)
                {
                    c.Value = 0;
                }
                context.SaveChanges();
            }
        }

        /// <summary>
        /// Clear all User rows for users not included in the Followers table.
        /// </summary>
        public void ClearUsersNotFollowers()
        {
            lock (GUIDataManagerLock.Lock)
            {
                using var context = dbContextFactory.CreateDbContext();

                context.Users.RemoveRange((IEnumerable<Users>)(from user in context.Users
                                                               join follower in context.Followers on user.UserId equals follower.UserId into UserFollow
                                                               from subuser in UserFollow.DefaultIfEmpty()
                                                               where !subuser.IsFollower
                                                               select subuser));
                context.SaveChanges();
            }
        }

        /// <summary>
        /// Clear all user watchtimes
        /// </summary>
        public void ClearWatchTime()
        {
            lock (GUIDataManagerLock.Lock)
            {
                using var context = dbContextFactory.CreateDbContext();
                foreach (var userstat in from US in context.UserStats
                                         select US)
                {
                    userstat.WatchTime = new(0);
                }
                context.SaveChanges();
            }
        }

        public void DeleteDataRows(IEnumerable<DataRow> dataRows)
        {
        }

        public string EditCommand(string cmd, List<string> Arglist)
        {
            string result = "";

            lock (GUIDataManagerLock.Lock)
            {
                using var context = dbContextFactory.CreateDbContext();
                Dictionary<string, string> EditParamsDict = CommandParams.ParseEditCommandParams(Arglist);
                Commands EditCom = (from C in context.Commands
                                    where C.CmdName == cmd
                                    select C).FirstOrDefault();

                if (EditCom != default)
                {
                    foreach (string k in EditParamsDict.Keys)
                    {
                        EditCom.GetType().GetProperty(k).SetValue(EditCom, EditParamsDict[k]);
                    }
                    result = string.Format(CultureInfo.CurrentCulture, LocalizedMsgSystem.GetDefaultComMsg(DefaultCommand.editcommand), cmd);
                    context.SaveChanges(true);
                }
                else
                {
                    result = string.Format(CultureInfo.CurrentCulture, LocalizedMsgSystem.GetVar("Msgcommandnotfound"), cmd);

                }
            }
            return result;
        }

        public Tuple<ModActions, Enums.BanReasons, int> FindRemedy(ViewerTypes viewerTypes, MsgTypes msgTypes)
        {
            lock (GUIDataManagerLock.Lock)
            {
                using var context = dbContextFactory.CreateDbContext();
                Enums.BanReasons banReasons = (from Br in context.BanReasons
                                               where Br.MsgType == msgTypes
                                               select Br.BanReason).FirstOrDefault();
                BanRules banRules = (from B in context.BanRules
                                     where (B.ViewerTypes == ViewerTypes.Viewer && B.MsgTypes == msgTypes)
                                     select B).FirstOrDefault();

                return new(banRules?.ModAction ?? ModActions.Allow, banReasons, banRules.TimeoutSeconds);
            }
        }

        public CommandData GetCommand(string cmd)
        {
            lock (GUIDataManagerLock.Lock)
            {
                using var context = dbContextFactory.CreateDbContext();
                return new((from Com in context.Commands
                            where Com.CmdName == cmd
                            select Com).FirstOrDefault());
            }
        }

        public IEnumerable<string> GetCommandList()
        {
            return GetCommands().Split(", ");
        }

        public string GetCommands()
        {
            lock (GUIDataManagerLock.Lock)
            {
                using var context = dbContextFactory.CreateDbContext();
                return string.Join(", ", (from Com in context.Commands
                                          where (Com.Message == DefaulSocialMsg && Com.IsEnabled)
                                          orderby Com.CmdName
                                          select $"!{Com.CmdName}"));
            }
        }

        public List<string> GetCurrencyNames()
        {
            lock (GUIDataManagerLock.Lock)
            {
                using var context = dbContextFactory.CreateDbContext();
                return new(from C in context.CurrencyType
                           select C.CurrencyName);
            }
        }

        public int GetDeathCounter(string currCategory)
        {
            lock (GUIDataManagerLock.Lock)
            {
                using var context = dbContextFactory.CreateDbContext();
                return (from D in context.GameDeadCounter
                        where D.Category == currCategory
                        select D.Counter).FirstOrDefault();
            }
        }

        public string GetEventRowData(ChannelEventActions rowcriteria, out bool Enabled, out short Multi)
        {
            lock (GUIDataManagerLock.Lock)
            {
                using var context = dbContextFactory.CreateDbContext();
                ChannelEvents found = (from Event in context.ChannelEvents
                                       where Event.Name == rowcriteria.ToString()
                                       select Event).FirstOrDefault();
                Enabled = found.IsEnabled;
                Multi = found.RepeatMsg;
                return found.Message;
            }

        }

        public int GetFollowerCount()
        {
            lock (GUIDataManagerLock.Lock)
            {
                using var context = dbContextFactory.CreateDbContext();
                return (from F in context.Followers
                        select F).Count();
            }
        }

        public List<Tuple<string, string>> GetGameCategories()
        {
            lock (GUIDataManagerLock.Lock)
            {
                using var context = dbContextFactory.CreateDbContext();
                return new(from G in context.CategoryList
                           let game = new Tuple<string, string>(G.CategoryId, G.Category)
                           select game);
            }
        }

        public string GetKey(string Table)
        {
            lock (GUIDataManagerLock.Lock)
            {
                using var context = dbContextFactory.CreateDbContext();
                return context.Model.FindEntityType(Table).FindPrimaryKey().GetName();
            }
        }

        public IEnumerable<string> GetKeys(string Table)
        {
            lock (GUIDataManagerLock.Lock)
            {
                using var context = dbContextFactory.CreateDbContext();
                return new List<string>(from P in context.Model.FindEntityType(Table).FindPrimaryKey().Properties select P.Name);
            }
        }

        public string GetNewestFollower()
        {
            lock (GUIDataManagerLock.Lock)
            {
                using var context = dbContextFactory.CreateDbContext();
                return context.Followers.MaxBy((i) => i.FollowedDate).UserName;
            }
        }

        public List<OverlayActionType> GetOverlayActions(string overlayType, string overlayAction, string username)
        {
            lock (GUIDataManagerLock.Lock)
            {
                using var context = dbContextFactory.CreateDbContext();
                return new(from O in context.OverlayServices
                           where (O.IsEnabled
                           && O.OverlayType.ToString() == overlayType
                           && (string.IsNullOrEmpty(O.UserName) || O.UserName == username)
                           && O.OverlayAction == overlayAction)
                           select new OverlayActionType()
                           {
                               ActionValue = O.OverlayAction,
                               Duration = O.Duration,
                               MediaFile = O.MediaFile,
                               ImageFile = O.ImageFile,
                               Message = O.Message,
                               OverlayType = O.OverlayType,
                               UserName = O.UserName,
                               UseChatMsg = O.UseChatMsg
                           });
            }
        }

        public string GetQuote(int QuoteNum)
        {
            lock (GUIDataManagerLock.Lock)
            {
                using var context = dbContextFactory.CreateDbContext();
                return (from Q in context.Quotes
                        where Q.Number == QuoteNum
                        select $"{Q.Number}: {Q.Quote}").FirstOrDefault();
            }
        }

        public int GetQuoteCount()
        {
            lock (GUIDataManagerLock.Lock)
            {
                using var context = dbContextFactory.CreateDbContext();
                return context.Quotes.MaxBy((q) => q.Number)?.Number ?? 0;
            }

        }

        public Dictionary<string, List<string>> GetOverlayActions()
        {
            lock (GUIDataManagerLock.Lock)
            {
                using var context = dbContextFactory.CreateDbContext();
                return new()
                {
                    { nameof(Commands), new(from C in context.Commands select C.CmdName) },
                    { nameof(ChannelEvents), new(from E in context.ChannelEvents select E.Name) }
                };
            }
        }

        public List<string> GetSocialComs()
        {
            lock (GUIDataManagerLock.Lock)
            {
                using var context = dbContextFactory.CreateDbContext();
                return new(from SC in context.Commands.IntersectBy((string[])Enum.GetValues(typeof(DefaultSocials)), (c) => c.CmdName)
                           select SC.CmdName);
            }
        }

        public string GetSocials()
        {
            lock (GUIDataManagerLock.Lock)
            {
                using var context = dbContextFactory.CreateDbContext();
                return string.Join(" ", (from SC in
                                             context.Commands.IntersectBy((string[])Enum.GetValues(typeof(DefaultSocials)),
                                                (c) => c.CmdName)
                                         select SC));
            }
        }

        public StreamStat GetStreamData(DateTime dateTime)
        {
            lock (GUIDataManagerLock.Lock)
            {
                using var context = dbContextFactory.CreateDbContext();
                return (from SD in context.StreamStats
                        where SD.StreamStart == dateTime
                        select new StreamStat(SD)).FirstOrDefault();
            }
        }

        public List<string> GetTableFields(string TableName)
        {
            lock (GUIDataManagerLock.Lock)
            {
                using var context = dbContextFactory.CreateDbContext();
                return new(from T in context.Model.FindEntityType(TableName).GetMembers()
                           select T.Name);
            }
        }

        public List<string> GetTableNames()
        {
            lock (GUIDataManagerLock.Lock)
            {
                using var context = dbContextFactory.CreateDbContext();
                return new(from N in context.Model.GetEntityTypes()
                           select N.Name);
            }
        }

        public List<TickerItem> GetTickerItems()
        {
            lock (GUIDataManagerLock.Lock)
            {
                using var context = dbContextFactory.CreateDbContext();
                return new(from F in context.OverlayTicker
                           select new TickerItem(F.TickerName, F.UserName));
            }
        }

        public Tuple<string, int, string[]> GetTimerCommand(string Cmd)
        {
            lock (GUIDataManagerLock.Lock)
            {
                using var context = dbContextFactory.CreateDbContext();
                return (from R in context.Commands
                        where R.RepeatTimer > 0
                        select new Tuple<string, int, string[]>(R.CmdName, R.RepeatTimer, (from C in R.Category
                                                                                            select C).ToArray())).FirstOrDefault();
            }
        }

        public List<Tuple<string, int, string[]>> GetTimerCommands()
        {
            lock (GUIDataManagerLock.Lock)
            {
                using var context = dbContextFactory.CreateDbContext();
                return new(from R in context.Commands
                           where R.RepeatTimer > 0
                           select new Tuple<string, int, string[]>(R.CmdName, R.RepeatTimer, (from C in R.Category
                                                                                               select C).ToArray()));
            }
        }

        public int GetTimerCommandTime(string Cmd)
        {
            lock (GUIDataManagerLock.Lock)
            {
                using var context = dbContextFactory.CreateDbContext();
                return (from R in context.Commands
                        where R.CmdName == Cmd
                        select R.RepeatTimer).FirstOrDefault();
            }
        }

        public string GetUsage(string command)
        {
            lock (GUIDataManagerLock.Lock)
            {
                using var context = dbContextFactory.CreateDbContext();
                return (from C in context.Commands
                        where C.CmdName == command
                        select C.Usage).FirstOrDefault();
            }
        }

        public LiveUser GetUser(string UserName)
        {
            lock (GUIDataManagerLock.Lock)
            {
                using var context = dbContextFactory.CreateDbContext();
                return (from U in context.Users
                        where U.UserName == UserName
                        select new LiveUser(U.UserName, U.Platform, U.UserId)).FirstOrDefault();
            }
        }

        public string GetUserId(LiveUser User)
        {
            lock (GUIDataManagerLock.Lock)
            {
                using var context = dbContextFactory.CreateDbContext();
                return (from U in context.Users
                        where U.UserName == User.UserName
                        select U.UserId).FirstOrDefault();
            }
        }

        public List<Tuple<bool, Uri>> GetWebhooks(WebhooksSource webhooksSource, WebhooksKind webhooks)
        {
            lock (GUIDataManagerLock.Lock)
            {
                using var context = dbContextFactory.CreateDbContext();
                return new(from W in context.Webhooks
                           where (W.WebhooksSource == webhooksSource && W.Kind == webhooks)
                           select new Tuple<bool, Uri>(W.AddEveryone, W.Webhook));
            }
        }

        public void Initialize()
        {
            throw new NotImplementedException();
        }
        // TODO: complete the DBContext for "perform query"
        public object[] PerformQuery(Commands row, int Top = 0)
        {
            throw new NotImplementedException();
        }

        public object PerformQuery(Commands row, string ParamValue)
        {
            throw new NotImplementedException();
        }

        public bool PostCategory(string CategoryId, string newCategory)
        {
            bool found = false;
            if (string.IsNullOrEmpty(CategoryId) && string.IsNullOrEmpty(newCategory))
            {
                found = false;
            }
            else
            {
                lock (GUIDataManagerLock.Lock)
                {
                    using var context = dbContextFactory.CreateDbContext();
                    CategoryList categoryList = (from CL in context.CategoryList
                                                 where (CL.Category == FormatData.AddEscapeFormat(newCategory)) || CL.CategoryId == CategoryId
                                                 select CL).FirstOrDefault();
                    if (categoryList == default)
                    {
                        context.CategoryList.Add(new(categoryId: CategoryId, category: newCategory));
                        found = true;
                    }
                    else
                    {
                        categoryList.CategoryId ??= CategoryId;
                        categoryList.Category ??= newCategory;
                        if (OptionFlags.IsStreamOnline)
                        {
                            categoryList.StreamCount++;
                        }
                        found = true;
                    }
                    context.SaveChanges(true);
                }
            }
            return found;
        }

        public bool PostClip(int ClipId, DateTime CreatedAt, decimal Duration, string GameId, string Language, string Title, string Url)
        {
            lock (GUIDataManagerLock.Lock)
            {
                using var context = dbContextFactory.CreateDbContext();
                if (!(from C in context.Clips
                      where (C.Id == ClipId)
                      select C).Any())
                {
                    context.Clips.Add(new(ClipId, CreatedAt, Title, GameId, Language, Duration, Url));
                    context.SaveChanges(true);
                    return true;
                }
                return false;
            }
        }

        public string PostCommand(string cmd, CommandParams Params)
        {
            lock (GUIDataManagerLock.Lock)
            {
                using var context = dbContextFactory.CreateDbContext();
                if (!(from Com in context.Commands
                      where Com.CmdName == cmd
                      select Com).Any())
                {
                    context.Commands.Add(new(id: 0, cmd, Params.AddMe, Params.Permission,
                    Params.IsEnabled, Params.Message, Params.Timer, Params.RepeatMsg, new[] { Params.Category },
                    Params.AllowParam, Params.Usage, Params.LookupData, Params.Table, GetKey(Params.Table),
                    Params.Field, Params.Currency, Params.Unit, Params.Action, Params.Top, Params.Sort));

                    context.SaveChanges(true);
                    return string.Format(CultureInfo.CurrentCulture, LocalizedMsgSystem.GetDefaultComMsg(DefaultCommand.addcommand), cmd);
                }
                return string.Format(CultureInfo.CurrentCulture, LocalizedMsgSystem.GetVar(Msg.MsgAddCommandFailed), cmd);
            }
        }

        //public void PostCurrencyRows()
        //{
        //    lock (GUIDataManagerLock.Lock)
        //    {
        //        using var context = dbContextFactory.CreateDbContext();
        //        List<string> currencyTypeName = new(from CT in context.CurrencyType
        //                                            select CT.CurrencyName);

        //        foreach(Users U in context.Users)
        //        {
        //            foreach(string CurrencyName in currencyTypeName)
        //            {
        //                if(!(from C in context.Currency where (C.UserName == U.UserName && C.CurrencyName == CurrencyName) select C.CurrencyName).Any())
        //                {
        //                    context.Currency.Add()
        //                }
        //            }

        //        }
        //    } 
        //}

        //public void PostCurrencyRows(ref DataSource.UsersRow usersRow)
        //{
        //    lock (GUIDataManagerLock.Lock)
        //    {
        //        using var context = dbContextFactory.CreateDbContext();
        //    }
        //}

        public void PostCurrencyUpdate(LiveUser User, double value, string CurrencyName)
        {
            lock (GUIDataManagerLock.Lock)
            {
                using var context = dbContextFactory.CreateDbContext();
                Currency currency = (from C in context.Currency
                                     where (C.CurrencyName == CurrencyName && C.UserName == User.UserName)
                                     select C).FirstOrDefault();
                if (currency != default)
                {
                    currency.Value = Math.Min(Math.Round(currency.Value + value, 2), currency.CurrencyType.MaxValue);
                }
                context.SaveChanges();
            }
        }

        public int PostDeathCounterUpdate(string currCategory, bool Reset = false, int updateValue = 1)
        {
            lock (GUIDataManagerLock.Lock)
            {
                using var context = dbContextFactory.CreateDbContext();
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
                context.SaveChanges();
                return update?.Counter ?? 0;
            }
        }

        /// <summary>
        /// Add a new follower to the database.
        /// </summary>
        /// <param name="follow">The follower information to add to the database.</param>
        /// <returns>
        ///     <code>true</code>: first time follower; 
        ///     <code>false</code>: user previously followed.
        /// </returns>
        public bool PostFollower(Follow follow)
        {
            lock (GUIDataManagerLock.Lock)
            {
                using var context = dbContextFactory.CreateDbContext();

                followsQueue.Enqueue(follow);
                PostFollowsQueue();

                return !(from F in context.Followers
                         where F.UserId == follow.FromUser.UserId && F.Platform == follow.FromUser.Source
                         select F).Any();
            }
        }

        private static bool ProcessFollowQueuestarted = false;

        /// <summary>
        /// Threaded database update to add followers.
        /// </summary>
        private void PostFollowsQueue()
        {
            if (!ProcessFollowQueuestarted)
            {
                ProcessFollowQueuestarted = true;

                ThreadManager.CreateThreadStart(() =>
                {
                    DateTime currTime = DateTime.Now.ToLocalTime();
                    Thread.Sleep(1000); // wait some to stay inside while loop for lots of followers at one time
                    using var context = dbContextFactory.CreateDbContext();
                    while (followsQueue.TryDequeue(out Follow currUser))
                    {
                        lock (GUIDataManagerLock.Lock)
                        {
                            Followers userfollow = (from F in context.Followers
                                                    where (F.UserId == currUser.FromUser.UserId
                                                    && F.UserName == currUser.FromUser.UserName
                                                    && F.Platform == currUser.FromUser.Source)
                                                    select F).FirstOrDefault();
                            if (userfollow != default)
                            {
                                userfollow.IsFollower = true;
                                userfollow.FollowedDate = currUser.FollowedAt;
                                if (string.IsNullOrEmpty(userfollow.Category))
                                {
                                    userfollow.Category = currUser.Category;
                                }
                            }
                            else
                            {
                                context.Followers.Add(new(userId: currUser.FromUser.UserId,
                                    userName: currUser.FromUser.UserName, platform: currUser.FromUser.Source,
                                    isFollower: true, followedDate: currUser.FollowedAt,
                                    statusChangeDate: currUser.FollowedAt, addDate: currTime,
                                    category: currUser.Category));
                            }
                        }
                    }
                    lock (GUIDataManagerLock.Lock)
                    {
                        context.SaveChanges(true);
                    }
                    ProcessFollowQueuestarted = false;
                });

            }
        }

        public void PostGiveawayData(string DisplayName, DateTime dateTime)
        {
            lock (GUIDataManagerLock.Lock)
            {
                using var context = dbContextFactory.CreateDbContext();
                context.GiveawayUserData.Add(new(dateTime, userName: DisplayName));
                context.SaveChanges();
            }
        }

        public void PostInRaidData(string user, DateTime time, int viewers, string gamename, Platform platform)
        {
            lock (GUIDataManagerLock.Lock)
            {
                using var context = dbContextFactory.CreateDbContext();
                context.InRaidData.Add(new(userName: user, raidDate: time, viewerCount: viewers, category: gamename, platform: platform));
                context.SaveChanges();
            }
        }

        public void PostLearnMsgsRow(string Message, MsgTypes MsgType)
        {
            lock (GUIDataManagerLock.Lock)
            {
                using var context = dbContextFactory.CreateDbContext();
                if (!(from M in context.LearnMsgs
                      where M.TeachingMsg == Message
                      select M).Any())
                {
                    context.LearnMsgs.Add(new(msgType: MsgType, teachingMsg: Message));
                    LearnMsgChanged = true;
                }
                context.SaveChanges();
            }
        }

        public bool PostMergeUserStats(string CurrUser, string SourceUser, Platform platform)
        {
            lock (GUIDataManagerLock.Lock)
            {
                using var context = dbContextFactory.CreateDbContext();
                IEnumerable<Currency> userCurrency = from uCu in context.Currency where uCu.UserName == CurrUser select uCu;
                IEnumerable<Currency> srcCurrency = from sCu in context.Currency where sCu.UserName == SourceUser select sCu;

                foreach ((Currency UC, Currency SC) in (from U in userCurrency
                                                        from S in srcCurrency
                                                        where U.CurrencyName == S.CurrencyName
                                                        select (U, S)))
                {
                    UC.Add(SC);
                }
                context.Currency.RemoveRange(srcCurrency);

                UserStats currUserstat = (from Cu in context.UserStats where (Cu.UserName == CurrUser && Cu.Platform == platform) select Cu).FirstOrDefault();
                UserStats sourceUser = (from Su in context.UserStats where (Su.UserName == SourceUser && Su.Platform == platform) select Su).FirstOrDefault();

                if (currUserstat != default && sourceUser != default)
                {
                    currUserstat += sourceUser;

                    context.UserStats.Remove(sourceUser);
                    context.SaveChanges(true);

                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public void PostNewAutoShoutUser(string UserName, string UserId, Platform platform)
        {
            lock (GUIDataManagerLock.Lock)
            {
                using var context = dbContextFactory.CreateDbContext();
                if (!(from SO in context.ShoutOuts where (SO.UserId == UserId && SO.UserName == UserName && SO.Platform == platform) select SO).Any())
                {
                    context.ShoutOuts.Add(new(userId: UserId, userName: UserName, platform: platform));
                }
                context.SaveChanges();
            }
        }

        private Users PostNewUser(LiveUser User, DateTime FirstSeen)
        {
            lock (GUIDataManagerLock.Lock)
            {
                using var context = dbContextFactory.CreateDbContext();
                Users newuser = (from U in context.Users where (U.UserName == User.UserName && U.Platform == User.Source) select U).FirstOrDefault();
                if (newuser == default)
                {
                    newuser = context.Users.Add(new(userId: User.UserId, userName: User.UserName, platform: User.Source, firstDateSeen: FirstSeen, currLoginDate: FirstSeen, lastDateSeen: FirstSeen)).Entity;
                }
                if (FirstSeen <= newuser.FirstDateSeen) { newuser.FirstDateSeen = FirstSeen; }
                newuser.UserId ??= User.UserId;
                if (newuser.Platform == default) { newuser.Platform = User.Source; }
                context.SaveChanges();
                return newuser;
            }
        }

        public void PostOutgoingRaid(string HostedChannel, DateTime dateTime)
        {
            lock (GUIDataManagerLock.Lock)
            {
                using var context = dbContextFactory.CreateDbContext();
                context.OutRaidData.Add(new(channelRaided: HostedChannel, raidDate: dateTime));
                context.SaveChanges();
            }
        }

        public int PostQuote(string Text)
        {
            lock (GUIDataManagerLock.Lock)
            {
                using var context = dbContextFactory.CreateDbContext();

                List<Quotes> quotes = new(from Q in context.Quotes select Q);
                short opennum = (from Q in context.Quotes select Q.Number).IntersectBy(Enumerable.Range(1, quotes.Count > 0 ? quotes.Max((f) => f.Number) : 1), q => q).Min();

                context.Quotes.Add(new(opennum, Text));
                context.SaveChanges();
                return opennum;
            }
        }

        /// <summary>
        /// Starts a new Stream record, if it doesn't currently exist.
        /// </summary>
        /// <param name="StreamStart">The time of stream start.</param>
        /// <returns><code>true</code>: for posting a new stream start; <code>false</code>: when a stream start date row already exists</returns>
        public bool PostStream(DateTime StreamStart)
        {
            lock (GUIDataManagerLock.Lock)
            {
                using var context = dbContextFactory.CreateDbContext();
                bool addstream = !(from S in context.StreamStats where S.StreamStart == StreamStart select S).Any();
                if (addstream)
                {
                    context.StreamStats.Add(new(streamStart: StreamStart, streamEnd: StreamStart));
                    context.SaveChanges(true);
                }

                return addstream;
            }
        }

        public void PostStreamStat(StreamStat streamStat)
        {
            lock (GUIDataManagerLock.Lock)
            {
                using var context = dbContextFactory.CreateDbContext();
                StreamStats currStream = (from S in context.StreamStats where S.StreamStart == streamStat.StreamStart select S).FirstOrDefault();
                if (currStream != default)
                {
                    currStream.Update(streamStat);
                    context.SaveChanges(true);
                }
            }
        }

        public void PostUserCustomWelcome(LiveUser User, string WelcomeMsg)
        {
            lock (GUIDataManagerLock.Lock)
            {
                using var context = dbContextFactory.CreateDbContext();
                if (!(from W in context.CustomWelcome where W.UserId == User.UserId select W).Any())
                {
                    context.CustomWelcome.Add(new(userId: User.UserId, userName: User.UserName, platform: User.Source, message: WelcomeMsg));
                    context.SaveChanges(true);
                }
            }
        }

        public void RemoveAllFollowers()
        {
            lock (GUIDataManagerLock.Lock)
            {
                using var context = dbContextFactory.CreateDbContext();
                context.Followers.RemoveRange(context.Followers);
                context.SaveChanges();
            }
        }

        public void RemoveAllGiveawayData()
        {
            lock (GUIDataManagerLock.Lock)
            {
                using var context = dbContextFactory.CreateDbContext();
                context.GiveawayUserData.RemoveRange(context.GiveawayUserData);
                context.SaveChanges();
            }
        }

        public void RemoveAllInRaidData()
        {
            lock (GUIDataManagerLock.Lock)
            {
                using var context = dbContextFactory.CreateDbContext();
                context.InRaidData.RemoveRange(context.InRaidData);
                context.SaveChanges();
            }
        }

        public void RemoveAllOutRaidData()
        {
            lock (GUIDataManagerLock.Lock)
            {
                using var context = dbContextFactory.CreateDbContext();
                context.OutRaidData.RemoveRange(context.OutRaidData);
                context.SaveChanges();
            }
        }

        public void RemoveAllOverlayTickerData()
        {
            lock (GUIDataManagerLock.Lock)
            {
                using var context = dbContextFactory.CreateDbContext();
                context.OverlayTicker.RemoveRange(context.OverlayTicker);
                context.SaveChanges();
            }
        }

        public void RemoveAllStreamStats()
        {
            lock (GUIDataManagerLock.Lock)
            {
                using var context = dbContextFactory.CreateDbContext();
                context.StreamStats.RemoveRange(context.StreamStats);
                context.SaveChanges();
            }
        }

        public void RemoveAllUsers()
        {
            lock (GUIDataManagerLock.Lock)
            {
                using var context = dbContextFactory.CreateDbContext();
                context.Users.RemoveRange(context.Users);
                context.SaveChanges();
            }
        }

        public bool RemoveCommand(string command)
        {
            lock (GUIDataManagerLock.Lock)
            {
                bool found = false;
                using var context = dbContextFactory.CreateDbContext();
                Commands cmd = (from C in context.Commands where C.CmdName == command select C).FirstOrDefault();
                if (cmd != default)
                {
                    context.Commands.Remove(cmd);
                    found = true;
                }
                context.SaveChanges();
                return found;
            }
        }

        public bool RemoveQuote(int QuoteNum)
        {
            lock (GUIDataManagerLock.Lock)
            {
                bool found = false;
                using var context = dbContextFactory.CreateDbContext();
                Quotes quotes = (from Q in context.Quotes where Q.Number == QuoteNum select Q).FirstOrDefault();
                if (quotes != default)
                {
                    context.Quotes.Remove(quotes);
                    found = true;
                }
                context.SaveChanges();
                return found;
            }
        }

        public void SetBuiltInCommandsEnabled(bool Enabled)
        {
            lock (GUIDataManagerLock.Lock)
            {
                using var context = dbContextFactory.CreateDbContext();
                foreach (var Command in context.Commands.IntersectBy((string[])(Enum.GetValues(typeof(DefaultCommand))), (Com) => Com.CmdName))
                {
                    Command.IsEnabled = Enabled;
                }
                context.SaveChanges();
            }
        }

        public void SetWebhooksEnabled(bool Enabled)
        {
            lock (GUIDataManagerLock.Lock)
            {
                using var context = dbContextFactory.CreateDbContext();
                foreach (var webhook in context.Webhooks)
                {
                    webhook.IsEnabled = Enabled;
                }
                context.SaveChanges();
            }
        }

        public void SetIsEnabled(IEnumerable<DataRow> dataRows, bool IsEnabled = false)
        {
            throw new NotImplementedException();
        }

        public void SetSystemEventsEnabled(bool Enabled)
        {
            lock (GUIDataManagerLock.Lock)
            {
                using var context = dbContextFactory.CreateDbContext();
                foreach (var Sys in context.ChannelEvents)
                {
                    Sys.IsEnabled = Enabled;
                }
                context.SaveChanges();
            }
        }

        public void SetUserDefinedCommandsEnabled(bool Enabled)
        {
            lock (GUIDataManagerLock.Lock)
            {
                using var context = dbContextFactory.CreateDbContext();
                foreach (var Command in context.Commands.ExceptBy((string[])(Enum.GetValues(typeof(DefaultCommand))), (Com) => Com.CmdName))
                {
                    Command.IsEnabled = Enabled;
                }
                context.SaveChanges();
            }
        }

        public void StartBulkFollowers()
        {
            lock (GUIDataManagerLock.Lock)
            {
                using var context = dbContextFactory.CreateDbContext();
                foreach (Followers F in context.Followers)
                {
                    F.IsFollower = false; // reset all followers to not following, add existing followers back as followers
                }
                context.SaveChanges(true);
            }
        }

        public void StopBulkFollows()
        {
            lock (GUIDataManagerLock.Lock)
            {
                while (ProcessFollowQueuestarted) { } // spin until the queue processing finishes

                DateTime currtime = DateTime.Now.ToLocalTime();
                using var context = dbContextFactory.CreateDbContext();

                if (OptionFlags.TwitchPruneNonFollowers)
                {
                    context.Followers.RemoveRange(from R in context.Followers where !R.IsFollower select R);
                }
                else // if pruning followers, there won't be multiple 'UserId' records
                {
                    foreach (var UF in (from F in context.Followers
                                        orderby F.AddDate descending // the higher Id entries are after initial entries
                                        group F by F.UserId into UserIdFollows
                                        select UserIdFollows))
                    {
                        if (UF.Count() == 1 && UF.First().StatusChangeDate != UF.First().FollowedDate)
                        {
                            UF.First().StatusChangeDate = UF.First().FollowedDate;
                        }
                        else if (UF.Count() > 1)
                        { // adjust the status change date to the current time
                            List<Followers> currUser = new(UF.Take(2));

                            if (currUser[0].StatusChangeDate != currUser[1].StatusChangeDate)
                            { // only change when the dates are different, show user which records change
                                currUser[0].StatusChangeDate = currtime;
                                currUser[1].StatusChangeDate = currtime;
                            }
                        }
                    }
                }
                context.SaveChanges(true);
            }
        }

        public bool TestInRaidData(string user, DateTime time, string viewers, string gamename)
        {
            throw new NotImplementedException();
        }

        public bool TestOutRaidData(string HostedChannel, DateTime dateTime)
        {
            throw new NotImplementedException();
        }

        public void UpdateCurrency(List<string> Users, DateTime dateTime)
        {
            lock (GUIDataManagerLock.Lock)
            {
                using var context = dbContextFactory.CreateDbContext();
                foreach (string U in Users)
                {
                    UpdateCurrency((from user in context.Users where user.UserName == U select user).FirstOrDefault(), dateTime);
                }
                context.SaveChanges();
            }

        }

        private void UpdateCurrency(Users User, DateTime CurrTime)
        {
            lock (GUIDataManagerLock.Lock)
            {
                using var context = dbContextFactory.CreateDbContext();
                TimeSpan clock = CurrTime - User.CurrLoginDate;
                foreach (Currency currency in User.Currency)
                {
                    currency.Value =
                        Math.Min(
                            currency.CurrencyType.MaxValue,
                            Math.Round(currency.Value + (currency.CurrencyType.AccrueAmt * (clock.TotalSeconds / currency.CurrencyType.Seconds)), 2)
                        );
                }
            }
        }

        public void UpdateFollowers(IEnumerable<Follow> follows)
        {
            if (follows.Any())
            {
                foreach (Follow f in follows)
                {
                    PostFollower(f);
                }
            }
        }

        public List<LearnMsgRecord> UpdateLearnedMsgs()
        {
            lock (GUIDataManagerLock.Lock)
            {
                using var context = dbContextFactory.CreateDbContext();
                if (LearnMsgChanged)
                {
                    LearnMsgChanged = false;
                    return new(from L in context.LearnMsgs
                               select new LearnMsgRecord(L.Id, L.MsgType.ToString(), L.TeachingMsg));
                }
                else
                {
                    return null;
                }
            }
        }

        public void UpdateOverlayTicker(OverlayTickerItem item, string name)
        {
            lock (GUIDataManagerLock.Lock)
            {
                using var context = dbContextFactory.CreateDbContext();
                OverlayTicker ticker = (from T in context.OverlayTicker where T.TickerName == item select T).FirstOrDefault();
                if (ticker == default)
                {
                    context.OverlayTicker.Add(new(tickerName: item, userName: name));
                }
                else
                {
                    ticker.UserName = name;
                }
                context.SaveChanges();
            }
        }

        public void UpdateWatchTime(List<LiveUser> Users, DateTime CurrTime)
        {
            lock (GUIDataManagerLock.Lock)
            {
                using var context = dbContextFactory.CreateDbContext();
                foreach (UserStats U in context.UserStats.IntersectBy((from L in Users select L), (U) => new(U.UserName, U.Platform, U.UserId)))
                {
                    if (U.Users.LastDateSeen < CurrStreamStart)
                    {
                        U.Users.LastDateSeen = CurrStreamStart;
                    }

                    if (CurrTime > U.Users.LastDateSeen && CurrTime > CurrStreamStart)
                    {
                        U.WatchTime = U.WatchTime.Add(CurrTime - U.Users.LastDateSeen);
                    }
                }
                context.SaveChanges();
            }
        }

        public void UpdateWatchTime(LiveUser User, DateTime CurrTime)
        {
            UpdateWatchTime([User], CurrTime);
        }

        public void UserJoined(LiveUser User, DateTime NowSeen)
        {
            static DateTime Max(DateTime A, DateTime B) => A <= B ? B : A;

            lock (GUIDataManagerLock.Lock)
            {
                using var context = dbContextFactory.CreateDbContext();
                Users user = PostNewUser(User, NowSeen);
                user.CurrLoginDate = Max(user.CurrLoginDate, NowSeen);
                user.LastDateSeen = Max(user.LastDateSeen, NowSeen);
                context.SaveChanges();
            }
        }

        public void UserLeft(LiveUser User, DateTime LastSeen)
        {
            lock (GUIDataManagerLock.Lock)
            {
                using var context = dbContextFactory.CreateDbContext();
                Users user = (from U in context.Users where (U.UserName == User.UserName && U.Platform == User.Source) select U).FirstOrDefault();
                if (user != default)
                {
                    UpdateWatchTime(User, LastSeen);
                    if (OptionFlags.CurrencyStart && (OptionFlags.CurrencyOnline && OptionFlags.IsStreamOnline))
                    {
                        UpdateCurrency(user, LastSeen);
                    }
                    context.SaveChanges(true);
                }
            }
        }


        #region MultiLive data
        public event EventHandler UpdatedMonitoringChannels;
        public List<ArchiveMultiStream> CleanupList { get; } = [];
        private bool IsLiveStreamUpdated = false;
        public string MultiLiveStatusLog { get; set; } = "";

        public bool CheckMultiStreamDate(string ChannelName, Platform platform, DateTime dateTime)
        {
            lock (GUIDataManagerLock.Lock)
            {
                using var context = dbContextFactory.CreateDbContext();
                return (from P in context.MultiLiveStreams where (P.UserName == ChannelName && P.Platform == platform && P.LiveDate == dateTime) select P).Count() > 1;
            }
        }

        public bool GetMultiChannelName(string UserName, Platform platform)
        {
            lock (GUIDataManagerLock.Lock)
            {
                using var context = dbContextFactory.CreateDbContext();
                return (from M in context.MultiChannels where (M.UserName == UserName && M.Platform == platform) select M).Any();
            }
        }
        public List<string> GetMultiChannelNames()
        {
            lock (GUIDataManagerLock.Lock)
            {
                using var context = dbContextFactory.CreateDbContext();
                return new(from M in context.MultiChannels select M.UserName);
            }
        }

        public List<Tuple<WebhooksSource, Uri>> GetMultiWebHooks()
        {
            lock (GUIDataManagerLock.Lock)
            {
                using var context = dbContextFactory.CreateDbContext();
                return new(from W in context.MultiMsgEndPoints where W.IsEnabled select new Tuple<WebhooksSource, Uri>(W.WebhooksSource, W.Webhook));
            }
        }

        public void PostMonitorChannel(string username, string userid, Platform platform)
        {
            lock (GUIDataManagerLock.Lock)
            {
                using var context = dbContextFactory.CreateDbContext();
                context.MultiChannels.Add(new(userId: userid, userName: username, platform: platform));
                context.SaveChanges(true);
                UpdatedMonitoringChannels?.Invoke(this, new());
            }
        }

        public bool PostMultiStreamDate(string userid, string username, Platform platform, DateTime onDate)
        {
            lock (GUIDataManagerLock.Lock)
            {
                using var context = dbContextFactory.CreateDbContext();
                bool result = (from P in context.MultiLiveStreams where (P.UserId == userid && P.UserName == username && P.LiveDate == onDate) select P).Any();
                if (!result)
                {
                    context.MultiLiveStreams.Add(new(userId: userid, userName: username, platform: platform, liveDate: onDate));
                    context.SaveChanges(true);
                }

                return !result;
            }
        }

        public void SummarizeStreamData()
        {
            if (IsLiveStreamUpdated || CleanupList.Count == 0) // only perform if flag for update occurs
            {
                lock (GUIDataManagerLock.Lock)
                {
                    CleanupList.Clear();

                    using var context = dbContextFactory.CreateDbContext();

                    List<DateTime> AllDates = new(from ML in context.MultiLiveStreams select ML.LiveDate);
                    List<DateTime> UniqueDates = new(AllDates.Intersect(AllDates));
                    CleanupList.AddRange(UniqueDates.Select(uniqueDate => new ArchiveMultiStream()
                    {
                        ThroughDate = uniqueDate,
                        StreamCount = (int)(from DateTime dates in AllDates
                                             where dates.Date <= uniqueDate
                                             select dates).Count()
                    }));

                    IsLiveStreamUpdated = false; // reset update flag indicator
                }
            }
        }

        public void SummarizeStreamData(ArchiveMultiStream archiveRecord)
        {
            lock (GUIDataManagerLock.Lock)
            {
                using var context = dbContextFactory.CreateDbContext();

                var multiLiveStreams = (from MLS in context.MultiLiveStreams
                                        where MLS.LiveDate <= archiveRecord.ThroughDate.Date
                                        group MLS by
                                        new
                                        {
                                            name = MLS.UserName,
                                            date = MLS.LiveDate,
                                            platform = MLS.Platform,
                                            userid = MLS.UserId
                                        } into GroupedMLS
                                        select new ArchiveMultiStream()
                                        {
                                            Name = new(GroupedMLS.Key.name, GroupedMLS.Key.platform, GroupedMLS.Key.userid),
                                            ThroughDate = GroupedMLS.MaxBy(G => GroupedMLS.Key.date).LiveDate,
                                            StreamCount = (int)GroupedMLS.Count()
                                        });

                foreach (var GroupedStreams in
                    (from archive in multiLiveStreams
                     join sumlive in context.MultiSummaryLiveStreams on archive.Name.UserName equals sumlive.UserName into GroupedSum
                     from G in GroupedSum.DefaultIfEmpty()
                     select new { username = archive.Name, livestreamrow = archive, sumrow = G }))
                {
                    if (GroupedStreams != default)
                    {
                        GroupedStreams.sumrow.ThroughDate = GroupedStreams.livestreamrow.ThroughDate;
                        GroupedStreams.sumrow.StreamCount += GroupedStreams.livestreamrow.StreamCount;
                    }
                    else
                    {
                        context.MultiSummaryLiveStreams.Add(new(
                            userId: GroupedStreams.sumrow.UserId, userName: GroupedStreams.sumrow.UserName,
                            streamCount: GroupedStreams.livestreamrow.StreamCount, throughDate: GroupedStreams.livestreamrow.ThroughDate));
                    }
                }

                context.MultiLiveStreams.RemoveRange((from MLS in context.MultiLiveStreams
                                                      where MLS.LiveDate <= archiveRecord.ThroughDate.Date
                                                      select MLS));

                IsLiveStreamUpdated = true;

                CleanupList.Clear();
                context.SaveChanges(true);
                SummarizeStreamData();
            }
        }



        #endregion
    }
}
