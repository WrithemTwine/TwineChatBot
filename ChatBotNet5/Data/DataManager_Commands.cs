using ChatBot_Net5.Enum;
using ChatBot_Net5.Models;
using ChatBot_Net5.Static;
using ChatBot_Net5.Systems;

using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;

namespace ChatBot_Net5.Data
{
    public partial class DataManager
    {
        #region CommandSystem

        /*
 
!command: <switches-optional> <message>

switches:
-t:<table>   (requires -f)
-f:<field>    (requires -t)
-c:<currency> (requires -f, optional switch)
-unit:<field units>   (optional with -f, but recommended)

-p:<permission>
-top:<number>
-s:<sort>
-a:<action>
-param:<allow params to command>
-timer:<seconds>
-use:<usage message>

-m:<message> -> The message to display, may include parameters (e.g. #user, #field).
         */

        private readonly string DefaulSocialMsg = "Social media url here";

        /// <summary>
        /// Add all of the default commands to the table, ensure they are available
        /// </summary>
        private void SetDefaultCommandsTable()
        {
            lock (_DataSource.Commands)
            {
                if (_DataSource.CategoryList.Select("Category='" + LocalizedMsgSystem.GetVar(Msg.MsgAllCateogry) + "'").Length == 0)
                {
                    _DataSource.CategoryList.AddCategoryListRow(null, LocalizedMsgSystem.GetVar(Msg.MsgAllCateogry));
                    _DataSource.CategoryList.AcceptChanges();
                }

                DataSource.CategoryListRow categoryListRow = (DataSource.CategoryListRow)_DataSource.CategoryList.Select("Category='" + LocalizedMsgSystem.GetVar(Msg.MsgAllCateogry) + "'")[0];

                bool CheckName(string criteria)
                {
                    DataSource.CommandsRow[] datarow = (DataSource.CommandsRow[])_DataSource.Commands.Select("CmdName='" + criteria + "'");
                    if (datarow.Length > 0 && datarow[0].Category == string.Empty)
                    {
                        datarow[0].Category = LocalizedMsgSystem.GetVar(Msg.MsgAllCateogry);
                    }
                    return datarow.Length == 0;
                }

                // TODO: convert commands table to localized strings, except the query parameters should stay in English

                // command name     // msg   // params  
                Dictionary<string, Tuple<string, string>> DefCommandsDictionary = new()
                {
                    { DefaultCommand.addcommand.ToString(), new(LocalizedMsgSystem.GetDefaultComMsg(DefaultCommand.addcommand), "-p:Mod -use:!addcommand command <switches-optional> <message>. See documentation for <switches>.") },
                    // '-top:-1' means all items
                    { DefaultCommand.commands.ToString(), new(LocalizedMsgSystem.GetDefaultComMsg(DefaultCommand.commands), "-t:Commands -f:CmdName -top:-1 -s:ASC -use:!commands") },
                    { DefaultCommand.bot.ToString(), new(LocalizedMsgSystem.GetDefaultComMsg(DefaultCommand.bot), "-use:!bot") },
                    { DefaultCommand.lurk.ToString(), new(LocalizedMsgSystem.GetDefaultComMsg(DefaultCommand.lurk), "-use:!lurk") },
                    { DefaultCommand.worklurk.ToString(), new(LocalizedMsgSystem.GetDefaultComMsg(DefaultCommand.worklurk), "-use:!worklurk") },
                    { DefaultCommand.unlurk.ToString(), new(LocalizedMsgSystem.GetDefaultComMsg(DefaultCommand.unlurk), "-use:!unlurk") },
                    { DefaultCommand.socials.ToString(), new(LocalizedMsgSystem.GetDefaultComMsg(DefaultCommand.socials), "-use:!socials") },
                    { DefaultCommand.so.ToString(), new(LocalizedMsgSystem.GetDefaultComMsg(DefaultCommand.so), "-p:Mod -param:true -use:!so username - only mods can use !so.") },
                    { DefaultCommand.join.ToString(), new(LocalizedMsgSystem.GetDefaultComMsg(DefaultCommand.join), "-use:!join") },
                    { DefaultCommand.leave.ToString(), new(LocalizedMsgSystem.GetDefaultComMsg(DefaultCommand.leave), "-use:!leave") },
                    { DefaultCommand.queue.ToString(), new(LocalizedMsgSystem.GetDefaultComMsg(DefaultCommand.queue), "-p:Mod -use:!queue mods only") },
                    { DefaultCommand.qinfo.ToString(), new(LocalizedMsgSystem.GetDefaultComMsg(DefaultCommand.qinfo), "-use:!qinfo") },
                    { DefaultCommand.qstart.ToString(), new(LocalizedMsgSystem.GetDefaultComMsg(DefaultCommand.qstart), "-p:Mod -use:!qstart mod only") },
                    { DefaultCommand.qstop.ToString(), new(LocalizedMsgSystem.GetDefaultComMsg(DefaultCommand.qstop), "-p:Mod -use:!qstop mod only") },
                    { DefaultCommand.follow.ToString(), new(LocalizedMsgSystem.GetDefaultComMsg(DefaultCommand.follow), "-use:!follow") },
                    { DefaultCommand.watchtime.ToString(), new(LocalizedMsgSystem.GetDefaultComMsg(DefaultCommand.watchtime), "-t:Users -f:WatchTime -param:true -use:!watchtime or !watchtime <user>") },
                    { DefaultCommand.uptime.ToString(), new(LocalizedMsgSystem.GetDefaultComMsg(DefaultCommand.uptime), "-use:!uptime") },
                    { DefaultCommand.followage.ToString(), new(LocalizedMsgSystem.GetDefaultComMsg(DefaultCommand.followage), "-t:Followers -f:FollowedDate -param:true -use:!followage or !followage <user>") }
                };

                foreach (DefaultSocials social in System.Enum.GetValues(typeof(DefaultSocials)))
                {
                    DefCommandsDictionary.Add(social.ToString(), new(DefaulSocialMsg, "-use:!<social_name> -top:0"));
                }

                foreach (string key in DefCommandsDictionary.Keys)
                {
                    if (CheckName(key))
                    {
                        CommandParams param = CommandParams.Parse(DefCommandsDictionary[key].Item2);
                        _DataSource.Commands.AddCommandsRow(key, false, param.Permission.ToString(), DefCommandsDictionary[key].Item1, param.Timer, categoryListRow, param.AllowParam, param.Usage, param.LookupData, param.Table, GetKey(param.Table), param.Field, param.Currency, param.Unit, param.Action, param.Top, param.Sort);
                    }
                }
            }
        }

