using System;
using System.Collections.Generic;
using System.Linq;

using MediaOverlayServer.Enums;

namespace MediaOverlayServer
{
    /// <summary>
    /// Defines message payload to either load data or specify active data via the named pipe from the main bot process.
    /// </summary>
    [Serializable]
    public class OverlayActionType : EventArgs
    {
        /// <summary>
        /// Identify the category for the overlay type
        /// </summary>
        public OverlayTypes OverlayType { get; set; } = OverlayTypes.None;

        /// <summary>
        /// The value of the item to trigger, such as a Channel Point Redemption
        /// </summary>
        public string ActionValue { get; set; } = string.Empty;

        /// <summary>
        /// Specifies if this is part of loading data into the server. 
        /// Specific info would require connecting to sources
        /// </summary>
        public string Message { get; set; } = string.Empty;

        public int Duration { get; set; } = 0;

        public string MediaPath { get; set; } = string.Empty;

        public int HashCode
        {
            get
            {
                return string.GetHashCode(OverlayType.ToString() + ActionValue + Message.ToString() + Duration.ToString() + MediaPath);
            }
        }

        public override string ToString()
        {
            return $"{OverlayType}_{Duration}_{Message.Replace("_", " ")}_{ActionValue.Replace("_", " ")}_{MediaPath}";
        }

        public static OverlayActionType FromString(string? OverlayTypestring)
        {
            if (OverlayTypestring == null)
            {
                return new();
            }
            else
            {
                Queue<string> strings = new(OverlayTypestring.Split('_'));

                if (strings.Count > 3)
                {
                    OverlayTypes type = (OverlayTypes)Enum.Parse(typeof(OverlayTypes), strings.Dequeue());
                    int Duration = int.Parse(strings.Dequeue());
                    string Msg = strings.Dequeue();
                    string action = strings.Dequeue();
                    string MediaPath = string.Join(' ', strings); // filenames might have '_' per user requirements

                    return new() { ActionValue = action, Message = Msg, OverlayType = type, Duration = Duration, MediaPath = MediaPath };
                }
                else
                {
                    throw new ArgumentException("The provided string is not formatted from OverlayActionType.ToString()");
                }
            }
        }
    }
}
