using StreamerBotLib.BotIOController;
using StreamerBotLib.DataSQL.Models;
using StreamerBotLib.Models;
using StreamerBotLib.Models.Enums;
using StreamerBotLib.Models.Events;
using StreamerBotLib.Static;
using StreamerBotLib.Systems.Overlay.Enums;

using System.ComponentModel;
using System.Globalization;

namespace StreamerBotLib.Systems
{
    public partial class ActionSystem : INotifyPropertyChanged
    {
        // bubbles up messages from the event timers because there is no invoking method to receive this output message 
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<PostChannelMessageEventArgs> ProcessedCommand;
        public event EventHandler<TwitchShoutOutUsersEventArgs> TwitchShoutOutUser;

        internal static int LastLiveViewerCount = 0;


        /// <summary>
        /// Informs the GUI of updated info.
        /// </summary>
        /// <param name="ParamName"></param>
        public void NotifyPropertyChanged(string ParamName = "")
        {
            LogWriter.DebugLog("NotifyPropertyChanged", DebugLogTypes.CommandSystem, $"Notifying GUI of updated info: {ParamName}");
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(ParamName));
        }

        public IEnumerable<string> GetCommandList(bool prefix = true)
        {
            LogWriter.DebugLog("GetCommandList", DebugLogTypes.CommandSystem, "Getting command list.");
            return DataManage.GetCommandList(prefix);
        }

        public IEnumerable<string> GetCommandListNoParams(bool prefix = true)
        {
            LogWriter.DebugLog("GetCommandList", DebugLogTypes.CommandSystem, "Getting command list.");
            return DataManage.GetCommandListNoParams(prefix);
        }

        /// <summary>
        /// Establishes the permission level for the user who sends the message.
        /// </summary>
        /// <param name="chatMessage">The ChatMessage holding the characteristics of the user who invoked the chat command, which parses out the user permissions.</param>
        /// <returns>The ViewerType corresponding to the user's highest permission.</returns>
        public static ViewerTypes ParsePermission(CmdMessage chatMessage)
        {
            LogWriter.DebugLog("ParsePermission", DebugLogTypes.CommandSystem, $"Parsing user permissions for {chatMessage.DisplayName}.");
            if (chatMessage.IsBroadcaster)
            {
                return ViewerTypes.Broadcaster;
            }
            else if (chatMessage.IsModerator)
            {
                return ViewerTypes.Mod;
            }
            else if (chatMessage.IsVip)
            {
                return ViewerTypes.VIP;
            }
            else if (DataManage.CheckFollower(chatMessage.DisplayName))
            {
                return ViewerTypes.Follower;
            }
            else if (chatMessage.IsSubscriber)
            {
                return ViewerTypes.Sub;
            }
            else
            {
                return ViewerTypes.Viewer;
            }
        }

        /// <summary>
        /// Evaluate a command, and perform if calling user is allowed. Uses event pattern to provide output message response.
        /// </summary>
        /// <param name="cmdMessage">The whole message bundle from the calling user.</param>
        /// <param name="source">The platform of the call, for performing any API calls to that platform.</param>
        public void EvalCommand(CmdMessage cmdMessage, Platform source)
        {
            LogWriter.DebugLog("EvalCommand", DebugLogTypes.CommandSystem, $"Evaluating command: {cmdMessage.CommandText} from {cmdMessage.DisplayName}.");
            string result;
            cmdMessage.UserType = ParsePermission(cmdMessage);
            short multi = 0;

            CommandData cmdrow = DataManage.GetCommand(cmdMessage.CommandText);

            if (cmdrow == null)
            {
                LogWriter.DebugLog("EvalCommand", DebugLogTypes.CommandSystem, $"Command not found: {cmdMessage.CommandText}.");
                result = OptionFlags.MsgCommandNotFound ? LocalizedMsgSystem.GetVar(ChatBotExceptions.ExceptionKeyNotFound) : "";
            }
            else if (!cmdrow.IsEnabled)
            {
                LogWriter.DebugLog("EvalCommand", DebugLogTypes.CommandSystem, $"Command disabled: {cmdMessage.CommandText}.");
                result = "";
            }
            else if (cmdrow.Permission < cmdMessage.UserType)
            {
                LogWriter.DebugLog("EvalCommand", DebugLogTypes.CommandSystem, $"User does not have permission to run command: {cmdMessage.CommandText}.");
                Tuple<string, string> ApproveAction = GetApprovalRule(ModActionType.Commands, cmdMessage.CommandText);
                if (ApproveAction == null)
                {
                    LogWriter.DebugLog("EvalCommand", DebugLogTypes.CommandSystem, $"Command is not eligible for approval: {cmdMessage.CommandText}.");

                    result = LocalizedMsgSystem.GetVar(ChatBotExceptions.ExceptionInvalidCommand);
                }
                else
                {
                    LogWriter.DebugLog("EvalCommand", DebugLogTypes.CommandSystem, $"Command requires approval: {cmdMessage.CommandText}.");

                    PostApproval($"{cmdMessage.CommandText} {cmdMessage.DisplayName} {cmdMessage.Message}",
                        new(() => { FormatResult(ParseCommand(cmdMessage.CommandText, new(cmdMessage.DisplayName, source, cmdMessage.UserId), cmdMessage.CommandArguments, cmdrow, out multi), multi, cmdrow); }));
                    result = ParseCommand(LocalizedMsgSystem.GetVar(DefaultCommand.approve), new LiveUser(BotUserName, source), [], DataManage.GetCommand(LocalizedMsgSystem.GetVar(DefaultCommand.approve)), out multi);
                }
            }
            else
            {
                LogWriter.DebugLog("EvalCommand", DebugLogTypes.CommandSystem, $"Command is valid: {cmdMessage.CommandText}.");

                // parse commands, either built-in or custom
                result = ParseCommand(cmdMessage.CommandText, new(cmdMessage.DisplayName, source, cmdMessage.UserId), cmdMessage.CommandArguments, cmdrow, out multi);
                DataManage.UpdateStats(DBUserStats.Commands, cmdMessage.UserId, source);
            }

            FormatResult(result, multi, cmdrow);
        }

