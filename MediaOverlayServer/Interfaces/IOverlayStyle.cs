namespace MediaOverlayServer.Interfaces
{
    public interface IOverlayStyle
    {
        public string OverlayType { get; set; }
        public string OverlayStyleText { get; set; }

        public void SaveFile();
    }
}