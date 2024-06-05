using StreamerBotLib.Enums;

namespace StreamerBotLib.Models
{
    public class LearnedMessage
    {
        public string Message { get; set; }
        public MsgTypes MsgType { get; set; }

        public LearnedMessage(string Msg, MsgTypes msgTypes)
        {
            Message = Msg;
            MsgType = msgTypes;
        }

        public static List<LearnedMessage> BuildList(string[] Msgs, MsgTypes msgType)
        {
            List<LearnedMessage> output = new();
            foreach (string M in Msgs)
            {
                output.Add(new(M, msgType));
            }

            return output;
        }
    }
}
