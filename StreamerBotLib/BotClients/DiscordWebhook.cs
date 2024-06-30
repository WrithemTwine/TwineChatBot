using StreamerBotLib.Static;

using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

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
        public static void SendMessage(Uri uri, string Msg, string itemurl = null)
        {
            JsonContent content = JsonContent.Create(
                new WebhookJSON(Msg,
                                new([AllowedMentionTypes.everyone], null, null),
                                null));

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

                // wait so WebHooks doesn't complain about bots posting too fast to a page
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
    //    JSON/Form Params - WebHooks Webhooks
    //FIELD         TYPE                                DESCRIPTION                                     REQUIRED
    //content       string                              the message contents(up to 2000 characters)     one of content, file, embeds
    //username      string                              override the default username of the webhook    false
    //avatarurl    string                              override the default avatar of the webhook      false
    //tts           boolean                             true if this is a tts message                   false
    //file          file contents                       the contents of the file being sent             one of content, file, embeds
    //embeds        array of up to 10 embed objects     embedded rich content                           one of content, file, embeds
    //payloadjson    string                            See message create                              multipart/form-data only
    //allowedmentions allowed mention object           allowed mentions for the message	            false
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
    //  
    //  https://discord.com/developers/docs/resources/channel#embed-object
    // 
    // FIELD	TYPE	DESCRIPTION
    //  title? string title of embed
    //  type? string type of embed(always "rich" for webhook embeds)
    //  description? string description of embed
    //  url? string url of embed
    //  timestamp? ISO8601 timestamp timestamp of embed content
    //  color? integer color code of the embed
    //  footer? embed footer object footer information
    //  image? embed image object image information
    //  thumbnail? embed thumbnail object thumbnail information
    //  video? embed video object video information
    //  provider? embed provider object provider information
    //  author? embed author object author information
    //  fields? array of embed field objects    fields information, max of 25

    public enum AllowedMentionTypes { roles, users, everyone }

    internal class AllowedMentions
    {
        private const int max_data = 100;

        [AllowNull, JsonPropertyName("parse")]
        public string[] Parse { get; private set; }
        [AllowNull, JsonPropertyName("roles")]
        public string[] Roles { get; private set; }
        [AllowNull, JsonPropertyName("users")]
        public string[] Users { get; private set; }

        public AllowedMentions()
        {
        }

        public AllowedMentions(AllowedMentionTypes[] mentions, string[] roles, string[] users)
        {
            List<string> temp = [];
            List<AllowedMentionTypes> tempmentions = new(mentions);

            tempmentions.UniqueAdd(AllowedMentionTypes.everyone);
            tempmentions.UniqueAdd(AllowedMentionTypes.roles);
            tempmentions.UniqueAdd(AllowedMentionTypes.users);

            Parse = [.. temp];

            switch (Roles?.Length)
            {
                case > max_data:
                    {
                        List<string> temproles = new(roles);
                        temproles.RemoveRange(max_data, temproles.Count - max_data);
                        Roles = [.. temproles];
                        break;
                    }

                default:
                    Roles = roles;
                    break;
            }

            switch (Users?.Length)
            {
                case > max_data:
                    {
                        List<string> tempusers = new(users);
                        tempusers.RemoveRange(max_data, tempusers.Count - max_data);
                        Users = [.. tempusers];
                        break;
                    }

                default:
                    Users = users;
                    break;
            }
        }

    }

    internal class WebhookJSON(string content, AllowedMentions allowedmentions, string username = null, string avatarurl = null, bool tts = false,
        string payloadjson = null, object file = null, Embed[] embeds = null)
    {
        //private const int max_embeds = 10;

        [AllowNull, JsonPropertyName("content")]
        public string Content { get; private set; } = content != null ? content[..Math.Min(2000, content.Length)] : null;
        [AllowNull, JsonPropertyName("username")]
        public string Username { get; private set; } = username;
        [AllowNull, JsonPropertyName("avatarurl")]
        public string AvatarUrl { get; private set; } = avatarurl;
        [AllowNull, JsonPropertyName("tts")]
        public bool TTS { get; private set; } = tts;
        [AllowNull, JsonPropertyName("file")]
        public object File { get; private set; } = file;
        [AllowNull, JsonPropertyName("embeds")]
        public Embed[] Embeds { get; private set; } = embeds; // WebHooks expects to remove in future API updates
        [AllowNull, JsonPropertyName("payloadjson")]
        public string PayloadJson { get; private set; } = payloadjson;
        [AllowNull, JsonPropertyName("allowedmentions")]
        public AllowedMentions AllowedMentions { get; private set; } = allowedmentions;
    }

    internal class Embed(string title = null, string type = null, string description = null, string url = null,
        DateTime timestamp = default, int? color = null, object footer = null, object image = null,
        object thumbnail = null, object video = null, object provider = null, object author = null, object[] fields = null)
    {
        [AllowNull, JsonPropertyName("title")]
        public string Title { get; private set; } = title;
        [AllowNull, JsonPropertyName("type")]
        public string Type { get; private set; } = type;
        [AllowNull, JsonPropertyName("description")]
        public string Description { get; private set; } = description;
        [AllowNull, JsonPropertyName("url")]
        public string URL { get; private set; } = url;
        [AllowNull, JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; private set; } = timestamp;
        [AllowNull, JsonPropertyName("color")]
        public int? Color { get; private set; } = color;
        [AllowNull, JsonPropertyName("footer")]
        public object Footer { get; private set; } = footer;
        [AllowNull, JsonPropertyName("image")]
        public object Image { get; private set; } = image;
        [AllowNull, JsonPropertyName("thumbnail")]
        public object Thumbnail { get; private set; } = thumbnail;
        [AllowNull, JsonPropertyName("video")]
        public object Video { get; private set; } = video;
        [AllowNull, JsonPropertyName("provider")]
        public object Provider { get; private set; } = provider;
        [AllowNull, JsonPropertyName("author")]
        public object Author { get; private set; } = author;
        [AllowNull, JsonPropertyName("fields")]
        public object[] Fields { get; private set; } = fields;
    }

}