        /// <summary>
        /// Check if the provided table exists within the database system.
        /// </summary>
        /// <param name="table">The table name to check.</param>
        /// <returns><i>true</i> - if database contains the supplied table, <i>false</i> - if database doesn't contain the supplied table.</returns>
        internal bool CheckTable(string table)
        {
            lock (_DataSource)
            {
                return _DataSource.Tables.Contains(table);
            }
        }

        /// <summary>
        /// Check if the provided field is part of the supplied table.
        /// </summary>
        /// <param name="table">The table to check.</param>
        /// <param name="field">The field within the table to see if it exists.</param>
        /// <returns><i>true</i> - if table contains the supplied field, <i>false</i> - if table doesn't contain the supplied field.</returns>
        internal bool CheckField(string table, string field)
        {
            lock (_DataSource)
            {
                return _DataSource.Tables[table].Columns.Contains(field);
            }
        }

        /// <summary>
        /// Determine if the user invoking the command has permission to access the command.
        /// </summary>
        /// <param name="cmd">The command to verify the permission.</param>
        /// <param name="permission">The supplied permission to check.</param>
        /// <returns><i>true</i> - the permission is allowed to the command. <i>false</i> - the command permission is not allowed.</returns>
        /// <exception cref="InvalidOperationException">The command is not found.</exception>
        internal bool CheckPermission(string cmd, ViewerTypes permission)
        {
            lock (_DataSource.Commands)
            {
                DataSource.CommandsRow[] rows = (DataSource.CommandsRow[])_DataSource.Commands.Select("CmdName='" + cmd + "'");

                if (rows != null && rows.Length > 0)
                {
                    ViewerTypes cmdpermission = (ViewerTypes)System.Enum.Parse(typeof(ViewerTypes), rows[0].Permission);

                    return cmdpermission >= permission;
                }
                else
                    throw new InvalidOperationException(LocalizedMsgSystem.GetVar(ChatBotExceptions.ExceptionInvalidOpCommand));
            }
        }

