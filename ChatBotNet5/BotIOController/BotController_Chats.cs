using System;
using System.Globalization;
using System.Windows;
using System.Windows.Documents;

using TwitchLib.Client.Models;

namespace ChatBot_Net5.BotIOController
{
    public partial class BotController
    {
        public FlowDocument ChatData { get; private set; } = new();

        private delegate void ProcMessage(ChatMessage message);

        private void AddChatString(ChatMessage s)
        {
            ProcMessage Msg = UpdateMessage;
            Application.Current.Dispatcher.BeginInvoke(Msg, s);
        }

        private void UpdateMessage(ChatMessage s)
        {
            Paragraph p = new();
            string time = DateTime.Now.ToLocalTime().ToString("h:mm", CultureInfo.CurrentCulture) + " ";
            p.Inlines.Add(new Run(time));
            p.Inlines.Add(new Run(s.DisplayName + ": "));
            p.Inlines.Add(new Run(s.Message));
            //p.Foreground = new SolidColorBrush(Color.FromArgb(a: s.Color.A,
            //                                                  r: s.Color.R,
            //                                                  g: s.Color.G,
            //                                                  b: s.Color.B));
            ChatData.Blocks.Add(p);
        }
    }
}
