﻿
using StreamerBotLib.Static;

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;

namespace StreamerBotLib.BotClients
{

#if BrokenCode
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
        public static void SendMessage(Uri uri, string Msg, string MentionUser = "", bool AllowEveryone = false)
        {
            AllowedMentionTypes[] UserSpecMentions;
            if (AllowEveryone)
            {
                UserSpecMentions = new[] { AllowedMentionTypes.everyone, AllowedMentionTypes.users };
            } else
            {
                UserSpecMentions = new[] { AllowedMentionTypes.roles, AllowedMentionTypes.users };
            }

            //      JsonContent content = JsonContent.Create(new WebhookJSON(Msg, null, null, false, null, null, null,
            //      new AllowedMentions(UserSpecMentions, null, new[] { MentionUser })));

            JsonContent content = JsonContent.Create(new WebhookJSON(Msg, new AllowedMentions(new AllowedMentionTypes[] { AllowedMentionTypes.everyone }, null, null)));

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

            Parse = tempmentions.ConvertAll(m => m.ToString()).ToArray();

            this.Roles = Roles?.Length > max_data ? Roles[..max_data] : Roles;
            this.Users = Users?.Length > max_data ? Users[..max_data] : Users;
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

#endif
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
        public static void SendMessage(Uri uri, string Msg)
        {
            JsonContent content = JsonContent.Create(new WebhookJSON(Msg, new AllowedMentions(new AllowedMentionTypes[] { AllowedMentionTypes.everyone }, null, null)));

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

    public class AllowedMentions
    {
        private const int max_data = 100;

        public string[] Parse { get; private set; }
        public string[] Roles { get; private set; }
        public string[] Users { get; private set; }

        public AllowedMentions(AllowedMentionTypes[] mentions, string[] Roles, string[] Users)
        {
            List<string> temp = new();
            List<AllowedMentionTypes> tempmentions = new(mentions);

            tempmentions.UniqueAdd(AllowedMentionTypes.everyone);
            tempmentions.UniqueAdd(AllowedMentionTypes.roles);
            tempmentions.UniqueAdd(AllowedMentionTypes.users);

            Parse = temp.ToArray();

            if (Roles?.Length > max_data)
            {
                List<string> temproles = new(Roles);
                temproles.RemoveRange(max_data, temproles.Count - max_data);
                this.Roles = temproles.ToArray();
            }
            else
            {
                this.Roles = Roles;
            }

            if (Users?.Length > max_data)
            {
                List<string> tempusers = new(Users);
                tempusers.RemoveRange(max_data, tempusers.Count - max_data);
                this.Users = tempusers.ToArray();
            }
            else
            {
                this.Users = Users;
            }
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Names are required in lower case because of the Discord/Webhook JSON specification.")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "CA1707:Underscores in Names", Justification = "The underscores in names are required for the Discord/Webhook JSON specification.")]
    public class WebhookJSON
    {
        //private const int max_embeds = 10;

        public string content { get; private set; } // max 2000 characters
        public string username { get; private set; }
        public string avatar_url { get; private set; }
        public bool tts { get; private set; }

        public object file { get; private set; } = null;
        public object[] embeds { get; private set; } = null; // Discord expects to remove in future API updates
        public string payload_json { get; private set; }
        public AllowedMentions allowed_mentions { get; private set; }

        public WebhookJSON(string Content, AllowedMentions Allowed_Mentions, string Username = null, string Avatar_Url = null, bool TTS = false,
            string Payload_Json = null)
        {
            content = Content[..Math.Min(2000,Content.Length)];
            allowed_mentions = Allowed_Mentions;
            username = Username;
            avatar_url = Avatar_Url;
            tts = TTS;
            payload_json = Payload_Json;
        }
    }

}
