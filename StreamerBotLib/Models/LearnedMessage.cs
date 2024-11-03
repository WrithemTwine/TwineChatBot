using StreamerBotLib.Enums;

namespace StreamerBotLib.Models
{
    public class LearnedMessage(string Msg, MsgTypes msgTypes)
    {
        public string Message { get; set; } = Msg;
        public MsgTypes MsgType { get; set; } = msgTypes;

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
