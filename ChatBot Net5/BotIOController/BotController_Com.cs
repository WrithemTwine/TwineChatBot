using ChatBot_Net5.Models;

using System.Collections.ObjectModel;
using System.Windows.Documents;

using TwitchLib.Client.Models;

namespace ChatBot_Net5.BotIOController
{
    public partial class BotController
    {        
        public ObservableCollection<UserJoin> JoinCollection { get; set; } = new ();

        public FlowDocument ChatData { get; private set; } = new FlowDocument();
        
        private void AddChatString(ChatMessage s)
        {
            Paragraph p = new Paragraph();
            p.ElementStart.InsertTextInRun(s.Message);

           // ChatData.Blocks.Add(p);
        }


    }
}