        /// <summary>
        /// Verify if the provided UserName is within the ShoutOut table.
        /// </summary>
        /// <param name="UserName">The UserName to shoutout.</param>
        /// <returns>true if in the ShoutOut table.</returns>
        /// <remarks>Thread-safe</remarks>
        internal bool CheckShoutName(string UserName)
        {
            lock (_DataSource.ShoutOuts)
            {
                return _DataSource.ShoutOuts.Select("UserName='" + UserName + "'").Length > 0;
            }
        }

        internal string GetKey(string Table)
        {
            string key = "";

            if (Table != "")
            {
                DataColumn[] k = _DataSource?.Tables[Table]?.PrimaryKey;
                if (k?.Length > 1)
                {
                    foreach (DataColumn d in k)
                    {
                        if (d.ColumnName != "Id")
                        {
                            key = d.ColumnName;
                        }
                    }
                }
                else
                {
                    key = k?[0].ColumnName;
                }
            }
            return key;
        }

        internal string AddCommand(string cmd, CommandParams Params)
        {
            //string strParams = Params.DBParamsString();
            DataSource.CategoryListRow categoryListRow = (DataSource.CategoryListRow)_DataSource.CategoryList.Select("Category='" + LocalizedMsgSystem.GetVar(Msg.MsgAllCateogry) + "'")[0];

            lock (_DataSource.Commands)
            {
                _DataSource.Commands.AddCommandsRow(cmd, Params.AddMe, Params.Permission.ToString(), Params.Message, Params.Timer, categoryListRow, Params.AllowParam, Params.Usage, Params.LookupData, Params.Table, GetKey(Params.Table), Params.Field, Params.Currency, Params.Unit, Params.Action, Params.Top, Params.Sort);
                SaveData();
                OnPropertyChanged(nameof(Commands));
            }
            return string.Format(CultureInfo.CurrentCulture, "Command {0} added!", cmd);
        }

        internal string GetSocials()
        {
            string filter = "";

            foreach (DefaultSocials s in System.Enum.GetValues(typeof(DefaultSocials)))
            {
                filter += "'" + s.ToString() + "',";
            }

            DataSource.CommandsRow[] socialrows = null;
            lock (_DataSource.Commands)
            {
                socialrows = (DataSource.CommandsRow[])_DataSource.Commands.Select("CmdName='" + LocalizedMsgSystem.GetVar(DefaultCommand.socials) + "'");
            }

            string socials = socialrows[0].Message;

            if (OptionFlags.MsgPerComMe && socialrows[0].AddMe == true)
            {
                socials = "/me " + socialrows[0].Message;
            }

            lock (_DataSource.Commands)
            {
                socialrows = (DataSource.CommandsRow[])_DataSource.Commands.Select("CmdName IN (" + filter[0..^1] + ")");
            }

            foreach (DataSource.CommandsRow com in socialrows)
            {
                if (com.Message != DefaulSocialMsg && com.Message != string.Empty)
                {
                    socials += com.Message + " ";
                }
            }

            return socials.Trim();
        }

        internal string GetUsage(string command)
        {
            lock (_DataSource.Commands)
            {
                DataSource.CommandsRow[] usagerows = (DataSource.CommandsRow[])_DataSource.Commands.Select("CmdName='" + command + "'");

                return usagerows[0]?.Usage ?? LocalizedMsgSystem.GetVar(Msg.MsgNoUsage);
            }
        }

        // older code
        //internal string PerformCommand(string cmd, string InvokedUser, string ParamUser, List<string> ParamList=null)
        //{
        //    DataSource.CommandsRow[] comrow = null;

        //    lock (_DataSource.Commands)
        //    {
        //        comrow = (DataSource.CommandsRow[])_DataSource.Commands.Select("CmdName='" + cmd + "'");
        //    }

