using StreamerBotLib.Enums;
using StreamerBotLib.Events;
using StreamerBotLib.Models;
using StreamerBotLib.Static;

namespace StreamerBotLib.Systems
{
    public partial class ActionSystem
    {
        private static bool ElapsedThread;
        private bool ChatBotStarted;

        private const int TaskDelay = 5000;
        private DateTime chattime;
        private DateTime viewertime;
        private int chats;
        private int priorchats;
        private int viewers;
        private double diluteTime;
        private readonly string RepeatLock = "";

        private static int LastLiveViewerCount = 0;

        /// <summary>
        /// Starts up the repeat timer thread.
        /// </summary>
        public void StartElapsedTimerThread()
        {
            // don't start another thread if the current is still active
            if (!ElapsedThread && ((OptionFlags.RepeatWhenLive && OptionFlags.IsStreamOnline) || !OptionFlags.RepeatWhenLive))
            {
                ChatBotStarted = true;
                ThreadManager.CreateThreadStart("StartElapsedTimerThread", ElapsedCommandTimers);
                ElapsedThread = true;
            }
        }

        /// <summary>
        /// Stops repeat commands thread; for bot shutdown purposes.
        /// </summary>
        public void StopElapsedTimerThread()
        {
            ChatBotStarted = false;
            ElapsedThread = false;
        }

        /// <summary>
        /// Performs the commands with timers > 0 seconds. Runs on a separate thread.
        /// </summary>
        private void ElapsedCommandTimers()
        {
            // TODO: consider some AI bot chat when channel is slower

            List<TimerCommand> RepeatList = [];
            DateTime now = DateTime.Now;
            chattime = now; // the time to check chats sent
            viewertime = now; // the time to check viewers
            chats = GetCurrentChatCount;
            priorchats = chats;
            viewers = GetUserCount;

            try // because this code block runs in a separate thread
            {
                Task.Run(() =>
                {
                    while (ComputeRerunLoop())
                    {
                        diluteTime = CheckDilute();
                        foreach (var item in from Tuple<string, int, List<string>> Timers in DataManage.GetTimerCommands()
                                             where Timers.Item3.Contains(Category) || Timers.Item3.Contains(LocalizedMsgSystem.GetVar(Msg.MsgAllCategory))
                                             let item = new TimerCommand(Timers, diluteTime)
                                             select item)
                        {
                            if (RepeatList.UniqueAdd(item))
                            {
                                Task.Run(() => RepeatCmd(item));
                            }
                        }

                        RepeatList.RemoveAll((r) => r.RepeatTime == 0);

                        Task.Delay(TaskDelay * (1 + (DateTime.Now.Second / 60)));
                    }
                });
            }
            catch (ThreadInterruptedException ex)
            {
                LogWriter.LogException(ex, "ElapsedCommandTimers");
            }

        }

        private double CheckDilute()
        {
            double temp = 1.0; // return 1.0 if the user chooses not to dilute the timers

            UpdateChatUserStats();

            if (OptionFlags.RepeatTimerComSlowdown) // only calculate if user wants diluted/smart-mode repeat commands
            {
                double factor = 
                    // check if user wants to use chat count, viewer count, or both
                    // use factor from either selection or weighted (multiply together) for both 
                    (OptionFlags.RepeatAboveChatCount ? ((OptionFlags.RepeatChatCount == 0) ? 1 : (chats / OptionFlags.RepeatChatCount)) : 1) 
                    * (OptionFlags.RepeatAboveUserCount ? ((OptionFlags.RepeatUserCount == 0) ? 1 : (viewers / OptionFlags.RepeatUserCount)) : 1);

                temp = 1.0 + (factor > 1.0 ? 0 : 1.0 - factor);
            }

            return temp;
        }

        private void UpdateChatUserStats()
        {
            lock (RepeatLock)
            {
                DateTime now = DateTime.Now;

                if ((now - chattime) >= new TimeSpan(0, OptionFlags.RepeatChatMinutes, 0))
                {
                    chattime = now;
                    int currChats = GetCurrentChatCount;
                    chats = currChats - priorchats;
                    priorchats = currChats;
                }

                if ((now - viewertime) >= new TimeSpan(0, OptionFlags.RepeatUserMinutes, 0))
                {
                    viewertime = now;
                    viewers = GetUserCount;
                }
            }
        }

        private bool ComputeRerunLoop()
        {
           // LogWriter.DebugLog("ComputeRerunLoop", DebugLogTypes.RepeatCommandSystem,
           //     $"Variable values: OptionFlags.ActiveToken {OptionFlags.ActiveToken}, ChatBotStarted {ChatBotStarted}, OptionFlags.RepeatTimerCommands {OptionFlags.RepeatTimerCommands}, OptionFlags.RepeatWhenLive {OptionFlags.RepeatWhenLive}, OptionFlags.IsStreamOnline {OptionFlags.IsStreamOnline}, (OptionFlags.RepeatWhenLive && OptionFlags.IsStreamOnline) || !OptionFlags.RepeatWhenLive {(OptionFlags.RepeatWhenLive && OptionFlags.IsStreamOnline) || !OptionFlags.RepeatWhenLive}");

            return OptionFlags.ActiveToken 
                    && ChatBotStarted
                    && OptionFlags.RepeatTimerCommands
                    && ((OptionFlags.RepeatWhenLive && OptionFlags.IsStreamOnline) || !OptionFlags.RepeatWhenLive);
        }

