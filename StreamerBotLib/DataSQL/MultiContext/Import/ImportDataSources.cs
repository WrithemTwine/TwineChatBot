using StreamerBotLib.DataSQL.Models;
using StreamerBotLib.Enums;
using StreamerBotLib.Events;
using StreamerBotLib.GUI.Windows;
using StreamerBotLib.Overlay.Enums;
using StreamerBotLib.Static;

using System.Data;
using System.IO;
using System.Threading.Tasks;
using System.Xml;

using static StreamerBotLib.DataSQL.MultiContext.Import.DataSource;
using static StreamerBotLib.DataSQL.MultiContext.Import.Multi.DataSource;

using MultiDataSource = StreamerBotLib.DataSQL.MultiContext.Import.Multi.DataSource;

namespace StreamerBotLib.DataSQL.MultiContext.Import
{
    internal class ImportDataSources : BaseDataManager
    {
        private static readonly string DataFileXML = "ChatDataStore.xml";
        private readonly DataSource _DataSource;

        private static readonly string MultiDataFileXML = "MultiChatbotData.xml";
        private readonly MultiDataSource _MultiDataSource;

        public ImportDataSources()
        {
            LogWriter.WriteLog($"Loading the data from the prior datafile.");
            InitializeFileUsed(DataFileXML);
            _DataSource = new();
            LoadData();
            LogWriter.WriteLog($"Loading the multilive data, if available, from the prior datafile.");
            InitializeFileUsed(MultiDataFileXML);
            _MultiDataSource = new();
            MultiLoadData();
        }

