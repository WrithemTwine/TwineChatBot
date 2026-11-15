using StreamerBotLib.Models.Enums;

namespace StreamerBotLib.Models.BotManagement
{
    /// <summary>
    /// A key describing a frameworkelement's parameters for determining UI access and functionality
    /// </summary>
    public class BotIdKey
    {
        /// <summary>
        /// the API platform
        /// </summary>
        public Platform Platform { get; set; }
        /// <summary>
        /// which bot for the platform
        /// </summary>
        public Bots Bots { get; set; }
        /// <summary>
        /// the type of the token, with or without scopes per bot/streamer (depends on using the same or different bot account to streamer account)
        /// </summary>
        public BotType BotType { get; set; }
        /// <summary>
        /// specifies the scope for the control's enabled/visibilty
        /// </summary>
        public BotOperationTypes BotOperationType { get; set; }

        /// <summary>
        /// specifies if this keyed item requires additional 'second account' treatment
        /// </summary>
        public bool NeedSecondAccount { get; set; } = false;

        public BotStatusCategory BotStatusCategory { get; set; }
        /// <summary>
        /// The value of the operation to determine visibility.<br />
        /// e.g. BotOperationTypes.Authorized, LabelOperationVisibility = false; <br /> Label.Visibility = Authorized==false ? Visibility.Visible : Visibility.Collapsed;
        /// </summary>
        public bool LabelOperationVisibility { get; set; } = false;
        /// <summary>
        /// A flag to specify using an automatically refreshed token. Primarily for help context in Label visibility (showing help for the user about automatic refreshing tokens or help for other authentication methods).
        /// </summary>
        public bool UseAutoToken { get; set; } = false;

        /// <summary>
        /// Provides true if the object meets the criteria.
        /// </summary>
        /// <param name="platform">The object platform.</param>
        /// <param name="bots">The object bot name.</param>
        /// <param name="botType">The object bot type.</param>
        /// <param name="botOperationType">The object type of bot operation.</param>
        /// <param name="needSecondAccount">Whether the object is subject to a second account.</param>
        /// <returns><c>true</c> - if object meets parameters, <c>false</c> - if object doesn't meet parameters.</returns>
        public bool IsSelected(Platform platform, Bots bots, BotType botType, BotOperationTypes botOperationType, bool needSecondAccount = false)
        {
            return Platform == platform && Bots == bots && BotType == botType && BotOperationType == botOperationType && NeedSecondAccount == needSecondAccount;
        }
    }
}
