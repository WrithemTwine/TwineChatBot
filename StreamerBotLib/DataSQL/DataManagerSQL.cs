using Microsoft.EntityFrameworkCore;

using StreamerBotLib.Data;
using StreamerBotLib.DataSQL.Models;
using StreamerBotLib.Enums;
using StreamerBotLib.GUI;
using StreamerBotLib.Interfaces;
using StreamerBotLib.Models;
using StreamerBotLib.Overlay.Enums;
using StreamerBotLib.Overlay.Models;
using StreamerBotLib.Static;
using StreamerBotLib.Systems;

using System.Data;
using System.Globalization;


namespace StreamerBotLib.DataSQL
{
    //public class DataManagerSQL(IDbContextFactory<SQLDBContext> dbContextFactory) //: IDataManager, IDataManageReadOnly
    //{
    //    /// <summary>
    //    /// always true to begin one learning cycle
    //    /// </summary>
    //    private bool LearnMsgChanged = true;

    //    /// <summary>
    //    /// When the follower bot begins a bulk follower update, this flag 'locks' the database Follower table from changes until bulk update concludes.
    //    /// </summary>
    //    public bool UpdatingFollowers { get; set; }
    //    private readonly string DefaulSocialMsg = LocalizedMsgSystem.GetVar(Msg.MsgDefaultSocialMsg);


    //    private readonly IDbContextFactory<SQLDBContext> dbContextFactory = dbContextFactory;

    //    public bool CheckCurrency(LiveUser User, double value, string CurrencyName)
    //    {
    //        lock (GUIDataManagerLock.Lock)
    //        {
    //            using var context = dbContextFactory.CreateDbContext();
    //            return (from C in context.Currency
    //            where (C.UserName==User.UserName && C.CurrencyName==CurrencyName)
    //            select C.Value).FirstOrDefault() >= value;
    //        }
    //    }

    //    public bool CheckField(string table, string field)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public bool CheckFollower(string User)
    //    {
    //        lock (GUIDataManagerLock.Lock)
    //        {
    //            return CheckFollower(User, default);
    //        }
    //    }

    //    public bool CheckFollower(string User, DateTime ToDateTime)
    //    {
    //        lock (GUIDataManagerLock.Lock)
    //        {
    //            using var context = dbContextFactory.CreateDbContext();
    //            return (from f in context.Followers
    //                    where (f.IsFollower && f.UserName == User && (ToDateTime == default || f.FollowedDate < ToDateTime))
    //                    select f).Any();
    //        }
    //    }

    //    public Tuple<string, string> CheckModApprovalRule(ModActionType modActionType, string ModAction)
    //    {
    //        lock (GUIDataManagerLock.Lock)
    //        {
    //            using var context = dbContextFactory.CreateDbContext();
    //            return (from M in context.ModeratorApprove
    //                    where (M.ModActionType==modActionType && M.ModActionName==ModAction)
    //                    select new Tuple<string,string>(
    //                        !string.IsNullOrEmpty(M.ModPerformType.ToString()) ? M.ModPerformType.ToString() : M.ModActionType.ToString(),
    //                        !string.IsNullOrEmpty(M.ModPerformAction) ? M.ModPerformAction : M.ModActionName
    //                        )).FirstOrDefault();
    //        }
    //    }

    //    /// <summary>
    //    /// Determines if there are multiple streams based on the same start date.
    //    /// </summary>
    //    /// <param name="streamStart">The stream start date and time to check.</param>
    //    /// <returns><code>true</code> if there are multiple streams
    //    /// <code>false</code> if there is no more than one stream.</returns>
    //    public bool CheckMultiStreams(DateTime streamStart)
    //    {
    //        lock (GUIDataManagerLock.Lock)
    //        {
    //            using var context = dbContextFactory.CreateDbContext();
    //            return (from s in context.StreamStats
    //                    where (s.StreamStart == streamStart)
    //                    select s).Count() > 1;
    //        }
    //    }

