using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreamerBotLib.Events
{
    public class ImportDataProgressUpdateEventArgs(int currentProgressAmount) : EventArgs
    {
        public int CurrentProgressAmount { get; set; } = currentProgressAmount;
    }
}
