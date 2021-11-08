using StreamerBot.Data;
using StreamerBot.Events;

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

        private SystemsBase Stats { get; set; }

        public event EventHandler<PostChannelMessageEventArgs> PostChannelMessage;

        public SystemsController()
        {
            Stats = new();

            Stats.PostChannelMessage += Stats_PostChannelMessage;
        }

        private void Stats_PostChannelMessage(object sender, PostChannelMessageEventArgs e)
        {
            PostChannelMessage?.Invoke(this, e);
        }
    }
}
