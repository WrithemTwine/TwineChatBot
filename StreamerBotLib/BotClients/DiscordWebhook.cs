
using StreamerBotLib.Static;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading;

namespace StreamerBotLib.BotClients
{
    public static class DiscordWebhook
    {
        private static readonly HttpClient client = new();

        private static readonly Queue<Tuple<Uri, JsonContent>> DataJobs = new();
        private static bool JobThread;

        /// <summary>
        /// Send a message to provided Webhooks
        /// </summary>
        /// <param name="UriList">The POST Uris collection of webhooks.</param>
        /// <param name="Msg">The message to send.</param>
        public static void SendMessage(Uri uri, string Msg, bool AllowEveryone = false)
        {
            AllowedMentionTypes[] UserSpecMentions = null;
            if (AllowEveryone)
            {
                UserSpecMentions = new[] { AllowedMentionTypes.everyone };
            }
            
            JsonContent content = JsonContent.Create(new WebhookJSON(Msg, null, null, false, null, null, null,
                new AllowedMentions(UserSpecMentions, null, null)));

            lock (DataJobs)
            {
                DataJobs.Enqueue(new(uri, content));
            }

            if (!JobThread) // check if job thread is running
            {
                ThreadManager.CreateThreadStart(SendDataAsync);

                lock (DataJobs)
                {
                    JobThread = true;
                }
            }
        }

        private async static void SendDataAsync()
        {
            while (DataJobs.Count > 0)
            {
                Tuple<Uri, JsonContent> job;
                lock (DataJobs)
                {
                    job = DataJobs.Dequeue();
                }

                _ = await client.PostAsync(job.Item1.AbsoluteUri, job.Item2);

                // wait so Discord doesn't complain about bots posting too fast to a page
                Thread.Sleep(20000); // wait 20 seconds between posting, 3 posts a minute
            }

            lock (DataJobs)
            {
                JobThread = false;  // job thread stopped, signal to start another thread
            }
        }

    }


    // Webhook JSON format for sending data
    //
    // https://discord.com/developers/docs/resources/webhook#execute-webhook
    //
    //    JSON/Form Params - Discord Webhooks
    //FIELD         TYPE                                DESCRIPTION                                     REQUIRED
    //content       string                              the message contents(up to 2000 characters)     one of content, file, embeds
    //username      string                              override the default username of the webhook    false
    //avatar_url    string                              override the default avatar of the webhook      false
    //tts           boolean                             true if this is a TTS message                   false
    //file          file contents                       the contents of the file being sent             one of content, file, embeds
    //embeds        array of up to 10 embed objects     embedded rich content                           one of content, file, embeds
    //payload_json    string                            See message create                              multipart/form-data only
    //allowed_mentions allowed mention object           allowed mentions for the message	            false
    //
    //
    //
    // https://discord.com/developers/docs/resources/channel#allowed-mentions-object
    //
    //     Allowed Mention Types
    // TYPE                 VALUE       DESCRIPTION
    // Role Mentions	    "roles"	    Controls role mentions
    // User Mentions	    "users"	    Controls user mentions
    // Everyone Mentions	"everyone"	Controls @everyone and @here mentions
    //
    //     Allowed Mentions Structure
    // FIELD        TYPE                            DESCRIPTION
    // parse        array of allowed mention types  An array of allowed mention types to parse from the content.
    // roles        list of snowflakes              Array of role_ids to mention (Max size of 100)
    // users        list of snowflakes              Array of user_ids to mention(Max size of 100)
    // replied_user boolean                         For replies, whether to mention the author of the message being replied to(default false)
    //

    public enum AllowedMentionTypes { roles, users, everyone }

    public record AllowedMentions
    {
        private const int max_data = 100;


        [property: JsonPropertyName("parse")]
        public string[] Parse { get; init; }

        [property: JsonPropertyName("roles")]
        public string[] Roles { get; init; }

        [property: JsonPropertyName("users")]
        public string[] Users { get; init; }

        public AllowedMentions(AllowedMentionTypes[] mentions, string[] Roles, string[] Users)
        {
            List<AllowedMentionTypes> tempmentions = new(mentions);

            tempmentions.UniqueAdd(AllowedMentionTypes.everyone);
            tempmentions.UniqueAdd(AllowedMentionTypes.roles);
            tempmentions.UniqueAdd(AllowedMentionTypes.users);

            Parse = new List<string>().ToArray();

            this.Roles = Roles[..max_data];
            this.Users = Users[..max_data];
        }
    }

    public record WebhookJSON
    {
        public WebhookJSON(string content, string username, string avatar_url, bool tts, object file, object[] embeds, string payload_json, AllowedMentions allowed_mentions)
        {
            Content = content;
            Username = username;
            Avatar_url = avatar_url;
            Tts = tts;
            File = file;
            Embeds = embeds;
            Payload_json = payload_json;
            Allowed_mentions = allowed_mentions;
        }

        //private const int max_embeds = 10;

        [property: JsonPropertyName("content")]
        public string Content { get; init; }
        [property: JsonPropertyName("username")]
        public string Username { get; init; }
        [property: JsonPropertyName("avatar_url")]
        public string Avatar_url { get; init; }
        [property: JsonPropertyName("tts")]
        public bool Tts { get; init; }

        [property: JsonPropertyName("file")]
        public object File { get; init; } = null;
        [property: JsonPropertyName("embeds")]
        public object[] Embeds { get; init; } = null; // Discord expects to remove in future API updates
        [property: JsonPropertyName("payload_json")]
        public string Payload_json { get; init; }
        [property: JsonPropertyName("allowed_mentions")]
        public AllowedMentions Allowed_mentions { get; init; }


    }


}
