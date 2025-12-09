using StreamerBotLib.Models;
using StreamerBotLib.Models.Enums;
using StreamerBotLib.Static;
using StreamerBotLib.Systems.Overlay.Control;
using StreamerBotLib.Systems.Overlay.Enums;
using StreamerBotLib.Systems.Overlay.Models;
using StreamerBotLib.Systems.Overlay.Static;

using System.Drawing;
using System.IO;
using System.Xml.Linq;

namespace StreamerBotLib.Systems.Overlay.Communication
{
    /// <summary>
    /// Provides HTML strings to supply via the HTTP server, includes building an Overlay media page based on provided image, audio, and/or video.
    /// </summary>
    public static class ProcessHyperText
    {
        private static string RefreshToken(int Duration)
        {
            return $"<meta http-equiv=\"refresh\" Content=\"{Duration}\" />";
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

        public static string ProcessPage(string OverlayStyle, string OverlayBody, int Duration, bool IsMedia = false, string script = "", string bodyevent = "")
        {
            // LogWriter.DebugLog("ProcessPage", DebugLogTypes.OverlaySystem, $"Preparing text for Overlay, style {OverlayStyle}, body {OverlayBody}, duration {Duration}, IsMedia {IsMedia}, script {script}, bodyevent {bodyevent}");

            return $"<html>\n" +
                $"<head>{(IsMedia ? "" : RefreshToken(Duration))}\n{(script == "" ? "" : script)}\n" +
                $"<style>\n{OverlayStyle}\n</style>\n</head>\n" +
                $"<body {(bodyevent == "" ? "" : bodyevent)}>\n" +
                $"<div class=\"maindiv\">\n{OverlayBody}\n</div>\n</body>\n" +
                $"</html>\n";
        }

        public static string ProcessOverlay(OverlayActionType overlayActionType, OverlayStyle overlayStyle, out string ImageHyperText, out string VideoHyperText)
        {
            // LogWriter.DebugLog("ProcessOverlay", DebugLogTypes.OverlaySystem, $"Overlay type {overlayActionType}, Overlay style {overlayStyle}.");

            string Img = string.Empty;
            bool ShoutClip = false; // flag to set the refresh element of a clip page, due to not having the handlers for when the video finishes

            if (overlayActionType.ImageFile != "" && File.Exists(overlayActionType.ImageFile))
            {
                Size sz = Image.FromFile(overlayActionType.ImageFile, false).Size;
                Img = new XElement("img", new XAttribute("class", "image"), new XAttribute("src", overlayActionType.ImageFile), new XAttribute("width", sz.Width), new XAttribute("height", sz.Height)).ToString();
            }

            if (overlayActionType.ActionValue == LocalizedMsgSystem.GetVar(DefaultCommand.time))
            {
                Img = SVGClock();
            }

            string Media = string.Empty;

            Dictionary<string, string> audio = new() { { ".mp3", "audio/mpeg" }, { ".wav", "audio/wav" } };
            Dictionary<string, string> video = new() { { ".mp4", "video/mp4" }, { ".webm", "video/webm" }, { ".ogg", "video/ogg" } };
            string ext = Path.GetExtension(overlayActionType.MediaFile);

            string BuildMediaElement(string tag, string Extension, Dictionary<string, string> MediaTypes)
            {
                return new XElement($"{tag}", new XAttribute("id", "myalert"), new XAttribute("onended", "window.location.reload();"), new XElement("source", new XAttribute("src", overlayActionType.MediaFile), new XAttribute("type", MediaTypes[Extension]))).ToString().Replace($"<{tag}", $"<{tag} autoplay");
            }

            if (overlayActionType.OverlayType == OverlayTypes.Commands && overlayActionType.ActionValue == DefaultCommand.so.ToString())
            {
                ShoutClip = true;
                Media = new XElement("iframe",
                            new XAttribute("src", overlayActionType.MediaFile + $"&muted=false&parent=localhost&autoplay=true"),
                            new XAttribute("height", OptionFlags.MediaOverlayClipHeight),
                            new XAttribute("width", OptionFlags.MediaOverlayClipWidth)
                    ).ToString();
            }
            else if (overlayActionType.MediaFile.Contains("http"))
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

            ImageHyperText = ProcessPage(overlayStyle.OverlayStyleText, Img, overlayActionType.Duration);
            VideoHyperText = ProcessPage(overlayStyle.OverlayStyleText, Media, overlayActionType.Duration, Media != "" && !ShoutClip);

            return ProcessPage(overlayStyle.OverlayStyleText, Msg, overlayActionType.Duration);
        }

        public static OverlayPage ProcessTicker(TickerItem tickerItem, IEnumerable<OverlayStyle> overlayStyles)
        {
            string style = $"\n" +
                            $".maindiv {{\n" +
                            $"/* style class for page div,  <body><div class=\" maindiv\" />...</body>   */\n" +
                            $"  text-align: center;\n" +
                            $"}}\n" +
                            $"\n";

            string tickericonstyle = ParseTickerIconStyle([tickerItem]);

            return
                new OverlayPage()
                {
                    OverlayType = tickerItem.OverlayTickerItem.ToString(),
                    OverlayHyperText = ProcessPage(
                        style + "\n" + tickericonstyle + (from OverlayStyle O in overlayStyles
                                 where O.OverlayType == tickerItem.OverlayTickerItem.ToString()
                                 select O).FirstOrDefault()?.OverlayStyleText ?? "",
                        ProcessTicker(tickerItem.OverlayTickerItem, tickerItem.UserName).ToString()
                        , 5)
                };
        }

        private static XElement ProcessTicker(OverlayTickerItem tickerItem, string UserName)
        {
            if (OptionFlags.MediaOverlayTickerIcons)
            {
                int size = 50;

                return new XElement("div", 
                                    new XAttribute("class", tickerItem),
                            new XElement("img", 
                                new XAttribute("id", tickerItem.ToString()), 
                                new XAttribute("alt", $"{tickerItem} icon"), 
                                new XAttribute("width", size), 
                                new XAttribute("height", size)),
                            new XElement("span", $": {UserName}"));
            }
            else
            {
                return new XElement("span",
                          new XAttribute("class", tickerItem),
                                         $"{tickerItem}: {UserName}");
            }
        }

        private static string ParseTickerIconStyle(IEnumerable<TickerItem> tickerItems)
        {
            if (OptionFlags.MediaOverlayTickerIcons)
            { // use icons, build style
                string CompiledStyle = "";

                string[] IconFile = [];
                string iconstyle = Path.Combine(PublicConstants.BaseTickerPath, PublicConstants.BaseTickerIconPath, PublicConstants.TickerIconsStyle);

                if (File.Exists(iconstyle))
                { // read existing file
                    using(StreamReader reader = new(iconstyle))
                    {
                        IconFile = reader.ReadToEnd().Split('}');
                    }
                }

                foreach(TickerItem t in tickerItems)
                {
                    string found = "";
                    foreach(string s in IconFile)
                    { // check existing file for data
                        if (s.Contains(t.OverlayTickerItem.ToString()))
                        {
                            found = s + "}\n"; // the split removes the closing }
                        }
                    }

                    if(found == "")
                    { // build a default style
                        found = $"#{t.OverlayTickerItem} {{\n    content: Url('{PublicConstants.DefaultTickerIcons[t.OverlayTickerItem.ToString()]}');\n    float: left;\n}}\n\n ";
                    }

                    CompiledStyle += found;
                }

                return CompiledStyle;
            }
            else
            { // no icon style
                return "";
            }
        }

        public static OverlayPage ProcessTicker(IEnumerable<TickerItem> tickerItems, IEnumerable<OverlayStyle> overlayStyles)
        {
            //            string UpdaterEvents()
            //            {
            //                //$(""#LastSubscriber"").load(location.href + "" #LastSubscriber"");
            //                string values = "";

            //                foreach (SelectedTickerItem S in TickerFormatter.selectedTickerItems)
            //                {
            //                    values += $"$(\"#{S.OverlayTickerItem}\").load(location.href + \" #{S.OverlayTickerItem}\")\n";
            //                }

            //                return values;
            //            }

            //            string script = @"
            //<script src=""http://ajax.googleapis.com/ajax/libs/jquery/2.0.2/jquery.min.js""></script>
            //<script type=""text/javascript"">

            //function reloadelements()
            //{
            //    // commented out since jQuery requires http hosting, i.e. test in bot via webserver

            //    setInterval(
            //    function (){" +
            //    UpdaterEvents() +
            //@"    }    
            //    , 500);
            //}
            //</script>
            //";

            string style = $"\n" +
                            $".maindiv {{\n" +
                            $"  /* style class for page div,  <body><div class=\" maindiv\" />...</body>   */\n" +
                            $"  text-align: left;\n" +
                            $"}}\n";

            string tickericonstyle = ParseTickerIconStyle(tickerItems);

            string marqueeStyle = "";

            int reloadtime = 5;

            if (OptionFlags.MediaOverlayTickerRotate)
            {
                marqueeStyle = (from OverlayStyle O in overlayStyles
                                where O.OverlayType == TickerStyle.MultiRotate.ToString()
                                select O).First().OverlayStyleText;
                reloadtime = OptionFlags.MediaOverlayTickerRotateTime * tickerItems.Count();
            }
            else if (OptionFlags.MediaOverlayTickerMarquee)
            {
                marqueeStyle = (from OverlayStyle O in overlayStyles
                                where O.OverlayType == TickerStyle.MultiMarquee.ToString()
                                select O).First().OverlayStyleText;
                reloadtime = OptionFlags.MediaOverlayTickerMarqueeTime;
            }

            List<XElement> spans = [.. from TickerItem T in tickerItems
                                       select ProcessTicker(T.OverlayTickerItem, T.UserName)];

            string body = "";

            if (OptionFlags.MediaOverlayTickerMulti)
            {
                if (OptionFlags.MediaOverlayTickerMarquee)
                {
                    body = new XElement("div", new XAttribute("class", "marquee"), spans.ToArray()).ToString();
                }
                else if (OptionFlags.MediaOverlayTickerRotate)
                {
                    body = new XElement("div", new XAttribute("class", "rotate"), spans.ToArray()).ToString();
                }
            }
            else
            {
                foreach (XElement span in spans)
                {
                    body += span.ToString() + "\n";
                }
            }

            return
                new OverlayPage()
                {
                    OverlayType = "All",
                    OverlayHyperText = ProcessPage(
                        style + "\n" + tickericonstyle + "\n" + ((from OverlayStyle O in overlayStyles
                                  where O.OverlayType == PublicConstants.OverlayAllTickers
                                  select O).FirstOrDefault()?.OverlayStyleText ?? "") + marqueeStyle,
                        body
                        , reloadtime, false, "", "")
                };
        }

        internal static string DefaultTickerStyle(TickerStyle tickerStyle)
        {
            string output = "";

            switch (tickerStyle)
            {
                case TickerStyle.Single:
                    {
                        break;
                    }
                case TickerStyle.MultiMarquee:
                    {
                        output = @"
    .marquee {
        width: 180%;
        white-space: nowrap;
        background-color: #949494;
        overflow: hidden;
        display: inline-block;
        -moz-transform: translateX(100%);
        -webkit-transform: translateX(100%);
        transform: translateX(100%);
" +
$"        -moz-animation: ticker {OptionFlags.MediaOverlayTickerMarqueeTime}s linear infinite;\n" +
$"        -webkit-animation: ticker {OptionFlags.MediaOverlayTickerMarqueeTime}s linear infinite;\n" +
$"        animation: ticker {OptionFlags.MediaOverlayTickerMarqueeTime}s linear infinite;\n" +
@"    }

    .marquee span {
        width: auto;
    }

    @keyframes ticker {
        0% {
            opacity: 0%;
            -moz-transform: translateX(100%);
            -webkit-transform: translateX(100%);
            transform: translateX(100%);
        }

        20% { opacity: 100%; }
        50% { opacity: 100%; }

        100% {
            opacity: 0%;
            -moz-transform: translateX(-100%);
            -webkit-transform: translateX(-100%);
            transform: translateX(-100%);
        }
    }

    @-moz-keyframes ticker {
        0% {
            opacity: 0%;
            -moz-transform: translateX(100%);
        }

        20% { opacity: 100%; }
        50% { opacity: 100%; }

        100% {
            opacity: 0%;
            -moz-transform: translateX(-100%);
        }
    }

    @-webkit-keyframes ticker {
        0% {
            opacity: 0%;
            -webkit-transform: translateX(100%);
        }

        20% { opacity: 100%; }
        50% { opacity: 100%; }

        100% {
            opacity: 0%;
            -webkit-transform: translateX(-100%);
        }
    }";
                        break;
                    }
                case TickerStyle.MultiStatic:
                    {
                        break;
                    }
                case TickerStyle.MultiRotate:
                    {
                        int itemcount = TickerFormatter.selectedTickerItems.Count;

                        output = "\n.rotate span {\n" +
                            $"  position: absolute;\n" +
                            $"  opacity: 100%;\n" +
                            "   width: auto;\n" +
                            $"  animation: tickerrotate {OptionFlags.MediaOverlayTickerRotateTime * itemcount}s linear infinite;\n" +
                            $"}}\n\n" +
                            $".rotate {{" +
                            $"  background-color: #949494;\n" +
                            $"}}\n" +
                            $"@keyframes tickerrotate {{\n" +
                            $"  0% {{ opacity: 0%; }}\n" +
                            $"  {Math.Round(100.00 / (itemcount + 1), 2)}% {{ opacity: 100%; }}\n" +
                            $"  {Math.Round(200.00 / (itemcount + 1), 2)}% {{ opacity: 0%; }}\n" +
                            $"}}\n\n";

                        for (int x = 1; x <= itemcount; x++)
                        {
                            output += $".rotate span:nth-child({x}) {{" +
                                 "  opacity: 0%;\n" +
                                $"  animation-delay: {(x - 1) * OptionFlags.MediaOverlayTickerRotateTime}s;\n" +
                                $"}}\n\n";
                        }

                        break;
                    }
            }


            return output;
        }

        public static string DefaultTickerStyle(string TagClassName)
        {
            string style = "/* Ticker Items */\n" +
                            $"\n" +
                            $"span {{" +
                            $"  display: flex;" +
                            $"  float: left;\n" +
                            $"  padding: 10px;\n" +
                            $"  font-size: 175%;\n" +
                            $"  text-align: left;\n" +
                            $" /* set width so the tags don't move for different name lengths */" +
                            $"  width: 450px;" +
                            $"}}\n";

            /*
             * https://stackoverflow.com/questions/18490026/refresh-reload-the-Content-in-div-using-jquery-ajax
             * 
             * https://stackoverflow.com/questions/16231359/animating-elements-sequentially-in-pure-css3-on-loop
             * 
             * https://blog.hubspot.com/website/scrolling-text-css
             * 
             * https://stackoverflow.com/questions/56639772/how-to-create-a-marquee-that-appears-infinite-using-css-or-javascript
             * 
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
  align-Content: center;
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
  justify-Content: space-around;
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

            if (TagClassName == PublicConstants.OverlayAllTickers)
            {
                // since "All tickers" CSS is generated once, add all items if user changes which ticker items are added - the CSS tags are in the file.
                foreach (var S in Enum.GetNames<OverlayTickerItem>())
                {
                    style += TickerStyle(S);
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

        internal static string SVGClock()
        {
            return @"
<!--
Copyright (c) 2025 by Gene (https://codepen.io/gene7299/pen/eJeoPq)

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the ""Software""), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
-->

<svg version=""1.1"" class=""iconic-clock"" xmlns=""http://www.w3.org/2000/svg"" xmlns:xlink=""http://www.w3.org/1999/xlink"" x=""0px"" y=""0px"" width=""384px"" height=""384px"" viewBox=""0 0 384 384"" enable-background=""new 0 0 384 384"" xml:space=""preserve"">
  <path class=""iconic-clock-frame""    d=""M192,0C85.961,0,0,85.961,0,192s85.961,192,192,192s192-85.961,192-192S298.039,0,192,0zM315.037,315.037c-9.454,9.454-19.809,17.679-30.864,24.609l-14.976-25.939l-10.396,6l14.989,25.964c-23.156,12.363-48.947,19.312-75.792,20.216V336h-12v29.887c-26.845-0.903-52.636-7.854-75.793-20.217l14.989-25.963l-10.393-6l-14.976,25.938c-11.055-6.931-21.41-15.154-30.864-24.608s-17.679-19.809-24.61-30.864l25.939-14.976l-6-10.396l-25.961,14.99C25.966,250.637,19.017,224.846,18.113,198H48v-12H18.113c0.904-26.844,7.853-52.634,20.216-75.791l25.96,14.988l6.004-10.395L44.354,99.827c6.931-11.055,15.156-21.41,24.61-30.864s19.809-17.679,30.864-24.61l14.976,25.939l10.395-6L110.208,38.33C133.365,25.966,159.155,19.017,186,18.113V48h12V18.113c26.846,0.904,52.635,7.853,75.792,20.216l-14.991,25.965l10.395,6l14.978-25.942c11.056,6.931,21.41,15.156,30.865,24.611c9.454,9.454,17.679,19.808,24.608,30.863l-25.94,14.976l6,10.396l25.965-14.99c12.363,23.157,19.312,48.948,20.218,75.792H336v12h29.887c-0.904,26.845-7.853,52.636-20.216,75.792l-25.964-14.989l-6.002,10.396l25.941,14.978C332.715,295.229,324.491,305.583,315.037,315.037z"" />
  <line class=""iconic-clock-hour-hand"" id=""foo"" fill=""none"" stroke=""#000000"" stroke-width=""18"" stroke-miterlimit=""10"" x1=""192"" y1=""192"" x2=""192"" y2=""87.5""/>
  <line class=""iconic-clock-minute-hand"" id=""iconic-anim-clock-minute-hand"" fill=""none"" stroke=""#000000"" stroke-width=""12"" stroke-miterlimit=""10"" x1=""192"" y1=""192"" x2=""192"" y2=""54""/>
  <circle class=""iconic-clock-axis"" cx=""192"" cy=""192"" r=""9""/>
  <g class=""iconic-clock-second-hand"" id=""iconic-anim-clock-second-hand"">
      <line class=""iconic-clock-second-hand-arm"" fill=""none"" stroke=""#D53A1F"" stroke-width=""4"" stroke-miterlimit=""10"" x1=""192"" y1=""192"" x2=""192"" y2=""28.5""/>
      <circle class=""iconic-clock-second-hand-axis"" fill=""#D53A1F"" cx=""192"" cy=""192"" r=""4.5""/>
  </g>
  <defs>
    <animateTransform
          type=""rotate""
          fill=""remove""
          restart=""always""
          calcMode=""linear""
          accumulate=""none""
          additive=""sum""
          xlink:href=""#iconic-anim-clock-hour-hand""
          repeatCount=""indefinite""
          dur=""43200s""
          to=""360 192 192""
          from=""0 192 192""
          attributeName=""transform""
          attributeType=""xml"">
    </animateTransform>

    <animateTransform
          type=""rotate""
          fill=""remove""
          restart=""always""
          calcMode=""linear""
          accumulate=""none""
          additive=""sum""
          xlink:href=""#iconic-anim-clock-minute-hand""
          repeatCount=""indefinite""
          dur=""3600s""
          to=""360 192 192""
          from=""0 192 192""
          attributeName=""transform""
          attributeType=""xml"">
    </animateTransform>

    <animateTransform
          type=""rotate""
          fill=""remove""
          restart=""always""
          calcMode=""linear""
          accumulate=""none""
          additive=""sum""
          xlink:href=""#iconic-anim-clock-second-hand""
          repeatCount=""indefinite""
          dur=""60s""
          to=""360 192 192""
          from=""0 192 192""
          attributeName=""transform""
          attributeType=""xml"">
    </animateTransform>
  </defs>
  <script  type=""text/javascript"">
  <![CDATA[
      var date = new Date;
      var seconds = date.getSeconds();
      var minutes = date.getMinutes();
      var hours = date.getHours();
      hours = (hours > 12) ? hours - 12 : hours;

      minutes = (minutes * 60) + seconds;
      hours = (hours * 3600) + minutes;

      document.querySelector('.iconic-clock-second-hand').setAttribute('transform', 'rotate('+360*(seconds/60)+',192,192)');
      document.querySelector('.iconic-clock-minute-hand').setAttribute('transform', 'rotate('+360*(minutes/3600)+',192,192)');
      document.querySelector('.iconic-clock-hour-hand').setAttribute('transform', 'rotate('+360*(hours/43200)+',192,192)');
  ]]>
  </script>
</svg>

            ";
        }
    }
}
