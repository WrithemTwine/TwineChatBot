using StreamerBotLib.Models;
using StreamerBotLib.Models.Enums;
using StreamerBotLib.Static;
using StreamerBotLib.Systems.MLearning;

namespace StreamerBotLib.Systems
{
    public partial class ActionSystem
    {
        private readonly List<Tuple<string, Task, DateTime>> RequestApprovalList = [];

        private readonly List<string> describe = [];

        #region Auto-Mod Messages
        private static void ManageLearnedMsgList()
        {
            ThreadManager.AddTaskToGUIDispatcher(() =>
            {
                LogWriter.DebugLog("ManageLearnedMsgList", DebugLogTypes.ModerationSystem, "Checking for learned messages.");
                List<LearnMsgRecord> learnMsgsRows = DataManage.UpdateLearnedMsgs();
                if (learnMsgsRows != null)
                {
                    MessageAnalysis.UpdateLearningList((from LearnMsgRecord M in learnMsgsRows
                                                        select new BotModAction()
                                                        {
                                                            LearnMsg = M.TeachingMsg,
                                                            ModActions = (MsgTypes)Enum.Parse(typeof(MsgTypes), M.MsgType)
                                                        }).ToList());
                }
            });
        }

        private Tuple<ModActions, int, MsgTypes, BanReasons> ModerateMessage(CmdMessage MsgReceived)
        {
            LogWriter.DebugLog("ModerateMessage", DebugLogTypes.ModerationSystem, $"Moderating message from {MsgReceived.DisplayName}.");
            ManageLearnedMsgList();

            MsgTypes Found = MessageAnalysis.Predict(MsgReceived.Message);

            Tuple<ModActions, BanReasons, int> remedy = DataManage.FindRemedy(MsgReceived.UserType, Found);

            return new(remedy.Item1, remedy.Item3, Found, remedy.Item2);
        }
        #endregion

        #region User Requests

        public Tuple<string, string> GetApprovalRule(ModActionType ActionType, string Command)
        {
            LogWriter.DebugLog("GetApprovalRule", DebugLogTypes.ModerationSystem, $"Checking for approval rule for {Command}.");
            return DataManage.CheckModApprovalRule(ActionType, FormatData.AddEscapeFormat(Command));
        }

        /// <summary>
        /// Adds a request to the approval list.
        /// </summary>
        /// <param name="Description">A description of the request to approve.</param>
        /// <param name="Request">The Task of the request to perform once approved.</param>
        public void PostApproval(string Description, Task Request)
        {
            LogWriter.DebugLog("AddApprovalRequest", DebugLogTypes.ModerationSystem, $"Adding a new approval request for {Description}.");
            bool ItemCount = false;
            lock (RequestApprovalList)
            {
                ItemCount = RequestApprovalList.Count == 0;
                RequestApprovalList.Add(new(Description, Request, DateTime.Now.ToLocalTime()));
            }

            if (ItemCount)
            {
                ThreadManager.CreateThreadStart("AddApprovalRequest", () => { MonitorApprovals(); });
            }
        }

        /// <summary>
        /// Retrieve the numbered description list for each request.
        /// </summary>
        /// <returns>A numbered list of request descriptions.</returns>
        private List<string> GetDescriptions()
        {
            LogWriter.DebugLog("GetDescriptions", DebugLogTypes.ModerationSystem, $"Getting approval list.");
            describe.Clear();
            lock (RequestApprovalList)
            {
                int x = 1;
                foreach (Tuple<string, Task, DateTime> tuple in RequestApprovalList)
                {
                    describe.Add($"{x}. {tuple.Item1}");
                    x++;
                }
            }
            return describe;
        }

        /// <summary>
        /// Find the label of the queue list at a specific index.
        /// </summary>
        /// <param name="Idx">The index of the label to retrieve</param>
        /// <returns>The description label of the request.</returns>
        private string GetLabel(string Idx)
        {
            LogWriter.DebugLog("GetLabel", DebugLogTypes.ModerationSystem, $"Getting the label, {RequestApprovalList[Convert.ToInt32(Idx)].Item1}.");
            lock (RequestApprovalList)
            {
                return RequestApprovalList[Convert.ToInt32(Idx)].Item1;
            }
        }

        /// <summary>
        /// A moderator approved a specific request, and this method runs the approved request.
        /// </summary>
        /// <param name="Label">The specific request to approve.</param>
        private void RunApprovedRequest(string Label)
        {
            lock (RequestApprovalList)
            {
                LogWriter.DebugLog("RunApprovedRequest", DebugLogTypes.ModerationSystem, $"Performing approval for {Label}.");
                Tuple<string, Task, DateTime> RemoveTuple = null;
                foreach (var tuple in from Tuple<string, Task, DateTime> tuple in RequestApprovalList
                                      where tuple.Item1 == Label
                                      select tuple)
                {
                    ThreadManager.CreateThreadStart(tuple.Item2);
                    RemoveTuple = tuple;
                }

                if (RemoveTuple != null)
                {
                    RequestApprovalList.Remove(RemoveTuple);
                }
            }
        }

        /// <summary>
        /// A method with a 30second time loop, checking the RequestApprovalList for expired approvals
        /// and removing them.
        /// </summary>
        private void MonitorApprovals()
        {
            while (RequestApprovalList.Count > 0)
            {
                LogWriter.DebugLog("MonitorApprovals", DebugLogTypes.ModerationSystem, $"Found {RequestApprovalList.Count} items to approve.");

                DateTime Expiry = DateTime.Now;
                List<Tuple<string, Task, DateTime>> toRemove = [];

                lock (RequestApprovalList)
                {
                    foreach (var tuple in RequestApprovalList)
                    {
                        if (tuple.Item3.AddMinutes(OptionFlags.ModeratorApprovalTimeout) < Expiry)
                        {
                            toRemove.Add(tuple);
                        }
                    }

                    foreach (var tuple in toRemove)
                    {
                        RequestApprovalList.Remove(tuple);
                    }
                }

                Thread.Sleep(30000);
            }
        }

        #endregion
    }
}
