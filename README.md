# TwineChatBot
Twitch bots written using .NET 5.0/C# and TwitchLib, https://github.com/TwitchLib/TwitchLib.

- Multi-Live Bot:
This bot utilizes .NET 5.0 and TwitchLib, containing the LiveStreamMonitorService. It implements a WPF GUI, user settings are saved to user/App Data, and data grids (tables) show the data saved to the database (xml datagram) file. 

A user can use an existing Twitch channel user name or establish a new channel user account (recommended) as a bot. The application has further detail about how to get a 'client id' and 'access token'.

The user can add:
   - Discord links (current support, additional upon request) to post Twitch Channels going live
   - Message to display/send to Discord/social media, a tooltip shows the keyword variables per the user to include in the message
   - Channel Names - a list of channels to monitor for going live, and can be adjusted while the bot is active
   - Live Stream Stats - can view/edit the data the bot monitors, disabled while the bot is active (the updates don't refresh and crash the GUI)


(this needs more update)