    //    /// <summary>
    //    /// Determine if the user invoking the command has permission to access the command.
    //    /// </summary>
    //    /// <param name="cmd">The command to verify the permission.</param>
    //    /// <param name="permission">The supplied permission to check.</param>
    //    /// <returns><code>true</code> - the permission is allowed to the command. <code>false</code> - the command permission is not allowed.</returns>
    //    public bool CheckPermission(string cmd, ViewerTypes permission)
    //    {
    //        lock (GUIDataManagerLock.Lock)
    //        {
    //            using var context = dbContextFactory.CreateDbContext();
    //            return (from c in context.Commands
    //                    where c.CmdName == cmd
    //                    select c).FirstOrDefault().Permission > permission;
    //        }
    //    }

    //    /// <summary>
    //    /// Verify if the provided UserName is within the ShoutOut table.
    //    /// </summary>
    //    /// <param name="UserName">The UserName to shoutout.</param>
    //    /// <returns>true if in the ShoutOut table.</returns>
    //    public bool CheckShoutName(string UserName)
    //    {
    //        lock (GUIDataManagerLock.Lock)
    //        {
    //            using var context = dbContextFactory.CreateDbContext();
    //            return (from s in context.ShoutOuts
    //                    where s.UserName == UserName
    //                    select s).Any();
    //        }
    //    }

    //    /// <summary>
    //    /// Find if stream data already exists for the current stream
    //    /// </summary>
    //    /// <param name="CurrTime">The time to check</param>
    //    /// <returns><code>true</code>: the stream already has a data entry; <code>false</code>: the stream has no data entry</returns>
    //    public bool CheckStreamTime(DateTime CurrTime)
    //    {
    //        lock (GUIDataManagerLock.Lock)
    //        {
    //            using var context = dbContextFactory.CreateDbContext();
    //            return (from s in context.StreamStats
    //                    where s.StreamStart == CurrTime
    //                    select s).Any();
    //        }
    //    }

    //    /// <summary>
    //    /// Check to see if the <paramref name="User"/> has been in the channel prior to DateTime.MaxValue.
    //    /// </summary>
    //    /// <param name="User">The user to check in the database.</param>
    //    /// <returns><code>true</code> if the <paramref name="User"/> has arrived anytime, <code>false</code> otherwise.</returns>
    //    public bool CheckUser(LiveUser User)
    //    {
    //        lock (GUIDataManagerLock.Lock)
    //        {
    //            return CheckUser(User, default);
    //        }
    //    }

    //    /// <summary>
    //    /// Check if the <paramref name="User"/> has visited the channel prior to <paramref name="ToDateTime"/>, identified as either DateTime.Now.ToLocalTime() or the current start of the stream.
    //    /// </summary>
    //    /// <param name="User">The user to verify.</param>
    //    /// <param name="ToDateTime">Specify the date to check if the user arrived to the channel prior to this date and time.</param>
    //    /// <returns><c>True</c> if the <paramref name="User"/> has been in channel before <paramref name="ToDateTime"/>, <c>false</c> otherwise.</returns>
    //    public bool CheckUser(LiveUser User, DateTime ToDateTime)
    //    {
    //        lock (GUIDataManagerLock.Lock)
    //        {
    //            using var context = dbContextFactory.CreateDbContext();
    //            return (from s in context.Users
    //                    where (ToDateTime == default || s.FirstDateSeen < ToDateTime) && s.UserName == User.UserName && s.Platform == User.Source
    //                    select s).Any();
    //        }
    //    }

    //    /// <summary>
    //    /// Check the CustomWelcome table for the user and provide the message.
    //    /// </summary>
    //    /// <param name="User">The user to check for a welcome message.</param>
    //    /// <returns>The welcome message if user is available, or empty string if not found.</returns>
    //    public string CheckWelcomeUser(string User)
    //    {
    //        lock (GUIDataManagerLock.Lock)
    //        {
    //            using var context = dbContextFactory.CreateDbContext();
    //            return (from s in context.CustomWelcome
    //                    where s.UserName == User
    //                    select s.Message).FirstOrDefault() ?? "";
    //        }
    //    }

