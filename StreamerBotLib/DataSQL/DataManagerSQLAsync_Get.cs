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

        internal async Task<Tuple<ModActions, Enums.BanReasons, int>> FindRemedy(ViewerTypes viewerTypes, MsgTypes msgTypes)
        {
            return await Task.Run(() =>
            {
                using var context = BuildDataContext();
                Enums.BanReasons banReasons = (from Br in context.BanReasons
                                               where Br.MsgType == msgTypes
                                               select Br.BanReason).FirstOrDefault();
                BanRules banRules = (from B in context.BanRules
                                     where (B.ViewerTypes == ViewerTypes.Viewer && B.MsgType == msgTypes)
                                     select B).FirstOrDefault();


                return new Tuple<ModActions, Enums.BanReasons, int>(banRules?.ModAction ?? ModActions.Allow, banReasons, banRules.TimeoutSeconds);
            });
        }

        internal Task<bool> GetCmdAnnounce(string CmdName)
        {
            return Task.Run(() =>
            {
                using var context = BuildDataContext();

                bool commandAnnounce = (from C in context.Commands
                                        where C.CmdName == CmdName
                                        select C.Announce).FirstOrDefault();

                return commandAnnounce;
            });

        }

        internal Task<bool> GetEventAnnounce(ChannelEventActions EventName)
        {
            return Task.Run(() =>
            {
                using var context = BuildDataContext();
                bool currannounce = (from E in context.ChannelEvents
                                     where E.Name == EventName
                                     select E.Announce).FirstOrDefault();
                return currannounce;
            });
        }

        internal Task<CommandData> GetCommand(string cmd)
        {
            return Task.Run(async () =>
            {
                using var context = BuildDataContext();

                CommandsBase commands = (from C in context.CommandsBase where C.CmdName == cmd select C).FirstOrDefault();

                if (commands != null)
                {
                    commands.Calls++;
                }

                CommandData result = (commands != null) ? new(commands) : null;// commandsUser != null ? new(commandsUser.First()) : null;
                await context.SaveChangesAsync();
                RefreshCommandsList();
                RefreshCommandsUserList();

                return result;
            });
        }

        internal Task<string> GetCommandString()
        {
            return Task.Run(() =>
            {
                using var context = BuildDataContext();
                string result = string.Join(", ", GetCommandList().Result);

                return result;
            });
        }

        internal Task<List<string>> GetCommandList(bool prefix = true)
        {
            return Task.Run(() =>
            {
                using var context = BuildDataContext();
                var result = new List<string>(from Com in (context.CommandsBase)
                                              where (Com.Message != DefaulSocialMsg && Com.IsEnabled)
                                              orderby Com.CmdName
                                              select $"{(prefix ? "!" : "")}{Com.CmdName}");

                return result;
            });
        }

        internal Task<List<string>> GetCurrencyNames()
        {
            return Task.Run(() =>
            {
                using var context = BuildDataContext();
                List<string> result = [.. (from C in context.CurrencyType
                                       select C.CurrencyName)];

                return result;
            });
        }

        internal Task<int> GetDeathCounter(string currCategory)
        {
            return Task.Run(() =>
            {
                using var context = BuildDataContext();
                int result = (from D in context.GameDeadCounter
                              where D.Category == currCategory
                              select D.Counter).FirstOrDefault();

                return result;
            });
        }

        internal Task<Tuple<string, bool, short>> GetEventRowData(ChannelEventActions rowcriteria)
        {
            return Task.Run(() =>
            {
                using var context = BuildDataContext();
                ChannelEvents found = (from Event in context.ChannelEvents
                                       where Event.Name == rowcriteria
                                       select Event).FirstOrDefault();
                var output = new Tuple<string, bool, short>(
                    found?.Message,
                    found?.IsEnabled ?? false,
                    found?.RepeatMsg ?? 0);



                return output;
            });
        }

        internal Task<int> GetFollowerCount()
        {
            return Task.Run(() =>
            {
                using var context = BuildDataContext();
                var result = context.Followers.Count();

                return result;
            });
        }

        internal Task<List<CategoryData>> GetGameCategories()
        {
            return Task.Run(() =>
            {
                using var context = BuildDataContext();
                List<CategoryData> result = [.. from G in context.CategoryList
                                                let game = new CategoryData(G.CategoryId, G.Category)
                                                select game];

                return result;
            });
        }

        internal Task<string> GetKey(string Table)
        {
            return Task.Run(() =>
            {
                using var context = BuildDataContext();
                string result = context.Model.FindEntityType($"StreamerBotLib.DataSQL.Models.{Table}").FindPrimaryKey().GetName();

                return result;
            });
        }

        internal Task<IEnumerable<string>> GetKeys(string Table)
        {
            return Task.Run(() =>
            {
                using var context = BuildDataContext();
                IEnumerable<string> result = [.. from P in context.Model.FindEntityType($"StreamerBotLib.DataSQL.Models.{Table}").FindPrimaryKey().Properties select P.Name];

                return result;
            });
        }

        internal Task<List<OverlayActionType>> GetOverlayActions(OverlayTypes overlayType, string overlayAction, string username)
        {
            return Task.Run(() =>
            {
                using var context = BuildDataContext();
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

                return result;
            });
        }

        internal Task<string> GetQuote(int QuoteNum)
        {
            return Task.Run(() =>
            {
                using var context = BuildDataContext();
                var result = (from Q in context.Quotes
                              where Q.Number == QuoteNum
                              select $"{Q.Number}: {Q.Quote}").FirstOrDefault();

                return result;
            });
        }

        internal Task<int> GetQuoteCount()
        {
            return Task.Run(() =>
            {
                using var context = BuildDataContext();
                int result = context.Quotes.MaxBy((q) => q.Number)?.Number ?? 0;

                return result;
            });
        }

        internal Task<Dictionary<string, List<string>>> GetOverlayActions()
        {
            return Task.Run(() =>
            {
                using var context = BuildDataContext();
                Dictionary<string, List<string>> result = new()
                {
                    { nameof(Commands), new(from C in context.Commands select C.CmdName) },
                    { nameof(ChannelEvents), new(from E in context.ChannelEvents select E.Name.ToString()) }
                };

                return result;
            });
        }

        internal Task<List<string>> GetSocialComs()
        {
            return Task.Run(() =>
            {
                using var context = BuildDataContext();
                List<string> result = [.. from SC in context.Commands.IntersectBy((string[])Enum.GetValues(typeof(DefaultSocials)), (c) => c.CmdName)
                                          select SC.CmdName];

                return result;
            });
        }

        internal Task<string> GetSocials()
        {
            return Task.Run(() =>
            {
                using var context = BuildDataContext();
                var result = string.Join(" ", (from SC in
                                             context.Commands.IntersectBy((string[])Enum.GetValues(typeof(DefaultSocials)),
                                                (c) => c.CmdName)
                                               where SC.Message != DefaulSocialMsg
                                               select SC));

                return result;
            });
        }

        internal Task<StreamStat> GetStreamData(DateTime dateTime)
        {
            return Task.Run(() =>
            {
                using var context = BuildDataContext();

                var result = (from SD in context.StreamStats
                              where SD.StreamStart == dateTime
                              select StreamStat.Create(SD)).FirstOrDefault();

                return result;
            });
        }

        internal Task<List<string>> GetTableFields(string TableName)
        {
            return Task.Run(() =>
            {
                using var context = BuildDataContext();
                List<string> result = new(from T in context.Model.FindEntityType($"StreamerBotLib.DataSQL.Models.{TableName}").GetMembers()
                                          select T.Name);

                return result;
            });
        }

        internal Task<List<string>> GetTableNames()
        {
            return Task.Run(() =>
            {
                using var context = BuildDataContext();
                var list = new List<string>()
                {
                    nameof(Currency),
                    nameof(UserStats),
                    nameof(Commands),
                    nameof(CustomWelcome),
                    nameof(Followers)
                };
                list.Sort();

                return list;
            });
        }

        internal Task<List<TickerItem>> GetTickerItems()
        {
            return Task.Run(() =>
            {
                using var context = BuildDataContext();
                List<TickerItem> result = [.. from F in context.OverlayTicker
                                              select new TickerItem(F.TickerName, F.UserName)];

                return result;
            });
        }

        internal Task<Tuple<string, int, List<string>>> GetTimerCommand(string Cmd)
        {
            return Task.Run(() =>
            {
                using var context = BuildDataContext();
                Tuple<string, int, List<string>> result = (from R in context.Commands
                                                           where R.IsEnabled && R.RepeatTimer > 0
                                                           select new Tuple<string, int, List<string>>(R.CmdName, R.RepeatTimer, new(R.Category))).FirstOrDefault();

                return result;
            });
        }

        internal Task<List<Tuple<string, int, List<string>>>> GetTimerCommands()
        {
            return Task.Run(() =>
            {
                using var context = BuildDataContext();
                List<Tuple<string, int, List<string>>> result = [.. from R in context.CommandsBase
                                                                    where R.IsEnabled && R.RepeatTimer > 0
                                                                    select new Tuple<string, int, List<string>>(R.CmdName, R.RepeatTimer, new(R.Category))];

                return result;
            });
        }

        internal Task<int> GetTimerCommandTime(string Cmd)
        {
            return Task.Run(() =>
            {
                using var context = BuildDataContext();
                int result = (from R in context.Commands
                              where R.CmdName == Cmd
                              select R.RepeatTimer).FirstOrDefault();

                return result;
            });
        }

        internal Task<string> GetUsage(string command)
        {
            return Task.Run(() =>
            {
                using var context = BuildDataContext();
                string result = (from C in context.Commands
                                 where C.CmdName == command
                                 select C.Usage).FirstOrDefault();

                return result;
            });
        }

        internal Task<List<Tuple<bool, Uri>>> GetWebhooks(WebhooksSource webhooksSource, WebhooksKind webhooks)
        {
            return Task.Run(() =>
            {
                using var context = BuildDataContext();
                List<Tuple<bool, Uri>> result = [.. from W in context.Webhooks
                                                    where (W.WebhooksSource == webhooksSource
                                                    && W.Kind == webhooks && W.DataSource == WebhookDataSource.Channel
                                                    && W.IsEnabled == true)
                                                    select new Tuple<bool, Uri>(W.AddEveryone, W.Webhook)];

                return result;
            });
        }

        #endregion Get_Methods

    }
}
