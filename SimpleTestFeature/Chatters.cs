using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using TwitchLib.Api;
using TwitchLib.Api.Core;
using TwitchLib.Api.Helix.Models.Chat.GetChatters;

namespace SimpleTestFeature
{
    public class Chatters
    {
        public ObservableCollection<string> ChatList { get; private set; } = [];

        public Chatters()
        {
            string clientId = "";
            string accesstoken = "";
            string channelId = "";
            string moderatorId = "";

            Thread thread = new(() => { 
            
                foreach(string s in from U in TestGetChatAsync(clientId, accesstoken, channelId, moderatorId).Result.Data
                              select U.UserName)
                {
                    ChatList.Add(s);
                }
            
            });

            thread.Start();
        }

        public async Task<GetChattersResponse> TestGetChatAsync(string clientId, string accesstoken, string channelId, string moderatorId)
        {
            TwitchAPI testChat = new(settings: new ApiSettings() { AccessToken = accesstoken, ClientId = clientId });
            return await testChat.Helix.Chat.GetChattersAsync(channelId, moderatorId, accessToken: accesstoken);
        }
    }
}
