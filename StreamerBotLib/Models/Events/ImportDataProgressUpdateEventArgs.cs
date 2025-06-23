namespace StreamerBotLib.Models.Events
{
    public class ImportDataProgressUpdateEventArgs(int currentProgressAmount) : EventArgs
    {
        public int CurrentProgressAmount { get; set; } = currentProgressAmount;
    }
}
