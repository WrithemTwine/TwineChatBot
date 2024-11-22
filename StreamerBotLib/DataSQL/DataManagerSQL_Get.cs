using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

using StreamerBotLib.DataSQL.DiscriminatorEnums;
using StreamerBotLib.DataSQL.Models;
using StreamerBotLib.Enums;
using StreamerBotLib.GUI;
using StreamerBotLib.Interfaces;
using StreamerBotLib.Models;
using StreamerBotLib.Overlay.Enums;
using StreamerBotLib.Overlay.Models;

using System.Data;

namespace StreamerBotLib.DataSQL
{
    public partial class DataManagerSQL : IDataManager, IDataManagerReadOnly, IDataManagerTestMethods
    {
        #region Get_Methods

        public Tuple<ModActions, Enums.BanReasons, int> FindRemedy(ViewerTypes viewerTypes, MsgTypes msgTypes, SQLDBContext Refcontext = null)
        {
            lock (GUIDataManagerLock.Lock)
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();
                Enums.BanReasons banReasons = (from Br in context.BanReasons
                                               where Br.MsgType == msgTypes
                                               select Br.BanReason).FirstOrDefault();
                BanRules banRules = (from B in context.BanRules
                                     where (B.ViewerTypes == ViewerTypes.Viewer && B.MsgType == msgTypes)
                                     select B).FirstOrDefault();

                if (Refcontext == null) { ClearDataContext(context); }
                return new(banRules?.ModAction ?? ModActions.Allow, banReasons, banRules.TimeoutSeconds);
            }
        }

