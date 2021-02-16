namespace ChatBot_Net5.Models
{
    public enum ChatClient { All, Twitch }
    public enum CommandType { Normal }//, Currency, Game }

    // TwitchLib.Client.Enums.UserType

    public enum CommandAction { Respond, Random, JoinChannel, LeaveChannel, Follow, BeingHosted, Subscribe, Resubscribe, Raid, GiftSub, Bits, Live }
    public enum ResponseType { Channel, Whisper, Both }
    public enum CurrencyAction { JoinChannel, LeaveChannel, Chat, Emoticon, GiftSub, Bits, Points }

    public enum DataSourceTableName { ChannelEvents, Users, Discord }

    /// <summary>
    /// Designated table and column pair to retrieve
    /// </summary>
    public enum DataRetrieve { EventMessage, EventEnabled }
}