        //    if (comrow == null || comrow.Length == 0)
        //    {
        //        throw new KeyNotFoundException( "Command not found." );
        //    }

        //    //object[] value = comrow[0].Params != string.Empty ? PerformQuery(comrow[0], InvokedUser, ParamUser) : null;

        //    string user = (comrow[0].AllowParam ? ParamUser : InvokedUser);
        //    if (user.Contains('@'))
        //    {
        //        user = user.Remove(0,1);
        //    }

        //    Dictionary<string, string> datavalues = new()
        //    {
        //        { "#user", user },
        //        { "#url", "http://www.twitch.tv/" + user }
        //    };

        //    return BotController.ParseReplace(comrow[0].Message, datavalues);
        //}

        internal DataSource.CommandsRow GetCommand(string cmd)
        {
            DataSource.CommandsRow[] comrow = null;

            lock (_DataSource.Commands)
            {
                comrow = (DataSource.CommandsRow[])_DataSource.Commands.Select("CmdName='" + cmd + "'");
            }

            //if (comrow == null || comrow.Length == 0)
            //{
            //    throw new KeyNotFoundException(LocalizedMsgSystem.GetVar(ChatBotExceptions.ExceptionKeyNotFound));
            //}

            return comrow?[0];
        }

        internal object PerformQuery(DataSource.CommandsRow row, string ParamValue)
        {
            //CommandParams query = CommandParams.Parse(row.Params);
            DataRow result = null;

            lock (_DataSource)
            {
                DataRow[] temp = _DataSource.Tables[row.table].Select(row.key_field + "='" + ParamValue + "'");

                result = temp.Length > 0 ? temp[0] : null;


                if (result == null)
                {
                    return LocalizedMsgSystem.GetVar(Msg.MsgDataNotFound);
                }

                Type resulttype = result.GetType();

                // certain tables have certain outputs - still deciphering how to optimize the data query portion of commands
                if (resulttype == typeof(DataSource.UsersRow))
                {
                    DataSource.UsersRow usersRow = (DataSource.UsersRow)result;
                    return usersRow[row.data_field];
                }
                else if (resulttype == typeof(DataSource.FollowersRow))
                {
                    DataSource.FollowersRow follower = (DataSource.FollowersRow)result;

                    return follower.IsFollower ? follower.FollowedDate : LocalizedMsgSystem.GetVar(Msg.MsgNotFollower);
                }
                else if (resulttype == typeof(DataSource.CurrencyRow))
                {

                }
                else if (resulttype == typeof(DataSource.CurrencyTypeRow))
                {

                }
                else if (resulttype == typeof(DataSource.CommandsRow))
                {

                }
            }

            return result;
        }

        internal object[] PerformQuery(DataSource.CommandsRow row, int Top = 0)
        {
            DataTable tabledata = _DataSource.Tables[row.table]; // the table to query
            DataRow[] output;
            List<Tuple<object, object>> outlist = new();

            lock (_DataSource)
            {
                output = Top < 0 ? tabledata.Select() : tabledata.Select(null, row.key_field + " " + row.sort);

                foreach (DataRow d in output)
                {
                    outlist.Add(new(d[row.key_field], d[row.data_field]));
                }
            }

            if (Top > 0)
            {
                outlist.RemoveRange(Top, outlist.Count - Top);
            }

            outlist.Sort();

            return outlist.ToArray();
        }

        /// <summary>
        /// Retrieves the commands with a timer setting > 0 seconds.
        /// </summary>
        /// <returns>The list of commands and the seconds to repeat the command.</returns>
        internal List<Tuple<string, int, string[]>> GetTimerCommands()
        {
            lock (_DataSource.Commands)
            {
                List<Tuple<string, int, string[]>> TimerList = new();
                foreach (DataSource.CommandsRow row in (DataSource.CommandsRow[])_DataSource.Commands.Select("RepeatTimer>0"))
                {
                    TimerList.Add(new(row.CmdName, row.RepeatTimer, row.Category?.Split(',') ?? Array.Empty<string>()));
                }
                return TimerList;
            }
        }

        #endregion
    }
}
