﻿using StreamerBot.BotClients;

namespace StreamerBot.GUI
{
    public class GUIMultiLiveDataManager
    {
        public static MultiUserLiveBot.Data.DataManager MultiLiveDataManager { get; private set; } = BotsTwitch.MultiLiveDataManager;
    }
}
