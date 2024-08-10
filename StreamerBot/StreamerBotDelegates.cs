using StreamerBotLib.Interfaces;

using System;
using System.Windows.Controls;

namespace StreamerBot
{
    public partial class StreamerBotWindow
    {
        #region delegates
        internal delegate void RefreshBotOp(Button targetclick, Action<string> InvokeMethod);
        internal delegate void BotOperation();
        #endregion

        internal void DispatchStartBot(IIOModule sender)
        {
            Dispatcher.BeginInvoke(new BotOperation(() =>
            {
                if (sender.BotClientName == StreamerBotLib.Enums.Bots.MediaOverlayServer)
                {
                    PrepareMediaOverlayServerWindow();
                }

                (sender)?.StartBot();
            }));
        }

        internal void DispatchStopBot(IIOModule sender)
        {
            Dispatcher.BeginInvoke(new BotOperation(() =>
            {
                (sender)?.StopBot();
            }));
        }

    }
}
