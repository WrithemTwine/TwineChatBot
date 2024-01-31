

using StreamerBotLib.Data.DataSetCommonMethods;
using StreamerBotLib.Enums;
using StreamerBotLib.GUI;
using StreamerBotLib.Models;
using StreamerBotLib.Static;
using StreamerBotLib.Systems;

using System.Data;
using System.IO;
using System.Reflection;
using System.Xml;

using static StreamerBotLib.Data.DataSource;

namespace StreamerBotLib.Data
{
    public partial class DataManager : BaseDataManager
    {
        #region Load and Exit Ops

        /// <summary>
        /// Provide an internal notification event to save the data outside of any multi-threading mechanisms.
        /// </summary>
        public event EventHandler OnSaveData;
        //public event EventHandler<OnDataUpdatedEventArgs> OnDataUpdated;

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

            BeginLoadData(_DataSource.Tables);
            TryLoadFile((xmlfile) => LoadFile(xmlfile));
            EndLoadData(_DataSource.Tables);

            SaveData(this, new());
        }

        public void NotifySaveData()
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.DataManager, $"Notify to save database.");

            OnSaveData?.Invoke(this, new());
        }

        /// <summary>
        /// Save data to file upon exit and after data changes. Pauses for 15 seconds (unless exiting) to slow down multiple saves in a short time.
        /// </summary>
        public void SaveData(object sender, EventArgs e)
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.DataManager, $"Saving the database, managed as multiple threads collected and saving only occurs every few seconds.");

            if (OptionFlags.DataLoaded)
            {
                if (!UpdatingFollowers) // block saving data until the follower updating is completed
                {
                    SaveData(
                        _DataSource.WriteXml,
                        _DataSource.WriteXml,
                        GUIDataManagerLock.Lock,
                        (SaveDataMemoryStream) =>
                        {
                            DataSource testinput = new();   // start a new database
                            SaveDataMemoryStream.Position = 0;          // reset the reader
                            testinput.ReadXml(SaveDataMemoryStream);    // try to read the database, when in valid state this doesn't cause an exception (try/catch)
                        }
                        );
                }
            }
        }

        #endregion

        public void Initialize()
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.DataManager, $"Initializing the database.");

            SetDefaultChannelEventsTable();  // check all default ChannelEvents names
            SetDefaultCommandsTable(); // check all default Commands
            SetLearnedMessages();
            CleanUpTables();
            NotifySaveData();
        }

        private readonly string DefaulSocialMsg = "Social media url here";

        /// <summary>
        /// Add all of the default commands to the table, ensure they are available
        /// </summary>
        private void SetDefaultCommandsTable()
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.DataManager, $"Setting up and checking default commands, adding missing commands.");

            lock (GUIDataManagerLock.Lock)
            {
                if (_DataSource.CategoryList.Select($"Category='{LocalizedMsgSystem.GetVar(Msg.MsgAllCategory)}'").Length == 0)
                {
                    _DataSource.CategoryList.AddCategoryListRow(null, LocalizedMsgSystem.GetVar(Msg.MsgAllCategory), 0);
                    _DataSource.CategoryList.AcceptChanges();
                }

                bool CheckName(string criteria)
                {
                    CommandsRow datarow = (CommandsRow)_DataSource.Commands.Select($"CmdName='{criteria}'").FirstOrDefault();
                    if (datarow != null)
                    {
                        if (datarow.Category == string.Empty)
                        {
                            datarow.Category = LocalizedMsgSystem.GetVar(Msg.MsgAllCategory);
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

                bool found = false;
                foreach (var (key, param) in from string key in DefCommandsDictionary.Keys
                                             where CheckName(key)
                                             let param = CommandParams.Parse(DefCommandsDictionary[key].Item2)
                                             select (key, param))
                {
                    _DataSource.Commands.AddCommandsRow(key, false, param.Permission.ToString(), param.IsEnabled, DefCommandsDictionary[key].Item1, param.Timer, param.RepeatMsg, param.Category, param.AllowParam, param.Usage, param.LookupData, param.Table, DataSetStatic.GetKey(_DataSource.Tables[param.Table], param.Table), param.Field, param.Currency, param.Unit, param.Action, param.Top, param.Sort);
                    found = true;
                }

                if (found)
                {
                    _DataSource.Commands.AcceptChanges();
                }
            }
        }

        #region Regular Channel Events
        /// <summary>
        /// Add default data to Channel Events table, to ensure the data is available to use in event messages.
        /// </summary>
        private void SetDefaultChannelEventsTable()
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.DataManager, $"Setting default channel events, adding any missing events.");

            bool CheckName(string criteria)
            {
                ChannelEventsRow channelEventsRow = (ChannelEventsRow)DataSetStatic.GetRow(_DataSource.ChannelEvents, $"{_DataSource.ChannelEvents.NameColumn.ColumnName}='{criteria}'");

                if (channelEventsRow != null && DBNull.Value.Equals(channelEventsRow["RepeatMsg"]))
                {
                    channelEventsRow.RepeatMsg = 0;
                }

                return channelEventsRow == null;
            }

            bool found = false;

            Dictionary<ChannelEventActions, Tuple<string, string>> dictionary = new()
            {
                {
                    ChannelEventActions.BeingHosted,
                    new(LocalizedMsgSystem.GetEventMsg(ChannelEventActions.BeingHosted, out _, out _), VariableParser.ConvertVars([MsgVars.user, MsgVars.autohost, MsgVars.viewers]))
                },
                {
                    ChannelEventActions.Bits,
                    new(LocalizedMsgSystem.GetEventMsg(ChannelEventActions.Bits, out _, out _), VariableParser.ConvertVars([MsgVars.user, MsgVars.bits]))
                },
                {
                    ChannelEventActions.CommunitySubs,
                    new(LocalizedMsgSystem.GetEventMsg(ChannelEventActions.CommunitySubs, out _, out _), VariableParser.ConvertVars([MsgVars.user, MsgVars.count, MsgVars.subplan]))
                },
                {
                    ChannelEventActions.NewFollow,
                    new(LocalizedMsgSystem.GetEventMsg(ChannelEventActions.NewFollow, out _, out _), VariableParser.ConvertVars([MsgVars.user]))
                },
                {
                    ChannelEventActions.GiftSub,
                    new(LocalizedMsgSystem.GetEventMsg(ChannelEventActions.GiftSub, out _, out _), VariableParser.ConvertVars([MsgVars.user, MsgVars.months, MsgVars.receiveuser, MsgVars.subplan, MsgVars.subplanname]))
                },
                {
                    ChannelEventActions.Live,
                    new(LocalizedMsgSystem.GetEventMsg(ChannelEventActions.Live, out _, out _), VariableParser.ConvertVars([MsgVars.user, MsgVars.category, MsgVars.title, MsgVars.url, MsgVars.everyone]))
                },
                {
                    ChannelEventActions.Raid,
                    new(LocalizedMsgSystem.GetEventMsg(ChannelEventActions.Raid, out _, out _), VariableParser.ConvertVars([MsgVars.user, MsgVars.viewers]))
                },
                {
                    ChannelEventActions.Resubscribe,
                    new(LocalizedMsgSystem.GetEventMsg(ChannelEventActions.Resubscribe, out _, out _), VariableParser.ConvertVars([MsgVars.user, MsgVars.months, MsgVars.submonths, MsgVars.subplan, MsgVars.subplanname, MsgVars.streak]))
                },
                {
                    ChannelEventActions.Subscribe,
                    new(LocalizedMsgSystem.GetEventMsg(ChannelEventActions.Subscribe, out _, out _), VariableParser.ConvertVars([MsgVars.user, MsgVars.submonths, MsgVars.subplan, MsgVars.subplanname]))
                },
                {
                    ChannelEventActions.UserJoined,
                    new(LocalizedMsgSystem.GetEventMsg(ChannelEventActions.UserJoined, out _, out _), VariableParser.ConvertVars([MsgVars.user]))
                },
                {
                    ChannelEventActions.ReturnUserJoined,
                    new(LocalizedMsgSystem.GetEventMsg(ChannelEventActions.ReturnUserJoined, out _, out _), VariableParser.ConvertVars([MsgVars.user]))
                },
                {
                    ChannelEventActions.SupporterJoined,
                    new(LocalizedMsgSystem.GetEventMsg(ChannelEventActions.SupporterJoined, out _, out _), VariableParser.ConvertVars([MsgVars.user]))
                },
                {
                    ChannelEventActions.BannedUser,
                    new(LocalizedMsgSystem.GetEventMsg(ChannelEventActions.BannedUser, out _, out _), VariableParser.ConvertVars([MsgVars.user]))
                }
            };
            lock (GUIDataManagerLock.Lock)
            {
                foreach (var (command, values) in from ChannelEventActions command in System.Enum.GetValues(typeof(ChannelEventActions))// consider only the values in the dictionary, check if data is already defined in the data table
                                                  where dictionary.ContainsKey(command) && CheckName(command.ToString())// extract the default data from the dictionary and add to the data table
                                                  let values = dictionary[command]
                                                  select (command, values))
                {
                    _ = _DataSource.ChannelEvents.AddChannelEventsRow(command.ToString(), 0, false, true, values.Item1, values.Item2);
                    found = true;
                }

                if (found)
                {
                    _DataSource.ChannelEvents.AcceptChanges();
                }
            }
        }
        #endregion Regular Channel Events

        private void CleanUpTables()
        {
            lock (GUIDataManagerLock.Lock)
            {
                foreach (var UR in from UsersRow UR in _DataSource.Users.Select()
                                   where DBNull.Value.Equals(UR["Platform"])
                                   select UR)
                {
                    UR.Platform = Platform.Twitch.ToString();
                }
                _DataSource.Users.AcceptChanges();
                foreach (var FR in from FollowersRow FR in _DataSource.Followers.Select()
                                   where DBNull.Value.Equals(FR["Platform"])
                                   select FR)
                {
                    FR.Platform = Platform.Twitch.ToString();
                }
                _DataSource.Followers.AcceptChanges();

                foreach (var SOR in from ShoutOutsRow SR in _DataSource.ShoutOuts.Select()
                                    where DBNull.Value.Equals(SR["UserId"])
                                    select SR)
                {
                    UsersRow user = (UsersRow)DataSetStatic.GetRow(_DataSource.Users, $"{_DataSource.Users.UserNameColumn.ColumnName}='{SOR.UserName}'");
                    if (user != null)
                    {
                        SOR.UserId = user.UserId;
                        SOR.Platform = user.Platform;
                    }
                }
            }
        }
    }
}
