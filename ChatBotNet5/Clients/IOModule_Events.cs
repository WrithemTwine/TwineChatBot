namespace ChatBot_Net5.Clients
{
    // add events for additional events from other chat bots API
    internal enum IO_Events
    {
          OnBeingHosted
        , OnChannelStateChanged
        , OnChatCleared
        , OnChatCommandReceived
        , OnCommunitySubscription
        , OnConnected
        , OnConnectionError
        , OnDisconnected
        , OnExistingUsersDetected
        , OnFailureToReceiveJoinConfirmation
        , OnGiftedSubscription
        , OnHostingStarted
        , OnHostingStopped
        , OnIncorrectLogin
        , OnJoinedChannel
        , OnLeftChannel
        , OnLog
        , OnMessageCleared
        , OnMessageReceived
        , OnMessageSent
        , OnModeratorJoined
        , OnModeratorLeft
        , OnModeratorsReceived
        , OnNewSubscriber
        , OnNowHosting
        , OnRaidNotification
        , OnReSubscriber
        , OnRitualNewChatter
        , OnSendReceiveData
        , OnUnaccounted
        , OnUserBanned
        , OnUserJoined
        , OnUserLeft
        , OnUserStateChanged
        , OnUserTimedout
        , OnVIPsReceived
        , OnWhisperCommandReceived
        , OnWhisperReceived
        , OnWhisperSent
    };

    //internal class IOModule_Events
    //{
    //    public List<(IO_Events, EventCallBack)> EventCollection { get; private set; } = new List<(IO_Events, EventCallBack)>();

    //    /// <summary>
    //    /// Adds a new event handler to the list, replaces an existing handler.
    //    /// </summary>
    //    /// <param name="eventtype">they enum type of the event</param>
    //    /// <param name="Callback">the handler to attach</param>
    //    public void AddReplaceCommand(IO_Events eventtype, Action Callback)
    //    {
    //        int idx = GetIndex(eventtype);

    //        if (idx != -1)
    //        {
    //            EventCollection.RemoveAt(idx);
    //        }
            
    //        EventCollection.Add( (eventtype, Callback) );
    //    }

    //    public Action RetrieveAction(IO_Events events)
    //    {
    //        int idx = GetIndex(events);

    //        return idx != -1 ? EventCollection[idx].Item2 : null;
    //    }

    //    /// <summary>
    //    /// Search collection list for the index of the supplied event.
    //    /// </summary>
    //    /// <param name="events">Enumerated reference to event.</param>
    //    /// <returns>index of requested item, or -1 for not found</returns>
    //    public int GetIndex(IO_Events events)
    //    {
    //        return EventCollection.FindIndex(
    //            (e) =>
    //            {
    //                if (e.Item1 == events) return true; else return false;
    //            }
    //        );
    //    }

    //}

    
}
