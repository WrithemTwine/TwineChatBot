using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

using StreamerBotLib.DataSQL.DiscriminatorEnums;
using StreamerBotLib.DataSQL.Models;
using StreamerBotLib.Enums;
using StreamerBotLib.Models;
using StreamerBotLib.Overlay.Enums;
using StreamerBotLib.Overlay.Models;

using System.Data;

namespace StreamerBotLib.DataSQL
{
    internal partial class DataManagerSQLAsync
    {
        #region Get_Methods

        internal async Task<Tuple<ModActions, Enums.BanReasons, int>> FindRemedy(ViewerTypes viewerTypes, MsgTypes msgTypes, SQLDBContext Refcontext = null)
        {
            return await Task.Run(() =>
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();
                Enums.BanReasons banReasons = (from Br in context.BanReasons
                                               where Br.MsgType == msgTypes
                                               select Br.BanReason).FirstOrDefault();
                BanRules banRules = (from B in context.BanRules
                                     where (B.ViewerTypes == ViewerTypes.Viewer && B.MsgType == msgTypes)
                                     select B).FirstOrDefault();

                if (Refcontext == null) { ClearDataContext(context); }
                return new Tuple<ModActions, Enums.BanReasons, int>(banRules?.ModAction ?? ModActions.Allow, banReasons, banRules.TimeoutSeconds);
            });
        }

