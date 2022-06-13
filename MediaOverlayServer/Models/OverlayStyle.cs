﻿using MediaOverlayServer.Communication;
using MediaOverlayServer.Interfaces;
using MediaOverlayServer.Static;

using System.IO;

namespace MediaOverlayServer.Models
{
    /// <summary>
    /// Loads a style and saves to a file.
    /// </summary>
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
            OverlayType = OptionFlags.UseSameOverlayStyle ? "All" : overlayType;

            FileNameStyle = Path.Combine(PublicConstants.BaseOverlayPath, $"{(OptionFlags.UseSameOverlayStyle || OverlayType == "None" ? "" : OverlayType)}", $"{PublicConstants.OverlayStyle}");

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
        /// Saves the file to subfolders based on OverlayType. If user selects "UseSameOverlayStyle" setting, saves in base folder.
        /// </summary>
        public void SaveFile()
        {
            File.WriteAllText(FileNameStyle, OverlayStyleText);
        }
    }
}