    //    public void ClearAllCurrencyValues()
    //    {
    //        lock (GUIDataManagerLock.Lock)
    //        {
    //            using var context = dbContextFactory.CreateDbContext();
    //        foreach (Currency c in from u in context.Currency
    //                               select u)
    //        {
    //            c.Value = 0;
    //        }
    //        context.SaveChanges();
    //    } }

    //    /// <summary>
    //    /// Clear all User rows for users not included in the Followers table.
    //    /// </summary>
    //    public void ClearUsersNotFollowers()
    //    {
    //        lock (GUIDataManagerLock.Lock)
    //        {
    //            using var context = dbContextFactory.CreateDbContext();

    //            context.Users.RemoveRange((IEnumerable<Users>)(from user in context.Users
    //                                                           join follower in context.Followers on user.UserId equals follower.UserId into UserFollow
    //                                                           from subuser in UserFollow.DefaultIfEmpty()
    //                                                           where !subuser.IsFollower
    //                                                           select subuser));
    //            context.SaveChanges();
    //        }
    //    }

    //    /// <summary>
    //    /// Clear all user watchtimes
    //    /// </summary>
    //    public void ClearWatchTime()
    //    {
    //        lock (GUIDataManagerLock.Lock)
    //        {
    //            using var context = dbContextFactory.CreateDbContext();
    //            foreach(var userstat in from US in context.UserStats
    //                                    select US)
    //            {
    //                userstat.WatchTime = new(0);
    //            }
    //            context.SaveChanges();
    //        }
    //    }

    //    public void DeleteDataRows(IEnumerable<DataRow> dataRows)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public string EditCommand(string cmd, List<string> Arglist)
    //    {
    //        string result = "";

    //        lock (GUIDataManagerLock.Lock)
    //        {
    //            using var context = dbContextFactory.CreateDbContext();
    //            Dictionary<string, string> EditParamsDict = CommandParams.ParseEditCommandParams(Arglist);
    //            Commands EditCom = (from C in context.Commands
    //                                where C.CmdName == cmd
    //                                select C).FirstOrDefault();

    //            if (EditCom != default)
    //            {
    //                foreach (string k in EditParamsDict.Keys)
    //                {
    //                    EditCom.GetType().GetProperty(k).SetValue(EditCom, EditParamsDict[k]);
    //                }
    //                result = string.Format(CultureInfo.CurrentCulture, LocalizedMsgSystem.GetDefaultComMsg(DefaultCommand.editcommand), cmd);
    //                context.SaveChanges();
    //            }
    //            else
    //            {
    //                result = string.Format(CultureInfo.CurrentCulture, LocalizedMsgSystem.GetVar("Msgcommandnotfound"), cmd);

    //            }
    //        }
    //        return result;
    //    }

    //    public Tuple<ModActions, Enums.BanReasons, int> FindRemedy(ViewerTypes viewerTypes, MsgTypes msgTypes)
    //    {
    //        lock (GUIDataManagerLock.Lock)
    //        {
    //            using var context = dbContextFactory.CreateDbContext();
    //            Enums.BanReasons banReasons = (from Br in context.BanReasons
    //                                            where Br.MsgType==msgTypes
    //                                            select Br.BanReason).FirstOrDefault();
    //            BanRules banRules = (from B in context.BanRules
    //                                 where (B.ViewerTypes == ViewerTypes.Viewer && B.MsgTypes == msgTypes)
    //                                 select B).FirstOrDefault();

    //            return new(banRules?.ModAction ?? ModActions.Allow, banReasons, banRules.TimeoutSeconds);
    //        }
    //    }

