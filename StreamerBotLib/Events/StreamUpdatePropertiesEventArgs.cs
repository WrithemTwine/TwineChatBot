namespace StreamerBotLib.Events
{
    public class StreamUpdatePropertiesEventArgs(string categoryId = "", string categoryName = "") : EventArgs
    {
        public string CategoryId { get; set; } = categoryId;

        public string CategoryName { get; set; } = categoryName;
    }
}
