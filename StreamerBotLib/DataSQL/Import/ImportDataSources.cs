using StreamerBotLib.DataSQL.Models;
using StreamerBotLib.Enums;
using StreamerBotLib.Events;
using StreamerBotLib.GUI;
using StreamerBotLib.GUI.Windows;
using StreamerBotLib.Overlay.Enums;
using StreamerBotLib.Static;

using System.Data;
using System.IO;
using System.Reflection;
using System.Xml;

using static StreamerBotLib.DataSQL.Import.DataSource;
using static StreamerBotLib.DataSQL.Import.Multi.DataSource;

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

        private event EventHandler<ImportDataProgressUpdateEventArgs> ProgressUpdate;
        private event EventHandler<EventArgs> ImportCompleted;

        /// <summary>
        /// Converts data from the primary database or the multilive database files from the XML Datagram used in the prior bot version
        /// </summary>
        /// <param name="context">The new Database context to use for importing data.</param>
        /// <param name="dataManagerSQL">To access already built methods for entering data into the database using the application data flow.</param>
        public void ConvertData(SQLDBContext context, DataManagerSQL dataManagerSQL)
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
            //MultiMsgEndPoints
            //MultiSummaryLiveStreams
            */

            #endregion

            #region Add Non-Related Data

            LogWriter.WriteLog("Adding Channel Events data.");

            if (_DataSource.ChannelEvents.Count > 0)
            {
                context.ChannelEvents.AddRange(from CE in _DataSource.ChannelEvents
                                               select new ChannelEvents(
                                                   name: Enum.Parse<ChannelEventActions>(CE.Name),
                                                   repeatMsg: CE.RepeatMsg,
                                                   addMe: CE.AddMe,
                                                   isEnabled: CE.IsEnabled,
                                                   message: CE.Message,
                                                   commands: CE.Commands
                                                   ));
            }

            CurrentTotalProgress += _DataSource.ChannelEvents.Rows.Count;
            ProgressUpdate?.Invoke(this, new(CurrentTotalProgress));

            LogWriter.WriteLog("Adding InRaid data.");

            if (_DataSource.InRaidData.Count > 0)
            {
                foreach (InRaidDataRow A in from IR in _DataSource.InRaidData
                                            select IR)
                {
                    string uId = ((UsersRow)_DataSource.Users.Select($"[UserName]='{A.UserName}'").FirstOrDefault())?.UserId;

                    if (string.IsNullOrEmpty(uId))
                    {
                        LogWriter.WriteLog($"Did not import for null user Id: {ConvertDataRow(A, _DataSource.InRaidData.Columns.Count)}");
                    }
                    else
                    {
                        dataManagerSQL.PostInRaidData(new(A.UserName, Platform.Twitch, uId), A.DateTime, Convert.ToInt32(A.ViewerCount), A.Category);
                    }
                }
            }

            CurrentTotalProgress += _DataSource.InRaidData.Rows.Count;
            ProgressUpdate?.Invoke(this, new(CurrentTotalProgress));


            LogWriter.WriteLog("Adding OutRaid data.");
            if (_DataSource.OutRaidData.Count > 0)
            {
                context.OutRaidData.AddRange(from OR in _DataSource.OutRaidData
                                             select new OutRaidData(
                                                 channelRaided: OR.ChannelRaided,
                                                 raidDate: OR.DateTime
                                                 ));
            }

            CurrentTotalProgress += _DataSource.OutRaidData.Rows.Count;
            ProgressUpdate?.Invoke(this, new(CurrentTotalProgress));


            LogWriter.WriteLog("Adding clips data.");
            if (_DataSource.Clips.Count > 0)
            {
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
            }

            CurrentTotalProgress += _DataSource.Clips.Rows.Count;
            ProgressUpdate?.Invoke(this, new(CurrentTotalProgress));


            LogWriter.WriteLog("Adding OverlayServices data.");
            if (_DataSource.OverlayServices.Count > 0)
            {
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
            }

            CurrentTotalProgress += _DataSource.OverlayServices.Rows.Count;
            ProgressUpdate?.Invoke(this, new(CurrentTotalProgress));

            LogWriter.WriteLog("Adding OverlayTicker data.");
            if (_DataSource.OverlayTicker.Count > 0)
            {
                context.OverlayTicker.AddRange(from OT in _DataSource.OverlayTicker
                                               select new OverlayTicker(
                                                   tickerName: Enum.Parse<OverlayTickerItem>(OT.TickerName),
                                                   userName: OT.UserName
                                                   ));
            }

            CurrentTotalProgress += _DataSource.OverlayTicker.Rows.Count;
            ProgressUpdate?.Invoke(this, new(CurrentTotalProgress));

            LogWriter.WriteLog("Adding Quotes data.");
            if (_DataSource.Quotes.Count > 0)
            {
                context.Quotes.AddRange(from Q in _DataSource.Quotes
                                        select new Quotes(
                                            number: Q.Number,
                                            quote: Q.Quote
                                            )
                                         );
            }

            CurrentTotalProgress += _DataSource.Quotes.Rows.Count;
            ProgressUpdate?.Invoke(this, new(CurrentTotalProgress));

            LogWriter.WriteLog("Adding Webhooks data.");
            if (_DataSource.Discord.Count > 0)
            {
                context.Webhooks.AddRange(from W in _DataSource.Discord
                                          select new Webhooks(
                                              isEnabled: W.IsEnabled,
                                              webhooksSource: WebhooksSource.Discord,
                                              server: W.Server,
                                              kind: Enum.Parse<WebhooksKind>(W.Kind),
                                              addEveryone: W.AddEveryone,
                                              webhook: new(W.Webhook)
                                              ));
            }

            CurrentTotalProgress += _DataSource.Discord.Rows.Count;
            ProgressUpdate?.Invoke(this, new(CurrentTotalProgress));

            LogWriter.WriteLog("Adding BanReasons data.");
            if (_DataSource.BanReasons.Count > 0)
            {
                context.BanReasons.AddRange(from BR in _DataSource.BanReasons
                                            select new Models.BanReasons(
                                                msgType: Enum.Parse<MsgTypes>(BR.MsgType),
                                                banReason: Enum.Parse<Enums.BanReasons>(BR.BanReason)));
            }

            CurrentTotalProgress += _DataSource.BanReasons.Rows.Count;
            ProgressUpdate?.Invoke(this, new(CurrentTotalProgress));

            LogWriter.WriteLog("Adding BanRules data.");
            if (_DataSource.BanRules.Count > 0)
            {
                context.BanRules.AddRange(from BR in _DataSource.BanRules
                                          select new BanRules(
                                              viewerTypes: Enum.Parse<ViewerTypes>(BR.ViewerTypes),
                                              msgType: Enum.Parse<MsgTypes>(BR.MsgType),
                                              modAction: Enum.Parse<ModActions>(BR.ModAction),
                                              timeoutSeconds: Convert.ToInt32(BR.TimeoutSeconds)));
            }

            CurrentTotalProgress += _DataSource.BanRules.Rows.Count;
            ProgressUpdate?.Invoke(this, new(CurrentTotalProgress));

            LogWriter.WriteLog("Adding ModeratorApprove data.");
            if (_DataSource.ModeratorApprove.Count > 0)
            {
                context.ModeratorApprove.AddRange(from MA in _DataSource.ModeratorApprove
                                                  select new ModeratorApprove(
                                                      isEnabled: MA.IsEnabled,
                                                      modActionType: Enum.Parse<ModActionType>(MA.ModActionType),
                                                      modActionName: MA.ModActionName,
                                                      modPerformAction: MA.ModPerformAction,
                                                      modPerformType: Enum.Parse<ModPerformType>(MA.ModPerformType)));
            }

            CurrentTotalProgress += _DataSource.ModeratorApprove.Rows.Count;
            ProgressUpdate?.Invoke(this, new(CurrentTotalProgress));

            LogWriter.WriteLog("Adding LearnMsgs data.");
            if (_DataSource.LearnMsgs.Count > 0)
            {
                context.LearnMsgs.AddRange(from LM in _DataSource.LearnMsgs
                                           select new LearnMsgs(
                                                msgType: Enum.Parse<MsgTypes>(LM.MsgType),
                                                teachingMsg: LM.TeachingMsg));
            }

            CurrentTotalProgress += _DataSource.LearnMsgs.Rows.Count;
            ProgressUpdate?.Invoke(this, new(CurrentTotalProgress));

            LogWriter.WriteLog("Adding Default Commands data.");
            if ((from C in _DataSource.Commands
                 where Enum.GetNames<DefaultCommand>().Contains(C.CmdName)
                 select C).Count() > 0)
            {
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
            }

            LogWriter.WriteLog("Adding User-defined Commands data.");
            if ((from C in _DataSource.Commands
                 where !Enum.GetNames<DefaultCommand>().Contains(C.CmdName)
                 select C).Count() > 0)
            {
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
            }

            CurrentTotalProgress += _DataSource.Commands.Rows.Count;
            ProgressUpdate?.Invoke(this, new(CurrentTotalProgress));


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

                    if (!(DBNull.Value.Equals(U.UserId)) && (U.UserId != null) && currUser == null)
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
            ProgressUpdate?.Invoke(this, new(CurrentTotalProgress));

            // post multilive stream data

            //StreamWriter multilive = new("debug_MultiLiveStream.txt") { AutoFlush = true };

            LogWriter.WriteLog("Adding Multilive Livestream data.");

            //List<MultiChannels> multiChannels = [];
            //List<MultiLiveStreams> multiLiveStreams = [];

            if (_MultiDataSource.LiveStream.Count > 0)
            {
                foreach (LiveStreamRow L in _MultiDataSource.LiveStream)
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

                    //if (channelsRow != null) // && !string.Equals(channelsRow.UserName, L.ChannelName, StringComparison.OrdinalIgnoreCase))
                    //{
                    //    if(channelsRow.UserName != L.ChannelName)
                    //    {
                    //        channelsRow.UserName = L.ChannelName;
                    //    }
                    //    //MultiChannels currChannel = new(L.UserId, L.ChannelName, Platform.Twitch);
                    //    MultiLiveStreams currMultiLive = new(userId: usersRow.UserId, platform: Platform.Twitch, liveDate: L.LiveDate);
                    //    //multiChannels.UniqueAdd(currChannel, currChannel);
                    //    //multiLiveStreams.UniqueAdd(currMultiLive, currMultiLive);
                    //}
                    if (channelsRow != null) // (founddata.Any() || usersRow != null) && !DBNull.Value.Equals(usersRow.UserId) && !DBNull.Value.Equals(usersRow.UserName)) // only add if we can find user ID in multilive channel table
                    {
                        if (channelsRow.UserName != L.ChannelName)
                        {
                            channelsRow.UserName = L.ChannelName;
                        }
                        //multilive.WriteLine(ConvertDataRow(L, _MultiDataSource.LiveStream.Columns.Count));
                        dataManagerSQL.PostMultiStreamDate(userid: channelsRow.UserId, username: channelsRow.UserName, platform: Platform.Twitch, onDate: L.LiveDate);
                    }
                    else
                    {
                        LogWriter.WriteLog($"Found invalid or mismatched userId or userName, could not import: {ConvertDataRow(L, _MultiDataSource.LiveStream.Columns.Count)}");
                    }
                    context.SaveChanges(true);
                }
            }

            //if (multiChannels.Count > 0) { context.MultiChannels.AddRange(multiChannels); }
            //if (multiLiveStreams.Count > 0) { context.MultiLiveStreams.AddRange(multiLiveStreams); }

            CurrentTotalProgress += _MultiDataSource.LiveStream.Rows.Count;
            ProgressUpdate?.Invoke(this, new(CurrentTotalProgress));

            LogWriter.WriteLog("Adding Multilive Webhook data.");

            if (_MultiDataSource.MsgEndPoints.Count > 0)
            {
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
            }

            CurrentTotalProgress += _MultiDataSource.MsgEndPoints.Rows.Count;
            ProgressUpdate?.Invoke(this, new(CurrentTotalProgress));

            LogWriter.WriteLog("Adding Multilive SummaryStream data.");

            if (_MultiDataSource.SummaryLiveStream.Count > 0)
            {
                foreach (SummaryLiveStreamRow S in _MultiDataSource.SummaryLiveStream)
                {
                    var founddata = (from C in context.MultiChannels
                                     where C.UserName == S.ChannelName
                                     select C);

                    //UsersRow usersRow = (from U in _DataSource.Users
                    //                     where string.Equals(U.UserName, S.ChannelName, StringComparison.OrdinalIgnoreCase)
                    //                     select U).FirstOrDefault();

                    if (founddata.Any() )
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

            CurrentTotalProgress += _MultiDataSource.SummaryLiveStream.Rows.Count;
            ProgressUpdate?.Invoke(this, new(CurrentTotalProgress));

            #endregion


            #region User

            LogWriter.WriteLog("Adding Users and UserStats data.");

            if (_DataSource.Users.Count > 0)
            { 
                foreach (UsersRow U in _DataSource.Users)
                {
                    if (!(DBNull.Value.Equals(U.UserId)) && (U.UserId != null))
                    {
                        Users CurrUser = (from CU in context.Users where U.UserId == CU.UserId select CU).FirstOrDefault();

                        if (CurrUser != null)
                        {
                            if (CurrUser.FirstDateSeen < U.FirstDateSeen)
                            {
                                CurrUser.UserName = U.UserName;
                                CurrUser.LastDateSeen = U.LastDateSeen;
                            }

                            CurrUser.UserStats.WatchTime += U.WatchTime; // don't bother with other user stats during import, these stats were not previously established 
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


            CurrentTotalProgress += _DataSource.Users.Rows.Count;
            ProgressUpdate?.Invoke(this, new(CurrentTotalProgress));

            LogWriter.WriteLog("Adding CustomWelcome data.");

            if (_DataSource.CustomWelcome.Count > 0)
            {
                foreach (CustomWelcomeRow CW in _DataSource.CustomWelcome)
                {
                    UsersRow usersRow = ((UsersRow)_DataSource.Users.Select($"[UserName]='{CW.UserName}'").FirstOrDefault());
                    if (usersRow != null && !string.IsNullOrEmpty(usersRow.UserId))
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
            ProgressUpdate?.Invoke(this, new(CurrentTotalProgress));

            #region Currency

            LogWriter.WriteLog("Adding CurrencyType data.");

            if (_DataSource.CurrencyType.Count > 0)
            {
                context.CurrencyType.AddRange(from CT in _DataSource.CurrencyType
                                              select new Models.CurrencyType(
                                                accrueAmt: CT.AccrueAmt,
                                                seconds: (int)CT.Seconds,
                                                maxValue: CT.MaxValue,
                                                currencyName: CT.CurrencyName
                                               ));
            }

            CurrentTotalProgress += _DataSource.CurrencyType.Rows.Count;
            ProgressUpdate?.Invoke(this, new(CurrentTotalProgress));

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
                            context.Currency.Add(new Currency(CurrUser.UserId, Platform.Twitch, (!DBNull.Value.Equals(C.Value) && !string.IsNullOrEmpty(C.Value.ToString())) ? C.Value : 0, C.CurrencyName));
                        }
                        else
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
            ProgressUpdate?.Invoke(this, new(CurrentTotalProgress));

            #endregion

            #region Category & GameDeadCounter & Clips & Quotes & Overlay Data & Webhooks & StreamStats

            LogWriter.WriteLog("Adding CategoryList data.");

            if (_DataSource.CategoryList.Count > 0)
            {
                context.CategoryList.AddRange(from CL in _DataSource.CategoryList
                                              where (CL.CategoryId != null) || (CL.Category == "All")
                                              select new CategoryList(
                                                  categoryId: CL.Category == "All" ? "0" : string.IsNullOrEmpty(CL.CategoryId) ? "missing" : CL.CategoryId,
                                                  category: CL.Category,
                                                  streamCount: CL.StreamCount)
                                                  );
            }

            CurrentTotalProgress += _DataSource.CategoryList.Rows.Count;
            ProgressUpdate?.Invoke(this, new(CurrentTotalProgress));

            LogWriter.WriteLog("Adding GameDeadCounter data.");

            if (_DataSource.GameDeadCounter.Count > 0)
            {
                context.GameDeadCounter.AddRange(from GDC in _DataSource.GameDeadCounter
                                                 let CI = ((CategoryListRow)(_DataSource.CategoryList.Select($"[Category] = '{GDC.Category}'")).First()).CategoryId
                                                 select new GameDeadCounter(
                                                     categoryId: CI,
                                                     category: GDC.Category,
                                                     counter: GDC.Counter
                                                 ));
            }

            CurrentTotalProgress += _DataSource.GameDeadCounter.Rows.Count;
            ProgressUpdate?.Invoke(this, new(CurrentTotalProgress));

            LogWriter.WriteLog("Adding StreamStats data.");

            if (_DataSource.StreamStats.Count > 0)
            {
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
            }

            CurrentTotalProgress += _DataSource.StreamStats.Rows.Count;
            ProgressUpdate?.Invoke(this, new(CurrentTotalProgress));

            #endregion


            LogWriter.WriteLog("Adding Followers data.");

            if (_DataSource.Followers.Count > 0)
            {
                context.Followers.AddRange(from F in _DataSource.Followers
                                           where !(DBNull.Value.Equals(F.UserId)) && (F.UserId != null) && (!string.IsNullOrEmpty(F.Category))
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
            }

            CurrentTotalProgress += _DataSource.Followers.Rows.Count;
            ProgressUpdate?.Invoke(this, new(CurrentTotalProgress));

            LogWriter.WriteLog("Adding GiveawayUser data.");

            if (_DataSource.GiveawayUserData.Count > 0)
            {
                context.GiveawayUserData.AddRange(from G in _DataSource.GiveawayUserData
                                                  let uId = ((UsersRow)_DataSource.Users.Select($"[UserName]='{G.DisplayName}'").First()).UserId
                                                  where uId is not null
                                                  select new GiveawayUserData(
                                                      dateTime: G.DateTime,
                                                      userId: uId,
                                                      platform: Platform.Twitch
                                                      ));
            }

            CurrentTotalProgress += _DataSource.GiveawayUserData.Rows.Count;
            ProgressUpdate?.Invoke(this, new(CurrentTotalProgress));

            LogWriter.WriteLog("Adding ShoutOuts data.");

            if (_DataSource.ShoutOuts.Count > 0)
            {
                foreach (ShoutOutsRow S in _DataSource.ShoutOuts)
                {
                    UsersRow usersRow = (from U in _DataSource.Users
                                         where string.Equals(U.UserName, S.UserName, StringComparison.OrdinalIgnoreCase)
                                         select U).FirstOrDefault();

                    if (usersRow == null || DBNull.Value.Equals(usersRow.UserId) || string.IsNullOrEmpty(usersRow.UserId))
                    {
                        LogWriter.WriteLog($"Could not import data row: {ConvertDataRow(S, _DataSource.ShoutOuts.Columns.Count)}");
                    }
                    else
                    {
                        string username = S.UserName;
                        if (usersRow.UserId == S.UserId && !string.Equals(usersRow.UserName, S.UserName, StringComparison.Ordinal))
                        {
                            username = usersRow.UserName;
                        }

                        dataManagerSQL.PostNewAutoShoutUser(usersRow.UserId, Platform.Twitch);
                    }
                }
            }

            CurrentTotalProgress += _DataSource.ShoutOuts.Rows.Count;
            ProgressUpdate?.Invoke(this, new(CurrentTotalProgress));

            #endregion

            #endregion

            ImportCompleted?.Invoke(this, new());

        }

        private string ConvertDataRow(DataRow data, int maxcolumns)
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
