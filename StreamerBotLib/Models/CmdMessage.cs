using StreamerBotLib.Models.Enums;

using System.Diagnostics;

namespace StreamerBotLib.Models
{
    [DebuggerDisplay("CommandText={CommandText}, Channel={Channel}, UserId={UserId}, DisplayName={DisplayName}")]
    public record CmdMessage
    {
        public string CommandText { get; set; }
        public List<string> CommandArguments { get; set; }
        public string Channel { get; set; }
        public string UserId { get; set; }
        public string DisplayName { get; set; }
        public bool IsBroadcaster { get; set; }
        public bool IsHighlighted { get; set; }
        public bool IsMe { get; set; }
        public bool IsModerator { get; set; }
        public bool IsPartner { get; set; }
        public bool IsSkippingSubMode { get; set; }
        public bool IsStaff { get; set; }
        public bool IsSubscriber { get; set; }
        public bool IsTurbo { get; set; }
        public bool IsVip { get; set; }
        public string Message { get; set; }
        public int Bits { get; set; }
        public ViewerTypes UserType { get; set; }
    }
}
