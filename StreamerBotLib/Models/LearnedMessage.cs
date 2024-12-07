using StreamerBotLib.Enums;

namespace StreamerBotLib.Models
{
    public class LearnedMessage(string Msg, MsgTypes msgTypes)
    {
        public string Message { get; set; } = Msg;
        public MsgTypes MsgType { get; set; } = msgTypes;

        public static List<LearnedMessage> BuildList(string[] Msgs, MsgTypes msgType)
        {
            return (from string M in Msgs
                    select new LearnedMessage(M, msgType)).ToList();
        }
    }
}