        private void FormatResult(string result, short multi, CommandData cmdrow)
        {
            LogWriter.DebugLog("FormatResult", DebugLogTypes.CommandSystem, $"Formatting result: {result}.");
            result = $"{(cmdrow != null && cmdrow.IsEnabled && ((OptionFlags.MsgPerComMe && cmdrow.AddMe) || OptionFlags.MsgAddMe) && !result.StartsWith("/me ") ? "/me " : "")}{result}";

            LogWriter.DebugLog("FormatResult", DebugLogTypes.CommandSystem, $"Sending formatted result: {result}.");
            OnProcessCommand(result, cmdrow.Announce, multi);
        }

        /// <summary>
        /// Call to check all users in the stream, and shout them.
        /// </summary>
        /// <param name="Source">The name of the Bot calling the shout-outs, for purposes of which platform to call the category.</param>
        private void AutoShoutUsers()
        {
            LogWriter.DebugLog("AutoShoutUsers", DebugLogTypes.CommandSystem, "Received AutoShoutUsers command.");
            LogWriter.DebugLog("AutoShoutUsers", DebugLogTypes.CommandSystem, "Now checking if each active user is on the shout list.");

            ThreadManager.CreateThreadStart("AutoShoutUsers", () =>
            {
                foreach (LiveUser u in StreamViewers.GetCurrentActiveUsers(true))
                {
                    LogWriter.DebugLog("AutoShoutUsers", DebugLogTypes.CommandSystem, $"Checking for auto-shout: {u.UserName}, {u.UserId}, {u.Platform}");
                    CheckShout(u, out _);
                }
            });
        }

        /// <summary>
        /// See if the user is part of the user's auto-shout out list to determine if the message should be called, or shout-out from a raid or other similar event.
        /// </summary>
        /// <param name="UserName">The user to check</param>
        /// <param name="response">the response message template</param>
        /// <param name="AutoShout">true-check if the user is on the autoshout list, false-the method call is from a command, no autoshout check</param>
        /// <returns></returns>
        public void CheckShout(LiveUser User, out string response, bool AutoShout = true)
        {
            response = "";
            if (DataManage.CheckShoutName(User.UserId) || !AutoShout)
            {
                LogWriter.DebugLog("CheckShout", DebugLogTypes.CommandSystem, $"User {User.UserName} is on the shout list.");
                if (OptionFlags.MsgSendSOToChat)
                {
                    OnProcessCommand($"!{LocalizedMsgSystem.GetVar(DefaultCommand.so)} {User.UserName}");
                }

                LogWriter.DebugLog("CheckShout", DebugLogTypes.CommandSystem, $"Send shout message to chat for {User.UserName}.");
                response = ParseCommand(LocalizedMsgSystem.GetVar(DefaultCommand.so), User, [], DataManage.GetCommand(LocalizedMsgSystem.GetVar(DefaultCommand.so)), out short multi);

                // handle when returned without #category in the message
                if (response is not "" and not "/me ")
                {
                    OnProcessCommand(response, DataManage.GetCmdAnnounce(LocalizedMsgSystem.GetVar(DefaultCommand.so)), multi);
                    LogWriter.DebugLog("CheckShout", DebugLogTypes.CommandSystem, "Sent message with no #category symbol.");
                }
            }
        }

        /// <summary>
        /// Checks for a user welcome message.
        /// </summary>
        /// <param name="User">The user to check.</param>
        /// <returns>The user's welcome message, or empty string if it's not found.</returns>
        public static string CheckWelcomeUser(string UserId)
        {
            LogWriter.DebugLog("CheckWelcomeUser", DebugLogTypes.CommandSystem, $"Checking for welcome message for user {UserId}.");

            return DataManage.CheckWelcomeUser(UserId);
        }

