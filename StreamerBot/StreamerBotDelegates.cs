using StreamerBotLib.Interfaces;

using System.Windows.Controls;

namespace StreamerBot
{
    public partial class StreamerBotWindow
    {
        #region delegates
        internal delegate void RefreshBotOp(Button targetclick, Action<string> InvokeMethod);
        internal delegate void BotOperation();
        #endregion

        internal async void DispatchStartBotAsync(IIOModule sender)
        {
            await sender?.StartBot();
        }

        internal async void DispatchStopBot(IIOModule sender)
        {
            await sender?.StopBot();
        }


    }
}
