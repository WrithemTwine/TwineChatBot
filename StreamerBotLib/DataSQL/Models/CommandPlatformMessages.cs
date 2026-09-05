using Microsoft.EntityFrameworkCore;

using StreamerBotLib.Models.Enums;

using System.Diagnostics;

namespace StreamerBotLib.DataSQL.Models
{
    [PrimaryKey(nameof(CmdName), nameof(Platform))]
    [DebuggerDisplay("Command = {CmdName}, Platform = {Platform}, Message = {Message}")]
    public class CommandPlatformMessages(string cmdName = null, Platform platform = Platform.Default, string message = null) : EntityBase
    {
        /// <summary>
        /// The command name for the message.
        /// </summary>
        public string CmdName { get; set; } = cmdName;
        /// <summary>
        /// The platform for the message.
        /// </summary>
        public Platform Platform { get; set; } = platform;
        /// <summary>
        /// The message content.
        /// </summary>
        public string Message { get; set; } = message;

        /// <summary>
        /// Navigation property to the associated CommandsBase entity.
        /// </summary>
        public CommandsBase CommandsBase { get; set; } = null;

    }
}