    //    public CommandData GetCommand(string cmd)
    //    {
    //        lock (GUIDataManagerLock.Lock)
    //        {
    //            using var context = dbContextFactory.CreateDbContext();
    //            return new((from Com in context.Commands
    //                        where Com.CmdName == cmd
    //                        select Com).FirstOrDefault());
    //        }
    //    }

    //    public IEnumerable<string> GetCommandList()
    //    {
    //        return GetCommands().Split(", ");
    //    }

    //    public string GetCommands()
    //    {
    //        lock (GUIDataManagerLock.Lock)
    //        {
    //            using var context = dbContextFactory.CreateDbContext();
    //            return string.Join(", ",(from Com in context.Commands
    //                        where (Com.Message == DefaulSocialMsg && Com.IsEnabled)
    //                        orderby Com.CmdName
    //                        select $"!{Com.CmdName}"));
    //        }
    //    }

    //    public IEnumerable<string> GetCurrencyNames()
    //    {
    //        lock (GUIDataManagerLock.Lock)
    //        {
    //            using var context = dbContextFactory.CreateDbContext();
    //            return (from C in context.CurrencyType
    //                    select C.CurrencyName);
    //        }
    //    }

    //    public uint GetDeathCounter(string currCategory)
    //    {
    //        lock (GUIDataManagerLock.Lock)
    //        {
    //            using var context = dbContextFactory.CreateDbContext();
    //            return (from D in context.GameDeadCounter
    //                    where D.Category==currCategory
    //                    select D.Counter).FirstOrDefault();
    //        }
    //    }

    //    public string GetEventRowData(ChannelEventActions rowcriteria, out bool Enabled, out ushort Multi)
    //    {
    //        lock (GUIDataManagerLock.Lock)
    //        {
    //            using var context = dbContextFactory.CreateDbContext();
    //            ChannelEvents found = (from Event in context.ChannelEvents
    //                        where Event.Name == rowcriteria.ToString()
    //                        select Event).FirstOrDefault();
    //            Enabled = found.IsEnabled;
    //            Multi = found.RepeatMsg;
    //            return found.Message;
    //        }

    //    }

    //    public int GetFollowerCount()
    //    {
    //        lock (GUIDataManagerLock.Lock)
    //        {
    //            using var context = dbContextFactory.CreateDbContext();
    //            return (from F in context.Followers
    //                        select F).Count();
    //        }
    //    }

    //    public IEnumerable<Tuple<string, string>> GetGameCategories()
    //    {
    //        lock (GUIDataManagerLock.Lock)
    //        {
    //            using var context = dbContextFactory.CreateDbContext();
    //            return (from G in context.CategoryList
    //                    let game = new Tuple<string, string>(G.CategoryId, G.Category)
    //                        select game);
    //        }
    //    }

    //    public string GetKey(string Table)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public IEnumerable<string> GetKeys(string Table)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public string GetNewestFollower()
    //    {
    //        lock (GUIDataManagerLock.Lock)
    //        {
    //            using var context = dbContextFactory.CreateDbContext();
    //            return context.Followers.MaxBy((i)=>i.FollowedDate).UserName;
    //        }
    //    }

    //    public IEnumerable<OverlayActionType> GetOverlayActions(string overlayType, string overlayAction, string username)
    //    {
    //        lock (GUIDataManagerLock.Lock)
    //        {
    //            using var context = dbContextFactory.CreateDbContext();
    //            return (from O in context.OverlayServices
    //                        where (O.IsEnabled 
    //                        && O.OverlayType.ToString() == overlayType 
    //                        && (string.IsNullOrEmpty(O.UserName) || O.UserName==username) 
    //                        && O.OverlayAction == overlayAction)
    //                       select new OverlayActionType()
    //                        {
    //                            ActionValue = O.OverlayAction,
    //                            Duration = O.Duration,
    //                            MediaFile = O.MediaFile,
    //                            ImageFile = O.ImageFile,
    //                            Message = O.Message,
    //                            OverlayType = O.OverlayType,
    //                            UserName = O.UserName,
    //                            UseChatMsg = O.UseChatMsg
    //                        });
    //        }
    //    }

