﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ChatBot_Net5.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "16.10.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool UpgradeRequired {
            get {
                return ((bool)(this["UpgradeRequired"]));
            }
            set {
                this["UpgradeRequired"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("From registered bot at https://dev.twitch.tv/console")]
        public string TwitchClientID {
            get {
                return ((string)(this["TwitchClientID"]));
            }
            set {
                this["TwitchClientID"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Twitch channel the bot will monitor")]
        public string TwitchChannelName {
            get {
                return ((string)(this["TwitchChannelName"]));
            }
            set {
                this["TwitchChannelName"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Specify the OAuth access token for bot authentication")]
        public string TwitchAccessToken {
            get {
                return ((string)(this["TwitchAccessToken"]));
            }
            set {
                this["TwitchAccessToken"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Optional, some sites provide this token to refresh an OAuth access token")]
        public string TwitchRefreshToken {
            get {
                return ((string)(this["TwitchRefreshToken"]));
            }
            set {
                this["TwitchRefreshToken"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("1900-01-01")]
        public global::System.DateTime TwitchRefreshDate {
            get {
                return ((global::System.DateTime)(this["TwitchRefreshDate"]));
            }
            set {
                this["TwitchRefreshDate"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Twitch account the bot will use to post")]
        public string TwitchBotUserName {
            get {
                return ((string)(this["TwitchBotUserName"]));
            }
            set {
                this["TwitchBotUserName"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("30")]
        public int TwitchFrequencyFollow {
            get {
                return ((int)(this["TwitchFrequencyFollow"]));
            }
            set {
                this["TwitchFrequencyFollow"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool MsgBotConnection {
            get {
                return ((bool)(this["MsgBotConnection"]));
            }
            set {
                this["MsgBotConnection"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool TwitchAddFollowersStart {
            get {
                return ((bool)(this["TwitchAddFollowersStart"]));
            }
            set {
                this["TwitchAddFollowersStart"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("300")]
        public int TwitchGoLiveFrequency {
            get {
                return ((int)(this["TwitchGoLiveFrequency"]));
            }
            set {
                this["TwitchGoLiveFrequency"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool PostMultiLive {
            get {
                return ((bool)(this["PostMultiLive"]));
            }
            set {
                this["PostMultiLive"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool MsgInsertMe {
            get {
                return ((bool)(this["MsgInsertMe"]));
            }
            set {
                this["MsgInsertMe"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool WelcomeChatMsg {
            get {
                return ((bool)(this["WelcomeChatMsg"]));
            }
            set {
                this["WelcomeChatMsg"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool MsgAutoShout {
            get {
                return ((bool)(this["MsgAutoShout"]));
            }
            set {
                this["MsgAutoShout"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool RepeatTimerCommands {
            get {
                return ((bool)(this["RepeatTimerCommands"]));
            }
            set {
                this["RepeatTimerCommands"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool WelcomeUserJoined {
            get {
                return ((bool)(this["WelcomeUserJoined"]));
            }
            set {
                this["WelcomeUserJoined"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool WelcomeDisabled {
            get {
                return ((bool)(this["WelcomeDisabled"]));
            }
            set {
                this["WelcomeDisabled"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool MsgNoMe {
            get {
                return ((bool)(this["MsgNoMe"]));
            }
            set {
                this["MsgNoMe"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool MsgPerComMe {
            get {
                return ((bool)(this["MsgPerComMe"]));
            }
            set {
                this["MsgPerComMe"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool UserPartyStart {
            get {
                return ((bool)(this["UserPartyStart"]));
            }
            set {
                this["UserPartyStart"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool UserPartyStop {
            get {
                return ((bool)(this["UserPartyStop"]));
            }
            set {
                this["UserPartyStop"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool RepeatWhenLive {
            get {
                return ((bool)(this["RepeatWhenLive"]));
            }
            set {
                this["RepeatWhenLive"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool TwitchChatBotAutoStart {
            get {
                return ((bool)(this["TwitchChatBotAutoStart"]));
            }
            set {
                this["TwitchChatBotAutoStart"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool TwitchLiveStreamSvcAutoStart {
            get {
                return ((bool)(this["TwitchLiveStreamSvcAutoStart"]));
            }
            set {
                this["TwitchLiveStreamSvcAutoStart"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool TwitchFollowerSvcAutoStart {
            get {
                return ((bool)(this["TwitchFollowerSvcAutoStart"]));
            }
            set {
                this["TwitchFollowerSvcAutoStart"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("#user is now live streaming #category - #title! Come join and say hi at: #url")]
        public string MsgLive {
            get {
                return ((string)(this["MsgLive"]));
            }
            set {
                this["MsgLive"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool TwitchMultiLiveAutoStart {
            get {
                return ((bool)(this["TwitchMultiLiveAutoStart"]));
            }
            set {
                this["TwitchMultiLiveAutoStart"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool ManageUsers {
            get {
                return ((bool)(this["ManageUsers"]));
            }
            set {
                this["ManageUsers"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool ManageFollowers {
            get {
                return ((bool)(this["ManageFollowers"]));
            }
            set {
                this["ManageFollowers"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool ManageStreamStats {
            get {
                return ((bool)(this["ManageStreamStats"]));
            }
            set {
                this["ManageStreamStats"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool TwitchRaidShoutOut {
            get {
                return ((bool)(this["TwitchRaidShoutOut"]));
            }
            set {
                this["TwitchRaidShoutOut"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0.5")]
        public decimal ChatPopOutOpacity {
            get {
                return ((decimal)(this["ChatPopOutOpacity"]));
            }
            set {
                this["ChatPopOutOpacity"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool TwitchFollowerFollowBack {
            get {
                return ((bool)(this["TwitchFollowerFollowBack"]));
            }
            set {
                this["TwitchFollowerFollowBack"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool TwitchRaidFollowBack {
            get {
                return ((bool)(this["TwitchRaidFollowBack"]));
            }
            set {
                this["TwitchRaidFollowBack"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool TwitchPruneNonFollowers {
            get {
                return ((bool)(this["TwitchPruneNonFollowers"]));
            }
            set {
                this["TwitchPruneNonFollowers"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool TwitchAddFollowerNotification {
            get {
                return ((bool)(this["TwitchAddFollowerNotification"]));
            }
            set {
                this["TwitchAddFollowerNotification"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool MsgWelcomeStreamer {
            get {
                return ((bool)(this["MsgWelcomeStreamer"]));
            }
            set {
                this["MsgWelcomeStreamer"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool WelcomeCustomMsg {
            get {
                return ((bool)(this["WelcomeCustomMsg"]));
            }
            set {
                this["WelcomeCustomMsg"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool RepeatTimerComSlowdown {
            get {
                return ((bool)(this["RepeatTimerComSlowdown"]));
            }
            set {
                this["RepeatTimerComSlowdown"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("The client Id for the bot attached to streamer channel.")]
        public string TwitchStreamerChannel {
            get {
                return ((string)(this["TwitchStreamerChannel"]));
            }
            set {
                this["TwitchStreamerChannel"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Add token for streamer channel, specifically for only follow-back")]
        public string TwitchStreamerToken {
            get {
                return ((string)(this["TwitchStreamerToken"]));
            }
            set {
                this["TwitchStreamerToken"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool TwitchDisconnectBot {
            get {
                return ((bool)(this["TwitchDisconnectBot"]));
            }
            set {
                this["TwitchDisconnectBot"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("1900-01-01")]
        public global::System.DateTime TwitchStreamTokenDate {
            get {
                return ((global::System.DateTime)(this["TwitchStreamTokenDate"]));
            }
            set {
                this["TwitchStreamTokenDate"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool TwitchFollowbackBotChoice {
            get {
                return ((bool)(this["TwitchFollowbackBotChoice"]));
            }
            set {
                this["TwitchFollowbackBotChoice"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool TwitchFollowbackStreamerChoice {
            get {
                return ((bool)(this["TwitchFollowbackStreamerChoice"]));
            }
            set {
                this["TwitchFollowbackStreamerChoice"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool TwitchChatBotDisconnectOffline {
            get {
                return ((bool)(this["TwitchChatBotDisconnectOffline"]));
            }
            set {
                this["TwitchChatBotDisconnectOffline"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool TwitchChatBotConnectOnline {
            get {
                return ((bool)(this["TwitchChatBotConnectOnline"]));
            }
            set {
                this["TwitchChatBotConnectOnline"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("300")]
        public double TwitchFrequencyClipTime {
            get {
                return ((double)(this["TwitchFrequencyClipTime"]));
            }
            set {
                this["TwitchFrequencyClipTime"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool TwitchClipPostDiscord {
            get {
                return ((bool)(this["TwitchClipPostDiscord"]));
            }
            set {
                this["TwitchClipPostDiscord"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool TwitchClipAutoStart {
            get {
                return ((bool)(this["TwitchClipAutoStart"]));
            }
            set {
                this["TwitchClipAutoStart"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool TwitchClipPostChat {
            get {
                return ((bool)(this["TwitchClipPostChat"]));
            }
            set {
                this["TwitchClipPostChat"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool LogBotStatus {
            get {
                return ((bool)(this["LogBotStatus"]));
            }
            set {
                this["LogBotStatus"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool LogExceptions {
            get {
                return ((bool)(this["LogExceptions"]));
            }
            set {
                this["LogExceptions"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool TwitchCurrencyStart {
            get {
                return ((bool)(this["TwitchCurrencyStart"]));
            }
            set {
                this["TwitchCurrencyStart"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool TwitchCurrencyOnline {
            get {
                return ((bool)(this["TwitchCurrencyOnline"]));
            }
            set {
                this["TwitchCurrencyOnline"] = value;
            }
        }
    }
}
