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
        /// The applicable User Name
        /// </summary>
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// Data flag as bot user's preference to use the message generated from what is sent to chat, in place of standard messaging defined with Overlay
        /// </summary>
        public bool UseChatMsg { get; set; } = false;

        /// <summary>
        /// Specifies if this is part of loading data into the server. 
        /// Specific info would require connecting to sources
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// The user provided length of the notification.
        /// </summary>
        public int Duration { get; set; } = 0;

        /// <summary>
        /// The path to the media for the event.
        /// </summary>
        public string MediaPath { get; set; } = string.Empty;

        /// <summary>
        /// Object HashCode.
        /// </summary>
        public int HashCode
        {
            get
            {
                return string.GetHashCode($"{OverlayType}_{Duration}_{UserName}_{Message.Replace("_", " ")}_{ActionValue.Replace("_", " ")}_{MediaPath}");
            }
        }

        /// <summary>
        /// Provides specific procedure to combine the elements of this class.
        /// </summary>
        /// <returns>A class specific string of the object contents.</returns>
        public override string ToString()
        {
            return $"{OverlayType}_{Duration}_{UserName}_{Message.Replace("_", " ")}_{ActionValue.Replace("_", " ")}_{MediaPath}";
        }

        /// <summary>
        /// Converts a <code>ToString()</code> generated string back to a class object, usefully for stream transmissions.
        /// </summary>
        /// <param name="OverlayTypestring">The string to decode.</param>
        /// <returns>A new class object with properties assigned the values from the input string.</returns>
        /// <exception cref="ArgumentException"></exception>
        public static OverlayActionType FromString(string? OverlayTypestring)
        {
            if (OverlayTypestring == null)
            {
                return new();
            }
            else
            {
                Queue<string> strings = new(OverlayTypestring.Split('_'));

                if (strings.Count > 6)
                {
                    OverlayTypes type = (OverlayTypes)Enum.Parse(typeof(OverlayTypes), strings.Dequeue());
                    int Duration = int.Parse(strings.Dequeue());
                    string User = strings.Dequeue();
                    string Msg = strings.Dequeue();
                    string action = strings.Dequeue();
                    string MediaPath = strings.Dequeue();

                    return new() { ActionValue = action, Message = Msg, UserName=User, OverlayType = type, Duration = Duration, MediaPath = MediaPath };
                }
                else
                {
                    throw new ArgumentException("The provided string is not formatted from OverlayActionType.ToString()");
                }
            }
        }
    }
}