        /// <summary>
        /// Load the data source and populate with default data; if regular data source is corrupted, attempt to load backup data.
        /// </summary>
        private void LoadData()
        {
            LogWriter.DebugLog("LoadData", DebugLogTypes.DataManager, $"Loading the database.");

            void LoadFile(string filename)
            {
                if (File.Exists(filename))
                {
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
        private static string FormatFileName(string fileName)
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
                    if (File.Exists(filename))
                    {

                        _ = _MultiDataSource.ReadXml(new XmlTextReader(filename), XmlReadMode.DiffGram);

                    }
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

        private event EventHandler<ImportDataProgressUpdateEventArgs> ProgressUpdate;
        private event EventHandler<EventArgs> ImportCompleted;

        /// <summary>
        /// Converts data from the primary database or the multilive database files from the XML Datagram used in the prior bot version
        /// </summary>
        /// <param name="context">The new Database context to use for importing data.</param>
        /// <param name="dataManagerSQL">To access already built methods for entering data into the database using the application data flow.</param>
        public async Task ConvertData(SQLDBContext context, DataManagerSQLAsync dataManagerSQL)
        {
            int totalTables = _DataSource?.Tables.Count ?? 0 + _MultiDataSource?.Tables.Count ?? 0;
            int totalRows = 0;
            int CurrentTotalProgress = 0; // add rows as completed and send through event


            foreach (DataTable t in _DataSource?.Tables)
            {
                totalRows += t.Rows.Count;
            }

            foreach (DataTable t in _MultiDataSource?.Tables)
            {
                totalRows -= t.Rows.Count;
            }

            ImportUpdate progressWindow = new(totalTables, totalRows);
            ProgressUpdate += progressWindow.HandleImplementProgressUpdate;
            ImportCompleted += progressWindow.HandleImportCompleted;
            progressWindow.Show();


            LogWriter.WriteLog($"Starting datagram import into database.");
            LogWriter.WriteLog("This log messaging will note each table, regardless if it actually contains any data.");
            LogWriter.WriteLog($"There are {totalTables} tables and {totalRows} row(s) to import.");

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
            //MultiWebhooks
            //MultiSummaryLiveStreams
            */

            #endregion

            #region Add Non-Related Data

            LogWriter.WriteLog("Adding Channel Events data.");

            if (_DataSource.ChannelEvents.Count > 0)
            {
                foreach (ChannelEventsRow CE in _DataSource.ChannelEvents)
                {

                    if (!(from C in context.ChannelEvents where C.Name == Enum.Parse<ChannelEventActions>(CE.Name) select C).Any())
                    {
                        context.ChannelEvents.Add(new ChannelEvents(
                                                           name: Enum.Parse<ChannelEventActions>(CE.Name),
                                                           repeatMsg: CE.RepeatMsg,
                                                           addMe: CE.AddMe,
                                                           isEnabled: CE.IsEnabled,
                                                           message: CE.Message,
                                                           commands: CE.Commands
                                                           ));
                    }
                }
            }

            CurrentTotalProgress += _DataSource.ChannelEvents.Rows.Count;
            ProgressUpdate?.BeginInvoke(this, new(CurrentTotalProgress), null, null);

            LogWriter.WriteLog("Adding OutRaid data.");
            if (_DataSource.OutRaidData.Count > 0)
            {
                foreach (OutRaidDataRow OR in _DataSource.OutRaidData)
                {
                    if (!(from O in context.OutRaidData where OR.ChannelRaided == O.ChannelRaided && O.RaidDate == OR.DateTime select O).Any())
                    {
                        context.OutRaidData.Add(new OutRaidData(
                                                         channelRaided: OR.ChannelRaided,
                                                         raidDate: OR.DateTime
                                                         ));
                    }
                }
            }

            CurrentTotalProgress += _DataSource.OutRaidData.Rows.Count;
            ProgressUpdate?.BeginInvoke(this, new(CurrentTotalProgress), null, null);


            LogWriter.WriteLog("Adding clips data.");
            if (_DataSource.Clips.Count > 0)
            {
                foreach (ClipsRow C in _DataSource.Clips)
                {
                    if (!(from CR in context.Clips where CR.ClipId == C.Id && CR.CreatedAt == C.CreatedAt select CR).Any())
                    {
                        context.Clips.Add(new Clips(
                                                   clipId: C.Id,
                                                   createdAt: C.CreatedAt,
                                                   title: C.Title,
                                                   categoryId: C.GameId,
                                                   language: C.Language,
                                                   duration: (float)C.Duration,
                                                   url: C.Url
                                                   )
                                                );
                    }
                }
            }

            CurrentTotalProgress += _DataSource.Clips.Rows.Count;
            ProgressUpdate?.BeginInvoke(this, new(CurrentTotalProgress), null, null);


            LogWriter.WriteLog("Adding OverlayServices data.");
            if (_DataSource.OverlayServices.Count > 0)
            {
                foreach (OverlayServicesRow OS in _DataSource.OverlayServices)
                {
                    if (!(from O in context.OverlayServices where O.Id == OS.Id select O).Any())
                    {
                        context.OverlayServices.Add(new OverlayServices(
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
                    }
                }
            }

            CurrentTotalProgress += _DataSource.OverlayServices.Rows.Count;
            ProgressUpdate?.BeginInvoke(this, new(CurrentTotalProgress), null, null);

            LogWriter.WriteLog("Adding OverlayTicker data.");
            if (_DataSource.OverlayTicker.Count > 0)
            {
                foreach (OverlayTickerRow OT in _DataSource.OverlayTicker)
                {
                    if (!(from T in context.OverlayTicker where T.TickerName == Enum.Parse<OverlayTickerItem>(OT.TickerName) select T).Any())
                    {
                        context.OverlayTicker.Add(new OverlayTicker(
                                                           tickerName: Enum.Parse<OverlayTickerItem>(OT.TickerName),
                                                           userName: OT.UserName
                                                           ));
                    }
                }
            }

            CurrentTotalProgress += _DataSource.OverlayTicker.Rows.Count;
            ProgressUpdate?.BeginInvoke(this, new(CurrentTotalProgress), null, null);

            LogWriter.WriteLog("Adding Quotes data.");
            if (_DataSource.Quotes.Count > 0)
            {
                foreach (QuotesRow Q in _DataSource.Quotes)
                {
                    if (!(from Quote in context.Quotes where Quote.Number == Q.Number select Quote).Any())
                    {
                        context.Quotes.Add(new Quotes(
                                                    number: Q.Number,
                                                    quote: Q.Quote
                                                    )
                                                 );
                    }
                }
            }

            CurrentTotalProgress += _DataSource.Quotes.Rows.Count;
            ProgressUpdate?.BeginInvoke(this, new(CurrentTotalProgress), null, null);

            LogWriter.WriteLog("Adding Webhooks data.");
            if (_DataSource.Discord.Count > 0)
            {
                foreach (DiscordRow W in _DataSource.Discord)
                {
                    if (!(from WH in context.Webhooks where W.Server == WH.Server && new Uri(W.Webhook) == WH.Webhook select WH).Any())
                    {
                        context.Webhooks.Add(new Webhooks(
                                                      isEnabled: W.IsEnabled,
                                                      webhooksSource: WebhooksSource.Discord,
                                                      server: W.Server,
                                                      kind: Enum.Parse<WebhooksKind>(W.Kind),
                                                      addEveryone: W.AddEveryone,
                                                      webhook: new(W.Webhook)
                                                      ));
                    }
                }
            }

            CurrentTotalProgress += _DataSource.Discord.Rows.Count;
            ProgressUpdate?.BeginInvoke(this, new(CurrentTotalProgress), null, null);

            LogWriter.WriteLog("Adding BanReasons data.");
            if (_DataSource.BanReasons.Count > 0)
            {
                foreach (BanReasonsRow BR in _DataSource.BanReasons)
                {
                    if (!(from B in context.BanReasons
                          where Enum.Parse<MsgTypes>(BR.MsgType) == B.MsgType && Enum.Parse<Enums.BanReasons>(BR.BanReason) == B.BanReason
                          select B).Any())
                    {
                        context.BanReasons.Add(new Models.BanReasons(
                                                        msgType: Enum.Parse<MsgTypes>(BR.MsgType),
                                                        banReason: Enum.Parse<Enums.BanReasons>(BR.BanReason)));
                    }
                }
            }

            CurrentTotalProgress += _DataSource.BanReasons.Rows.Count;
            ProgressUpdate?.BeginInvoke(this, new(CurrentTotalProgress), null, null);

            LogWriter.WriteLog("Adding BanRules data.");
            if (_DataSource.BanRules.Count > 0)
            {
                foreach (BanRulesRow BR in _DataSource.BanRules)
                {
                    if (!(from B in context.BanRules
                          where
                             Enum.Parse<ViewerTypes>(BR.ViewerTypes) == B.ViewerTypes &&
                             Enum.Parse<MsgTypes>(BR.MsgType) == B.MsgType &&
                             Enum.Parse<ModActions>(BR.ModAction) == B.ModAction
                          select B).Any())
                    {
                        context.BanRules.Add(new BanRules(
                                                      viewerTypes: Enum.Parse<ViewerTypes>(BR.ViewerTypes),
                                                      msgType: Enum.Parse<MsgTypes>(BR.MsgType),
                                                      modAction: Enum.Parse<ModActions>(BR.ModAction),
                                                      timeoutSeconds: Convert.ToInt32(BR.TimeoutSeconds)));
                    }
                }
            }

            CurrentTotalProgress += _DataSource.BanRules.Rows.Count;
            ProgressUpdate?.BeginInvoke(this, new(CurrentTotalProgress), null, null);

            LogWriter.WriteLog("Adding ModeratorApprove data.");
            if (_DataSource.ModeratorApprove.Count > 0)
            {
                foreach (ModeratorApproveRow MA in _DataSource.ModeratorApprove)
                {
                    if (!(from M in context.ModeratorApprove
                          where
                         MA.ModActionName == M.ModActionName
                         && Enum.Parse<ModActionType>(MA.ModActionType) == M.ModActionType
                         && MA.ModPerformAction == M.ModPerformAction
                         && Enum.Parse<ModPerformType>(MA.ModPerformType) == M.ModPerformType
                          select M).Any())
                    {
                        context.ModeratorApprove.Add(new ModeratorApprove(
                                                         isEnabled: MA.IsEnabled,
                                                         modActionType: Enum.Parse<ModActionType>(MA.ModActionType),
                                                         modActionName: MA.ModActionName,
                                                         modPerformAction: MA.ModPerformAction,
                                                         modPerformType: Enum.Parse<ModPerformType>(MA.ModPerformType)));
                    }
                }
            }

            CurrentTotalProgress += _DataSource.ModeratorApprove.Rows.Count;
            ProgressUpdate?.BeginInvoke(this, new(CurrentTotalProgress), null, null);

            LogWriter.WriteLog("Adding LearnMsgs data.");
            if (_DataSource.LearnMsgs.Count > 0)
            {
                foreach (LearnMsgsRow LM in _DataSource.LearnMsgs)
                {
                    if (!(from L in context.LearnMsgs where L.MsgType == Enum.Parse<MsgTypes>(LM.MsgType) && L.TeachingMsg == LM.TeachingMsg select L).Any())
                    {
                        context.LearnMsgs.Add(new LearnMsgs(
                                                        msgType: Enum.Parse<MsgTypes>(LM.MsgType),
                                                        teachingMsg: LM.TeachingMsg));
                    }
                }
            }

            CurrentTotalProgress += _DataSource.LearnMsgs.Rows.Count;
            ProgressUpdate?.BeginInvoke(this, new(CurrentTotalProgress), null, null);

#if DEBUG
            StreamWriter DebugCom = new("debug_commands.txt") { AutoFlush = true };
            DebugCom.WriteLine("Now adding Commands to database.");
#endif

            LogWriter.WriteLog("Adding Default Commands data.");
            foreach (CommandsRow C in (from C in _DataSource.Commands
                                       where Enum.GetNames<DefaultCommand>().Contains(C.CmdName) || Enum.GetNames<DefaultSocials>().Contains(C.CmdName)
                                       select C))
            {
                if (!(from DC in context.Commands where C.CmdName == DC.CmdName select DC).Any())
                {
#if DEBUG
                    DebugCom.WriteLine($"Now adding {C.CmdName}");
#endif   
                    context.Commands.Add(new Commands(
                cmdName: C.CmdName,
                                                  addMe: C.AddMe,
                                                  permission: Enum.Parse<ViewerTypes>(C.Permission),
                                                  isEnabled: C.IsEnabled,
                                                  message: C.Message,
                                                  repeatTimer: C.RepeatTimer,
                                                  sendMsgCount: C.SendMsgCount,
                                                  category: new List<string>(C.Category.Split(",")),
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
                }
                context.SaveChanges(true);
            }

            LogWriter.WriteLog("Adding User-defined Commands data.");

#if DEBUG
            DebugCom.WriteLine("Now adding User-Defined Commands to database.");
#endif

            foreach (CommandsRow C in (from C in _DataSource.Commands
                                       where !(Enum.GetNames<DefaultCommand>().Contains(C.CmdName) || Enum.GetNames<DefaultSocials>().Contains(C.CmdName))
                                       select C))
            {

                if (!(from CR in context.CommandsUser where C.CmdName == CR.CmdName select CR).Any())
                {
#if DEBUG
                    DebugCom.WriteLine($"Now adding {C.CmdName}");
#endif
                    context.CommandsUser.Add(new CommandsUser(
                                              cmdName: C.CmdName,
                                              addMe: C.AddMe,
                                              permission: Enum.Parse<ViewerTypes>(C.Permission),
                                              isEnabled: C.IsEnabled,
                                              message: C.Message,
                                              repeatTimer: C.RepeatTimer,
                                              sendMsgCount: C.SendMsgCount,
                                              category: new List<string>(C.Category.Split(",")),
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
                }
                context.SaveChanges(true);
            }


            CurrentTotalProgress += _DataSource.Commands.Rows.Count;
            ProgressUpdate?.BeginInvoke(this, new(CurrentTotalProgress), null, null);

            #endregion

            #region Adding Related Data
            #region Multi data

            LogWriter.WriteLog("Adding Multilive Channel data.");

            if (_MultiDataSource.Channels.Count > 0)
            {
                foreach (ChannelsRow U in _MultiDataSource.Channels)
                {
                    //UsersRow usersRow = (from R in _DataSource.Users
                    //                     where string.Equals(R.UserName, U.ChannelName, StringComparison.OrdinalIgnoreCase)
                    //                     select R).FirstOrDefault();

                    MultiChannels currUser = (from MC in context.MultiChannels where MC.UserId == U.UserId select MC).FirstOrDefault();

                    if (!(DBNull.Value.Equals(U["UserId"])) && (U.UserId != null) && currUser == null)
                    {
                        context.MultiChannels.Add(new(userId: U.UserId,
                                                   userName: U.ChannelName,
                                                   platform: Platform.Twitch));
                    }
                    else
                    {
                        LogWriter.WriteLog($"Found invalid or mismatched userId or userName, could not import: {ConvertDataRow(U, _MultiDataSource.Channels.Columns.Count)}");
                    }
                    context.SaveChanges(true);
                }
            }

            CurrentTotalProgress += _MultiDataSource.Channels.Rows.Count;
            ProgressUpdate?.BeginInvoke(this, new(CurrentTotalProgress), null, null);

            LogWriter.WriteLog("Adding Multilive SummaryStream data.");

            if (_MultiDataSource.SummaryLiveStream.Count > 0)
            {
                foreach (SummaryLiveStreamRow S in _MultiDataSource.SummaryLiveStream)
                {
                    if (!(from MS in context.MultiSummaryLiveStreams
                          where S.ChannelsRow.UserId == MS.UserId
                          select MS).Any())
                    {
                        var founddata = (from C in _MultiDataSource.LiveStream
                                         where C.ChannelName == S.ChannelName
                                         select C);

                        if (founddata.Any())
                        {
                            context.MultiSummaryLiveStreams.Add(new MultiSummaryLiveStreams(streamCount: S.StreamCount, throughDate: S.ThroughDate,
                                userId: founddata.First().UserId, platform: Platform.Twitch));
                        }
                        else
                        {
                            LogWriter.WriteLog($"Could not import summary data for row: {ConvertDataRow(S, _MultiDataSource.SummaryLiveStream.Columns.Count)}");
                        }
                    }
                }
            }

            CurrentTotalProgress += _MultiDataSource.SummaryLiveStream.Rows.Count;
            ProgressUpdate?.BeginInvoke(this, new(CurrentTotalProgress), null, null);

            // post multilive stream data

            LogWriter.WriteLog("Adding Multilive Livestream data.");

            if (_MultiDataSource.LiveStream.Count > 0)
            {
                foreach (LiveStreamRow L in _MultiDataSource.LiveStream)
                {
                    if (!(from LS in context.MultiLiveStreams
                          join MC in context.MultiChannels on LS.UserId equals MC.UserId
                          where MC.UserName == L.ChannelName && LS.LiveDate == L.LiveDate
                          select LS
                         ).Any())
                    {
                        var founddata = (from C in context.MultiChannels
                                         where C.UserId == L.UserId
                                         select C);

                        UsersRow usersRow = (from U in _DataSource.Users
                                             where string.Equals(U.UserName, L.ChannelName, StringComparison.OrdinalIgnoreCase)
                                             select U).FirstOrDefault();
                        MultiChannels channelsRow = (from C in context.MultiChannels
                                                     where C.UserId == L.UserId
                                                     select C).FirstOrDefault();

                        if (channelsRow != null) // (founddata.Any() || usersRow != null) && !DBNull.Value.Equals(usersRow.UserId) && !DBNull.Value.Equals(usersRow.UserName)) // only add if we can find user ID in multilive channel table
                        {
                            if (channelsRow.UserName != L.ChannelName)
                            {
                                channelsRow.UserName = L.ChannelName;
                            }
                           await  dataManagerSQL.PostMultiStreamDate(liveUser: new(userId: channelsRow.UserId, userName: channelsRow.UserName, botSource: Platform.Twitch), onDate: L.LiveDate);
                        }
                        else
                        {
                            LogWriter.WriteLog($"Found invalid or mismatched userId or userName, could not import: {ConvertDataRow(L, _MultiDataSource.LiveStream.Columns.Count)}");
                        }
                        context.SaveChanges(true);
                    }
                }
            }

            CurrentTotalProgress += _MultiDataSource.LiveStream.Rows.Count;
            ProgressUpdate?.BeginInvoke(this, new(CurrentTotalProgress), null, null);

            LogWriter.WriteLog("Adding Multilive Webhook data.");

            if (_MultiDataSource.MsgEndPoints.Count > 0)
            {
                foreach (MsgEndPointsRow E in _MultiDataSource.MsgEndPoints)
                {
                    {
                        if (!(from M in context.MultiWebhooks
                              where
                             M.Webhook == new Uri(E.URL)
                             && M.Server == E.Server
                             && M.WebhooksSource == Enum.Parse<WebhooksSource>(E.Type)
                              select M).Any())
                        {
                            context.MultiWebhooks.Add(new MultiWebhooks(
                                                                   isEnabled: E.IsEnabled,
                                                                   webhooksSource: Enum.Parse<WebhooksSource>(E.Type),
                                                                   webhook: new(E.URL),
                                                                   server: E.Server,
                                                                   kind: WebhooksKind.Live,
                                                                   addEveryone: false
                                                           )
                                                               );
                        }
                    }
                }
            }

            CurrentTotalProgress += _MultiDataSource.MsgEndPoints.Rows.Count;
            ProgressUpdate?.BeginInvoke(this, new(CurrentTotalProgress), null, null);

            #endregion

            #region User

            LogWriter.WriteLog("Adding Users and UserStats data.");

            if (_DataSource.Users.Count > 0)
            {
                foreach (UsersRow U in _DataSource.Users)
                {
                    if (!(DBNull.Value.Equals(U["UserId"])) && (U.UserId != null))
                    {
                        Users CurrUser = (from CU in context.Users where U.UserId == CU.UserId select CU).FirstOrDefault();

                        if (CurrUser != null)
                        {
                            if (CurrUser.FirstDateSeen < U.FirstDateSeen || CurrUser.Follower?.IsFollower == true)
                            {
                                CurrUser.UserName = U.UserName;
                                CurrUser.LastDateSeen = U.LastDateSeen;
                            }

                            if (!(from US in context.UserStats where US.UserId == CurrUser.UserId select US).Any())
                            {
                                context.UserStats.Add(new(watchTime: U.WatchTime, userId: U.UserId, platform: Platform.Twitch));
                            }
                            else if (CurrUser.UserStats != null)
                            {
                                CurrUser.UserStats.WatchTime += U.WatchTime; // don't bother with other user stats during import, these stats were not previously established 
                            }
                        }
                        else
                        {
                            context.Users.Add(new(firstDateSeen: U.FirstDateSeen,
                                                  currLoginDate: U.CurrLoginDate,
                                                  lastDateSeen: U.LastDateSeen,
                                                  userId: U.UserId,
                                                  userName: U.UserName,
                                                  platform: Platform.Twitch));

                            context.UserStats.Add(new(watchTime: U.WatchTime, userId: U.UserId, platform: Platform.Twitch));
                        }
                    }
                    else
                    {
                        LogWriter.WriteLog($"Could not import users row: {ConvertDataRow(U, _DataSource.Users.Columns.Count)}");
                    }

                    context.SaveChanges(true);
                }
            }

            LogWriter.WriteLog("Adding InRaid data.");

            if (_DataSource.InRaidData.Count > 0)
            {
                foreach (InRaidDataRow A in from IR in _DataSource.InRaidData
                                            select IR)
                {
                    string uId = ((UsersRow)_DataSource.Users.Select($"[UserName]='{A.UserName}'").FirstOrDefault())?.UserId;
                    string categoryId = ((CategoryListRow)_DataSource.CategoryList.Select($"{_DataSource.CategoryList.CategoryColumn.ColumnName}='{A.Category}'").FirstOrDefault()).CategoryId;

                    if (string.IsNullOrEmpty(uId) || string.IsNullOrEmpty(categoryId))
                    {
                        LogWriter.WriteLog($"Did not import for null user Id: {ConvertDataRow(A, _DataSource.InRaidData.Columns.Count)}");
                    }
                    else if (!(from I in context.InRaidData where I.UserId == uId && I.RaidDate == A.DateTime select I).Any())
                    {
                       await  dataManagerSQL.PostInRaidData(new(A.UserName, Platform.Twitch, uId), A.DateTime, Convert.ToInt32(A.ViewerCount), new(categoryId, A.Category));
                    }
                }
            }

            CurrentTotalProgress += _DataSource.InRaidData.Rows.Count;
            ProgressUpdate?.BeginInvoke(this, new(CurrentTotalProgress), null, null);

            CurrentTotalProgress += _DataSource.Users.Rows.Count;
            ProgressUpdate?.BeginInvoke(this, new(CurrentTotalProgress), null, null);

            LogWriter.WriteLog("Adding CustomWelcome data.");

            if (_DataSource.CustomWelcome.Count > 0)
            {
                foreach (CustomWelcomeRow CW in _DataSource.CustomWelcome)
                {
                    UsersRow usersRow = ((UsersRow)_DataSource.Users.Select($"[UserName]='{CW.UserName}'").FirstOrDefault());
                    if (usersRow != null && !string.IsNullOrEmpty(usersRow.UserId) &&
                        !(from C in context.CustomWelcome where C.UserId == usersRow.UserId select C).Any())
                    {
                        context.CustomWelcome.Add(new(message: CW.Message,
                                                   userId: usersRow.UserId,
                                                   platform: Platform.Twitch));
                    }
                    else
                    {
                        LogWriter.WriteLog($"Could not import row: {ConvertDataRow(CW, _DataSource.CustomWelcome.Columns.Count)}");
                    }
                }
            }

            CurrentTotalProgress += _DataSource.CustomWelcome.Rows.Count;
            ProgressUpdate?.BeginInvoke(this, new(CurrentTotalProgress), null, null);

            #region Currency

            LogWriter.WriteLog("Adding CurrencyType data.");

            if (_DataSource.CurrencyType.Count > 0)
            {
                foreach (CurrencyTypeRow CT in _DataSource.CurrencyType)
                {
                    if (!(from C in context.CurrencyType where C.CurrencyName == CT.CurrencyName select C).Any())
                    {
                        context.CurrencyType.Add(new Models.CurrencyType(
                                                        accrueAmt: CT.AccrueAmt,
                                                        seconds: (int)CT.Seconds,
                                                        maxValue: CT.MaxValue,
                                                        currencyName: CT.CurrencyName
                                                       ));
                    }
                }
            }

            CurrentTotalProgress += _DataSource.CurrencyType.Rows.Count;
            ProgressUpdate?.BeginInvoke(this, new(CurrentTotalProgress), null, null);

            LogWriter.WriteLog("Adding user Currency data.");

            if (_DataSource.Currency.Count > 0)
            {
                foreach (CurrencyRow C in _DataSource.Currency)
                {
                    if (C.UsersRow != null && !string.IsNullOrEmpty(C.UsersRow.UserId))
                    {
                        Users CurrUser = (from U in context.Users where C.UsersRow.UserId == U.UserId select U).FirstOrDefault();
                        Currency CurrCurrency = (from CU in context.Currency where CU.UserId == CurrUser.UserId select CU).FirstOrDefault();

                        if (CurrCurrency == null)
                        {
                            context.Currency.Add(new Currency(CurrUser.UserId, Platform.Twitch, (!DBNull.Value.Equals(C["Value"]) && !string.IsNullOrEmpty(C.Value.ToString())) ? C.Value : 0, C.CurrencyName));
                        }
                        else if (CurrCurrency.Value != C.Value)
                        {
                            CurrCurrency.Value += C.Value;
                        }
                    }
                    else
                    {
                        LogWriter.WriteLog($"Could not import row: {ConvertDataRow(C, _DataSource.Currency.Columns.Count)}");
                    }
                    context.SaveChanges(true);
                }
            }

            CurrentTotalProgress += _DataSource.Currency.Rows.Count;
            ProgressUpdate?.BeginInvoke(this, new(CurrentTotalProgress), null, null);

            #endregion

            #region Category & GameDeadCounter & Clips & Quotes & Overlay Data & Webhooks & StreamStats

            LogWriter.WriteLog("Adding CategoryList data.");

            if (_DataSource.CategoryList.Count > 0)
            {
                foreach (CategoryListRow CL in _DataSource.CategoryList)
                {
                    if (!(from C in context.CategoryList
                          where
                         (!DBNull.Value.Equals(CL["CategoryId"]) && C.CategoryId == CL.CategoryId)
                         || (!DBNull.Value.Equals(CL["Category"]) && C.Category == CL.Category)
                          select C).Any())
                    {
                        context.CategoryList.Add(new CategoryList(
                                                     categoryId: CL.Category == "All" ? "0" : string.IsNullOrEmpty(CL.CategoryId) ? "missing" : CL.CategoryId,
                                                     category: CL.Category,
                                                     streamCount: CL.StreamCount)
                                                          );
                    }
                }
            }

            CurrentTotalProgress += _DataSource.CategoryList.Rows.Count;
            ProgressUpdate?.BeginInvoke(this, new(CurrentTotalProgress), null, null);

            LogWriter.WriteLog("Adding GameDeadCounter data.");

            if (_DataSource.GameDeadCounter.Count > 0)
            {
                foreach (GameDeadCounterRow GDC in _DataSource.GameDeadCounter)
                {
                    string CI = ((CategoryListRow)(_DataSource.CategoryList.Select($"[Category] = '{GDC.Category}'")).First()).CategoryId;
                    var GameDC = (from G in context.GameDeadCounter where G.CategoryId == CI select G);
                    if (!GameDC.Any())
                    {
                        context.GameDeadCounter.Add(new GameDeadCounter(
                                                             categoryId: CI,
                                                             category: GDC.Category,
                                                             counter: GDC.Counter
                                                         ));
                    }
                }
            }

            CurrentTotalProgress += _DataSource.GameDeadCounter.Rows.Count;
            ProgressUpdate?.BeginInvoke(this, new(CurrentTotalProgress), null, null);

            LogWriter.WriteLog("Adding StreamStats data.");

            if (_DataSource.StreamStats.Count > 0)
            {
                foreach (StreamStatsRow S in _DataSource.StreamStats)
                {
                    if (!(from SS in context.StreamStats where S.StreamStart == SS.StreamStart && S.StreamEnd == SS.StreamEnd select SS).Any())
                    {
                        context.StreamStats.Add(new StreamStats(
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
                                                         commandMsgs: S.Commands,
                                                         automatedEvents: S.AutomatedEvents,
                                                         automatedCommands: S.AutomatedCommands,
                                                         webhookMsgs: S.DiscordMsgs,
                                                         clipsMade: S.ClipsMade,
                                                         channelPtCount: S.ChannelPtCount,
                                                         channelChallenge: S.ChannelChallenge,
                                                         maxUsers: S.MaxUsers
                                                         ));
                    }
                }
            }

            CurrentTotalProgress += _DataSource.StreamStats.Rows.Count;
            ProgressUpdate?.BeginInvoke(this, new(CurrentTotalProgress), null, null);

            #endregion

#if DEBUG
            StreamWriter DebugFollow = new("debug_followers.txt") { AutoFlush = true };

#endif
            LogWriter.WriteLog("Adding Followers data.");

            if (_DataSource.Followers.Count > 0)
            {
                foreach (FollowersRow F in (from FR in _DataSource.Followers
                                            orderby FR.StatusChangeDate descending
                                            select FR)
                                            )
                {
                    if (F.IsFollower)
                    {
                        Users currUser = (from U in context.Users where U.UserId == F.UserId && U.Platform == Enum.Parse<Platform>(F.Platform) select U).FirstOrDefault();
                        if (currUser.UserName != F.UserName)
                        {
                            currUser.UserName = F.UserName;
                        }
                    }
                    if (!(DBNull.Value.Equals(F["UserId"])) && !DBNull.Value.Equals(F["Platform"]) && !string.IsNullOrEmpty(F.UserId))
                    {
                        if (F.IsFollower && !(from CF in context.Followers where F.UserId == CF.UserId && CF.User.UserName == F.UserName select CF).Any())
                        {
                            context.Followers.Add(new(
                                                    isFollower: F.IsFollower,
                                                    followedDate: F.FollowedDate,
                                                    statusChangeDate: F.StatusChangeDate,
                                                    category: (DBNull.Value.Equals(F["Category"]) || F.Category == "N/A" || F.Category == "") ? "All" : F.Category,
                                                    addDate: F.FollowedDate,
                                                    userId: F.UserId,
                                                    platform: Enum.Parse<Platform>(F.Platform)
                                                ));
#if DEBUG
                            DebugFollow.WriteLine($"Now added, {F.UserId} {F.UserName} to context.Followers.");
#endif
                        }
                        else if (!(from OF in context.OldFollowUsers where OF.UserId == F.UserId && OF.UserName == F.UserName select OF).Any())
                        {
                            context.OldFollowUsers.Add(new(
                                                        isFollower: F.IsFollower,
                                                        followedDate: F.FollowedDate,
                                                        statusChangeDate: F.StatusChangeDate,
                                                        category: (DBNull.Value.Equals(F["Category"]) || F.Category == "N/A" || F.Category == "") ? "All" : F.Category,
                                                        addDate: F.FollowedDate,
                                                        userId: F.UserId,
                                                        userName: F.UserName,
                                                        platform: Enum.Parse<Platform>(F.Platform)
                                                ));
#if DEBUG
                            DebugFollow.WriteLine($"Now added, {F.UserId} {F.UserName} to context.OldFollowUsers.");
#endif
                        }
                        context.SaveChanges(true);
                    }
                    else
                    {
                        LogWriter.WriteLog($"Could not import data row: {ConvertDataRow(F, _DataSource.Followers.Columns.Count)}");
                    }
                }
            }

            context.SaveChanges(true);

            CurrentTotalProgress += _DataSource.Followers.Rows.Count;
            ProgressUpdate?.BeginInvoke(this, new(CurrentTotalProgress), null, null);

            LogWriter.WriteLog("Adding GiveawayUser data.");

            if (_DataSource.GiveawayUserData.Count > 0)
            {
                foreach (GiveawayUserDataRow G in _DataSource.GiveawayUserData)
                {
                    string uId = ((UsersRow)_DataSource.Users.Select($"[UserName]='{G.DisplayName}'").First()).UserId;
                    if (!(from GUD in context.GiveawayUserData where G.DateTime == GUD.DateTime && GUD.UserId == uId select GUD).Any())
                    {
                        context.GiveawayUserData.AddRange(new GiveawayUserData(
                                                              dateTime: G.DateTime,
                                                              userId: uId,
                                                              platform: Platform.Twitch
                                                              ));
                    }
                }
            }

            CurrentTotalProgress += _DataSource.GiveawayUserData.Rows.Count;
            ProgressUpdate?.BeginInvoke(this, new(CurrentTotalProgress), null, null);

            LogWriter.WriteLog("Adding ShoutOuts data.");

#if DEBUG
            StreamWriter DebugShoutOuts = new("debug_shoutouts.txt") { AutoFlush = true };
#endif
            if (_DataSource.ShoutOuts.Count > 0)
            {
                foreach (ShoutOutsRow S in _DataSource.ShoutOuts)
                {
#if DEBUG

                    DebugShoutOuts.WriteLine($"Now adding row {{DBNull.Value.Equals(S[\"UserId\"]) ? \"Null\" : S.UserId}} {S.UserName}");
#endif
                    if (!DBNull.Value.Equals(S["UserId"]))
                    {
                        UsersRow currUser = (from U in _DataSource.Users
                                             where string.Equals(U.UserName, S.UserName, StringComparison.OrdinalIgnoreCase)
                                             select U).FirstOrDefault();
                        if (currUser != null)
                        {
                            await dataManagerSQL.PostNewAutoShoutUser(currUser.UserId, Platform.Twitch);
                        }
                        else
                        {
                            LogWriter.WriteLog($"Could not import data row: {ConvertDataRow(S, _DataSource.ShoutOuts.Columns.Count)}");
                        }
                    }
                }
            }

            CurrentTotalProgress += _DataSource.ShoutOuts.Rows.Count;
            ProgressUpdate?.BeginInvoke(this, new(CurrentTotalProgress), null, null);

            #endregion

            #endregion

            ImportCompleted?.Invoke(this, new());

        }

        private static string ConvertDataRow(DataRow data, int maxcolumns)
        {
            List<string> cols = [];

            for (int i = 0; i < maxcolumns; i++)
            {
                cols.Add(data[i].ToString());
            }

            return string.Join(' ', cols);
        }
    }
}
