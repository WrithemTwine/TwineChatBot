using StreamerBotLib.Overlay.Communication;
using StreamerBotLib.Overlay.Enums;
using StreamerBotLib.Overlay.Interfaces;
using StreamerBotLib.Overlay.Static;
using StreamerBotLib.Static;

using System;
using System.Diagnostics;
using System.IO;

namespace StreamerBotLib.Overlay.Models
{
    /// <summary>
    /// Loads a style and saves to a file.
    /// </summary>
    [DebuggerDisplay("OverlayType = {OverlayType}")]
    public class OverlayStyle : IOverlayStyle, IEquatable<OverlayStyle>
    {
        private readonly string FileNameStyle;

        /// <summary>
        /// The type of Overlay.
        /// </summary>
        public string OverlayType { get; set; }

        /// <summary>
        /// The content of the style file, default loads for no file.
        /// </summary>
        public string OverlayStyleText { get; set; }

        private string PriorText { get; set; }

        /// <summary>
        /// Used to hold the edit page text for the GUI.
        /// </summary>
        /// <param name="overlayType">Specify the type of the Overlay.</param>
        public OverlayStyle(string overlayType)
        {
            OverlayType = OptionFlags.MediaOverlayUseSameStyle ? PublicConstants.OverlayAllActions : overlayType;

            FileNameStyle = Path.Combine(PublicConstants.BaseOverlayPath, $"{(OptionFlags.MediaOverlayUseSameStyle || OverlayType == PublicConstants.OverlayAllActions ? "" : OverlayType)}", $"{PublicConstants.OverlayStyle}");

            if (File.Exists(FileNameStyle))
            {
                using (StreamReader sr = new(FileNameStyle))
                {
                    OverlayStyleText = sr.ReadToEnd();
                }
            }
            else
            {
                OverlayStyleText = ProcessHyperText.DefaultStyle;
            }
        }

        /// <summary>
        /// Used to hold the edit page text for the GUI, for the specified Ticker.
        /// </summary>
        /// <param name="overlayTickerItem">The specific ticker item for its style.</param>
        public OverlayStyle(OverlayTickerItem overlayTickerItem)
        {
            OverlayType = OptionFlags.MediaOverlayTickerMulti ? PublicConstants.OverlayAllTickers : overlayTickerItem.ToString();

            FileNameStyle = Path.Combine(PublicConstants.BaseTickerPath, $"{(OptionFlags.MediaOverlayTickerMulti ? PublicConstants.TickerFile("ticker") : PublicConstants.TickerFile(OverlayType))}");

            if (File.Exists(FileNameStyle))
            {
                StreamReader sr = new(FileNameStyle);
                OverlayStyleText = sr.ReadToEnd();
            }
            else
            {
                OverlayStyleText = ProcessHyperText.DefaultTickerStyle(OverlayType);
            }
        }

        internal OverlayStyle(TickerStyle tickerStyle, bool refresh = false)
        {
            OverlayType = tickerStyle.ToString();

            FileNameStyle = Path.Combine(PublicConstants.BaseTickerPath, $"{PublicConstants.TickerFile(tickerStyle)}");

            if (File.Exists(FileNameStyle) && !refresh)
            {
                OverlayStyleText = File.ReadAllText(FileNameStyle);
            }
            else
            {
                OverlayStyleText = ProcessHyperText.DefaultTickerStyle(tickerStyle);
            }

            PriorText = OverlayStyleText;
        }

        /// <summary>
        /// Saves the file to subfolders based on OverlayType. If user selects "UseSameOverlayStyle" setting, saves in base folder.
        /// </summary>
        public void SaveFile()
        {
            if (OverlayStyleText != PriorText)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(FileNameStyle));
                File.WriteAllText(FileNameStyle, OverlayStyleText);
                PriorText = OverlayStyleText;
            }
        }

        public bool Equals(OverlayStyle other)
        {
            return OverlayType == other.OverlayType;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as OverlayStyle);
        }

        public override int GetHashCode()
        {
            return (OverlayType + OverlayStyleText).GetHashCode();
        }
    }
}
