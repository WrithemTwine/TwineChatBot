using SimpleTestFeature.Com;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;

using TwitchLib.Api;
using TwitchLib.Api.Core;
using TwitchLib.Api.Helix.Models.Chat.GetChatters;

namespace SimpleTestFeature
{
    public class Chatters
    {
        public ObservableCollection<string> ChatList { get; private set; } = [];

        private event EventHandler<GetChatterCompletedResponse> GetChatterCompleted;
        public Chatters()
        {
            GetChatterCompleted += Chatters_GetChatterCompleted;

            string clientId = "";
            string accesstoken = "";
            string channelId = "";
            string moderatorId = "";

            Thread thread = new(() =>
            {

                List<string> chatlisting = new(from U in TestGetChatAsync(clientId, accesstoken, channelId, moderatorId).Data
                                               select U.UserName);

                GetChatterCompleted?.Invoke(this, new() { Chatters = chatlisting });

                List<string> chatlistingHelix = new(from U in TestHelixGetChatters(clientId, accesstoken, channelId, moderatorId).Data
                                                    select U.UserName);

                GetChatterCompleted?.Invoke(this, new() { Chatters = chatlistingHelix });

            });

            thread.Start();

            // moderator:read:chatters
            // channel:moderate moderator:read:followers moderator:read:chatters
        }

        private void Chatters_GetChatterCompleted(object sender, GetChatterCompletedResponse e)
        {
            string a = "was getting exception here";
        }

        public static GetChattersResponse TestGetChatAsync(string clientId, string accesstoken, string channelId, string moderatorId)
        {
            //TwitchAPI testChat = new(settings: new ApiSettings() { AccessToken = accesstoken, ClientId = clientId });
            //return await testChat.Helix.Chat.GetChattersAsync(channelId, moderatorId, accessToken: accesstoken);

            HttpRequestMessage httpRequest = new();
            httpRequest.Headers.Add($"Client-Id", $" {clientId}");
            httpRequest.Headers.Add($"Authorization", $"Bearer {accesstoken}");
            httpRequest.Headers.Add(HttpRequestHeader.Accept.ToString(), "application/json");
            httpRequest.Headers.Add(HttpRequestHeader.AcceptEncoding.ToString(), "gzip,deflate");

            Dictionary<string, string> query = new()
            {
                { "broadcaster_id", channelId },
                { "moderator_id", moderatorId },
                { "first", "100" }
            };

            return HelixIO.TwitchGet<GetChattersResponse>(Enums.RequestUrlType.HelixUrl, $"chat/chatters", $"?{Helpers.BuildQueryString(query)}", httpRequest);
        }

        public static GetChattersResponse TestHelixGetChatters(string clientid, string accesstoken, string channelid, string moderatorid)
        {
            TwitchAPI helixApi = new(settings: new ApiSettings() { AccessToken = accesstoken, ClientId = clientid });

            return helixApi.Helix.Chat.GetChattersAsync(channelid, moderatorid).Result;
        }

        private class GetChatterCompletedResponse : EventArgs
        {
            public List<string> Chatters { get; set; }
        }
    }
}
