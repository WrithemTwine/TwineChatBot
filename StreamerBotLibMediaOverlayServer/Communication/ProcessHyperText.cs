using StreamerBotLibMediaOverlayServer.Models;

using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Xml.Linq;

namespace StreamerBotLibMediaOverlayServer.Communication
{
    /// <summary>
    /// Provides HTML strings to supply via the HTTP server, includes building an Overlay media page based on provided image, audio, and/or video.
    /// </summary>
    public static class ProcessHyperText
    {
        private static string RefreshToken(int Duration)
        {
            return $"<meta http-equiv=\"refresh\" content=\"{Duration}\" />";
        }

        public static string DefaultPage
        {
            get
            {
                return $"<html><head>{RefreshToken(2)}</head><body></body></html>";
            }
        }

        public static string DefaultActionPage
        {
            get
            {
                return $"<html><head>{RefreshToken(2)}</head><body></body></html>";
            }
        }

        public static string ProcessPage(string OverlayStyle, string OverlayBody, int Duration, bool IsMedia = false)
        {
            return $"<html><head>{(IsMedia ? "" : RefreshToken(Duration))}<style>{OverlayStyle}</style></head><body><div class=\"maindiv\">{OverlayBody}</div></body></html>";
        }

        public static string ProcessOverlay(OverlayActionType overlayActionType)
        {
            // todo: finish building output

            string Img = "";

            if (overlayActionType.ImageFile != "" && File.Exists(overlayActionType.ImageFile))
            {
                Size sz = Image.FromFile(overlayActionType.ImageFile, false).Size;
                Img = new XElement("img", new XAttribute("class", "image"), new XAttribute("src", overlayActionType.ImageFile), new XAttribute("width", sz.Width), new XAttribute("height", sz.Height)).ToString();
            }

            string Media = "";

            Dictionary<string, string> audio = new() { { ".mp3", "audio/mpeg" }, { ".wav", "audio/wav" } };
            Dictionary<string, string> video = new() { { ".mp4", "video/mp4" }, { ".webm", "video/webm" }, { ".ogg", "video/ogg" } };
            string ext = Path.GetExtension(overlayActionType.MediaFile);

            string BuildMediaElement(string tag, string Extension, Dictionary<string, string> MediaTypes)
            {
                return new XElement($"{tag}", new XAttribute("id", "myalert"), new XAttribute("onended", "window.location.reload();"), new XElement("source", new XAttribute("src", overlayActionType.MediaFile), new XAttribute("type", MediaTypes[Extension]))).ToString().Replace($"<{tag}", $"<{tag} autoplay");
            }

            if (overlayActionType.MediaFile.Contains("http"))
            {
                Media = new XElement("iframe", new XAttribute("id", "myalert"), new XElement("src", overlayActionType.MediaFile)).ToString();
            }
            else if (audio.ContainsKey(ext) && File.Exists(overlayActionType.MediaFile))
            {
                Media = BuildMediaElement("audio", ext, audio);
            }
            else if (video.ContainsKey(ext) && File.Exists(overlayActionType.MediaFile))
            {
                Media = BuildMediaElement("video", ext, video);
            }

            string Msg = $"<div class=\"message\">{overlayActionType.Message}</div>";

            return ProcessPage(new OverlayStyle(overlayActionType.OverlayType.ToString()).OverlayStyleText, Img + Media + Msg, overlayActionType.Duration, Media != "");
        }

        public static string DefaultStyle
        {
            get
            {
                return @"
        .maindiv {
            /* style class for page div,  <body><div class=""maindiv"" />...</body>   */
            text-align: center;
        }

        .message {
            /* style class for Overlay message   */

            color: black;
            text-align: center;
        }

    /*  image styling element  */
        .image {
            height: 500px;
        }

    /*  For external video/media links, currently not really used since Twitch requires secure access for the embedded video player, possibly YouTube videos  */
        #myalert {
            width: 480px;
            height: 320px;
        }

    /*  styles for message variables - these are from the primary chat message and parsed to add these classes based on the variable.  */
    /*  e.g. ""Thanks #user for following!!"" -parses to-> ""Thanks <span=""user"">{user name}</span> for following!"" using the ""user"" class style. */


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
