namespace StreamerBotLib.Models
{
    public record LearnMsgRecord
    {
        public uint Id { get; set; }
        public string MsgType { get; set; }
        public string TeachingMsg { get; set; }

        public LearnMsgRecord(uint id, string msgType, string teachingMsg)
        {
            Id = id;
            MsgType = msgType;
            TeachingMsg = teachingMsg;
        }
    }
}
