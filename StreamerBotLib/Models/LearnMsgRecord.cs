namespace StreamerBotLib.Models
{
    public record LearnMsgRecord
    {
        public int Id { get; set; }
        public string MsgType { get; set; }
        public string TeachingMsg { get; set; }

        public LearnMsgRecord(int id, string msgType, string teachingMsg)
        {
            Id = id;
            MsgType = msgType;
            TeachingMsg = teachingMsg;
        }
    }
}
