using StreamerBotLib.Models.Enums;
using StreamerBotLib.Models.Events;
using StreamerBotLib.Static;
using StreamerBotLib.Systems;

namespace StreamerBotLib.Models.Repeat
{
    internal class RepeatManager
    {
        internal event EventHandler<TimerCommandsEventArgs> OnRepeatEventOccured;
        internal event EventHandler<EventArgs> OnRepeatCheckStopped;

        internal bool IsStarted { get; private set; } = false;

        private readonly ActionSystem _actionsystem;
        private RepeatCommandMode _repeatcommandmethod;

        private const int TaskDelay = 5000;
        private DateTime chattime;
        private DateTime viewertime;
        private int chats;
        private int priorchats;
        private int viewers;
        private double diluteTime = 1.0;

        internal RepeatManager(ActionSystem actionsystem)
        {
            LogWriter.DebugLog(".ctor_RepeatManager", DebugLogTypes.RepeatCommandSystem, "Build repeat manager object.");
            _actionsystem = actionsystem;
        }

        internal void Start()
        {
            if (!IsStarted)
            {
                LogWriter.DebugLog("Start", DebugLogTypes.RepeatCommandSystem, "Starting the repeat manager.");
                IsStarted = true;

                if (OptionFlags.RepeatParallelMode)
                {
                    LogWriter.DebugLog("Start", DebugLogTypes.RepeatCommandSystem, "Starting parallel mode.");
                    _repeatcommandmethod = new RepeatParallelMode();
                }
                else
                {
                    LogWriter.DebugLog("Start", DebugLogTypes.RepeatCommandSystem, "Starting serial mode.");
                    _repeatcommandmethod = new RepeatSerialMode();
                }

                _repeatcommandmethod.OnRepeatEventOccured += repeatcommandmethod_OnRepeatEventOccured;
                UpdateCommands();

                ThreadManager.CreateThreadStart("RepeatManager_Start", () => ComputeUpdate());
            }
        }

        private void repeatcommandmethod_OnRepeatEventOccured(object sender, RepeatCommandFoundEventArgs e)
        {
            LogWriter.DebugLog("RepeatEventOccured", DebugLogTypes.RepeatCommandSystem, "A repeat command is ready to perform.");
            LogWriter.DebugLog("RepeatEventOccured", DebugLogTypes.RepeatCommandSystem, $"Testing parameters, Straight time {OptionFlags.RepeatNoAdjustment}; Smart slowdown {OptionFlags.RepeatTimerComSlowdown}; Use Thresholds {OptionFlags.RepeatUseThresholds}: viewers {viewers} >= {OptionFlags.RepeatUserCount}: chats {chats} >= {OptionFlags.RepeatChatCount}.");

            // wait until last possible moment to decide whether to fire off the repeat command, if user
            // changes the settings
            if (OptionFlags.RepeatNoAdjustment // no limits, just perform repeat command
             || OptionFlags.RepeatTimerComSlowdown // diluted command, performance time
             || OptionFlags.RepeatUseThresholds
                 && (!OptionFlags.RepeatAboveUserCount || viewers >= OptionFlags.RepeatUserCount) // if user threshold, check threshold, else, accept the check
                 && (!OptionFlags.RepeatAboveChatCount || chats >= OptionFlags.RepeatChatCount) // if chat threshold, check threshold, else, accept the check
                 )
            {
                if (e.platform == Platform.Twitch)
                {
                    LogWriter.DebugLog("RepeatEventOccured", DebugLogTypes.RepeatCommandSystem, $"Perform the repeat command: {e.Command}.");
                    OnRepeatEventOccured?.Invoke(this, new TimerCommandsEventArgs()
                    {
                        Message = _actionsystem.ParseCommand(
                                           e.Command,
                                           new(OptionFlags.TwitchBotUserName, e.platform),
                                           [],
                                           ActionSystem.DataManage.GetCommand(e.Command),
                                           out short multi, true),
                        RepeatMsg = multi
                    });
                }
            }
        }

        private void ComputeUpdate()
        {
            while (OptionFlags.ActiveToken && IsStarted)
            {
                UpdateChatUserStats();

                if (OptionFlags.RepeatTimerComSlowdown) // only calculate if user wants diluted/smart-mode repeat commands
                {
                    double factor =
                        // check if user wants to use chat count, viewer count, or both
                        // use factor from either selection or weighted (multiply together) for both 
                        (OptionFlags.RepeatAboveChatCount ? (OptionFlags.RepeatChatCount == 0 ? 1 : chats / OptionFlags.RepeatChatCount) : 1)
                        * (OptionFlags.RepeatAboveUserCount ? (OptionFlags.RepeatUserCount == 0 ? 1 : viewers / OptionFlags.RepeatUserCount) : 1);

                    diluteTime = 1.0 + (factor > 1.0 ? 0 : 1.0 - factor);
                    _repeatcommandmethod.UpdateDiluteTime(diluteTime);
                }

                Task.Delay(TaskDelay).Wait();
            }
            OnRepeatCheckStopped?.Invoke(this, new());
        }

        private void UpdateChatUserStats()
        {
            LogWriter.DebugLog("UpdateChatUserStats", DebugLogTypes.RepeatCommandSystem, "Updating the chat user stats for computing dilute time.");
            DateTime now = DateTime.Now;

            if (_actionsystem.GetCurrentChatCount >= OptionFlags.RepeatChatCount || now - chattime >= new TimeSpan(0, OptionFlags.RepeatChatMinutes, 0))
            {
                chattime = now;
                chats = _actionsystem.GetCurrentChatCount - priorchats;
                priorchats = _actionsystem.GetCurrentChatCount;
                LogWriter.DebugLog("UpdateChatUserStats", DebugLogTypes.RepeatCommandSystem, $"Updated the chat counts: current {chats}, prior {priorchats}.");
            }

            if (_actionsystem.GetUserCount >= OptionFlags.RepeatUserCount || now - viewertime >= new TimeSpan(0, OptionFlags.RepeatUserMinutes, 0))
            {
                viewertime = now;
                viewers = _actionsystem.GetUserCount;
                LogWriter.DebugLog("UpdateChatUserStats", DebugLogTypes.RepeatCommandSystem, $"Updated the viewer counts: viewers {viewers}.");
            }
        }

        internal void UpdateCommands()
        {
            LogWriter.DebugLog("UpdateCommands", DebugLogTypes.RepeatCommandSystem, "Updating commands.");
            _repeatcommandmethod?.AddCommands(ActionSystem.DataManage.GetTimerCommands(), diluteTime);
        }

        internal void UpdateCategory(string CategoryName)
        {
            LogWriter.DebugLog("UpdateCategory", DebugLogTypes.RepeatCommandSystem, $"Updating the new category {CategoryName}.");
            RepeatCommandMode.UpdateCategory(CategoryName);
        }

        internal void NotifyRepeatManager_StreamOnline()
        {
            _repeatcommandmethod?.NotifyRepeatManager_StreamOnline();
        }

        internal void Stop()
        {
            if (IsStarted)
            {
                LogWriter.DebugLog("Stop", DebugLogTypes.RepeatCommandSystem, "Stopping the repeat manager.");
                IsStarted = false;
            }
        }
    }
}