        internal string ParseCommand(string command, LiveUser User, List<string> arglist, CommandData cmdrow, out short multi, bool ElapsedTimer = false)
        {
            LogWriter.DebugLog("ParseCommand", DebugLogTypes.CommandSystem, $"Parsing command: {command} for {User.UserId} : {User.UserName} : {User.Platform}.");

            string result = "";
            string tempHTMLResponse = "";
            Dictionary<string, string> datavalues = null;
            if (command == LocalizedMsgSystem.GetVar(DefaultCommand.addcommand))
            {
                LogWriter.DebugLog("ParseCommand", DebugLogTypes.CommandSystem, "Adding a command.");

                string newcom = arglist[0][0] == '!' ? arglist[0] : string.Empty;
                arglist.RemoveAt(0);
                result = DataManage.PostCommand(newcom[1..], CommandParams.Parse(arglist));
            }
            else if (command == LocalizedMsgSystem.GetVar(DefaultCommand.settitle))
            {
                LogWriter.DebugLog("ParseCommand", DebugLogTypes.CommandSystem, "Setting the title.");

                if (arglist.Count > 0)
                {
                    bool success = BotController.ModifyChannelInformation(User.Platform, Title: string.Join(' ', arglist));
                    result = success ? cmdrow.Message : LocalizedMsgSystem.GetVar("MsgNoSuccess");
                }
                else
                {
                    LogWriter.DebugLog("ParseCommand", DebugLogTypes.CommandSystem, "No title provided.");

                    result = LocalizedMsgSystem.GetVar("MsgNoTitleCategory");
                }
            }
            else if (command == LocalizedMsgSystem.GetVar(DefaultCommand.raid))
            {
                LogWriter.DebugLog("ParseCommand", DebugLogTypes.CommandSystem, "Raiding a channel.");
                if (arglist.Count > 0)
                {
                    LogWriter.DebugLog("ParseCommand", DebugLogTypes.CommandSystem, $"Channel provided, {arglist[0]}.");
                    BotController.RaidChannel(arglist[0].Replace("@", ""), User.Platform);
                    result = cmdrow.Message;

                    LogWriter.DebugLog("ParseCommand", DebugLogTypes.CommandSystem, $"Output message: {result}.");

                }
                else
                {
                    LogWriter.DebugLog("ParseCommand", DebugLogTypes.CommandSystem, "No channel provided.");
                    result = DataManage.GetUsage(command);
                }
            }
            else if (command == LocalizedMsgSystem.GetVar(DefaultCommand.cancelraid))
            {
                LogWriter.DebugLog("ParseCommand", DebugLogTypes.CommandSystem, "Cancelling a raid.");
                BotController.CancelRaidChannel(User.Platform);
                result = cmdrow.Message;
            }
            else if (command == LocalizedMsgSystem.GetVar(DefaultCommand.approve))
            {
                LogWriter.DebugLog("ParseCommand", DebugLogTypes.CommandSystem, "Approving a command.");
                if (arglist.Count == 0)
                {
                    LogWriter.DebugLog("ParseCommand", DebugLogTypes.CommandSystem, "No command provided.");
                    datavalues = VariableParser.BuildDictionary(new Tuple<MsgVars, string>[]
                    {
                        new(MsgVars.list, string.Join(", ",GetDescriptions()) ),
                        new(MsgVars.usage, cmdrow.Usage)
                    });

                    result = VariableParser.ParseReplace(cmdrow.Message, datavalues);
                }
                else if (arglist.Count == 1)
                {
                    LogWriter.DebugLog("ParseCommand", DebugLogTypes.CommandSystem, $"Approving command: {arglist[0]}.");
                    string AppLabel = GetLabel(arglist[0]);
                    if (AppLabel != null)
                    {
                        LogWriter.DebugLog("ParseCommand", DebugLogTypes.CommandSystem, $"Command found: {AppLabel}.");
                        RunApprovedRequest(AppLabel);
                        result = LocalizedMsgSystem.GetVar(Msg.MsgModApproved);
                    }
                    else
                    {
                        LogWriter.DebugLog("ParseCommand", DebugLogTypes.CommandSystem, "Mod Approval not found.");
                        result = LocalizedMsgSystem.GetVar(Msg.MsgModApproveNotFound);
                    }
                }
            }
            else if (command == LocalizedMsgSystem.GetVar(DefaultCommand.setintro))
            {
                LogWriter.DebugLog("ParseCommand", DebugLogTypes.CommandSystem, "Setting the intro message.");
                if (arglist.Count > 2)
                {
                    string adduser = arglist[0].Replace("@", "");
                    string message = string.Join(' ', arglist.Skip(1));

                    LogWriter.DebugLog("ParseCommand", DebugLogTypes.CommandSystem, $"Intro message provided, {message}.");
                    DataManage.PostUserCustomWelcome(DataManage.GetUser(adduser), message);
                }
                else
                {
                    LogWriter.DebugLog("ParseCommand", DebugLogTypes.CommandSystem, "No intro message provided.");
                    result = cmdrow.Usage;
                }
            }
            else if (command == LocalizedMsgSystem.GetVar(DefaultCommand.mergeaccounts))
            {
                LogWriter.DebugLog("ParseCommand", DebugLogTypes.CommandSystem, "Merging accounts.");

                bool? output = null;
                if (arglist.Count == 0)
                {
                    LogWriter.DebugLog("ParseCommand", DebugLogTypes.CommandSystem, "No accounts provided.");
                    result = cmdrow.Usage;
                }
                else
                {
                    LogWriter.DebugLog("ParseCommand", DebugLogTypes.CommandSystem, $"Merging accounts: {User.UserName} {arglist[0]}.");
                    /* -p:Mod 
                     * -use:(Mod level) !mergeaccounts <currname> <previousname> 'or' (user level) !mergeaccounts <previousname>
                     */
                    string CurrUser, SrcUsr;

                    switch (arglist.Count)
                    {
                        case 1:
                            CurrUser = User.UserName;
                            SrcUsr = arglist[0];
                            break;
                        case 2:
                        default:
                            CurrUser = arglist[0];
                            SrcUsr = arglist[1];
                            break;
                    }

                    output = DataManage.PostMergeUserStats(CurrUser.Replace("@", ""), SrcUsr.Replace("@", ""), User.Platform);
                }
                LogWriter.DebugLog("ParseCommand", DebugLogTypes.CommandSystem, $"Merge result: {output}.");
                result = output == null ? result : output == true ? LocalizedMsgSystem.GetVar(Msg.MsgMergeSuccessful) : LocalizedMsgSystem.GetVar(Msg.MsgMergeFailed);
            }
            else if (command == LocalizedMsgSystem.GetVar(DefaultCommand.setcategory))
            {
                LogWriter.DebugLog("ParseCommand", DebugLogTypes.CommandSystem, "Setting the category.");

                if (arglist.Count > 0)
                {
                    LogWriter.DebugLog("ParseCommand", DebugLogTypes.CommandSystem, $"Category provided, {arglist[0]}.");
                    if (int.TryParse(arglist[0], out int GameId))
                    {
                        BotController.ModifyChannelInformation(User.Platform, CategoryId: GameId.ToString());
                        result = cmdrow.Message;
                    }
                    else
                    {
                        bool success = false;
                        string CategoryName = string.Join(' ', arglist);

                        CategoryData found = DataManage.GetGameCategories().Find((x) => x.CategoryName == CategoryName);

                        if (found != null)
                        {
                            success = BotController.ModifyChannelInformation(User.Platform, CategoryId: found.CategoryId);
                        }
                        else
                        {
                            success = BotController.ModifyChannelInformation(User.Platform, CategoryName: CategoryName);
                        }

                        result = success ? cmdrow.Message : LocalizedMsgSystem.GetVar("MsgNoSuccess");
                    }
                }
                else
                {
                    LogWriter.DebugLog("ParseCommand", DebugLogTypes.CommandSystem, "No category provided.");
                    result = LocalizedMsgSystem.GetVar("MsgNoTitleCategory");
                }
            }
            else if (command == LocalizedMsgSystem.GetVar(DefaultCommand.editcommand))
            {
                string newcom = arglist[0][0] == '!' ? arglist[0] : string.Empty;
                arglist.RemoveAt(0);
                LogWriter.DebugLog("ParseCommand", DebugLogTypes.CommandSystem, $"Editing command: {newcom}.");
                result = DataManage.EditCommand(newcom[1..], arglist);
            }
            else if (command == LocalizedMsgSystem.GetVar(DefaultCommand.removecommand))
            {
                LogWriter.DebugLog("ParseCommand", DebugLogTypes.CommandSystem, "Removing a command.");
                if (!LocalizedMsgSystem.CheckDefaultCommand(arglist[0]))
                {
                    LogWriter.DebugLog("ParseCommand", DebugLogTypes.CommandSystem, $"Command found: {arglist[0]}.");
                    result = DataManage.RemoveCommand(arglist[0])
                        ? LocalizedMsgSystem.GetDefaultComMsg(DefaultCommand.removecommand)
                        : LocalizedMsgSystem.GetVar("Msgcommandnotfound");
                }
                else
                {
                    LogWriter.DebugLog("ParseCommand", DebugLogTypes.CommandSystem, "Command not found.");
                    result = LocalizedMsgSystem.GetVar("Msgdefaultcommand");
                }
            }
            else if (command == LocalizedMsgSystem.GetVar(DefaultCommand.accountage))
            {
                LogWriter.DebugLog("ParseCommand", DebugLogTypes.CommandSystem, "Checking account age.");

                string ParamUser = arglist.Count == 1 ? arglist[0].Replace("@", "") : User.UserName;

                ThreadManager.CreateThreadStart("ParseCommand", () =>
                {
                    LogWriter.DebugLog("ParseCommand", DebugLogTypes.CommandSystem, $"Checking account age for {ParamUser}.");

                    DateTime created = BotController.GetUserAccountAge(ParamUser, User.Platform);
                    datavalues = VariableParser.BuildDictionary(new Tuple<MsgVars, string>[]
                    {
                        new(MsgVars.user,ParamUser),
                        new(MsgVars.date, created == DateTime.MinValue ? "not found" : FormatData.FormatTimes(created.ToLocalTime()))
                    });

                    OnProcessCommand(VariableParser.ParseReplace(cmdrow.Message, datavalues));
                });
            }
            else if (command == LocalizedMsgSystem.GetVar(DefaultCommand.socials))
            {
                LogWriter.DebugLog("ParseCommand", DebugLogTypes.CommandSystem, "Checking socials.");
                // User chose to send separate messages for the socials
                if (OptionFlags.MsgSocialSeparate)
                {
                    LogWriter.DebugLog("ParseCommand", DebugLogTypes.CommandSystem, "Sending separate social messages.");

                    string Socialresult = "";
                    foreach (string Social in DataManage.GetSocialComs())
                    {
                        CommandData SocialRow = DataManage.GetCommand(Social);
                        Socialresult = ParseCommand(Social, User, null, SocialRow, out multi);
                        FormatResult(Socialresult, SocialRow.SendMsgCount, SocialRow);
                    }
                }
                else
                {
                    LogWriter.DebugLog("ParseCommand", DebugLogTypes.CommandSystem, "Sending combined social message.");
                    result = cmdrow.Message + " " + DataManage.GetSocials();
                }
            }
            else if (command == LocalizedMsgSystem.GetVar(DefaultCommand.uptime))
            {
                LogWriter.DebugLog("ParseCommand", DebugLogTypes.CommandSystem, "Checking uptime.");
                if (arglist.Count == 0 && cmdrow.Message.Contains(MsgVars.viewers.ToString()))
                {
                    LogWriter.DebugLog("ParseCommand", DebugLogTypes.CommandSystem, "No uptime provided.");
                    BotController.GetViewerCount(User.Platform);
                    result = "";
                }
                else
                {
                    LogWriter.DebugLog("ParseCommand", DebugLogTypes.CommandSystem, "Uptime provided.");
                    int DeltaViewers = Convert.ToInt32(arglist[0]) - LastLiveViewerCount;

                    result = VariableParser.ParseReplace(OptionFlags.IsStreamOnline ?
                        (DataManage.GetCommand(command).Message ?? LocalizedMsgSystem.GetDefaultComMsg(DefaultCommand.uptime)) :
                        LocalizedMsgSystem.GetVar(Msg.Msgstreamoffline),
                    VariableParser.BuildDictionary(new Tuple<MsgVars, string>[]
                    {
                        new( MsgVars.user, ChannelName ),
                        new( MsgVars.uptime, FormatData.FormatTimes(GetCurrentStreamStart) ),
                        new( MsgVars.viewers, FormatData.Plurality(arglist.Count > 0 ? arglist[0] : "", MsgVars.Pluralviewers) ),
                        new( MsgVars.deltaviewers, $"{(DeltaViewers>0?'+':"")}{DeltaViewers}" ),
                        new( MsgVars.viewrate, arglist.Count > 0 ? (Convert.ToDouble(arglist[0])/( Math.Max( DataManage.GetFollowerCount(), 1) )).ToString("0.#00 %") : "0 %")
                    }));

                    LastLiveViewerCount = Convert.ToInt32(arglist[0]);
                }
            }
            else if (command == LocalizedMsgSystem.GetVar(DefaultCommand.clip))
            {
                LogWriter.DebugLog("ParseCommand", DebugLogTypes.CommandSystem, "Creating a clip.");
                BotController.CreateClip();
            }
            else if (command == LocalizedMsgSystem.GetVar(DefaultCommand.commands))
            {
                LogWriter.DebugLog("ParseCommand", DebugLogTypes.CommandSystem, "Checking commands.");
                result = DataManage.GetCommandString();
            }
            // capture all of the join queue commands
            else if (command == LocalizedMsgSystem.GetVar(DefaultCommand.join)
                || command == LocalizedMsgSystem.GetVar(DefaultCommand.enqueue)
                || command == LocalizedMsgSystem.GetVar(DefaultCommand.leave)
                || command == LocalizedMsgSystem.GetVar(DefaultCommand.dequeue)
                || command == LocalizedMsgSystem.GetVar(DefaultCommand.queue)
                || command == LocalizedMsgSystem.GetVar(DefaultCommand.qinfo))
            {
                LogWriter.DebugLog("ParseCommand", DebugLogTypes.CommandSystem, "Checking queue commands.");

                result = OptionFlags.UserPartyStart
                    ? PartyCommand(command, User.UserName, arglist.Count > 0 ? arglist[0] : "", cmdrow)
                    : ElapsedTimer ? "" : LocalizedMsgSystem.GetDefaultComMsg(DefaultCommand.qstop);
            }
            else if (command == LocalizedMsgSystem.GetVar(DefaultCommand.qstart)
                || command == LocalizedMsgSystem.GetVar(DefaultCommand.qstop))
            {
                LogWriter.DebugLog("ParseCommand", DebugLogTypes.CommandSystem, "Starting or stopping the queue.");

                result = cmdrow.Message;
                OptionFlags.SetParty(command == LocalizedMsgSystem.GetVar(DefaultCommand.qstart));
                NotifyPropertyChanged("UserPartyStart");
                NotifyPropertyChanged("UserPartyStop");
            }
            else if (command == LocalizedMsgSystem.GetVar(DefaultCommand.soactive))
            {
                LogWriter.DebugLog("ParseCommand", DebugLogTypes.CommandSystem, "Checking active shout-outs.");
                AutoShoutUsers();
            }
            else if (command == LocalizedMsgSystem.GetVar(DefaultCommand.blackjack))
            {
                LogWriter.DebugLog("ParseCommand", DebugLogTypes.CommandSystem, "Playing blackjack.");
                // in repeat command case, the arglist may be empty and needs capacity check
                bool TryConvertInt = int.TryParse((arglist != null && arglist.Count > 0) ? arglist[0] : "0", out int Wager);

                if (arglist.Count == 1 && TryConvertInt)
                {
                    LogWriter.DebugLog("ParseCommand", DebugLogTypes.CommandSystem, $"Playing blackjack with wager: {Wager}.");
                    GamePlayBlackJack(cmdrow, User, Wager);
                }
                else
                {
                    LogWriter.DebugLog("ParseCommand", DebugLogTypes.CommandSystem, "No wager provided.");
                    result = cmdrow.Usage;
                }
            }
            else if (command == LocalizedMsgSystem.GetVar(DefaultCommand.death))
            {
                LogWriter.DebugLog("ParseCommand", DebugLogTypes.CommandSystem, "Adding a gamecounter death.");
                int counter = AddDeathCounter();

                if (counter != -1)
                {
                    LogWriter.DebugLog("ParseCommand", DebugLogTypes.CommandSystem, $"Death counter added: {counter}.");
                    result = VariableParser.ParseReplace(cmdrow.Message, VariableParser.BuildDictionary(new Tuple<MsgVars, string>[]{
                        new(MsgVars.user, ChannelName),
                        new(MsgVars.value, FormatData.Plurality(counter,MsgVars.Pluraltime)),
                        new(MsgVars.category, Category)
                    }));
                }
            }
            else if (command == LocalizedMsgSystem.GetVar(DefaultCommand.resetdeath))
            {
                LogWriter.DebugLog("ParseCommand", DebugLogTypes.CommandSystem, "Resetting the game death counter.");
                int counter = ResetDeathCounter(arglist.Count != 0 ? Convert.ToInt32(arglist[0]) : 0);

                result = VariableParser.ParseReplace(cmdrow.Message, VariableParser.BuildDictionary(new Tuple<MsgVars, string>[]{
                        new(MsgVars.value, FormatData.Plurality(counter,MsgVars.Pluraltime)),
                        new(MsgVars.category, Category)
                    }));
            }
            else if (command == LocalizedMsgSystem.GetVar(DefaultCommand.viewdeath))
            {
                LogWriter.DebugLog("ParseCommand", DebugLogTypes.CommandSystem, "Viewing the game death counter.");

                int counter = DataManage.GetDeathCounter(FormatData.AddEscapeFormat(Category));

                result = VariableParser.ParseReplace(counter != -1 ? cmdrow.Message : LocalizedMsgSystem.GetVar(Msg.MsgNoDeathCounter), VariableParser.BuildDictionary(new Tuple<MsgVars, string>[]{
                        new(MsgVars.user, ChannelName),
                        new(MsgVars.value, FormatData.Plurality( counter ,MsgVars.Pluraltime)),
                        new(MsgVars.category, Category)
                    }));
            }
            else if (command == LocalizedMsgSystem.GetVar(DefaultCommand.addquote))
            {
                LogWriter.DebugLog("ParseCommand", DebugLogTypes.CommandSystem, "Adding a quote.");

                if (arglist.Count == 0)
                {
                    LogWriter.DebugLog("ParseCommand", DebugLogTypes.CommandSystem, "No quote provided.");
                    result = cmdrow.Usage;
                }
                else
                {
                    LogWriter.DebugLog("ParseCommand", DebugLogTypes.CommandSystem, $"Adding quote: {string.Join(' ', arglist)}.");
                    int quoteNum;

                    quoteNum = DataManage.PostQuote(string.Join(' ', arglist));

                    result = VariableParser.ParseReplace(cmdrow.Message, VariableParser.BuildDictionary(new Tuple<MsgVars, string>[]
                    {
                        new(MsgVars.quotenum, quoteNum.ToString())
                    }));
                }
            }
            else if (command == LocalizedMsgSystem.GetVar(DefaultCommand.quote))
            {
                LogWriter.DebugLog("ParseCommand", DebugLogTypes.CommandSystem, "Getting a quote count.");
                if (arglist.Count > 1)
                {
                    LogWriter.DebugLog("ParseCommand", DebugLogTypes.CommandSystem, "Too many arguments provided.");
                    result = cmdrow.Usage;
                }
                else if (arglist.Count == 0)
                {
                    LogWriter.DebugLog("ParseCommand", DebugLogTypes.CommandSystem, "No quote number provided.");
                    int QuoteCount = DataManage.GetQuoteCount();

                    result = VariableParser.ParseReplace(LocalizedMsgSystem.GetVar(Msg.MsgQuoteNumber), VariableParser.BuildDictionary(new Tuple<MsgVars, string>[]
                       {
                            new(MsgVars.quotenum, FormatData.Plurality(QuoteCount, MsgVars.Pluralquote)),  // determine plurality of "quote/quotes" based on quote count
                            new(MsgVars.be, FormatData.PluralityOnlyWord(QuoteCount, MsgVars.Pluralbe))     // convert 'be' to singular "is" or plural "are" per QuoteCount
                       }));
                }
                else
                {
                    LogWriter.DebugLog("ParseCommand", DebugLogTypes.CommandSystem, $"Getting quote: {arglist[0]}.");
                    result = VariableParser.ParseReplace(cmdrow.Message, VariableParser.BuildDictionary(new Tuple<MsgVars, string>[]
                        {
                            new(MsgVars.quote,
                            $"{LocalizedMsgSystem.GetVar(DefaultCommand.quote)} {DataManage.GetQuote(Convert.ToInt32(arglist[0])) ?? LocalizedMsgSystem.GetVar(Msg.MsgDefaultQuote)}" )
                        }));
                }
            }
            else if (command == LocalizedMsgSystem.GetVar(DefaultCommand.removequote))
            {
                LogWriter.DebugLog("ParseCommand", DebugLogTypes.CommandSystem, "Removing a quote.");
                result = VariableParser.ParseReplace(cmdrow.Message, VariableParser.BuildDictionary(new Tuple<MsgVars, string>[]
                       {
                            new(MsgVars.quotenum, DataManage.RemoveQuote(Convert.ToInt32(arglist[0])) ? arglist[0] : LocalizedMsgSystem.GetVar(Msg.MsgDefaultQuote))
                       }));
            }
            else
            {
                string paramvalue = cmdrow.AllowParam
                    ? arglist == null || arglist.Count == 0 || arglist[0] == string.Empty
                        ? User.UserName
                        : arglist[0].Contains('@') ? arglist[0][1..] : arglist[0]
                    : User.UserName;

                LogWriter.DebugLog("ParseCommand", DebugLogTypes.CommandSystem, $"Parameter value: {paramvalue}.");

                LogWriter.DebugLog("ParseCommand", DebugLogTypes.CommandSystem, "Building variable dictionary for command.");
                datavalues = VariableParser.BuildDictionary(new Tuple<MsgVars, string>[]
                {
                    new( MsgVars.username, paramvalue),
                    new( MsgVars.user, paramvalue ),
                    new( MsgVars.url, paramvalue ),
                    new( MsgVars.time, DateTime.Now.ToLocalTime().ToShortTimeString() ),
                    new( MsgVars.date, DateTime.Now.ToLocalTime().ToShortDateString() ),
                    new( MsgVars.com, paramvalue )
                });

                string UpdateRandomVariable = "";

                if (cmdrow.Message.Contains(VariableParser.Prefix + MsgVars.random.ToString()))
                {
                    LogWriter.DebugLog("ParseCommand", DebugLogTypes.CommandSystem, "Command contains #random_N_D, adding random number to variable dictionary.");
                    
                    string msg = cmdrow.Message;
                    int r_index = msg.IndexOf(VariableParser.Prefix + MsgVars.random.ToString());
                    string randomVar = msg.Substring(r_index, msg.IndexOf(' ', r_index) - r_index); // get the full #random_N_D or #random_N_D_% variable with the N and D

                    string[] randomParams = randomVar.Split('_'); // split into parts

                    Random rnd = new();
                    string randomNumber = Math.Round( 
                                                rnd.NextDouble() * (randomParams.Length > 1 ? Convert.ToInt32(randomParams[1]) : 100), 
                                                Convert.ToInt32(randomParams.Length > 2 ? randomParams[2] : 0) 
                                            ).ToString();

                    if(randomParams.Length == 4)
                    {
                        randomNumber += $" {randomParams[3]}"; // append the optional suffix if it exists
                    }

                    UpdateRandomVariable = randomVar;

                    VariableParser.AddData(ref datavalues, new Tuple<MsgVars, string>[] { new(MsgVars.random, randomNumber) });
                }

                string ShoutuserId = DataManage.GetUserId(new(paramvalue, User.Platform));

                if (command == LocalizedMsgSystem.GetVar(DefaultCommand.so)
                    && !(ShoutuserId != null || BotController.VerifyUserExist(paramvalue, User.Platform)))
                {
                    LogWriter.DebugLog("ParseCommand", DebugLogTypes.CommandSystem, "No user found for shout-out.");
                    result = LocalizedMsgSystem.GetVar(Msg.MsgNoUserFound);
                }
                else
                {
                    LogWriter.DebugLog("ParseCommand", DebugLogTypes.CommandSystem, $"Other command found: {command}.");
                    if (cmdrow.Lookupdata)
                    {
                        LogWriter.DebugLog("ParseCommand", DebugLogTypes.CommandSystem, "Looking up additional data for command.");
                        LookupQuery(cmdrow, paramvalue, ref datavalues);
                    }

                    if (command == LocalizedMsgSystem.GetVar(DefaultCommand.so)
                        && User.Platform == Platform.Twitch)
                    {
                        if (OptionFlags.TwitchChannelUserShoutAPI)
                        {
                            LogWriter.DebugLog("ParseCommand", DebugLogTypes.CommandSystem, "Using Twitch API to shout out user.");
                            LiveUser shoutoutUser = new(userName: paramvalue, Platform.Twitch, userId: ShoutuserId);
                            TwitchShoutOutUser?.Invoke(this, new(shoutoutUser));
                        }
                    }

                    if (cmdrow.Message.Contains(MsgVars.category.ToString()))
                    {
                        ThreadManager.CreateThreadStart("ParseCommand", () =>
                        {
                            LogWriter.DebugLog("ParseCommand", DebugLogTypes.CommandSystem, "Command contains #category, looking up category.");
                            VariableParser.AddData(ref datavalues,
                            new Tuple<MsgVars, string>[] { new(MsgVars.category, BotController.GetUserCategory(ChannelName: paramvalue, UserId: ShoutuserId, bots: User.Platform) ?? LocalizedMsgSystem.GetVar(Msg.MsgNoCategory)) });

                            string resultcat = VariableParser.ParseReplace(cmdrow.Message, datavalues);
                            tempHTMLResponse = VariableParser.ParseReplace(cmdrow.Message, datavalues, true);
                            resultcat = (((OptionFlags.MsgPerComMe && cmdrow.AddMe) || OptionFlags.MsgAddMe) && !resultcat.StartsWith("/me ") ? "/me " : "") + resultcat;

                            OnProcessCommand(resultcat, cmdrow.Announce, cmdrow.SendMsgCount);

                            LogWriter.DebugLog("ParseCommand", DebugLogTypes.CommandSystem, $"Found !so message with a category, {resultcat}.");

                            LogWriter.DebugLog("ParseCommand", DebugLogTypes.CommandSystem, "Checking for Overlay Event.");
                            CheckForOverlayEvent(overlayType: OverlayTypes.Commands,
                                Action: DefaultCommand.so.ToString(),
                                new(userName: paramvalue, Platform.Twitch, userId: ShoutuserId), UserMsg: tempHTMLResponse);
                        });

                        result = "";
                    }
                    else
                    {
                        result = VariableParser.ParseReplace(cmdrow.Message.Replace(UpdateRandomVariable, VariableParser.Prefix+MsgVars.random.ToString()), datavalues);
                        tempHTMLResponse = VariableParser.ParseReplace(cmdrow.Message.Replace(UpdateRandomVariable, VariableParser.Prefix + MsgVars.random.ToString()), datavalues, true);

                        if (command == LocalizedMsgSystem.GetVar(DefaultCommand.so))
                        {
                            LogWriter.DebugLog("ParseCommand", DebugLogTypes.CommandSystem, $"Found !so message without a category, {result}");
                        }
                    }
                }
            }

            if (result != "")
            {
                LogWriter.DebugLog("ParseCommand", DebugLogTypes.CommandSystem, $"Command performed, resulting message: {result}. Checking for Overlay Event.");
                string paramvalue = cmdrow.AllowParam
                    ? arglist == null || arglist.Count == 0 || arglist[0] == string.Empty
                        ? User.UserName
                        : arglist[0].Contains('@') ? arglist[0][1..] : arglist[0]
                    : User.UserName;
                string ShoutuserId = DataManage.GetUserId(new(paramvalue, User.Platform));

                CheckForOverlayEvent(overlayType: OverlayTypes.Commands, Action: command, new(userName: paramvalue, Platform.Twitch, userId: ShoutuserId), UserMsg: tempHTMLResponse);
            }

            result ??= "";
            result = ((((OptionFlags.MsgPerComMe && cmdrow.AddMe) || OptionFlags.MsgAddMe) && !result.StartsWith("/me ") && result != "")
                ? "/me " : "") + result;

            multi = cmdrow.SendMsgCount;

            return result;
        }

