#if DEBUG
#define noLogDataManager_Actions
#endif

using StreamerBotLib.Enums;
using StreamerBotLib.GUI;
using StreamerBotLib.Models;
using StreamerBotLib.Static;
using StreamerBotLib.Systems;

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Xml;

using static StreamerBotLib.Data.DataSource;

namespace StreamerBotLib.Data
{
    public partial class DataManager
    {
        private readonly Queue<Task> SaveTasks = new();
        private bool SaveThreadStarted = false;
        private const int SaveThreadWait = 10000;

        private int BackupSaveToken = 0;
        private const int BackupSaveIntervalMins = 15;
        private const int BackupHrInterval = 60 / BackupSaveIntervalMins;
        private readonly string BackupDataFileXML = $"Backup_{DataFileXML}";

        #region Load and Exit Ops

        /// <summary>
        /// Provide an internal notification event to save the data outside of any multi-threading mechanisms.
        /// </summary>
        public event EventHandler OnSaveData;

        /// <summary>
        /// Load the data source and populate with default data; if regular data source is corrupted, attempt to load backup data.
        /// </summary>
        private void LoadData()
        {
#if LogDataManager_Actions
            LogWriter.DataActionLog(MethodBase.GetCurrentMethod().Name, $"Loading the database.");
#endif

            void LoadFile(string filename)
            {
                lock(GUIDataManagerLock.Lock)
                {
                    if (!File.Exists(filename))
                    {
                        _DataSource.WriteXml(filename);
                    }

                    using XmlReader xmlreader = new XmlTextReader(filename);
                    _ = _DataSource.ReadXml(xmlreader, XmlReadMode.DiffGram);

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

            foreach (DataTable table in _DataSource.Tables)
            {
                table.BeginLoadData();
            }

            try // try to catch any exception when loading the backup working file, incase there's an issue loading the backup file
            {
                try // try the regular working file
                {
                    LoadFile(DataFileName);
                }
                catch (Exception ex) // catch if exception loading the data file, e.g. file corrupted from system crash
                {
                    LogWriter.LogException(ex, MethodBase.GetCurrentMethod().Name);
                    File.Copy(DataFileName, $"Failed_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}_{Path.GetFileName(DataFileName)}");
                    LoadFile(BackupDataFileXML);
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex, MethodBase.GetCurrentMethod().Name);
            }

            foreach (DataTable table in _DataSource.Tables)
            {
                table.EndLoadData();
            }

            SaveData(this, new());
        }

        public void NotifySaveData()
        {
#if LogDataManager_Actions
            LogWriter.DataActionLog(MethodBase.GetCurrentMethod().Name, $"Notify the database is saved.");
#endif

            OnSaveData?.Invoke(this, new());
        }

        /// <summary>
        /// Save data to file upon exit and after data changes. Pauses for 15 seconds (unless exiting) to slow down multiple saves in a short time.
        /// </summary>
        public void SaveData(object sender, EventArgs e)
        {
#if LogDataManager_Actions
            LogWriter.DataActionLog(MethodBase.GetCurrentMethod().Name, $"Saving the database, managed as multiple threads collected and saving only occurs every few seconds.");
#endif

            if (OptionFlags.DataLoaded)
            {
                int CurrMins = DateTime.Now.Minute;
                bool IsBackup = CurrMins >= BackupSaveToken * BackupSaveIntervalMins && CurrMins < (BackupSaveToken + 1) % BackupHrInterval * BackupSaveIntervalMins;

                if (IsBackup)
                {
                    lock (BackupDataFileXML)
                    {
                        BackupSaveToken = (CurrMins / BackupSaveIntervalMins) % BackupHrInterval;

                    }
                }

                if (!UpdatingFollowers) // block saving data until the follower updating is completed
                {
                    if (!SaveThreadStarted) // only start the thread once per save cycle, flag is an object lock
                    {
                        SaveThreadStarted = true;
                        ThreadManager.CreateThreadStart(PerformSaveOp, ThreadWaitStates.Wait, ThreadExitPriority.Low); // need to wait, else could corrupt datafile
                    }

                    if (_DataSource.HasChanges())
                    {
                        lock (GUIDataManagerLock.Lock)
                        {
                            _DataSource.AcceptChanges();
                        }

                        lock (SaveTasks) // lock the Queue, block thread if currently save task has started
                        {
                            SaveTasks.Enqueue(new(() =>
                            {
                                lock (GUIDataManagerLock.Lock)
                                {
                                    try
                                    {
                                        MemoryStream SaveData = new();  // new memory stream

                                        _DataSource.WriteXml(SaveData, XmlWriteMode.DiffGram); // save the database to the memory stream

                                        DataSource testinput = new();   // start a new database
                                        SaveData.Position = 0;          // reset the reader
                                        testinput.ReadXml(SaveData);    // try to read the database, when in valid state this doesn't cause an exception (try/catch)

                                        _DataSource.WriteXml(DataFileName, XmlWriteMode.DiffGram); // write the valid data to file

                                        // determine if current time is within a certain time frame, and perform the save
                                        if (IsBackup && OptionFlags.IsStreamOnline)
                                        {
                                            // write backup file
                                            _DataSource.WriteXml(BackupDataFileXML, XmlWriteMode.DiffGram); // write the valid data to file
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        LogWriter.LogException(ex, MethodBase.GetCurrentMethod().Name);
                                    }
                                }
                            }));
                        }
                    }
                }
            }
        }

        private void PerformSaveOp()
        {
#if LogDataManager_Actions
            LogWriter.DataActionLog(MethodBase.GetCurrentMethod().Name, $"Managed database save data.");
#endif

            if (OptionFlags.ActiveToken) // don't sleep if exiting app
            {
                Thread.Sleep(SaveThreadWait);
            }

            lock (SaveTasks) // in case save actions arrive during save try
            {
                if (SaveTasks.Count >= 1)
                {
                    SaveTasks.Dequeue().Start(); // only run 1 of the save tasks
                }
                SaveTasks.Clear();
            }
            SaveThreadStarted = false; // indicate start another thread to save data
        }

        #endregion

        public void Initialize()
        {
#if LogDataManager_Actions
            LogWriter.DataActionLog(MethodBase.GetCurrentMethod().Name, $"Initializing the database.");
#endif

            SetDefaultChannelEventsTable();  // check all default ChannelEvents names
            SetDefaultCommandsTable(); // check all default Commands
            SetLearnedMessages();
            NotifySaveData();
        }

        private readonly string DefaulSocialMsg = "Social media url here";
        /// <summary>
        /// Add all of the default commands to the table, ensure they are available
        /// </summary>
        private void SetDefaultCommandsTable()
        {
#if LogDataManager_Actions
            LogWriter.DataActionLog(MethodBase.GetCurrentMethod().Name, $"Setting up and checking default commands, adding missing commands.");
#endif

            // TODO: move !intro to default commands <- for the custom welcome message
            lock (GUIDataManagerLock.Lock)
            {
                if (_DataSource.CategoryList.Select($"Category='{LocalizedMsgSystem.GetVar(Msg.MsgAllCateogry)}'").Length == 0)
                {
                    _DataSource.CategoryList.AddCategoryListRow(null, LocalizedMsgSystem.GetVar(Msg.MsgAllCateogry), 0);
                    _DataSource.CategoryList.AcceptChanges();
                }

                bool CheckName(string criteria)
                {
                    CommandsRow datarow = (CommandsRow)_DataSource.Commands.Select($"CmdName='{criteria}'").FirstOrDefault();
                    if (datarow != null)
                    {
                        if (datarow.Category == string.Empty)
                        {
                            datarow.Category = LocalizedMsgSystem.GetVar(Msg.MsgAllCateogry);
                        }
                        if (DBNull.Value.Equals(datarow["SendMsgCount"]))
                        {
                            datarow.SendMsgCount = 0;
                        }
                    }
                    return datarow == null;
                }

                // dictionary with commands, messages, and parameters
                // command name     // msg   // params
                Dictionary<string, Tuple<string, string>> DefCommandsDictionary = new();

                // add each of the default commands with localized strings
                foreach (DefaultCommand com in Enum.GetValues(typeof(DefaultCommand)))
                {
                    DefCommandsDictionary.Add(com.ToString(), new(LocalizedMsgSystem.GetDefaultComMsg(com), LocalizedMsgSystem.GetDefaultComParam(com)));
                }

                // add each of the social commands
                foreach (DefaultSocials social in Enum.GetValues(typeof(DefaultSocials)))
                {
                    DefCommandsDictionary.Add(social.ToString(), new(DefaulSocialMsg, LocalizedMsgSystem.GetVar("Parameachsocial")));
                }

                foreach (var (key, param) in from string key in DefCommandsDictionary.Keys
                                             where CheckName(key)
                                             let param = CommandParams.Parse(DefCommandsDictionary[key].Item2)
                                             select (key, param))
                {
                    _DataSource.Commands.AddCommandsRow(key, false, param.Permission.ToString(), param.IsEnabled, DefCommandsDictionary[key].Item1, param.Timer, param.RepeatMsg, param.Category, param.AllowParam, param.Usage, param.LookupData, param.Table, GetKey(param.Table), param.Field, param.Currency, param.Unit, param.Action, param.Top, param.Sort);
                }
            }
        }

        #region Regular Channel Events
        /// <summary>
        /// Add default data to Channel Events table, to ensure the data is available to use in event messages.
        /// </summary>
        private void SetDefaultChannelEventsTable()
        {
#if LogDataManager_Actions
            LogWriter.DataActionLog(MethodBase.GetCurrentMethod().Name, $"Setting default channel events, adding any missing events.");
#endif

            bool CheckName(string criteria)
            {
                ChannelEventsRow channelEventsRow = (ChannelEventsRow)GetRow(_DataSource.ChannelEvents, $"{_DataSource.ChannelEvents.NameColumn.ColumnName}='{criteria}'");

                if (channelEventsRow != null && DBNull.Value.Equals(channelEventsRow["RepeatMsg"]))
                {
                    channelEventsRow.RepeatMsg = 0;
                }

                return channelEventsRow == null;
            }

            Dictionary<ChannelEventActions, Tuple<string, string>> dictionary = new()
            {
                {
                    ChannelEventActions.BeingHosted,
                    new(LocalizedMsgSystem.GetEventMsg(ChannelEventActions.BeingHosted, out _, out _), VariableParser.ConvertVars(new[] { MsgVars.user, MsgVars.autohost, MsgVars.viewers }))
                },
                {
                    ChannelEventActions.Bits,
                    new(LocalizedMsgSystem.GetEventMsg(ChannelEventActions.Bits, out _, out _), VariableParser.ConvertVars(new[] { MsgVars.user, MsgVars.bits }))
                },
                {
                    ChannelEventActions.CommunitySubs,
                    new(LocalizedMsgSystem.GetEventMsg(ChannelEventActions.CommunitySubs, out _, out _), VariableParser.ConvertVars(new[] { MsgVars.user, MsgVars.count, MsgVars.subplan }))
                },
                {
                    ChannelEventActions.NewFollow,
                    new(LocalizedMsgSystem.GetEventMsg(ChannelEventActions.NewFollow, out _, out _), VariableParser.ConvertVars(new[] { MsgVars.user }))
                },
                {
                    ChannelEventActions.GiftSub,
                    new(LocalizedMsgSystem.GetEventMsg(ChannelEventActions.GiftSub, out _, out _), VariableParser.ConvertVars(new[] { MsgVars.user, MsgVars.months, MsgVars.receiveuser, MsgVars.subplan, MsgVars.subplanname }))
                },
                {
                    ChannelEventActions.Live,
                    new(LocalizedMsgSystem.GetEventMsg(ChannelEventActions.Live, out _, out _), VariableParser.ConvertVars(new[] { MsgVars.user, MsgVars.category, MsgVars.title, MsgVars.url, MsgVars.everyone }))
                },
                {
                    ChannelEventActions.Raid,
                    new(LocalizedMsgSystem.GetEventMsg(ChannelEventActions.Raid, out _, out _), VariableParser.ConvertVars(new[] { MsgVars.user, MsgVars.viewers }))
                },
                {
                    ChannelEventActions.Resubscribe,
                    new(LocalizedMsgSystem.GetEventMsg(ChannelEventActions.Resubscribe, out _, out _), VariableParser.ConvertVars(new[] { MsgVars.user, MsgVars.months, MsgVars.submonths, MsgVars.subplan, MsgVars.subplanname, MsgVars.streak }))
                },
                {
                    ChannelEventActions.Subscribe,
                    new(LocalizedMsgSystem.GetEventMsg(ChannelEventActions.Subscribe, out _, out _), VariableParser.ConvertVars(new[] { MsgVars.user, MsgVars.submonths, MsgVars.subplan, MsgVars.subplanname }))
                },
                {
                    ChannelEventActions.UserJoined,
                    new(LocalizedMsgSystem.GetEventMsg(ChannelEventActions.UserJoined, out _, out _), VariableParser.ConvertVars(new[] { MsgVars.user }))
                },
                {
                    ChannelEventActions.ReturnUserJoined,
                    new(LocalizedMsgSystem.GetEventMsg(ChannelEventActions.ReturnUserJoined, out _, out _), VariableParser.ConvertVars(new[] { MsgVars.user }))
                },
                {
                    ChannelEventActions.SupporterJoined,
                    new(LocalizedMsgSystem.GetEventMsg(ChannelEventActions.SupporterJoined, out _, out _), VariableParser.ConvertVars(new[] { MsgVars.user }))
                },
                {
                    ChannelEventActions.BannedUser,
                    new(LocalizedMsgSystem.GetEventMsg(ChannelEventActions.BannedUser, out _, out _), VariableParser.ConvertVars(new[] { MsgVars.user }))
                }
            };
            lock(GUIDataManagerLock.Lock)
            {
                foreach (var (command, values) in from ChannelEventActions command in System.Enum.GetValues(typeof(ChannelEventActions))// consider only the values in the dictionary, check if data is already defined in the data table
                                                  where dictionary.ContainsKey(command) && CheckName(command.ToString())// extract the default data from the dictionary and add to the data table
                                                  let values = dictionary[command]
                                                  select (command, values))
                {
                    _ = _DataSource.ChannelEvents.AddChannelEventsRow(command.ToString(), 0, false, true, values.Item1, values.Item2);
                }
            }
        }
        #endregion Regular Channel Events

    }
}
