using StreamerBotLib.DataSQL.Models;
using StreamerBotLib.Enums;
using StreamerBotLib.GUI;
using StreamerBotLib.Overlay.Enums;
using StreamerBotLib.Static;

using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Controls;
using System.Xml;

using static StreamerBotLib.DataSQL.Import.DataSource;

using MultiDataSource = StreamerBotLib.DataSQL.Import.Multi.DataSource;

namespace StreamerBotLib.DataSQL.Import
{
    internal class ImportDataSources : BaseDataManager
    {
        private static readonly string DataFileXML = "ChatDataStore.xml";
        private DataSource _DataSource;

        private static readonly string MultiDataFileXML = "MultiChatbotData.xml";
        private readonly MultiDataSource _MultiDataSource;

        public ImportDataSources()
        {
            InitializeFileUsed(DataFileXML);
            _DataSource = new();
            LoadData();
            InitializeFileUsed(MultiDataFileXML);
            _MultiDataSource = new();
            MultiLoadData();
        }

        /// <summary>
        /// Load the data source and populate with default data; if regular data source is corrupted, attempt to load backup data.
        /// </summary>
        private void LoadData()
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.DataManager, $"Loading the database.");

            void LoadFile(string filename)
            {
                lock (GUIDataManagerLock.Lock)
                {
                    if (!File.Exists(filename))
                    {
                        _DataSource.WriteXml(filename);
                    }

                    _ = _DataSource.ReadXml(new XmlTextReader(filename), XmlReadMode.DiffGram);

                    foreach (CommandsRow c in _DataSource.Commands.Select())
                    {
                        if (DBNull.Value.Equals(c["IsEnabled"]))
                        {
                            c["IsEnabled"] = true;
                        }
                    }
                }
                OptionFlags.DataLoaded = true;
            }

