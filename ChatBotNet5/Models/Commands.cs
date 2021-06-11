using System.Collections.Generic;
using System.Diagnostics;

namespace ChatBot_Net5.BotIOController.Models
{
    [DebuggerDisplay("Parameter={Parameter}, Value={Value}")]
    public class Command
    {
        public string Parameter { get; set; }
        public string Value { get; set; }
    }

    public class CommandCollection : IComparer<Command>
    {
        public List<Command> Collection { get; private set; }

        public CommandCollection()
        {
            Collection = new List<Command>()
            {
                new Command() {Parameter= "#autohost", Value=" - permits distinguishing if the channel is hosted or autohosted"},
                new Command() {Parameter= "#count", Value=" - the number of donated subscriptions with tier plan: e.g. 5 Tier 1 Subscriptions"},
                new Command() {Parameter= "#months", Value=" - how many months subscribed this time"},
                new Command() {Parameter= "#receiveuser", Value=" - the user who received the gifted subscription"},
                new Command() {Parameter= "#streak", Value=" - current sub streak months, depends on user preference to share"},
                new Command() {Parameter= "#submonths", Value=" - how many overall months subscribed"},
                new Command() {Parameter= "#subplan", Value=" - the subscription plan"},
                new Command() {Parameter= "#subplanname", Value=" - the subscription plan name"},
                new Command() {Parameter= "#user", Value=" - display name of the user sending action"},
                new Command() {Parameter= "#viewers", Value=" - display name of the user sending action"},
                new Command() {Parameter= "#category", Value=" - current category"},
                new Command() {Parameter= "#title", Value=" - current live stream title"},
                new Command() {Parameter= "#url", Value=" - Twitch URL to the user's page"}
            };

            Collection.Sort(Compare);
        }

        public int Compare(Command x, Command y)
        {
            return x.Parameter.CompareTo(y.Parameter);
        }
    }
}
