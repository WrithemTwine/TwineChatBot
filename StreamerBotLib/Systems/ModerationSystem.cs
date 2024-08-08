using StreamerBotLib.Enums;
using StreamerBotLib.MLearning;
using StreamerBotLib.Models;
using StreamerBotLib.Static;

using System.Reflection;

namespace StreamerBotLib.Systems
{
    internal partial class ActionSystem
    {
        private List<Tuple<string, Task, DateTime>> RequestApprovalList = new();

        private List<string> describe = new();

        #region Auto-Mod Messages
        public void ManageLearnedMsgList()
        {
            lock (GUI.GUIDataManagerLock.Lock)
            {
                List<LearnMsgRecord> learnMsgsRows = DataManage.UpdateLearnedMsgs();
                if (learnMsgsRows != null)
                {
                    List<BotModAction> botModActions = new();

                    foreach (LearnMsgRecord M in learnMsgsRows)
                    {
                        botModActions.Add(new()
                        {
                            LearnMsg = M.TeachingMsg,
                            ModActions = (MsgTypes)Enum.Parse(typeof(MsgTypes), M.MsgType)
                        });
                    }

                    MessageAnalysis.UpdateLearningList(botModActions);
                }
            }
        }

        public Tuple<ModActions, int, MsgTypes, BanReasons> ModerateMessage(CmdMessage MsgReceived)
        {
            ManageLearnedMsgList();

            lock (GUI.GUIDataManagerLock.Lock)
            {
                MsgTypes Found = MessageAnalysis.Predict(MsgReceived.Message);

                Tuple<ModActions, BanReasons, int> remedy = DataManage.FindRemedy(MsgReceived.UserType, Found);

                return new(remedy.Item1, remedy.Item3, Found, remedy.Item2);
            }
        }
        #endregion

        #region User Requests

        internal static Tuple<string, string> GetApprovalRule(ModActionType ActionType, string Command)
        {
            return DataManage.CheckModApprovalRule(ActionType, FormatData.AddEscapeFormat(Command));
        }

        /// <summary>
        /// Adds a request to the approval list.
        /// </summary>
        /// <param name="Description">A description of the request to approve.</param>
        /// <param name="Request">The Task of the request to perform once approved.</param>
        public void AddApprovalRequest(string Description, Task Request)
        {
            bool ItemCount = false;
            lock (RequestApprovalList)
            {
                ItemCount = RequestApprovalList.Count == 0;
                RequestApprovalList.Add(new(Description, Request, DateTime.Now.ToLocalTime()));
            }

            if (ItemCount)
            {
                ThreadManager.CreateThreadStart(MethodBase.GetCurrentMethod().Name, () => { MonitorApprovals(); });
            }
        }

        /// <summary>
        /// Retrieve the numbered description list for each request.
        /// </summary>
        /// <returns>A numbered list of request descriptions.</returns>
        public List<string> GetDescriptions()
        {
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
        public string GetLabel(string Idx)
        {
            lock (RequestApprovalList)
            {
                return RequestApprovalList[Convert.ToInt32(Idx)].Item1;
            }
        }

        /// <summary>
        /// A moderator approved a specific request, and this method runs the approved request.
        /// </summary>
        /// <param name="Label">The specific request to approve.</param>
        public void RunApprovedRequest(string Label)
        {
            lock (RequestApprovalList)
            {
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
                DateTime Expiry = DateTime.Now;
                List<Tuple<string, Task, DateTime>> toRemove = new();

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
