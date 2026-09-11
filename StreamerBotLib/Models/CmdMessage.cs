using StreamerBotLib.Models.Enums;

using System.Diagnostics;

namespace StreamerBotLib.Models
{
    [DebuggerDisplay("CommandText={CommandText}, Channel={Channel}, UserId={UserId}, DisplayName={DisplayName}")]
    public class CmdMessage()
    {
        public CmdMessage(string commandText, List<string> commandArguments, string channel, string userId, string displayName, bool isBroadcaster, bool isHighlighted, bool isMe, bool isModerator, bool isPartner, bool isSkippingSubMode, bool isStaff, bool isSubscriber, bool isTurbo, bool isVip, string message, int bits,
        ViewerTypes userType, Platform platform) : this()
        {
            CommandText = commandText;
            CommandArguments = commandArguments;
            Channel = channel;
            UserId = userId;
            DisplayName = displayName;
            IsBroadcaster = isBroadcaster;
            IsHighlighted = isHighlighted;
            IsMe = isMe;
            IsModerator = isModerator;
            IsPartner = isPartner;
            IsSkippingSubMode = isSkippingSubMode;
            IsStaff = isStaff;
            IsSubscriber = isSubscriber;
            IsTurbo = isTurbo;
            IsVip = isVip;
            Message = message;
            Bits = bits;
            UserType = userType;
            Platform = platform;
        }

        public CmdMessage(string commandText, List<string> commandArguments, ViewerTypes userType, bool isBroadcaster, string displayName, string channel, string message) : this()
        {
            CommandText = commandText;
            IsBroadcaster = isBroadcaster;
            DisplayName = displayName;
            Channel = channel;
            Message = message;
        }

        public CmdMessage(string channel, string userId, string displayName, bool isBroadcaster, bool isModerator, bool isStaff, bool isSubscriber, bool isVip, string message, int bits, ViewerTypes userType, Platform platform) : this()
        {
            Channel = channel;
            UserId = userId;
            DisplayName = displayName;
            IsBroadcaster = isBroadcaster;
            IsModerator = isModerator;
            IsStaff = isStaff;
            IsSubscriber = isSubscriber;
            IsVip = isVip;
            Message = message;
            Bits = bits;
            UserType = userType;
            Platform = platform;
        }

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
        public Platform Platform { get; set; }

        private LiveUser _user;
        public LiveUser User
        {
            get
            {
                return _user ??= new(DisplayName, Platform, UserId);
            }
        }
    }
}
