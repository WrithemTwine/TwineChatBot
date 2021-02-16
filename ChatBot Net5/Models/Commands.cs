using System.Collections.Generic;

namespace ChatBot_Net5.BotIOController.Models
{
    public class Command
    {
        public string Parameter { get; set; }
        public string Value { get; set; }
    }

    public class CommandCollection
    {
        public List<Command> Collection { get; private set; }

        public CommandCollection()
        {
            Collection = new List<Command>()
            {
                new Command() {Parameter= "#autohost", Value=" - permits distinguishing if the channel is hosted or autohosted"},
                new Command() {Parameter= "#months", Value=" - how many months subscribed"},
                new Command() {Parameter= "#receiveuser", Value=" - the user who received the gifted subscription"},
                new Command() {Parameter= " #streak", Value=" - current sub streak months, depends on user preference to share"},
                new Command() {Parameter= "#submonths", Value=" - display name of the user sending action"},
                new Command() {Parameter= "#subplan", Value=" - the subscription plan"},
                new Command() {Parameter= "#subplanname", Value=" - the subscription plan name"},
                new Command() {Parameter= "#user", Value=" - display name of the user sending action"},
                new Command() {Parameter= " #viewers", Value=" - display name of the user sending action"},
                new Command() {Parameter= " #category", Value=" - current category"},
                new Command() {Parameter= " #title", Value=" - current live stream title"},
                new Command() {Parameter= " #url", Value=" - Twitch URL to the user's page"}

            };
        }
    }
}