    //    public string GetQuote(int QuoteNum)
    //    {
    //        lock (GUIDataManagerLock.Lock)
    //        {
    //            using var context = dbContextFactory.CreateDbContext();
    //            return (from Q in context.Quotes
    //                    where Q.Number == QuoteNum
    //                    select $"{Q.Number}: {Q.Quote}").FirstOrDefault();
    //        }
    //    }

    //    public int GetQuoteCount()
    //    {
    //        lock (GUIDataManagerLock.Lock)
    //        {
    //            using var context = dbContextFactory.CreateDbContext();
    //            return context.Quotes.MaxBy((q)=>q.Number)?.Number ?? 0;
    //        }

    //    }

    //   public Dictionary<string, List<string>> GetOverlayActions()
    //    {
    //        lock (GUIDataManagerLock.Lock)
    //        {
    //            using var context = dbContextFactory.CreateDbContext();
    //            return new()
    //            { 
    //                { nameof(Commands), new(from C in context.Commands select C.CmdName) },
    //                { nameof(ChannelEvents), new(from E in context.ChannelEvents select E.Name) } 
    //            };
    //        }
    //    }

    //    public IEnumerable<string> GetSocialComs()
    //    {
    //        lock (GUIDataManagerLock.Lock)
    //        {
    //            using var context = dbContextFactory.CreateDbContext();
    //            return from SC in context.Commands.IntersectBy((string[])Enum.GetValues(typeof(DefaultSocials)), (c) => c.CmdName)
    //                                     select SC.CmdName;
    //        }
    //    }

    //    public string GetSocials()
    //    {
    //        lock (GUIDataManagerLock.Lock)
    //        {
    //            using var context = dbContextFactory.CreateDbContext();
    //            return string.Join(" ", (from SC in 
    //                                         context.Commands.IntersectBy((string[])Enum.GetValues(typeof(DefaultSocials)),
    //                                            (c) => c.CmdName)
    //                                     select SC));
    //        }
    //    }

    //    public StreamStat GetStreamData(DateTime dateTime)
    //    {
    //        lock (GUIDataManagerLock.Lock)
    //        {
    //            using var context = dbContextFactory.CreateDbContext();
    //            return (from SD in context.StreamStats
    //                    where SD.StreamStart == dateTime
    //                    select new StreamStat(SD)).FirstOrDefault();
    //        }
    //    }

    //    public IEnumerable<string> GetTableFields(string TableName)
    //    {
    //        lock (GUIDataManagerLock.Lock)
    //        {
    //            using var context = dbContextFactory.CreateDbContext();
    //            return (from T in context.Model.FindEntityType(TableName).GetMembers()
    //                    select T.Name);
    //        }
    //    }

    //    public IEnumerable<string> GetTableNames()
    //    {
    //        lock (GUIDataManagerLock.Lock)
    //        {
    //            using var context = dbContextFactory.CreateDbContext();
    //            return (from N in context.Model.GetEntityTypes()
    //                    select N.Name);
    //        }
    //    }

    //    public IEnumerable<TickerItem> GetTickerItems()
    //    {
    //        lock (GUIDataManagerLock.Lock)
    //        {
    //            using var context = dbContextFactory.CreateDbContext();
    //            return from F in context.OverlayTicker
    //                    select new TickerItem(F.TickerName, F.UserName);
    //        }
    //    }

    //    public Tuple<string, uint, string[]> GetTimerCommand(string Cmd)
    //    {
    //        lock (GUIDataManagerLock.Lock)
    //        {
    //            using var context = dbContextFactory.CreateDbContext();
    //            return (from R in context.Commands
    //                    where R.RepeatTimer > 0
    //                    select new Tuple<string,uint,string[]>(R.CmdName,R.RepeatTimer, (from C in R.Category
    //                                                                                    select C.Category).ToArray())).FirstOrDefault();
    //        }
    //    }

