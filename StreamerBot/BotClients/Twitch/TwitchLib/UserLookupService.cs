using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using TwitchLib.Api.Helix.Models.Channels.GetChannelInformation;
using TwitchLib.Api.Interfaces;
using TwitchLib.Api.Services;

namespace StreamerBot.BotClients.Twitch.TwitchLib
{
    public class UserLookupService : ApiService
    {
        public UserLookupService(ITwitchAPI api, int checkIntervalInSeconds = 60) : base(api, checkIntervalInSeconds)
        {
        }

        public async Task<GetChannelInformationResponse> GetChannelInformation(string UserId = null, string UserName = null)
        {
            if (UserId != null)
            {
                return await _api.Helix.Channels.GetChannelInformationAsync(UserId);
            }
            else if (UserName != null)
            {
                string channelId = (await _api.Helix.Users.GetUsersAsync(logins: new List<string> { UserName })).Users.FirstOrDefault()?.Id;
                return await _api.Helix.Channels.GetChannelInformationAsync(channelId);
            }

            return null;
        }
    }
}
