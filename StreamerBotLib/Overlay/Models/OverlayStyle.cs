using StreamerBotLib.Enums;
using StreamerBotLib.Overlay.Communication;

using StreamerBotLib.Overlay.Interfaces;
using StreamerBotLib.Overlay.Static;
using StreamerBotLib.Static;

using System.Diagnostics;
using System.IO;

namespace StreamerBotLib.Overlay.Models
{
    /// <summary>
    /// Loads a style and saves to a file.
    /// </summary>
    [DebuggerDisplay("OverlayType = {OverlayType}")]
    public class OverlayStyle : IOverlayStyle
    {
        private string FileNameStyle;

        /// <summary>
        /// The type of Overlay.
        /// </summary>
        public string OverlayType { get; set; }

        /// <summary>
        /// The content of the style file, default loads for no file.
        /// </summary>
        public string OverlayStyleText { get; set; }

        /// <summary>
        /// Used to hold the edit page text for the GUI.
        /// </summary>
        /// <param name="overlayType">Specify the type of the Overlay.</param>
        public OverlayStyle(string overlayType)
        {
            OverlayType = OptionFlags.MediaOverlayUseSameStyle ? "All Overlays" : overlayType;

            FileNameStyle = Path.Combine(PublicConstants.BaseOverlayPath, $"{(OptionFlags.MediaOverlayUseSameStyle || OverlayType == "None" ? "" : OverlayType)}", $"{PublicConstants.OverlayStyle}");

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
            OverlayType = OptionFlags.MediaOverlayTickerMulti ? "All Tickers" : overlayTickerItem.ToString();

            FileNameStyle = Path.Combine(PublicConstants.BaseTickerPath, $"{(OptionFlags.MediaOverlayTickerMulti ? PublicConstants.TickerStyle("ticker") : PublicConstants.TickerStyle(OverlayType))}");
            
            if (File.Exists(FileNameStyle))
            {
                using (StreamReader sr = new(FileNameStyle))
                {
                    OverlayStyleText = sr.ReadToEnd();
                }
            }
            else
            {
                OverlayStyleText = ProcessHyperText.DefaultTickerStyle(OverlayType);
            }
        }

        /// <summary>
        /// Saves the file to subfolders based on OverlayType. If user selects "UseSameOverlayStyle" setting, saves in base folder.
        /// </summary>
        public void SaveFile()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(FileNameStyle));
            File.WriteAllText(FileNameStyle, OverlayStyleText);
        }
    }
}
