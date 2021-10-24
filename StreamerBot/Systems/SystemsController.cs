using StreamerBot.Data;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreamerBot.Systems
{
    public class SystemsController
    {
        public static DataManager DataManage { get; private set; } = new();
    }
}
