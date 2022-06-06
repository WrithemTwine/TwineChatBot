using MediaOverlayServer.Models;

using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Xml.Linq;


namespace MediaOverlayServer.Communication
{
    internal static class ProcessHyperText
    {
        private static string RefreshToken(int Duration)
        {
            return $"<meta http-equiv=\"refresh\" content=\"{Duration}\" />";
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

        internal static string ProcessPage(string OverlayStyle, string OverlayBody, int Duration, bool IsMedia=false)
        {
            return $"<html><head>{(IsMedia?"":RefreshToken(Duration))}<style>{OverlayStyle}</style></head><body><div>{OverlayBody}</div></body></html>";
        }

        internal static string ProcessOverlay(OverlayActionType overlayActionType)
        {
            // todo: finish building output

            string Img = "";

            if (overlayActionType.ImageFile != "")
            {
                Size sz = Image.FromFile(overlayActionType.ImageFile, false).Size;

                Img = new XElement("img", new XAttribute("src", overlayActionType.ImageFile), new XAttribute("width", sz.Width), new XAttribute("height", sz.Height)).ToString();
            }

            string Media = "";

            Dictionary<string, string> audio = new() { { ".mp3", "audio/mpeg" }, { ".wav", "audio/wav" } };
            Dictionary<string, string> video = new() { { ".mp4", "video/mp4" }, { ".webm", "video/webm" }, { ".ogg", "video/ogg" } };
            string ext = Path.GetExtension(overlayActionType.MediaFile);

            string BuildMediaElement(string tag, string Extension, Dictionary<string, string> MediaTypes)
            {
                return new XElement($"{tag}", new XAttribute("id","myalert"), new XAttribute("onended","window.location.reload();"), new XElement("source", new XAttribute("src", overlayActionType.MediaFile), new XAttribute("type", MediaTypes[Extension]))).ToString().Replace($"<{tag}", $"<{tag} autoplay");
            }

            if (overlayActionType.MediaFile.Contains("http"))
            {
                Media = new XElement("iframe", new XAttribute("id", "myalert"), new XElement("src", overlayActionType.MediaFile)).ToString();
            } 
            else if (audio.ContainsKey(ext))
            {
                Media = BuildMediaElement("audio", ext, audio);
            }
            else if (video.ContainsKey(ext))
            {
                Media = BuildMediaElement("video", ext, video);
            }

            string Msg = $"<div class=\"message\">{overlayActionType.Message}</div>";

            return ProcessPage(new OverlayStyle(overlayActionType.OverlayType.ToString()).OverlayStyleText, Img + Media + Msg, overlayActionType.Duration, Media != "");
        }

        internal static string DefaultStyle
        {
            get
            {
                return @"
        .message {
            color: black;
            text-align: center;
        }

        #myalert {
            width: 480px;
            height: 320px;
        }

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
