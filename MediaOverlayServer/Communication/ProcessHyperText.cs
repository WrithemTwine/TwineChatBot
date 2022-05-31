using MediaOverlayServer.Models;

namespace MediaOverlayServer.Communication
{
    internal static class ProcessHyperText
    {
        private static string RefreshToken(int Duration)
        {
            return $"<meta http-equiv=\"refresh\" content=\"{Duration}\">";
        }

        internal static string DefaultPage
        {
            get
            {
                return $"<html><head>{RefreshToken(2)}</head><body></body></html>";
            }
        }

        internal static string DefaultActionPage
        {
            get
            {
                return $"<html><head>{RefreshToken(2)}</head><body></body></html>";
            }
        }

        internal static string ProcessPage(string OverlayStyle, string OverlayBody, int Duration)
        {
            return $"<html><head>{RefreshToken(Duration)}<style>{OverlayStyle}</style></head><body><div>{OverlayBody}</div></body></html>";
        }

        internal static string ProcessOverlay(OverlayActionType overlayActionType)
        {
            // todo: finish building output

            string Img = overlayActionType.ImageFile == "" ? string.Empty : $"<img src=\"{overlayActionType.ImageFile}\" />";
            string Media = $"<media />";
            string Msg = $"<div>{overlayActionType.Message}</div>";


            return ProcessPage(new OverlayStyle(overlayActionType.OverlayType.ToString()).OverlayStyleText, Img + Media + Msg, overlayActionType.Duration );
        }

        internal static string DefaultStyle
        {
            get
            {
                return @"
        .autohost {
            font-size: 24px;
        }

        .bits {
            font-size: 24px;
        }

        .category {
            font-size: 24px;
        }

        .com {
            font-size: 24px;
        }

        .count {
            font-size: 24px;
        }

        .date {
            font-size: 24px;
        }

        .months {
            font-size: 24px;
        }

        .query {
            font-size: 24px;
        }

        .receiveuser {
            font-size: 24px;
        }

        .streak {
            font-size: 24px;
        }

        .submonths {
            font-size: 24px;
        }

        .subplan {
            font-size: 24px;
        }

        .subplanname {
            font-size: 24px;
        }

        .time {
            font-size: 24px;
        }

        .title {
            font-size: 24px;
        }

        .uptime {
            font-size: 24px;
        }

        .url {
            font-size: 24px;
        }

        .winner {
            font-size: 24px;
        }

        .viewers {
            font-size: 24px;
        }

        .user {
            font-size: 24px;
        }
";
            }
        }
    }
}
