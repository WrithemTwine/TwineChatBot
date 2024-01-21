using Newtonsoft.Json;

using SimpleTestFeature.Enums;
using SimpleTestFeature.Exceptions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;


namespace SimpleTestFeature.Com
{
    internal static class HelixIO
    {
        // TODO: Helix calls to Twitch require client Id and OAuth access token for api calls -
        // currently this code does not include

        private static readonly string OauthUrl = "https://id.twitch.tv/oauth2";
        private static readonly string HelixUrl = "https://api.twitch.tv/helix";

        private static string PickUrl(RequestUrlType requestUrlType = RequestUrlType.HelixUrl)
        {
            return requestUrlType switch
            {
                RequestUrlType.HelixUrl => HelixUrl,
                RequestUrlType.OauthUrl => OauthUrl,
                _ => null,
            };
        }

        private static T ParseResult<T>(HttpResponseMessage response, string result)
        {
            if (response.IsSuccessStatusCode)
            {
                T output = (T)JsonConvert.DeserializeObject<T>(result, new JsonSerializerSettings() { NullValueHandling=NullValueHandling.Ignore, MissingMemberHandling=MissingMemberHandling.Ignore});

                return output;
            }
            else if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                throw new TooManyRequestsException("Too many requests within a minute.",
                    DateTime.Parse(response.Headers.GetValues("Ratelimit-Reset").First()),
                    response);
            }
            else if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                throw new BadRequestException("The HTTP POST resulted in a bad request, and did not succeed.");
            }
            else if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedException("Access to this Url is unauthorized.");
            }
            else
            {
                return default;
            }
        }

        /// <summary>
        /// POST body content to a Twitch endpoint, according to the Uri type; either a Helix or Oauth2 endpoint.
        /// </summary>
        /// <typeparam name="T">The model class for the Json output response.</typeparam>
        /// <param name="baseUrl">The base Uri for the POST.</param>
        /// <param name="EndPoint">The API endpoint for the Uri, to accept the POST.</param>
        /// <param name="bodycontent">An Enumerated keyvalue pair to include in the body.</param>
        /// <returns>The response of the POST, in the form of the <typeparamref name="T"/> class.</returns>
        /// <exception cref="TooManyRequestsException">Twitch endpoints limit requests per minute, and Twitch server returns an HTTP 429 status code.</exception>
        /// <exception cref="BadRequestException">The access token or refresh token is no longer valid.</exception>
        internal static T TwitchPost<T>(RequestUrlType baseUrl, string EndPoint, IEnumerable<KeyValuePair<string, string>> bodycontent)
        {
            HttpResponseMessage response = TwitchHttpClient.PostAsync(Helpers.BuildUri(PickUrl(baseUrl), EndPoint), new FormUrlEncodedContent(bodycontent)).Result;
            string result = response.Content.ReadAsStringAsync().Result;

            return ParseResult<T>(response, result);
        }

        internal static T TwitchGet<T>(RequestUrlType baseUrl, string EndPoint, string QueryString, HttpRequestMessage content = null)
        {
            content.Method = HttpMethod.Get;

            HttpResponseMessage response = TwitchHttpClient.GetAsync(Helpers.BuildUri(PickUrl(baseUrl), EndPoint + QueryString), content).Result;
            string result = response.Content.ReadAsStringAsync().Result;
            return ParseResult<T>(response, result);
        }
    }
}
