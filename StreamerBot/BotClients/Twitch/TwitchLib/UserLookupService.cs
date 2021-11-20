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

        public async Task<GetChannelInformationResponse> GetChannelInformation(string UserId)
        {
            return await _api.Helix.Channels.GetChannelInformationAsync(UserId);
        }
    }
}
