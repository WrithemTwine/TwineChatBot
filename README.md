# TwineChatBot
Twitch bots written using .NET 6.0/C# and TwitchLib, https://github.com/TwitchLib/TwitchLib. It implements a WPF GUI, user settings are saved to user/App Data, and data grids (tables) show the data saved to the database (xml datagram) file and future support may include SQL databases for storage. 


[![build .NET 6.0 C#](https://github.com/WrithemTwine/TwineChatBot/actions/workflows/main.yml/badge.svg)](https://github.com/WrithemTwine/TwineChatBot/actions/workflows/main.yml)



--------------------------
# Application - Multi-Live Bot:

This bot utilizes TwitchLib containing the LiveStreamMonitorService. 

A user can use an existing Twitch channel user name or establish a new channel user account (recommended) as a bot. The application has further detail about how to get a 'client id' and 'access token'.

The user can add:
   - Discord links (current support, additional upon request) to post Twitch Channels going live
   - Message to display/send to Discord/social media, a tooltip shows the keyword variables per the user to include in the message
   - Channel Names - a list of channels to monitor for going live, and can be adjusted while the bot is active
   - Live Stream Stats - can view/edit the data the bot monitors, disabled while the bot is active (the updates don't refresh and crash the GUI)

----------------------------------
# Application - Twine Streamer Bot 
(documentation updated periodically, may not reflect actual current feature set)

Features: This bot utilizes TwitchLib.

The user can attach the bot to their channel for interacting with viewers through chat commands, repeating command timers, and responses to channel events.

Implements:
   - Twitch Chat bot
   - Twitch Live bot
   - Twitch Follower bot
   - Twitch "Clip" bot
   - Twitch PubSub Bot

   - App Services Bot
     - Media Overlay Server

(future scalability) the app setup allows adding more Twitch bots (different Twitch functions) and other platforms
   - TBD, next featured bot

Database management to organize:
   - viewers to a channel (supporting watch-time type commands)
   - followers to a channel (supporting follow age type commands)
   - a viewer list to auto shout out viewers when they are first recognized as arrived in the channel
   - Statistics for the current live stream
   - save basic statistics for incoming raid users, and outgoing raids
- Responses to events occuring in the channel:
   - Messages to viewer actions and events: welcome a viewer message, incoming raids, subscriptions and resubscriptions, gifted subscriptions, bits (Twitch), new follower, hosting messages, and going live messages
   - Command system 
      - built-in commands with editable response messages, includes social media link messages, uptime, watchtime, all commands list, and shout-out users
      - user-defined commands - set your own messages
      - types:
         - a simple text message, with certain variables to customize the message (e.g. refer to the viewer who called the command)
         - a data retrieval message - some implementation, currently updating and coding; to allow user customization for a data return message, define your own messages to get data returned from the database through a command message
         - repeat timers - repeat any of the above messages per defined seconds
   - Currency system - your viewers earn virtual currency to use in (future) chat games
   - Currency games (future feature) - using currency earned from watching
   - Overlay System - customize alerts to appear in stream overlay based on selected events

Options to manage bot actions (enable or disable):
   - saving bot authentication tokens, with a reminder for refreshing the token
   - (future feature?) automatically update Twitch access tokens
- App Features:
   - Manage channel viewers - saving in the database
   - Manage channel followers - saving in the database
   - Save stats for each live stream, including the current stream
   - Save stats for incoming stream raid users
   - Notify the channel when the bot connects
   - Post go live messages to social media
      - currently to Discord webhooks
      - additional as requested (and possible)
   - Repeat commands on a timer (in seconds)
      - all the time
      - only when live
      - slow down repeating when channel is slow (few viewers or chats in a timeframe)
   - Welcome viewers (only once per live stream)
      - when viewer joins channel
      - when viewer first chats
   - Auto shout out users (once per live stream)
      - when viewer first appears or chats, tied to welcoming viewers
      - when viewer raids the channel
   - Viewer Giveaways
      - Specify Commands or Channel Points (requires PubSub bot active) for the user to enter
      - Specify the user can enter 1 or more times up to a specified max limit
      - Giveaway - Start, Stop, and Winner customizable messages can be sent to chat
   - Manage Overlay Alerts
      - First, select events such as Channel Point rewards, Commands, Channel Event (new follower, subscription, etc), 
      - Then, select an image and/or video to display for an amount of time

- Twitch Features:
   - Add "/me" to messages (i.e. italicized message)
      - all messages or 
      - user specified selected message
   - Managing follows:
      - Update all followers when bot starts
      - Remove non-followers from Followers table
      - Notification message for new follows
      - Option to refresh all followers every specified number of hours - after Follow bot starts
   - Uses PubSub to listen to Channel Point Redemptions
       - For Giveaways
       - Future features upon request, per PubSub API
   - Automatically start when app starts:
      - Twitch Chat bot
      - Twitch Live bot
      - Twitch Follow bot
      - Twitch Clip bot
      - Multi-Live bot features - when Multi-Live Bot app is not started

Twitch (some features depend on settings)
   - Chat bot
      - interacts with viewers during live streams through commands and events
      - start and/or stop bot when stream is online or offline
      - when left running, tends to the channel like thanking new followers and repeating timer commands
   - Follow bot
      - registers new follows to the channel
      - when started: retrieves followers and removes non-followers from the database
      - routinely retrieve followers and remove non-followers without restarting the follow bot
      - (future feature?) message spam protection for large groups of followers
   - Live bot
      - registers when your channel goes live or goes offline
      - monitors other channels to share when they go live
   - Clip bot
      - retrieves clips upon startup
      - monitors channel for active clips
        - option to post to chat
        - option to post to Discord via Webhook
   - PubSub bot
      - listens to topics from Twitch to receive realtime updates
        - Custom Rewards redemption (Channel Points)
        - (future features) more topics available via Twitch API
    When Multi-Live Bot
      - is running, this feature is disabled
      - is not running, this feature is available and shares the same data file
   Note: Live bot and Multi-Live Bot read from the same file to save the channels you wish to promote their going live,  the social media links for posting messages, and a tracker of the going live channels to prevent any multiple message posting (if the option is enabled).

Future features not implemented - and may not be implemented:
- Currency system - currency (implemented-testing), the chat gaming side (in process)
- Message spam protection: if there are a large influx of follows at a time or significant number of subscriptions; limit the number of messages

- Possibly a Twitch extension to connect to the webserver, to help provide overlays when streaming e.g. Xbox or Playstation without using a capture card to overlay a camera and notifications before sending to a streaming platform

- Considering options for attaching bot to a user-specified SQL database, which would be easier to manage sizes and data archives.