    //    public IEnumerable<Tuple<string, uint, string[]>> GetTimerCommands()
    //    {
    //        lock (GUIDataManagerLock.Lock)
    //        {
    //            using var context = dbContextFactory.CreateDbContext();
    //            return (from R in context.Commands
    //                    where R.RepeatTimer > 0
    //                    select new Tuple<string, uint, string[]>(R.CmdName, R.RepeatTimer, (from C in R.Category
    //                                                                                        select C.Category).ToArray()));
    //        }
    //    }

    //    public uint GetTimerCommandTime(string Cmd)
    //    {
    //        lock (GUIDataManagerLock.Lock)
    //        {
    //            using var context = dbContextFactory.CreateDbContext();
    //            return (from R in context.Commands
    //                    where R.CmdName == Cmd
    //                    select R.RepeatTimer).FirstOrDefault();
    //        }
    //    }

    //    public string GetUsage(string command)
    //    {
    //        lock (GUIDataManagerLock.Lock)
    //        {
    //            using var context = dbContextFactory.CreateDbContext();
    //            return (from C in context.Commands
    //                    where C.CmdName == command
    //                    select C.Usage).FirstOrDefault();
    //        }
    //    }

    //    public string GetUserId(LiveUser User)
    //    {
    //        lock (GUIDataManagerLock.Lock)
    //        {
    //            using var context = dbContextFactory.CreateDbContext();
    //            return (from U in context.Users
    //                    where U.UserName == User.UserName
    //                    select U.UserId).FirstOrDefault();
    //        }
    //    }

    //    public IEnumerable<Tuple<bool, Uri>> GetWebhooks(WebhooksKind webhooks)
    //    {
    //        lock (GUIDataManagerLock.Lock)
    //        {
    //            using var context = dbContextFactory.CreateDbContext();
    //            return (from W in context.Discord
    //                    where W.Kind == webhooks
    //                    select new Tuple<bool, Uri>(W.AddEveryone, W.Webhook));
    //        }
    //    }

    //    public void Initialize()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public void NotifySaveData()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public object[] PerformQuery(CommandData row, int Top = 0)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public object PerformQuery(CommandData row, string ParamValue)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public bool PostCategory(string CategoryId, string newCategory)
    //    {
    //        bool found = false;
    //        if (string.IsNullOrEmpty(CategoryId) && string.IsNullOrEmpty(newCategory))
    //        {
    //            found = false;
    //        }
    //        else
    //        {
    //            lock (GUIDataManagerLock.Lock)
    //            {
    //                using var context = dbContextFactory.CreateDbContext();
    //                CategoryList categoryList = (from CL in context.CategoryList
    //                                             where (CL.Category == FormatData.AddEscapeFormat(newCategory)) || CL.CategoryId == CategoryId
    //                                             select CL).FirstOrDefault();
    //                if (categoryList == default)
    //                {
    //                    context.CategoryList.Add(new(categoryId: CategoryId, category: newCategory));
    //                    found = true;
    //                }
    //                else
    //                {
    //                    categoryList.CategoryId ??= CategoryId;
    //                    categoryList.Category ??= newCategory;
    //                    if (OptionFlags.IsStreamOnline)
    //                    {
    //                        categoryList.StreamCount++;
    //                    }
    //                    found = true;
    //                }
    //                context.SaveChanges();
    //            }
    //        }
    //        return found;
    //    }

    //    public bool PostClip(string ClipId, string CreatedAt, float Duration, string GameId, string Language, string Title, string Url)
    //    {
    //        lock (GUIDataManagerLock.Lock)
    //        {
    //            using var context = dbContextFactory.CreateDbContext();

    //        }
    //    }