        public CommandData GetCommand(string cmd, SQLDBContext Refcontext = null)
        {
            lock (GUIDataManagerLock.Lock)
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();

                CommandsBase commands = (from C in context.CommandsBase where C.CmdName == cmd select C).FirstOrDefault();
                //List<CommandsUser> commandsUser = new(from CU in context.CommandsUser where CU.CmdName == cmd select CU);
                if (commands != null)
                {
                    commands.Calls++;
                }

                CommandData result = (commands != null) ? new(commands) : null;// commandsUser != null ? new(commandsUser.First()) : null;
                RefreshCommandsObservableCollection();
                RefreshCommandsUserObservableCollection();
                if (Refcontext == null) { ClearDataContext(context); }
                return result;
            }
        }

        public string GetCommandString(SQLDBContext Refcontext = null)
        {
            lock (GUIDataManagerLock.Lock)
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();
                string result = string.Join(", ", GetCommandList());
                if (Refcontext == null) { ClearDataContext(context); }
                return result;
            }
        }

        public IEnumerable<string> GetCommandList(bool prefix = true, SQLDBContext Refcontext = null)
        {
            lock (GUIDataManagerLock.Lock)
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();
                var result = new List<string>(from Com in (context.CommandsBase)
                              where (Com.Message != DefaulSocialMsg && Com.IsEnabled)
                              orderby Com.CmdName
                              select $"{(prefix ? "!" : "")}{Com.CmdName}");
                if (Refcontext == null) { ClearDataContext(context); }
                return result;
            }
        }

        public List<string> GetCurrencyNames(SQLDBContext Refcontext = null)
        {
            lock (GUIDataManagerLock.Lock)
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();
                List<string> result = new(from C in context.CurrencyType
                                          select C.CurrencyName);
                if (Refcontext == null) { ClearDataContext(context); }
                return result;
            }
        }

        public int GetDeathCounter(string currCategory, SQLDBContext Refcontext = null)
        {
            lock (GUIDataManagerLock.Lock)
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();
                int result = (from D in context.GameDeadCounter
                              where D.Category == currCategory
                              select D.Counter).FirstOrDefault();
                if (Refcontext == null) { ClearDataContext(context); }
                return result;
            }
        }

        public string GetEventRowData(ChannelEventActions rowcriteria, out bool Enabled, out short Multi, SQLDBContext Refcontext = null)
        {
            lock (GUIDataManagerLock.Lock)
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();
                ChannelEvents found = (from Event in context.ChannelEvents
                                       where Event.Name == rowcriteria
                                       select Event).FirstOrDefault();
                Enabled = found?.IsEnabled ?? false;
                Multi = found?.RepeatMsg ?? 0;
                if (Refcontext == null) { ClearDataContext(context); }

                return found?.Message;
            }
        }

        public int GetFollowerCount(SQLDBContext Refcontext = null)
        {
            lock (GUIDataManagerLock.Lock)
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();
                var result = (from F in context.Followers
                              select F).Count();
                if (Refcontext == null) { ClearDataContext(context); }
                return result;
            }
        }

        public List<CategoryData> GetGameCategories(SQLDBContext Refcontext = null)
        {
            lock (GUIDataManagerLock.Lock)
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();
                List<CategoryData> result = new(from G in context.CategoryList
                                                let game = new CategoryData(G.CategoryId, G.Category)
                                                select game);
                if (Refcontext == null) { ClearDataContext(context); }
                return result;
            }
        }

        public string GetKey(string Table, SQLDBContext Refcontext = null)
        {
            lock (GUIDataManagerLock.Lock)
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();
                string result = context.Model.FindEntityType($"StreamerBotLib.DataSQL.Models.{Table}").FindPrimaryKey().GetName();
                if (Refcontext == null) { ClearDataContext(context); }
                return result;
            }
        }

        public IEnumerable<string> GetKeys(string Table, SQLDBContext Refcontext = null)
        {
            lock (GUIDataManagerLock.Lock)
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();
                IEnumerable<string> result = new List<string>(from P in context.Model.FindEntityType($"StreamerBotLib.DataSQL.Models.{Table}").FindPrimaryKey().Properties select P.Name);
                if (Refcontext == null) { ClearDataContext(context); }
                return result;
            }
        }

        public List<OverlayActionType> GetOverlayActions(OverlayTypes overlayType, string overlayAction, string username, SQLDBContext Refcontext = null)
        {
            lock (GUIDataManagerLock.Lock)
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();
                List<OverlayActionType> result = new(from O in context.OverlayServices
                                                     where (O.IsEnabled
                                                     && O.OverlayType == overlayType
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
                if (Refcontext == null) { ClearDataContext(context); }
                return result;
            }
        }

        public string GetQuote(int QuoteNum, SQLDBContext Refcontext = null)
        {
            lock (GUIDataManagerLock.Lock)
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();
                var result = (from Q in context.Quotes
                              where Q.Number == QuoteNum
                              select $"{Q.Number}: {Q.Quote}").FirstOrDefault();
                if (Refcontext == null) { ClearDataContext(context); }
                return result;
            }
        }

        public int GetQuoteCount(SQLDBContext Refcontext = null)
        {
            lock (GUIDataManagerLock.Lock)
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();
                int result = context.Quotes.MaxBy((q) => q.Number)?.Number ?? 0;
                if (Refcontext == null) { ClearDataContext(context); }
                return result;
            }
        }

        public Dictionary<string, List<string>> GetOverlayActions(SQLDBContext Refcontext = null)
        {
            lock (GUIDataManagerLock.Lock)
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();
                Dictionary<string, List<string>> result = new()
                {
                    { nameof(Commands), new(from C in context.Commands select C.CmdName) },
                    { nameof(ChannelEvents), new(from E in context.ChannelEvents select E.Name.ToString()) }
                };
                if (Refcontext == null) { ClearDataContext(context); }
                return result;
            }
        }

        public List<string> GetSocialComs(SQLDBContext Refcontext = null)
        {
            lock (GUIDataManagerLock.Lock)
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();
                List<string> result = new(from SC in context.Commands.IntersectBy((string[])Enum.GetValues(typeof(DefaultSocials)), (c) => c.CmdName)
                                          select SC.CmdName);
                if (Refcontext == null) { ClearDataContext(context); }
                return result;
            }
        }

        public string GetSocials(SQLDBContext Refcontext = null)
        {
            lock (GUIDataManagerLock.Lock)
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();
                var result = string.Join(" ", (from SC in
                                             context.Commands.IntersectBy((string[])Enum.GetValues(typeof(DefaultSocials)),
                                                (c) => c.CmdName)
                                               where SC.Message != DefaulSocialMsg
                                               select SC));
                if (Refcontext == null) { ClearDataContext(context); }
                return result;
            }
        }

        public StreamStat GetStreamData(DateTime dateTime, SQLDBContext Refcontext = null)
        {
            lock (GUIDataManagerLock.Lock)
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();

                var result = (from SD in context.StreamStats
                              where SD.StreamStart == dateTime
                              select StreamStat.Create(SD)).FirstOrDefault();
                if (Refcontext == null) { ClearDataContext(context); }
                return result;
            }
        }

        public List<string> GetTableFields(string TableName, SQLDBContext Refcontext = null)
        {
            lock (GUIDataManagerLock.Lock)
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();
                List<string> result = new(from T in context.Model.FindEntityType($"StreamerBotLib.DataSQL.Models.{TableName}").GetMembers()
                                          select T.Name);
                if (Refcontext == null) { ClearDataContext(context); }
                return result;
            }
        }

        public List<string> GetTableNames(SQLDBContext Refcontext = null)
        {
            lock (GUIDataManagerLock.Lock)
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();
                var list = new List<string>()
                {
                    nameof(Currency),
                    nameof(UserStats),
                    nameof(Commands),
                    nameof(CustomWelcome),
                    nameof(Followers)
                };
                list.Sort();
                if (Refcontext == null) { ClearDataContext(context); }
                return list;
            }
        }

        public List<TickerItem> GetTickerItems(SQLDBContext Refcontext = null)
        {
            lock (GUIDataManagerLock.Lock)
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();
                List<TickerItem> result = new(from F in context.OverlayTicker
                                              select new TickerItem(F.TickerName, F.UserName));
                if (Refcontext == null) { ClearDataContext(context); }
                return result;
            }
        }

        public Tuple<string, int, List<string>> GetTimerCommand(string Cmd, SQLDBContext Refcontext = null)
        {
            lock (GUIDataManagerLock.Lock)
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();
                Tuple<string, int, List<string>> result = (from R in context.Commands
                                                           where R.RepeatTimer > 0
                                                           select new Tuple<string, int, List<string>>(R.CmdName, R.RepeatTimer, new(R.Category))).FirstOrDefault();
                if (Refcontext == null) { ClearDataContext(context); }
                return result;
            }
        }

        public List<Tuple<string, int, List<string>>> GetTimerCommands(SQLDBContext Refcontext = null)
        {
            lock (GUIDataManagerLock.Lock)
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();
                List<Tuple<string, int, List<string>>> result = new(from R in context.Commands
                                                                    where R.RepeatTimer > 0
                                                                    select new Tuple<string, int, List<string>>(R.CmdName, R.RepeatTimer, new(R.Category)));
                if (Refcontext == null) { ClearDataContext(context); }
                return result;
            }
        }

        public int GetTimerCommandTime(string Cmd, SQLDBContext Refcontext = null)
        {
            lock (GUIDataManagerLock.Lock)
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();
                int result = (from R in context.Commands
                              where R.CmdName == Cmd
                              select R.RepeatTimer).FirstOrDefault();
                if (Refcontext == null) { ClearDataContext(context); }
                return result;
            }
        }

        public string GetUsage(string command, SQLDBContext Refcontext = null)
        {
            lock (GUIDataManagerLock.Lock)
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();
                string result = (from C in context.Commands
                                 where C.CmdName == command
                                 select C.Usage).FirstOrDefault();
                if (Refcontext == null) { ClearDataContext(context); }
                return result;
            }
        }

        public List<Tuple<bool, Uri>> GetWebhooks(WebhooksSource webhooksSource, WebhooksKind webhooks, SQLDBContext Refcontext = null)
        {
            lock (GUIDataManagerLock.Lock)
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();
                List<Tuple<bool, Uri>> result = new(from W in context.Webhooks
                                                    where (W.WebhooksSource == webhooksSource
                                                    && W.Kind == webhooks && W.DataSource == WebhookDataSource.Channel
                                                    && W.IsEnabled == true)
                                                    select new Tuple<bool, Uri>(W.AddEveryone, W.Webhook));
                if (Refcontext == null) { ClearDataContext(context); }
                return result;
            }
        }

        #endregion Get_Methods

    }
}