        private bool ComputeRerunLoop(List<string> CategoryList)
        {
            //LogWriter.DebugLog("ComputeRerunLoop", DebugLogTypes.RepeatCommandSystem, $"CategoryList.Contains(Category) || CategoryList.Contains(LocalizedMsgSystem.GetVar(Msg.MsgAllCategory)) {CategoryList.Contains(Category) || CategoryList.Contains(LocalizedMsgSystem.GetVar(Msg.MsgAllCategory))}");

            return ComputeRerunLoop()
                    && (CategoryList.Contains(Category) || CategoryList.Contains(LocalizedMsgSystem.GetVar(Msg.MsgAllCategory)));
        }

        private bool ComputeRepeat()
        {
            //LogWriter.DebugLog("ComputeRepeat", DebugLogTypes.RepeatCommandSystem, $"OptionFlags.RepeatNoAdjustment {OptionFlags.RepeatNoAdjustment}, OptionFlags.RepeatTimerComSlowdown {OptionFlags.RepeatTimerComSlowdown}, OptionFlags.RepeatUseThresholds {OptionFlags.RepeatUseThresholds}, !OptionFlags.RepeatAboveUserCount {!OptionFlags.RepeatAboveUserCount}, viewers >= OptionFlags.RepeatUserCount {viewers >= OptionFlags.RepeatUserCount}, !OptionFlags.RepeatAboveUserCount || viewers >= OptionFlags.RepeatUserCount {!OptionFlags.RepeatAboveUserCount || viewers >= OptionFlags.RepeatUserCount}, !OptionFlags.RepeatAboveChatCount {!OptionFlags.RepeatAboveChatCount}, chats >= OptionFlags.RepeatChatCount {chats >= OptionFlags.RepeatChatCount}, !OptionFlags.RepeatAboveChatCount || chats >= OptionFlags.RepeatChatCount {!OptionFlags.RepeatAboveChatCount || chats >= OptionFlags.RepeatChatCount}");

            return OptionFlags.RepeatNoAdjustment // no limits, just perform repeat command
              || OptionFlags.RepeatTimerComSlowdown // diluted command, performance time
              || (OptionFlags.RepeatUseThresholds
                  && (!OptionFlags.RepeatAboveUserCount || viewers >= OptionFlags.RepeatUserCount) // if user threshold, check threshold, else, accept the check
                  && (!OptionFlags.RepeatAboveChatCount || chats >= OptionFlags.RepeatChatCount) // if chat threshold, check threshold, else, accept the check
                  );
        }

        private void RepeatCmd(TimerCommand cmd)
        {
            int repeat = cmd.RepeatTime;  // determined seconds for the repeat timer commands
            bool ResetLive = false; // flag to check reset when going live and going offline, to avoid continuous resets

            while (repeat != 0 && ComputeRerunLoop(cmd.CategoryList))
            {
                LogWriter.DebugLog("RepeatCmd", DebugLogTypes.RepeatCommandSystem, $"Command {cmd.Command}");
                LogWriter.DebugLog("RepeatCmd", DebugLogTypes.RepeatCommandSystem, $"OptionFlags.ActiveToken {OptionFlags.ActiveToken}, repeat != 0 {repeat != 0}, OptionFlags.IsStreamOnline {OptionFlags.IsStreamOnline}, OptionFlags.RepeatLiveReset {OptionFlags.RepeatLiveReset}, !ResetLive {!ResetLive}");

                if (OptionFlags.IsStreamOnline && OptionFlags.RepeatLiveReset && !ResetLive)
                {
                    if (OptionFlags.RepeatLiveResetShow) // perform command when repeat timers are reset based on live online stream
                    {
                        cmd.SetNow(); // cause command to fire immediately
                    }
                    ResetLive = true;
                }
                else if (!OptionFlags.IsStreamOnline && ResetLive)
                {
                    ResetLive = false;
                }
                LogWriter.DebugLog("RepeatCmd", DebugLogTypes.RepeatCommandSystem, $"cmd.CheckFireTime() {cmd.CheckFireTime()}");

                if (OptionFlags.ActiveToken && cmd.CheckFireTime())
                {
                    try
                    {
                        if (ComputeRepeat())
                        {
                            LogWriter.DebugLog("RepeatCmd", DebugLogTypes.RepeatCommandSystem, $"Performing {cmd.Command}.");
                            OnRepeatEventOccured?.Invoke(this, new TimerCommandsEventArgs() { Message = ParseCommand(cmd.Command, new(BotUserName, Platform.Default), [], DataManage.GetCommand(cmd.Command), out short multi, true), RepeatMsg = multi });
                        }
                    }
                    catch (Exception ex)
                    {
                        LogWriter.LogException(ex, "RepeatCmd");
                    }
                    cmd.UpdateTime(diluteTime);
                }

                Task.Delay(TaskDelay * (1 + (DateTime.Now.Second / 60)));

                if (OptionFlags.ActiveToken)
                {
                    repeat = DataManage.GetTimerCommandTime(cmd.Command);
                }
                cmd.ModifyTime(repeat, diluteTime);
            }

            cmd.ModifyTime(0, diluteTime); // when user changes repeat time for a command to 0, reset the repeat time - outer thread will remove the repeat command

        }

    }
}