    //    public string PostCommand(string cmd, CommandParams Params)
    //    {
    //        lock (GUIDataManagerLock.Lock)
    //        {
    //            using var context = dbContextFactory.CreateDbContext();

    //        }
    //    }

    //        public void PostCurrencyRows()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public void PostCurrencyRows(ref DataSource.UsersRow usersRow)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public void PostCurrencyUpdate(LiveUser User, double value, string CurrencyName)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public int PostDeathCounterUpdate(string currCategory, bool Reset = false, int updateValue = 1)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public bool PostFollower(LiveUser User, DateTime FollowedDate, string Category, Platform platform)
    //    {
    //        lock (GUIDataManagerLock.Lock)
    //        {
    //            using var context = dbContextFactory.CreateDbContext();
    //            IEnumerable<Followers> follow = (from F in context.Followers
    //                                             where F.UserId == User.UserId && F.UserName == User.UserName && F.Platform == platform
    //                                             select F);
    //            bool found = follow.Any();

    //            if (!found)
    //            {
    //                context.Followers.Add(new(userId: User.UserId, userName: User.UserName, platform: platform, isFollower: true, followedDate: FollowedDate, category: Category));
    //            }
    //            else
    //            {

    //            }

    //            return found;
    //        }
    //    }

    //    public void PostGiveawayData(string DisplayName, DateTime dateTime)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public void PostInRaidData(string user, DateTime time, uint viewers, string gamename, Platform platform)
    //    {
    //        lock (GUIDataManagerLock.Lock)
    //        {
    //            using var context = dbContextFactory.CreateDbContext();
    //            context.InRaidData.Add(new(userName: user, raidDate: time, viewerCount: viewers, category: gamename, platform: platform));
    //            context.SaveChanges();
    //        }
    //    }

    //    public void PostLearnMsgsRow(string Message, MsgTypes MsgType)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public bool PostMergeUserStats(string CurrUser, string SourceUser, Platform platform)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public void PostNewAutoShoutUser(string UserName, string UserId, string platform)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public void PostOutgoingRaid(string HostedChannel, DateTime dateTime)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public int PostQuote(string Text)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public bool PostStream(DateTime StreamStart)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public void PostStreamStat(StreamStat streamStat)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public void PostUpdatedDataRow(bool RowChanged)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public void PostUserCustomWelcome(string User, string WelcomeMsg)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public void RemoveAllFollowers()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public void RemoveAllGiveawayData()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public void RemoveAllInRaidData()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public void RemoveAllOutRaidData()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public void RemoveAllOverlayTickerData()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public void RemoveAllStreamStats()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public void RemoveAllUsers()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public bool RemoveCommand(string command)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public bool RemoveQuote(int QuoteNum)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public void SaveData(object sender, EventArgs e)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public void SetBuiltInCommandsEnabled(bool Enabled)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public void SetDiscordWebhooksEnabled(bool Enabled)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public void SetIsEnabled(IEnumerable<DataRow> dataRows, bool IsEnabled = false)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public void SetSystemEventsEnabled(bool Enabled)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public void SetUserDefinedCommandsEnabled(bool Enabled)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public void StartBulkFollowers()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public void StopBulkFollows()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public bool TestInRaidData(string user, DateTime time, string viewers, string gamename)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public bool TestOutRaidData(string HostedChannel, DateTime dateTime)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public void UpdateCurrency(List<string> Users, DateTime dateTime)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public void UpdateFollowers(IEnumerable<Follow> follows, string Category)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public List<LearnMsgRecord> UpdateLearnedMsgs()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public void UpdateOverlayTicker(OverlayTickerItem item, string name)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public void UpdateWatchTime(List<string> Users, DateTime CurrTime)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public void UpdateWatchTime(string User, DateTime CurrTime)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public void UserJoined(LiveUser User, DateTime NowSeen)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public void UserLeft(LiveUser User, DateTime LastSeen)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}
}
