using StreamerBotLib.Enums;

using System.Collections;
using System.Diagnostics;

namespace StreamerBotLib.Models
{
    [DebuggerDisplay("{ViewerType}, {MsgType}, {ModAction}, {TimeoutSeconds}")]
    public class BanViewerRule : IEqualityComparer
    {
        public ViewerTypes ViewerType { get; set; }
        public MsgTypes MsgType { get; set; }
        public ModActions ModAction { get; set; }
        public string TimeoutSeconds { get; set; } = "0";

        public new bool Equals(object x, object y)
        {
            BanViewerRule BanViewX = (BanViewerRule)x;
            BanViewerRule BanViewY = (BanViewerRule)y;

            return BanViewX.ViewerType == BanViewY.ViewerType && BanViewX.MsgType == BanViewY.MsgType && BanViewX.ModAction == BanViewY.ModAction && BanViewX.TimeoutSeconds == BanViewY.TimeoutSeconds;
        }

        public int GetHashCode(object obj)
        {
            BanViewerRule data = (BanViewerRule)obj;

            return (data.ViewerType.ToString() + data.MsgType.ToString() + data.ModAction.ToString() + data.TimeoutSeconds.ToString()).GetHashCode();
        }
    }
}
