using StreamerBotLib.Enums;
using StreamerBotLib.Models;
using StreamerBotLib.Overlay.Control;
using StreamerBotLib.Overlay.Models;
using StreamerBotLib.Static;

using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace StreamerBotLib.Overlay.Communication
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
            return $"<html>\n" +
                $"<head>{(IsMedia ? "" : RefreshToken(Duration))}\n<style>\n{OverlayStyle}\n</style>\n</head>\n" +
                $"<body>\n<div class=\"maindiv\">{OverlayBody}</div>\n</body>\n" +
                $"</html>\n";
        }

        public static string ProcessOverlay(OverlayActionType overlayActionType)
        {
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

        public static OverlayPage ProcessTicker(TickerItem tickerItem)
        {
            string style =  $"\n" +
                            $".maindiv {{\n" +
                            $"/* style class for page div,  <body><div class=\" maindiv\" />...</body>   */\n" +
                            $"  text-align: center;\n" +
                            $"}}\n" +
                            $"\n";

            return
                new OverlayPage()
                {
                    OverlayType = tickerItem.OverlayTickerItem.ToString(),
                    OverlayHyperText = ProcessPage(
                        style + new OverlayStyle(tickerItem.OverlayTickerItem).OverlayStyleText,
                        ProcessTicker(tickerItem.OverlayTickerItem, tickerItem.UserName).ToString()
                        , 5)
                };
        }

        private static XElement ProcessTicker(OverlayTickerItem tickerItem, string UserName)
        {
            return new XElement("span",
                            new XAttribute("class", tickerItem),
                            $"{tickerItem}: {UserName}");
        }

        public static OverlayPage ProcessTicker(IEnumerable<TickerItem> tickerItems)
        {
            string style = $"\n" +
                            $".maindiv {{\n" +
                            $"  /* style class for page div,  <body><div class=\" maindiv\" />...</body>   */\n" +
                            $"  text-align: center;\n" +
                            $"}}\n";

            return
                new OverlayPage()
                {
                    OverlayType = "All",
                    OverlayHyperText = ProcessPage(
                        // just send any item, the correct style returns
                        style + new OverlayStyle(OverlayTickerItem.LastFollower).OverlayStyleText,
                           string.Join(" ",from TickerItem T in tickerItems
                            let Elem = ProcessTicker(T.OverlayTickerItem, T.UserName)
                            select Elem.ToString())
                        , 5)
                };
        }

        public static string DefaultTickerStyle(string TagClassName)
        {
            string style = "/* Ticker Items */\n" +
                            $"\n" +
                            $"span {{" +
                            $"  display: flex;" +
                            $"  float: left;\n"+
                            $"  padding: 10px;\n" +
                            $"  font-size: 175%;\n" +
                            $"  text-align: left;\n" +
                            $" /* set width so the tags don't move for different name lengths */" +
                            $"  width: 250px;"+
                            $"}}\n";

            /*
https://www.w3docs.com/snippets/css/how-to-have-the-marquee-effect-without-using-the-marquee-tag-with-css-javascript-and-jquery.html
      p {
        -moz-animation: marquee 10s linear infinite;
        -webkit-animation: marquee 10s linear infinite;
        animation: marquee 10s linear infinite;
      } 
            
      @-moz-keyframes marquee {
        0% {
          transform: translateX(100%);
        }
        100% {
          transform: translateX(-100%);
        }
      }
      @-webkit-keyframes marquee {
        0% {
          transform: translateX(100%);
        }
        100% {
          transform: translateX(-100%);
        }
      }
      @keyframes marquee {
        0% {
          -moz-transform: translateX(100%);
          -webkit-transform: translateX(100%);
          transform: translateX(100%)
        }
        100% {
          -moz-transform: translateX(-100%);
          -webkit-transform: translateX(-100%);
          transform: translateX(-100%);
        }
      }



https://isotropic.co/how-to-marquee-elements/
            @import url("https://fonts.googleapis.com/css2?family=Corben:wght@700&display=swap");

* {
  box-sizing: border-box;
}

body {
  min-height: 100vh;
}

.demo_marquee-wrap {
  --demo-marquee_space: 2rem;
  display: grid;
  align-content: center;
  overflow: hidden;
  gap: var(--demo-marquee_space);
  width: 100%;
  font-family: "Corben", system-ui, sans-serif;
  font-size: 1.5rem;
  line-height: 1.5;
}

.marquee {
  --demo-marquee_duration: 60s;
  --demo-marquee_gap: var(--demo-marquee_space);

  display: flex;
  overflow: hidden;
  user-select: none;
  gap: var(--demo-marquee_gap);
  transform: skewY(-3deg);
}

.marquee__group {
  flex-shrink: 0;
  display: flex;
  align-items: center;
  justify-content: space-around;
  gap: var(--demo-marquee_gap);
  min-width: 100%;
  animation: scroll var(--demo-marquee_duration) linear infinite;
}

@media (prefers-reduced-motion: reduce) {
  .marquee__group {
    animation-play-state: paused;
  }
}

.marquee__group img {
  max-width: clamp(10rem, 1rem + 28vmin, 20rem);
  aspect-ratio: 1;
  object-fit: cover;
  border-radius: 1rem;
}

.marquee__group p {
  color:#29303e;
}

.marquee--borders {
  border-block: 3px solid #29303e;
  padding-block: 0.75rem;
}

.marquee--reverse .marquee__group {
  animation-direction: reverse;
  animation-delay: calc(var(--demo-marquee_duration) / -2);
}

@keyframes scroll {
  0% {
    transform: translateX(0);
  }

  100% {
    transform: translateX(calc(-100% - var(--demo-marquee_gap)));
  }
}



        

            */

            if (TagClassName == "All Tickers")
            {
                foreach (SelectedTickerItem S in TickerFormatter.selectedTickerItems)
                {
                    style += TickerStyle(S.OverlayTickerItem);
                }
            }
            else
            {
                style += $"{TickerStyle(TagClassName)}";
            }

            static string TickerStyle(string TagName)
            {
                return $"\n" +
                    $".{TagName} {{\n" +
                    $"  color: red;\n" +
                    $"}}\n";
            }

            return style;
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
