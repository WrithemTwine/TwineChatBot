using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace ChatBot_Net5.Clients
{
    public static class DiscordWebhook
    {
        private static HttpClient client = new HttpClient();

        /// <summary>
        /// Send a message to provided Webhooks
        /// </summary>
        /// <param name="UriList">The POST Uris collection of webhooks.</param>
        /// <param name="Msg">The message to send.</param>
        public async static Task SendLiveMessage(Uri uri, string Msg)
        {
            JsonContent content = JsonContent.Create(new WebhookJSON(Msg, new AllowedMentions(new AllowedMentionTypes[] { AllowedMentionTypes.everyone }, null, null)));

            await client.PostAsync(uri.AbsoluteUri, content);
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

        public string[] parse { get; private set; }        
        public string[] roles { get; private set; }
        public string[] users { get; private set; }

        public AllowedMentions(AllowedMentionTypes[] mentions, string[] Roles, string[] Users)
        {
            List<string> temp = new List<string>();
            List<AllowedMentionTypes> tempmentions = new List<AllowedMentionTypes>(mentions);

            if (tempmentions.Contains(AllowedMentionTypes.everyone)) temp.Add(AllowedMentionTypes.everyone.ToString());
            if (tempmentions.Contains(AllowedMentionTypes.roles)) temp.Add(AllowedMentionTypes.roles.ToString());
            if (tempmentions.Contains(AllowedMentionTypes.users)) temp.Add(AllowedMentionTypes.users.ToString());

            parse = temp.ToArray();
            
            if(Roles?.Length > max_data)
            {
                List<string> temproles = new List<string>(Roles);
                temproles.RemoveRange(max_data, temproles.Count - max_data);
                roles = temproles.ToArray();
            } 
            else
            {
                roles = Roles;
            }

            if (Users?.Length > max_data)
            {
                List<string> tempusers = new List<string>(Users);
                tempusers.RemoveRange(max_data, tempusers.Count - max_data);
                users = tempusers.ToArray();
            }
            else
            {
                users = Users;
            }
        }
    }


    public class WebhookJSON
    {
        //private const int max_embeds = 10;

        public string content { get; private set; }
        public string username { get; private set; }
        public string avatar_url { get; private set; }
        public bool tts { get; private set; }
        public object file { get; private set; } = null;
        public object[] embeds { get; private set; } = null; // Discord expects to remove in future API updates
        public string payload_json { get; private set; }
        public AllowedMentions allowed_mentions { get; private set; }

        public WebhookJSON(string Content, AllowedMentions Allowed_Mentions, string Username=null, string Avatar_Url=null, bool TTS=false,
            string Payload_Json=null)
        {
            content = Content;
            allowed_mentions = Allowed_Mentions;
            username = Username;
            avatar_url = Avatar_Url;
            tts = TTS;
            payload_json = Payload_Json;
        }
    }


}
