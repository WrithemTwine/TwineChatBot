
namespace StreamerBotLib.DataSQL.MultiContext
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Internal;

    using StreamerBotLib.DataSQL.DiscriminatorEnums;
    using StreamerBotLib.DataSQL.Models;
    using StreamerBotLib.Models;
    using StreamerBotLib.Models.Enums;
    using StreamerBotLib.Systems.Overlay.Enums;
    using StreamerBotLib.Systems.Overlay.Models;

    using System.Data;
    internal partial class DataManagerSQLAsync
    {
        #region Get_Methods

        internal async Task<Tuple<ModActions, StreamerBotLib.Models.Enums.BanReasons, int>> FindRemedy(ViewerTypes viewerTypes, MsgTypes msgTypes)
        {
            using var context = BuildDataContext();

            // Fetch BanReasons asynchronously
            var banReasons = await context.BanReasons
                .Where(br => br.MsgType == msgTypes)
                .Select(br => br.BanReason)
                .FirstOrDefaultAsync();

            // Fetch BanRules asynchronously
            var banRules = await context.BanRules
                .Where(b => b.ViewerTypes == ViewerTypes.Viewer && b.MsgType == msgTypes)
                .FirstOrDefaultAsync();

            // Return the result as a tuple
            return new Tuple<ModActions, StreamerBotLib.Models.Enums.BanReasons, int>(
                banRules?.ModAction ?? ModActions.Allow,
                banReasons,
                banRules?.TimeoutSeconds ?? 0
            );
        }

        internal async Task<bool> GetCmdAnnounce(string CmdName)
        {
            using var context = BuildDataContext();

            return await context.Commands
                                .Where(C => C.CmdName == CmdName)
                                .Select(C => C.Announce)
                                .FirstOrDefaultAsync();
        }

        internal async Task<bool> GetEventAnnounce(ChannelEventActions EventName)
        {
            using var context = BuildDataContext();
            return await context.ChannelEvents
                            .Where(E => E.Name == EventName)
                            .Select(E => E.Announce)
                            .FirstOrDefaultAsync();
        }

        internal async Task<CommandData> GetCommand(string cmd)
        {
            using var context = BuildDataContext();

            var command = await context.CommandsBase
                .FirstOrDefaultAsync(c => c.CmdName == cmd);

            if (command != null)
            {
                command.Calls++;
                await context.SaveChangesAsync();
            }

            await RefreshCommandsList(true);
            await RefreshCommandsUserList(true);

            return command != null ? new CommandData(command) : null;
        }

        internal async Task<string> GetCommandString()
        {
            using var context = BuildDataContext();
            var commandList = await GetCommandList();
            return string.Join(", ", commandList);
        }

        internal async Task<List<string>> GetCommandList(bool prefix = true)
        {
            using var context = BuildDataContext();

            return await context.CommandsBase
                .Where(Com => Com.Message != DefaulSocialMsg && Com.IsEnabled)
                .OrderBy(Com => Com.CmdName)
                .Select(Com => $"{(prefix ? "!" : "")}{Com.CmdName}")
                .ToListAsync();
        }

        internal async Task<List<string>> GetCurrencyNames()
        {
            using var context = BuildDataContext();
            return await context.CurrencyType
                            .Select(C => C.CurrencyName)
                            .ToListAsync();
        }

        internal async Task<int> GetDeathCounter(string currCategory)
        {
            using var context = BuildDataContext();
            return await context.GameDeadCounter
                                .Where(D => D.Category == currCategory)
                                .Select(D => D.Counter)
                                .FirstOrDefaultAsync();
        }

        internal async Task<Tuple<string, bool, short>> GetEventRowData(ChannelEventActions rowcriteria)
        {
            using var context = BuildDataContext();

            return await context.ChannelEvents
                                .Where(E => E.Name == rowcriteria)
                                .Select(E => new Tuple<string, bool, short>(E.Message, E.IsEnabled, E.RepeatMsg))
                                .FirstOrDefaultAsync();
        }

        internal async Task<int> GetFollowerCount()
        {
            using var context = BuildDataContext();
            return await context.Followers.CountAsync();
        }

        internal async Task<List<CategoryData>> GetGameCategories()
        {
            using var context = BuildDataContext();
            return await context.CategoryList
                                .Select(C => new CategoryData(C.CategoryId, C.Category))
                                .ToListAsync();
        }

        internal async Task<string> GetKey(string table)
        {
            using var context = BuildDataContext();
            var entityType = context.Model.FindEntityType($"StreamerBotLib.DataSQL.Models.{table}");
            return entityType?.FindPrimaryKey()?.GetName();
        }

        internal async Task<IEnumerable<string>> GetKeys(string Table)
        {
            using var context = BuildDataContext();
            var entityType = context.Model.FindEntityType($"StreamerBotLib.DataSQL.Models.{Table}");
            return entityType?.FindPrimaryKey()?.Properties.Select(p => p.Name) ?? Enumerable.Empty<string>();
        }

        internal async Task<List<OverlayActionType>> GetOverlayActions(OverlayTypes overlayType, string overlayAction, string username)
        {
            using var context = BuildDataContext();
            return await context.OverlayServices
                .Where(O => O.IsEnabled && O.OverlayType == overlayType && (string.IsNullOrEmpty(O.UserName) || O.UserName == username) && O.OverlayAction == overlayAction)
                .Select(O => new OverlayActionType()
                {
                    ActionValue = O.OverlayAction,
                    Duration = O.Duration,
                    MediaFile = O.MediaFile,
                    ImageFile = O.ImageFile,
                    Message = O.Message,
                    OverlayType = O.OverlayType,
                    UserName = O.UserName,
                    UseChatMsg = O.UseChatMsg
                })
                .ToListAsync();
        }

        internal async Task<string> GetQuote(int QuoteNum)
        {
            using var context = BuildDataContext();

            return await context.Quotes
                                .Where(Q => Q.Number == QuoteNum)
                                .Select(Q => $"{Q.Number}: {Q.Quote}")
                                .FirstOrDefaultAsync();
        }

        internal async Task<int> GetQuoteCount()
        {
            using var context = BuildDataContext();

            return await context.Quotes.MaxAsync((q) => q.Number);
        }

        internal async Task<Dictionary<string, List<string>>> GetOverlayActions()
        {
            using var context = BuildDataContext();

            return new Dictionary<string, List<string>>
            {
                { nameof(Commands), await context.Commands.Select(C => C.CmdName).ToListAsync() },
                { nameof(ChannelEvents), await context.ChannelEvents.Select(E => E.Name.ToString()).ToListAsync() }
            };
        }

        internal async Task<List<string>> GetSocialComs()
        {
            using var context = BuildDataContext();
            var Socials = Enum.GetValues<DefaultSocials>().Cast<DefaultSocials>();

            return await context.Commands
                              .Where(c => Socials.Any(s => s.ToString() == c.CmdName))
                              .Select(c => c.CmdName)
                              .ToListAsync();
        }

        internal async Task<string> GetSocials()
        {
            using var context = BuildDataContext();

            var socials = Enum.GetValues<DefaultSocials>().Select(s => s.ToString());
            var result = await context.Commands
                .Where(c => socials.Contains(c.CmdName) && c.Message != DefaulSocialMsg)
                .Select(c => c.CmdName)
                .ToListAsync();

            return string.Join(" ", result);
        }

        internal async Task<StreamStat> GetStreamData(DateTime dateTime)
        {
            using var context = BuildDataContext();
            CurrStreamStart = dateTime;

            return await context.StreamStats
                                .Where(S => S.StreamStart == dateTime)
                                .Select(S => StreamStat.Create(S))
                                .FirstOrDefaultAsync();
        }

        internal async Task<List<string>> GetTableFields(string TableName)
        {
            using var context = BuildDataContext();

            return context.Model.FindEntityType($"StreamerBotLib.DataSQL.Models.{TableName}").GetMembers()
                            .Select(T => T.Name)
                            .ToList();
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

        internal async Task<List<TickerItem>> GetTickerItems()
        {
            using var context = BuildDataContext();

            return await context.OverlayTicker
                    .Select(F => new TickerItem(F.TickerName, F.UserName))
                    .ToListAsync();
        }

        internal async Task<Tuple<string, int, List<string>>> GetTimerCommand(string Cmd)
        {
            using var context = BuildDataContext();

            return await context.Commands
                                .Where(R => R.IsEnabled && R.RepeatTimer > 0)
                                .Select(R => new Tuple<string, int, List<string>>(R.CmdName, R.RepeatTimer, new(R.Category)))
                                .FirstOrDefaultAsync();
        }

        internal async Task<List<Tuple<string, int, List<string>>>> GetTimerCommands()
        {
            using var context = BuildDataContext();

            return await context.CommandsBase
                                .Where(R => R.IsEnabled && R.RepeatTimer > 0)
                                .Select(R => new Tuple<string, int, List<string>>(R.CmdName, R.RepeatTimer, new(R.Category)))
                                .ToListAsync();
        }

        internal async Task<int> GetTimerCommandTime(string Cmd)
        {
            using var context = BuildDataContext();

            return await context.Commands
                                .Where(R => R.CmdName == Cmd)
                                .Select(R => R.RepeatTimer)
                                .FirstOrDefaultAsync();
        }

        internal async Task<string> GetUsage(string command)
        {
            using var context = BuildDataContext();

            return await context.Commands
                                .Where(C => C.CmdName == command)
                                .Select(C => C.Usage)
                                .FirstOrDefaultAsync();
        }

        internal async Task<List<Tuple<bool, Uri>>> GetWebhooks(WebhooksSource webhooksSource, WebhooksKind webhooks)
        {
            using var context = BuildDataContext();

            return await context.Webhooks
                                .Where(W => W.WebhooksSource == webhooksSource
                                                && W.Kind == webhooks && W.DataSource == WebhookDataSource.Channel
                                                && W.IsEnabled == true)
                                .Select(W => new Tuple<bool, Uri>(W.AddEveryone, W.Webhook))
                                .ToListAsync();
        }

        #endregion Get_Methods

    }
}
