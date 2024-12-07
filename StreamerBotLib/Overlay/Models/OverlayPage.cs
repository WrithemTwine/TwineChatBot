using StreamerBotLib.Overlay.Interfaces;

namespace StreamerBotLib.Overlay.Models
{
    public class OverlayPage : IEquatable<OverlayPage>, IOverlayPageReadOnly
    {
        public string OverlayType { get; set; } = "";
        public string OverlayHyperText { get; set; } = "";

        public bool Equals(OverlayPage other)
        {
            return OverlayType == other?.OverlayType;
        }

        public OverlayPage()
        {

        }

        public override bool Equals(object obj)
        {
            return Equals(obj as OverlayPage);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(OverlayType, OverlayHyperText);
        }

        //public OverlayPage(OverlayActionType actionType)
        //{
        //    OverlayType = actionType.OverlayType.ToString();

        //    using(StreamReader sr = new($"/{(OptionFlags.UseSameOverlayStyle ? "" : OverlayType + "/")}{PublicConstants.OverlayPageName}"))
        //    {
        //        OverlayHyperText = sr.ReadToEnd();
        //    }

        //}


    }
}
