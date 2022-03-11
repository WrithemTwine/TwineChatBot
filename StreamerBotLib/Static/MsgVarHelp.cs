﻿using StreamerBotLib.Models;
using StreamerBotLib.Systems;

using System.Collections.Generic;

namespace StreamerBotLib.Static
{
    public class MsgVarHelp : IComparer<Command>
    {
        public List<Command> Collection { get; private set; }

        public MsgVarHelp()
        {
            Collection = LocalizedMsgSystem.GetCommandHelp();

            Collection.Sort(Compare);
        }

        public int Compare(Command x, Command y)
        {
            return x.Parameter.CompareTo(y.Parameter);
        }
    }
}