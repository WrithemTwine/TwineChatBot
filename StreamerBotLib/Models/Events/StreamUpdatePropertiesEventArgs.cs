namespace StreamerBotLib.Models.Events
{
    public class StreamUpdatePropertiesEventArgs(string categoryId = "", string categoryName = "") : BotPlatformEventArgs
    {
        public string CategoryId { get; set; } = categoryId;

        public string CategoryName { get; set; } = categoryName;
    }
}
