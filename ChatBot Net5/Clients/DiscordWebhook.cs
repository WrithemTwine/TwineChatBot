using System;
using System.Collections.Generic;
using System.Net.Http;

namespace ChatBot_Net5.Clients
{
    public static class DiscordWebhook
    {
        /// <summary>
        /// Send a message to provided Webhooks
        /// </summary>
        /// <param name="UriList">The POST Uris collection of webhooks.</param>
        /// <param name="Msg">The message to send.</param>
        public static void SendLiveMessage(IEnumerable<Uri> UriList, string Msg)
        {
            HttpClient client = new HttpClient();

            foreach(Uri u in UriList)
            {
                client.PostAsync(u.AbsoluteUri, new StringContent(Msg));
            }
        }
    }
}
