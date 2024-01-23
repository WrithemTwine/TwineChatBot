namespace StreamerBotLib.DataSQL.Models
{
    public class GameDeadCounter(uint id = 0, string categoryId = null, string category = null, uint streamCount = 0, uint counter = 0) : CategoryList(id, categoryId, category, streamCount)
    {
        public uint Counter { get; set; } = counter;
    }
}
