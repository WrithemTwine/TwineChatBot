# TwineStreamerBot
Twitch bots written using .NET 9.0/C#, Entity Framework Core (EFC) 9, and TwitchLib, https://github.com/TwitchLib/TwitchLib. It implements a WPF GUI, with data grids visualizing table data saved to a SQL database, and user settings are saved to user/App Data.

The goal is to provide a local running (not through a website) bot application for streamers to use and improve the stream viewer experience, and provide many useful features without needing other bots to supplement the feature set. Other streaming platforms may be supported with enough interest - the code is setup to support adding additional platforms in the future.

# EFC SQL Databases
The supported SQL databases are based on the freeware EFC 9 providers, https://learn.microsoft.com/en-us/ef/core/providers/:
   - Cosmos (Azure)
   - KNet (Apache Kafka(TM))
   - MySql
   - PomeloMySql
   - PostGre
   - Sqlite
   - SqlServer

Notes:
- The primary testing for this app is with Sqlite. Other versions may have issues not found without direct testing as LINQ calls may not be fully cross-compatible between database providers. Please submit a GitHub issue if you find any issues with a specific database provider.
- Other databases will/should work through other (paid) implemented EFC database providers. The provider version library must be the same as the EFC version used in the app (EFC 9) - the major versions are not cross-compatible. There are a few settings to adjust to use another EFC provider of choice. Please submit a GitHub issue if you want help to configure a specific database provider (specifically, reference the EFC provider library, the DbContext configuration to access the SQL database, and saving the connection settings within the app for ease of use).

----------------------------------
# Application - Twine Streamer Bot 
(documentation updated periodically, may not reflect actual current feature set)

Features: This bot utilizes TwitchLib, including Helix Api and EventSub.

The user can attach the bot to their channel for interacting with viewers through chat commands, repeating command timers, channel point redemptions, and responses to channel events.

Implements:
TwitchLib based Bots:
Use your own access tokens, to refresh every few months as per Twitch requirements, or authorize the app and it will manage token refresh automatically. Also, can use just your Twitch streamer account or also add-on a separate bot account to chat on your behalf (separate username in chat).
   - Twitch EventSub bots - read chats, stream online-offline, raids in/out, follower, channel point redemptions, bits/cheers, subscriptions viewer/gift
   - Twitch Multi-Live bot - monitor other streamer channels to post promotion to Discord channel(s)
   - Twitch "Clip" bot - can create new clips during a stream and locally save clip metadata for additional details.

App Service Bot(s):
   - Media Overlay Server - a local webserver to provide overlay alerts for streaming software (OBS, Streamlabs OBS, etc) to show text/images/videos on stream based on events, commands, and channel point redemptions
   - (reserved for future bot services)

Database management to organize:
   - viewers to a channel (supporting watch-time type commands)
   - currency for chat based games (currently Blackjack-21)
   - followers to a channel (supporting follow age type commands), and Old Followers (stopped following or username changes)
   - a viewer list to auto shout out viewers when they are first recognized as arrived in the channel
   - Statistics for the current live stream
   - Basic multichannel live activity for promoting other streamers when they go live (Multi-Live Bot feature): includes channel name, live activity tracking, and summary live data through a certain data
   - save basic statistics for incoming raid users and outgoing raids
   - Webhooks, such as for Discord, to post messages when the streamer goes live, and other channels you want to promote (Multi-Live Bot feature)
   - commands to use during streams, default and user-defined
   - Giveaway data
   - Streamer quotes
   - Overlay alerts to show images/videos on stream based on events, commands, and channel point redemptions

General app features for stream:
  - Help your stream viewers feel at home with custom messages from the bot:
	- Messages specific to new viewers, returning viewers, and followers
	- Specific viewer custom messages, e.g. their own welcome message
  - Respond with context messages as users interact with your channel - such as "Thanks User1 for following!", or other events such as subscriptions, bits/cheers, raids, and channel point redemptions
  - Channel commands, default and custom, with access level control, e.g. Moderator only commands
  - Repeating timer commands, e.g. remind viewers to follow the channel every X minutes
	 - 'Independent Timing' (Parallel) or 'Selection Order' (Serial) timer modes
		- Independent: timers run independently, so multiple timer messages can appear in chat at once
		- Selection: choose the order for commands to appear in chat, and they are performed one at a time, and the list restarts at the end.
	 - the repeat timer commands can have time variances:
		- Straight time - no adjustments to the timer interval
		- Smart slow-down timing - choose a threshold of chat and/or user activity, and command timers will slow down when conditions are below the threshold, and resume normal timing when above the threshold
		- Threshold based timing - choose a threshold of chat and/or user activity, and command timers will only run when conditions are at or above the threshold
  - Chat games: let users play chat games with their channel currency. Currently only Blackjack-21 is implemented. Future games will probably be added, no current plans (implemented the system, easier to add additional games).
  - Giveaways: run giveaways for your viewers, with options for entry methods (command entry or channel point redepemption), and random winner selection
  - Streamer quotes: save and retrieve memorable quotes from your stream
  - User shoutouts: can include a random clip from the user to play during the shoutout alert

Future features not currently implemented:
- Write the Wiki documentation for the app, including how to set up and use the various features

- Message spam protection: if there are a large influx of follows at a time or significant number of subscriptions; limit the number of messages

- Possibly a Twitch extension to connect to the webserver, to help provide overlays when streaming e.g. Xbox or Playstation without using a capture card to overlay a camera and notifications before sending to a streaming platform

- Additional features not yet determined, can submit GitHub issues for suggestions. Twitch can occassionally expand their Api features, opening up more possibilities.
