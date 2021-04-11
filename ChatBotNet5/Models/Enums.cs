namespace ChatBot_Net5.Models
{
    public enum ChatClient { All, Twitch }
    public enum CommandType { Normal }//, Currency, Game }

    // TwitchLib.Client.Enums.UserType

    public enum ChannelEventActions { Respond, Random, JoinChannel, LeaveChannel, Follow, BeingHosted, Subscribe, Resubscribe, Raid, GiftSub, Bits, Live, CommunitySubs, UserJoined }
    public enum ResponseType { Channel, Whisper, Both }
    public enum CurrencyType { JoinChannel, LeaveChannel, Chat, Emoticon, GiftSub, Bits, Points }
    
    public enum DefaultCommand { commands, addcommand, lurk, unlurk, worklurk, socials, bot, so, join, leave, queue }

    public enum DefaultSocials { twitter, youtube, instagram, discord, facebook, parlor, gab, telegram, rumble, tiktok }

    public enum DataSourceTableName { ChannelEvents, Users, Discord }

    public enum WebhooksKind { Live, Clips }

    /// <summary>
    /// Designated table and column pair to retrieve
    /// </summary>
    public enum DataRetrieve { EventMessage, EventEnabled }

    public enum ViewerTypes { Broadcaster, Mod, VIP, Follower, Sub, Viewer }
}
