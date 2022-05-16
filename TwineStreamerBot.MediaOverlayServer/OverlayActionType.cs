using System;
using System.Collections.Generic;
using System.Linq;

using TwineStreamerBot.MediaOverlayServer.Enums;

namespace TwineStreamerBot.MediaOverlayServer
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

        public string MediaPath { get; set; } = string.Empty;

        public int HashCode
        {
            get
            {
                return string.GetHashCode(OverlayType.ToString() + ActionValue + Message.ToString());
            }
        }

        public override string ToString()
        {
            return $"{OverlayType}_{Message}_{ActionValue}";
        }

        public static OverlayActionType FromString(string? OverlayTypestring)
        {
            if (OverlayTypestring == null)
            {
                return new();
            }
            else
            {
                List<string> strings = new(OverlayTypestring.Split('_'));

                if (strings.Count > 2)
                {
                    OverlayTypes type = (OverlayTypes)Enum.Parse(typeof(OverlayTypes), strings[0]);
                    string Msg = strings[1];
                    string action = string.Join(' ', strings.Skip(2));

                    return new() { ActionValue = action, Message = Msg, OverlayType = type };
                }
                else
                {
                    throw new ArgumentException("The provided string is not formatted from OverlayActionType.ToString()");
                }
            }
        }
    }
}
