namespace StreamerBotLib.Models.Enums
{
    // DO NOT CHANGE THE ORDER OF THIS ENUM, IT IS USED INTERNALLY WITHIN THE ENTITY FRAMEWORKCORE-DATABASE AS AN INTEGER VALUE, AND CHANGING THE ORDER WILL BREAK EXISTING DATABASES MAPPING THE INTEGER BACK TO THIS ENUM.
    // Only add new platforms to the end of this enum, and do not remove any existing platforms.

    /// <summary>
    /// Specifies the platform of the originating action. 
    /// Platform is used within <seealso cref="LiveUser"/> to identify a user's originating platform and where a command call originates. 
    /// Also used to specify output platforms, the response should return to just that platform. Any universal responses can send to all platforms using the 'Default' platform.
    /// 
    /// The 'Service' platform doesn't have any corresponding output mechanism, as of now, so it is unused.
    /// </summary>
    public enum Platform
    {
        Default,  // represent any platform, use this option to "SendMessage(...)" to all platforms, or to represent a user that is not platform specific.
        Twitch, // the streaming platform Twitch, sends & receives chat messages, user events, and other Twitch specific events.
        Service, // internal service bot, doesn't connect to any streaming service, no "SendMessage(...)" output per se. The Overlay service requires a data bundle to interpret the overlay event - currently, "SendMessage(...)" doesn't implement a package type object that would accomodate the overlay event.

        // unused platforms, added for future use, but not implemented yet. Move below value(s) to above here --this line-- when implemented.

        // TODO: add other platforms as needed, and implement the corresponding "SendMessage(...)" output for each platform.
        YouTube, // the streaming platform YouTube, sends & receives chat messages, user events, and other YouTube specific events.
        Rumble, // the streaming platform Rumble, sends & receives chat messages, user events, and other Rumble specific events.
        Pilled, // the streaming platform Pilled, sends & receives chat messages, user events, and other Pilled specific events.
        Kick // the streaming platform Kick, sends & receives chat messages, user events, and other Kick specific events.
    }
}
