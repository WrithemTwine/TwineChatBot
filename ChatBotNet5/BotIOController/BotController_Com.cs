using ChatBot_Net5.Models;

using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

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
            p.Inlines.Add(new Run(DateTime.Now.ToLocalTime().ToString("h:mm") + " "));
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
