using ChatBot_Net5.Systems;

using System.Collections.Generic;

namespace ChatBot_Net5.Models
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