        private void OnProcessCommand(string Message, bool announcement = false, int repeatMsg = 0)
        {
            LogWriter.DebugLog("OnProcessCommand", DebugLogTypes.CommandSystem, $"Processing command output: {Message}.");
            ProcessedCommand?.Invoke(this, new() { Msg = Message, Announcement = announcement, RepeatMsg = repeatMsg });
        }

        private string PartyCommand(string command, string DisplayName, string argument, CommandData cmdrow)
        {
            LogWriter.DebugLog("PartyCommand", DebugLogTypes.CommandSystem, $"Processing party command: {command}.");

            UserJoin newuser = new() { ChatUser = DisplayName };
            if (argument != "")
            {
                LogWriter.DebugLog("PartyCommand", DebugLogTypes.CommandSystem, $"Argument provided: {argument}.");
                newuser.GameUserName = argument;
            }

            string response;
            if (command == LocalizedMsgSystem.GetVar(DefaultCommand.queue))
            {
                LogWriter.DebugLog("PartyCommand", DebugLogTypes.CommandSystem, "Checking the queue.");

                List<string> JoinChatUsers = [];
                JoinChatUsers.AddRange(from UserJoin u in JoinCollection
                                       select u.ChatUser);
                response = string.Format("There are {0} users in the join queue: {1}", JoinCollection.Count, JoinCollection.Count == 0 ? "no users!" : string.Join(", ", JoinChatUsers));
            }
            else if (command == LocalizedMsgSystem.GetVar(DefaultCommand.qinfo))
            {
                LogWriter.DebugLog("PartyCommand", DebugLogTypes.CommandSystem, "Retrieving the queue info.");
                response = cmdrow.Message;
            }
            else if (command == LocalizedMsgSystem.GetVar(DefaultCommand.enqueue) || command == LocalizedMsgSystem.GetVar(DefaultCommand.join))
            {
                LogWriter.DebugLog("PartyCommand", DebugLogTypes.CommandSystem, "Joining the queue.");
                if (JoinCollection.Contains(newuser))
                {
                    response = $"You have already joined. You are currently number {JoinCollection.IndexOf(newuser) + 1}.";
                }
                else
                {
                    response = $"You have joined the queue. You are currently {JoinCollection.Count + 1}.";
                    JoinCollection.Add(newuser);
                }
            }
            else if (command == LocalizedMsgSystem.GetVar(DefaultCommand.leave) || command == LocalizedMsgSystem.GetVar(DefaultCommand.dequeue))
            {
                LogWriter.DebugLog("PartyCommand", DebugLogTypes.CommandSystem, "Leaving the queue.");
                response = JoinCollection.Remove(newuser) ? "You are no longer in the queue." : "You are not in the queue.";
            }
            else
            {
                response = "Command not understood!";
            }

            return response;
        }