            TryLoadFile((xmlfile) => LoadFile(xmlfile));

        }

        /// <summary>
        /// Format the file name to handle debug path locations, otherwise ignored in release versions.
        /// </summary>
        /// <param name="fileName">Name of the file to format, prepend debug directory.</param>
        /// <returns>File path, whether in debug directory or relative filename in current release directory - based on Current Working Directory.</returns>
        private string FormatFileName(string fileName)
        {
            return
#if DEBUG
            // add specific directory location for debug purposes, ignore for release
            Path.Combine(Directory.GetCurrentDirectory(),
#endif
                fileName
#if DEBUG
                )
#endif
                ;
        }

        /// <summary>
        /// Load the data source and populate with default data
        /// </summary>
        public void MultiLoadData()
        {
            _MultiDataSource.Clear();
            void LoadFile(string filename)
            {
                lock (_MultiDataSource)
                {
                    if (!File.Exists(filename))
                    {
                        _MultiDataSource.WriteXml(filename);
                    }

                    _ = _MultiDataSource.ReadXml(new XmlTextReader(filename), XmlReadMode.DiffGram);

                }
                OptionFlags.MultiDataLoaded = true;
            }

            TryLoadFile((xmlfile) => LoadFile(xmlfile));

            try
            {
                _MultiDataSource.AcceptChanges();
            }
            catch (ConstraintException)
            {
                _MultiDataSource.EnforceConstraints = false;

                //foreach (DataTable table in _MultiDataSource.Tables)
                //{
                //    List<DataRow> UniqueRows = [];
                //    List<DataRow> DuplicateRows = [];

                //    foreach (DataRow datarow in table.Rows)
                //    {
                //        if (!UniqueRows.UniqueAdd(datarow, new DataRowEquatableComparer()))
                //        {
                //            DuplicateRows.Add(datarow);
                //        }
                //    }

                //    DuplicateRows.ForEach(r => r.Delete());
                //}

                _MultiDataSource.AcceptChanges();
                _MultiDataSource.EnforceConstraints = true;
            }
        }

        /// <summary>
        /// Converts data from the primary database or the multilive database files from the XML Datagram used in the prior bot version
        /// </summary>
        /// <param name="context">The new Database context to use for importing data.</param>
        public void ConvertData(SQLDBContext context)
        {

            #region added

            /*
            //ChannelEvents

            //UserBase
            //Users
            //UserStats

            //BanReasons
            //BanRules
            //ModeratorApprove
            //LearnMsgs

            //Commands
            //CommandsUser
            //CustomWelcome
            //Followers
            //GiveawayUserData
            //InRaidData

            //OutRaidData
            //ShoutOuts

            //CategoryList
            //GameDeadCounter
            //Clips
            //Quotes
            //OverlayServices
            //OverlayTicker
            //Webhooks
            //StreamStats

            //Currency
            //CurrencyBase
            //CurrencyType

            //MultiChannels
            //MultiLiveStreams
            //MultiMsgEndPoints
            //MultiSummaryLiveStreams
            */

            #endregion


            if (_DataSource != null)
            {

                #region Channel Events

                context.ChannelEvents.AddRange(from CE in _DataSource.ChannelEvents
                                               select new ChannelEvents(
                                                   name: Enum.Parse<ChannelEventActions>( CE.Name),
                                                   repeatMsg: CE.RepeatMsg,
                                                   addMe: CE.AddMe,
                                                   isEnabled: CE.IsEnabled,
                                                   message: CE.Message,
                                                   commands: CE.Commands
                                                   ));

                #endregion

                #region Users

                context.Users.AddRange(from U in _DataSource.Users
                                       where !(DBNull.Value.Equals(U.UserId)) && (U.UserId != null)
                                       select new Users(
                                                    firstDateSeen: U.FirstDateSeen,
                                                    currLoginDate: U.CurrLoginDate,
                                                    lastDateSeen: U.LastDateSeen,
                                                    userId: U.UserId,
                                                    userName: U.UserName,
                                                    platform: Enum.Parse<Platform>(U.Platform))
                                       );

                context.UserStats.AddRange(from U in _DataSource.Users
                                           where !(DBNull.Value.Equals(U.UserId)) && (U.UserId != null)
                                           select new UserStats(
                                                watchTime: U.WatchTime,
                                                channelChat: 0,
                                                callCommands: 0,
                                                rewardRedeems: 0,
                                                clipsCreated: 0,
                                                userId: U.UserId,
                                                userName: U.UserName,
                                                platform: Platform.Twitch
                                           ));

                context.CustomWelcome.AddRange(from CW in _DataSource.CustomWelcome
                                               let uId = ((UsersRow)_DataSource.Users.Select($"[UserName]='{CW.UserName}'").First()).UserId
                                               where uId is not null
                                               select new CustomWelcome(
                                                   message: CW.Message,
                                                   userId: uId,
                                                   userName: CW.UserName,
                                                   platform: Platform.Twitch
                                                   ));

                context.Followers.AddRange(from F in _DataSource.Followers
                                           where !(DBNull.Value.Equals(F.UserId)) && (F.UserId != null)
                                           select new Followers(
                                                    isFollower: F.IsFollower,
                                                    followedDate: F.FollowedDate,
                                                    statusChangeDate: F.StatusChangeDate,
                                                    category: F.Category,
                                                    addDate: F.FollowedDate,
                                                    userId: F.UserId,
                                                    userName: F.UserName,
                                                    platform: Enum.Parse<Platform>(F.Platform)
                                               ));

                context.GiveawayUserData.AddRange(from G in _DataSource.GiveawayUserData
                                                  let uId = ((UsersRow)_DataSource.Users.Select($"[UserName]='{G.DisplayName}'").First()).UserId
                                                  where uId is not null
                                                  select new GiveawayUserData(
                                                      dateTime: G.DateTime,
                                                      userId: uId,
                                                      userName: G.DisplayName,
                                                      platform: Platform.Twitch
                                                      ));

                context.InRaidData.AddRange(from IR in _DataSource.InRaidData
                                            let uId = ((UsersRow)_DataSource.Users.Select($"[UserName]='{IR.UserName}'").FirstOrDefault())?.UserId
                                            where uId is not null
                                            select new InRaidData(
                                                viewerCount: Convert.ToInt32(IR.ViewerCount),
                                                raidDate: IR.DateTime,
                                                category: IR.Category,
                                                userId: uId,
                                                userName: IR.UserName,
                                                platform: Platform.Twitch
                                                ));

                context.OutRaidData.AddRange(from OR in _DataSource.OutRaidData
                                             select new OutRaidData(
                                                 id: OR.Id,
                                                 channelRaided: OR.ChannelRaided,
                                                 raidDate: OR.DateTime
                                                 ));

                context.ShoutOuts.AddRange(from S in _DataSource.ShoutOuts
                                           let uId = ((UsersRow)_DataSource.Users.Select($"[UserName]='{S.UserName}'").FirstOrDefault())?.UserId
                                           where (uId != null)
                                           select new ShoutOuts(
                                               userId: uId,
                                               userName: S.UserName,
                                               platform: Enum.Parse<Platform>(S.Platform)
                                               ));

                #endregion

                #region Currency

                context.CurrencyType.AddRange(from CT in _DataSource.CurrencyType
                                              select new Models.CurrencyType(
                                                accrueAmt: CT.AccrueAmt,
                                                seconds: (int)CT.Seconds,
                                                maxValue: CT.MaxValue,
                                                currencyName: CT.CurrencyName
                                               ));

                context.Currency.AddRange(from C in _DataSource.Currency
                                          let uId = ((UsersRow)_DataSource.Users.Select($"[UserName]='{C.UserName}'").First()).UserId
                                          where uId is not null
                                          select new Currency(
                                              userName: C.UserName,
                                              value: C.Value,
                                              currencyName: C.CurrencyName
                                           ));

                #endregion

                #region Category & GameDeadCounter & Clips & Quotes & Overlay Data & Webhooks & StreamStats

                context.CategoryList.AddRange(from CL in _DataSource.CategoryList
                                              where (CL.CategoryId != null) || (CL.Category == "All")
                                              select new CategoryList(
                                                  categoryId: CL.Category == "All" ? "0" : CL.CategoryId,
                                                  category: CL.Category,
                                                  streamCount: CL.StreamCount
                                                  )
                                              );

                context.GameDeadCounter.AddRange(from GDC in _DataSource.GameDeadCounter
                                                 let CI = ((CategoryListRow)(_DataSource.CategoryList.Select($"[Category] = '{GDC.Category}'")).First()).CategoryId
                                                 select new GameDeadCounter(
                                                     categoryId: CI,
                                                     category: GDC.Category,
                                                     counter: GDC.Counter
                                                 ));

                context.Clips.AddRange(from C in _DataSource.Clips
                                       select new Clips(
                                           clipId: C.Id,
                                           createdAt: C.CreatedAt,
                                           title: C.Title,
                                           categoryId: C.GameId,
                                           language: C.Language,
                                           duration: (float)C.Duration,
                                           url: C.Url
                                           )
                                        );

                context.Quotes.AddRange(from Q in _DataSource.Quotes
                                        select new Quotes(
                                            number: Q.Number,
                                            quote: Q.Quote
                                            )
                                         );

                context.OverlayServices.AddRange(from OS in _DataSource.OverlayServices
                                                 select new OverlayServices(
                                                       id: OS.Id,
                                                       isEnabled: OS.IsEnabled,
                                                       duration: OS.Duration,
                                                       overlayType: Enum.Parse<OverlayTypes>(OS.OverlayType),
                                                       overlayAction: OS.OverlayAction,
                                                       userName: OS.UserName,
                                                       useChatMsg: OS.UseChatMsg,
                                                       message: OS.Message,
                                                       imageFile: OS.ImageFile,
                                                       mediaFile: OS.MediaFile
                                                     )
                                                  );

                context.OverlayTicker.AddRange(from OT in _DataSource.OverlayTicker
                                               select new OverlayTicker(
                                                   tickerName: Enum.Parse<OverlayTickerItem>(OT.TickerName),
                                                   userName: OT.UserName
                                                   ));

                context.Webhooks.AddRange(from W in _DataSource.Discord
                                          select new Webhooks(
                                              isEnabled: W.IsEnabled,
                                              webhooksSource: WebhooksSource.Discord,
                                              server: W.Server,
                                              kind: Enum.Parse<WebhooksKind>(W.Kind),
                                              addEveryone: W.AddEveryone,
                                              webhook: new(W.Webhook)
                                              ));

                context.StreamStats.AddRange(from S in _DataSource.StreamStats
                                             select new StreamStats(
                                                 streamStart: S.StreamStart,
                                                 streamEnd: S.StreamEnd,
                                                 newFollows: S.NewFollows,
                                                 newSubscribers: S.NewSubscribers,
                                                 giftSubs: S.GiftSubs,
                                                 bits: (int)S.Bits,
                                                 raids: S.Raids,
                                                 hosted: S.Hosted,
                                                 usersBanned: S.UsersBanned,
                                                 usersTimedOut: S.UsersTimedOut,
                                                 moderatorsPresent: S.ModeratorsPresent,
                                                 subsPresent: S.SubsPresent,
                                                 vIPsPresent: S.VIPsPresent,
                                                 totalChats: S.TotalChats,
                                                 commandsMsgs: S.Commands,
                                                 automatedEvents: S.AutomatedEvents,
                                                 automatedCommands: S.AutomatedCommands,
                                                 webhookMsgs: S.DiscordMsgs,
                                                 clipsMade: S.ClipsMade,
                                                 channelPtCount: S.ChannelPtCount,
                                                 channelChallenge: S.ChannelChallenge,
                                                 maxUsers: S.MaxUsers
                                                 ));

                #endregion

                #region Ban & Approve & LearnMsgs

                context.BanReasons.AddRange(from BR in _DataSource.BanReasons
                        select new Models.BanReasons(
                            msgType: Enum.Parse<MsgTypes>(BR.MsgType), 
                            banReason: Enum.Parse<Enums.BanReasons>(BR.BanReason)) );
                
                context.BanRules.AddRange(from BR in _DataSource.BanRules
                        select new BanRules(
                            viewerTypes: Enum.Parse<ViewerTypes>(BR.ViewerTypes),
                            msgType: Enum.Parse<MsgTypes>(BR.MsgType),
                            modAction: Enum.Parse<ModActions>(BR.ModAction),
                            timeoutSeconds: Convert.ToInt32(BR.TimeoutSeconds)));

                context.ModeratorApprove.AddRange(from MA in _DataSource.ModeratorApprove
                        select new ModeratorApprove(
                            isEnabled: MA.IsEnabled,
                            modActionType: Enum.Parse<ModActionType>(MA.ModActionType),
                            modActionName: MA.ModActionName,
                            modPerformAction: MA.ModPerformAction,
                            modPerformType: Enum.Parse<ModPerformType>(MA.ModPerformType)));

                context.LearnMsgs.AddRange(from LM in _DataSource.LearnMsgs
                       select new LearnMsgs(
                            msgType: Enum.Parse<MsgTypes>(LM.MsgType),
                            teachingMsg: LM.TeachingMsg));

            #endregion

                #region Commands

                context.Commands.AddRange(from C in _DataSource.Commands
                                          where Enum.GetNames<DefaultCommand>().Contains(C.CmdName)
                                          select new Commands(
                                              cmdName: C.CmdName,
                                              addMe: C.AddMe,
                                              permission: Enum.Parse<ViewerTypes>(C.Permission),
                                              isEnabled: C.IsEnabled,
                                              message: C.Message,
                                              repeatTimer: C.RepeatTimer,
                                              sendMsgCount: C.SendMsgCount,
                                              category: C.Category.Split(","),
                                              allowParam: C.AllowParam,
                                              usage: C.Usage,
                                              lookupData: C.lookupdata,
                                              table: C.table,
                                              keyField: C.key_field,
                                              dataField: C.data_field,
                                              currencyField: C.currency_field,
                                              unit: C.unit,
                                              action: Enum.Parse<CommandAction>(C.action),
                                              top: C.top,
                                              sort: Enum.Parse<CommandSort>(C.sort))
                                          );

                context.CommandsUser.AddRange(from C in _DataSource.Commands
                                              where !Enum.GetNames<DefaultCommand>().Contains(C.CmdName)
                                              select new CommandsUser(
                                              cmdName: C.CmdName,
                                              addMe: C.AddMe,
                                              permission: Enum.Parse<ViewerTypes>(C.Permission),
                                              isEnabled: C.IsEnabled,
                                              message: C.Message,
                                              repeatTimer: C.RepeatTimer,
                                              sendMsgCount: C.SendMsgCount,
                                              category: C.Category.Split(","),
                                              allowParam: C.AllowParam,
                                              usage: C.Usage,
                                              lookupData: C.lookupdata,
                                              table: C.table,
                                              keyField: C.key_field,
                                              dataField: C.data_field,
                                              currencyField: C.currency_field,
                                              unit: C.unit,
                                              action: Enum.Parse<CommandAction>(C.action),
                                              top: C.top,
                                              sort: Enum.Parse<CommandSort>(C.sort))
                          );

                #endregion

            }
            else if (_MultiDataSource != null)
            {
                #region Multi data

                context.MultiChannels.AddRange(from U in (_MultiDataSource.Channels)
                                               where !(DBNull.Value.Equals(U.UserId)) && (U.UserId != null)
                                               select new MultiChannels(
                                                   userId: U.UserId,
                                                   userName: U.ChannelName,
                                                   platform: Platform.Twitch));

                context.MultiLiveStreams.AddRange(from L in (_MultiDataSource.LiveStream)
                                                  where !(DBNull.Value.Equals(L.UserId)) && (L.UserId != null)
                                                  select new MultiLiveStreams(
                                                      liveDate: L.LiveDate,
                                                      userId: L.UserId,
                                                      userName: L.ChannelName,
                                                      platform: Platform.Twitch));

                context.MultiMsgEndPoints.AddRange(from E in _MultiDataSource.MsgEndPoints
                                                   select new MultiMsgEndPoints(
                                                           isEnabled: E.IsEnabled,
                                                           webhooksSource: Enum.Parse<WebhooksSource>(E.Type),
                                                           webhook: new(E.URL),
                                                           server: E.Server,
                                                           kind: WebhooksKind.Live,
                                                           addEveryone: false
                                                   )
                                                   );


                context.MultiSummaryLiveStreams.AddRange(from S in (_MultiDataSource.SummaryLiveStream)
                                                         let uId = ((UsersRow)_DataSource.Users.Select($"[UserName]='{S.ChannelName}'").FirstOrDefault())?.UserId
                                                         where uId is not null
                                                         select new MultiSummaryLiveStreams(
                                                             userId: uId,
                                                             userName: S.ChannelName,
                                                             streamCount: S.StreamCount,
                                                             throughDate: S.ThroughDate)
                                                          );

                #endregion

            }
        }
    }
}