        internal Task<CommandData> GetCommand(string cmd, SQLDBContext Refcontext = null)
        {
            return Task.Run(async () =>
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();

                CommandsBase commands = (from C in context.CommandsBase where C.CmdName == cmd select C).FirstOrDefault();

                if (commands != null)
                {
                    commands.Calls++;
                }

                CommandData result = (commands != null) ? new(commands) : null;// commandsUser != null ? new(commandsUser.First()) : null;
                await context.SaveChangesAsync();
                RefreshCommandsObservableCollection();
                RefreshCommandsUserObservableCollection();
                if (Refcontext == null) { ClearDataContext(context); }
                return result;
            });
        }

        internal Task<string> GetCommandString(SQLDBContext Refcontext = null)
        {
            return Task.Run(() =>
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();
                string result = string.Join(", ", GetCommandList());
                if (Refcontext == null) { ClearDataContext(context); }
                return result;
            });
        }

        internal Task<List<string>> GetCommandList(bool prefix = true, SQLDBContext Refcontext = null)
        {
            return Task.Run(() =>
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();
                var result = new List<string>(from Com in (context.CommandsBase)
                                              where (Com.Message != DefaulSocialMsg && Com.IsEnabled)
                                              orderby Com.CmdName
                                              select $"{(prefix ? "!" : "")}{Com.CmdName}");
                if (Refcontext == null) { ClearDataContext(context); }
                return result;
            });
        }

        internal Task<List<string>> GetCurrencyNames(SQLDBContext Refcontext = null)
        {
            return Task.Run(() =>
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();
                List<string> result = (from C in context.CurrencyType
                                       select C.CurrencyName).ToList();
                if (Refcontext == null) { ClearDataContext(context); }
                return result;
            });
        }

        internal Task<int> GetDeathCounter(string currCategory, SQLDBContext Refcontext = null)
        {
            return Task.Run(() =>
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();
                int result = (from D in context.GameDeadCounter
                              where D.Category == currCategory
                              select D.Counter).FirstOrDefault();
                if (Refcontext == null) { ClearDataContext(context); }
                return result;
            });
        }

        internal Task<Tuple<string, bool, short>> GetEventRowData(ChannelEventActions rowcriteria, SQLDBContext Refcontext = null)
        {
            return Task.Run(() =>
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();
                ChannelEvents found = (from Event in context.ChannelEvents
                                       where Event.Name == rowcriteria
                                       select Event).FirstOrDefault();
                var output = new Tuple<string, bool, short>(
                    found?.Message,
                    found?.IsEnabled ?? false,
                    found?.RepeatMsg ?? 0);

                if (Refcontext == null) { ClearDataContext(context); }

                return output;
            });
        }

        internal Task<int> GetFollowerCount(SQLDBContext Refcontext = null)
        {
            return Task.Run(() =>
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();
                var result = context.Followers.Count();
                if (Refcontext == null) { ClearDataContext(context); }
                return result;
            });
        }

        internal Task<List<CategoryData>> GetGameCategories(SQLDBContext Refcontext = null)
        {
            return Task.Run(() =>
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();
                List<CategoryData> result = new(from G in context.CategoryList
                                                let game = new CategoryData(G.CategoryId, G.Category)
                                                select game);
                if (Refcontext == null) { ClearDataContext(context); }
                return result;
            });
        }

        internal Task<string> GetKey(string Table, SQLDBContext Refcontext = null)
        {
            return Task.Run(() =>
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();
                string result = context.Model.FindEntityType($"StreamerBotLib.DataSQL.Models.{Table}").FindPrimaryKey().GetName();
                if (Refcontext == null) { ClearDataContext(context); }
                return result;
            });
        }

        internal Task<IEnumerable<string>> GetKeys(string Table, SQLDBContext Refcontext = null)
        {
            return Task.Run(() =>
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();
                IEnumerable<string> result = new List<string>(from P in context.Model.FindEntityType($"StreamerBotLib.DataSQL.Models.{Table}").FindPrimaryKey().Properties select P.Name);
                if (Refcontext == null) { ClearDataContext(context); }
                return result;
            });
        }

        internal Task<List<OverlayActionType>> GetOverlayActions(OverlayTypes overlayType, string overlayAction, string username, SQLDBContext Refcontext = null)
        {
            return Task.Run(() =>
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();
                List<OverlayActionType> result = [.. (from O in context.OverlayServices
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
                                                  })];
                if (Refcontext == null) { ClearDataContext(context); }
                return result;
            });
        }

        internal Task<string> GetQuote(int QuoteNum, SQLDBContext Refcontext = null)
        {
            return Task.Run(() =>
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();
                var result = (from Q in context.Quotes
                              where Q.Number == QuoteNum
                              select $"{Q.Number}: {Q.Quote}").FirstOrDefault();
                if (Refcontext == null) { ClearDataContext(context); }
                return result;
            });
        }

        internal Task<int> GetQuoteCount(SQLDBContext Refcontext = null)
        {
            return Task.Run(() =>
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();
                int result = context.Quotes.MaxBy((q) => q.Number)?.Number ?? 0;
                if (Refcontext == null) { ClearDataContext(context); }
                return result;
            });
        }

        internal Task<Dictionary<string, List<string>>> GetOverlayActions(SQLDBContext Refcontext = null)
        {
            return Task.Run(() =>
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();
                Dictionary<string, List<string>> result = new()
                {
                    { nameof(Commands), new(from C in context.Commands select C.CmdName) },
                    { nameof(ChannelEvents), new(from E in context.ChannelEvents select E.Name.ToString()) }
                };
                if (Refcontext == null) { ClearDataContext(context); }
                return result;
            });
        }

        internal Task<List<string>> GetSocialComs(SQLDBContext Refcontext = null)
        {
            return Task.Run(() =>
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();
                List<string> result = new(from SC in context.Commands.IntersectBy((string[])Enum.GetValues(typeof(DefaultSocials)), (c) => c.CmdName)
                                          select SC.CmdName);
                if (Refcontext == null) { ClearDataContext(context); }
                return result;
            });
        }

        internal Task<string> GetSocials(SQLDBContext Refcontext = null)
        {
            return Task.Run(() =>
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();
                var result = string.Join(" ", (from SC in
                                             context.Commands.IntersectBy((string[])Enum.GetValues(typeof(DefaultSocials)),
                                                (c) => c.CmdName)
                                               where SC.Message != DefaulSocialMsg
                                               select SC));
                if (Refcontext == null) { ClearDataContext(context); }
                return result;
            });
        }

        internal Task<StreamStat> GetStreamData(DateTime dateTime, SQLDBContext Refcontext = null)
        {
            return Task.Run(() =>
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();

                var result = (from SD in context.StreamStats
                              where SD.StreamStart == dateTime
                              select StreamStat.Create(SD)).FirstOrDefault();
                if (Refcontext == null) { ClearDataContext(context); }
                return result;
            });
        }

        internal Task<List<string>> GetTableFields(string TableName, SQLDBContext Refcontext = null)
        {
            return Task.Run(() =>
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();
                List<string> result = new(from T in context.Model.FindEntityType($"StreamerBotLib.DataSQL.Models.{TableName}").GetMembers()
                                          select T.Name);
                if (Refcontext == null) { ClearDataContext(context); }
                return result;
            });
        }

        internal Task<List<string>> GetTableNames(SQLDBContext Refcontext = null)
        {
            return Task.Run(() =>
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
            });
        }

        internal Task<List<TickerItem>> GetTickerItems(SQLDBContext Refcontext = null)
        {
            return Task.Run(() =>
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();
                List<TickerItem> result = new(from F in context.OverlayTicker
                                              select new TickerItem(F.TickerName, F.UserName));
                if (Refcontext == null) { ClearDataContext(context); }
                return result;
            });
        }

        internal Task<Tuple<string, int, List<string>>> GetTimerCommand(string Cmd, SQLDBContext Refcontext = null)
        {
            return Task.Run(() =>
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();
                Tuple<string, int, List<string>> result = (from R in context.Commands
                                                           where R.RepeatTimer > 0
                                                           select new Tuple<string, int, List<string>>(R.CmdName, R.RepeatTimer, new(R.Category))).FirstOrDefault();
                if (Refcontext == null) { ClearDataContext(context); }
                return result;
            });
        }

        internal Task<List<Tuple<string, int, List<string>>>> GetTimerCommands(SQLDBContext Refcontext = null)
        {
            return Task.Run(() =>
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();
                List<Tuple<string, int, List<string>>> result = new(from R in context.CommandsBase
                                                                    where R.RepeatTimer > 0
                                                                    select new Tuple<string, int, List<string>>(R.CmdName, R.RepeatTimer, new(R.Category)));
                if (Refcontext == null) { ClearDataContext(context); }
                return result;
            });
        }

        internal Task<int> GetTimerCommandTime(string Cmd, SQLDBContext Refcontext = null)
        {
            return Task.Run(() =>
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();
                int result = (from R in context.Commands
                              where R.CmdName == Cmd
                              select R.RepeatTimer).FirstOrDefault();
                if (Refcontext == null) { ClearDataContext(context); }
                return result;
            });
        }

        internal Task<string> GetUsage(string command, SQLDBContext Refcontext = null)
        {
            return Task.Run(() =>
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();
                string result = (from C in context.Commands
                                 where C.CmdName == command
                                 select C.Usage).FirstOrDefault();
                if (Refcontext == null) { ClearDataContext(context); }
                return result;
            });
        }

        internal Task<List<Tuple<bool, Uri>>> GetWebhooks(WebhooksSource webhooksSource, WebhooksKind webhooks, SQLDBContext Refcontext = null)
        {
            return Task.Run(() =>
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();
                List<Tuple<bool, Uri>> result = new(from W in context.Webhooks
                                                    where (W.WebhooksSource == webhooksSource
                                                    && W.Kind == webhooks && W.DataSource == WebhookDataSource.Channel
                                                    && W.IsEnabled == true)
                                                    select new Tuple<bool, Uri>(W.AddEveryone, W.Webhook));
                if (Refcontext == null) { ClearDataContext(context); }
                return result;
            });
        }

        #endregion Get_Methods

    }
}
