using ChatBot_Net5.Models;

using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Documents;

using TwitchLib.Client.Models;

namespace ChatBot_Net5.BotIOController
{
    public partial class BotController
    {        
        public ObservableCollection<UserJoin> JoinCollection { get; set; } = new ();

        public FlowDocument ChatData { get; private set; } = new FlowDocument();
        private delegate void ProcMessage(ChatMessage message);

        private void AddChatString(ChatMessage s)
        {
            ProcMessage Msg = UpdateMessage;
            //Application.Current.Dispatcher.BeginInvoke(Msg, s);
        }

        private void UpdateMessage(ChatMessage s)
        {
            Paragraph p = new Paragraph();
            p.ElementStart.DocumentStart.InsertTextInRun(s.Message);
            ChatData.Blocks.Add(p);
        }        
    }
}