        private void LookupQuery(CommandData CommData, string paramvalue, ref Dictionary<string, string> datavalues)
        {
            //TODO: the query commands with data lookup needs a lot of work!
            LogWriter.DebugLog("LookupQuery", DebugLogTypes.CommandSystem, $"Performing query: {CommData.CmdName}.");

            switch (CommData.Top)
            {
                case > 0:
                case -1:
                    {
                        LogWriter.DebugLog("LookupQuery", DebugLogTypes.CommandSystem, "Query requests a single result.");
                        if (CommData.Action != CommandAction.Get)
                        {
                            throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, LocalizedMsgSystem.GetVar(ChatBotExceptions.ExceptionInvalidComUsage), CommData.CmdName, CommData.Action, CommandAction.Get.ToString()));
                        }

                        // convert multi-row output to a string
                        string queryoutput = string.Join(", ", from object r in DataManage.PerformQuery(Commands.GetCommands(CommData), CommData.Top)
                                                               let bundle = r as Tuple<object, object>
                                                               where bundle.Item1 == bundle.Item2
                                                               select bundle.Item1);

                        VariableParser.AddData(ref datavalues, new Tuple<MsgVars, string>[] { new(MsgVars.query, queryoutput) });
                        break;
                    }

                default:
                    {
                        LogWriter.DebugLog("LookupQuery", DebugLogTypes.CommandSystem, "Query requests a multiple result.");

                        object querydata = DataManage.PerformQuery(CommandsBase.GetCommands(CommData), paramvalue) ?? "";

                        string output = "";
                        if (querydata.GetType() == typeof(string))
                        {
                            output = (string)querydata;
                        }
                        else if (querydata.GetType() == typeof(TimeSpan))
                        {
                            output = FormatData.FormatTimes((TimeSpan)querydata);
                        }
                        else if (querydata.GetType() == typeof(DateTime))
                        {
                            output = FormatData.FormatTimes((DateTime)querydata);
                        }
                        else if (querydata.GetType() == typeof(int) || querydata.GetType() == typeof(double))
                        {
                            output = ((double)querydata).ToString("N2");
                        }
                        else
                        {
                            output = querydata.ToString();
                        }

                        if (output != null)
                        {
                            VariableParser.AddData(ref datavalues, new Tuple<MsgVars, string>[] { new(MsgVars.query, output) });
                        }
                        break;
                    }
            }
        }

    }
}
