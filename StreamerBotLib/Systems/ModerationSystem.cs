using StreamerBotLib.Enums;
using StreamerBotLib.MachineLearning;
using StreamerBotLib.Models;

using System;
using System.Collections.Generic;

using static StreamerBotLib.Data.DataSource;

namespace StreamerBotLib.Systems
{
    internal partial class ActionSystem
    {
        public void ManageLearnedMsgList()
        {
            lock (GUI.GUIDataManagerLock.Lock)
            {
                List<LearnMsgsRow> learnMsgsRows = DataManage.UpdateLearnedMsgs();
                if (learnMsgsRows != null)
                {
                    List<BotModAction> botModActions = new();

                    foreach (LearnMsgsRow M in learnMsgsRows)
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
    }
}
