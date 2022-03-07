namespace StreamerBotLib.Enums
{
    /// <summary>
    /// The different types of Twitch viewers. Do not sort because the order matters for access hierarchy
    /// </summary>
    public enum ViewerTypes { Broadcaster, Mod, VIP, Follower, Sub, Viewer }

    public class ViewerTypesString
    {
        public string ViewerTypesValues
        {
            get
            {
                string Data = "";

                foreach(ViewerTypes s in System.Enum.GetValues(typeof(ViewerTypes)))
                {
                    Data += s.ToString() + ", ";
                }

                return Data[0..^2];
            }
        }
    }
}
