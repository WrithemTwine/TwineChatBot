namespace StreamerBotLib.Static
{
    internal static class Helpers
    {
        /// <summary>
        /// Converts an Dictionary of key,value pairs into a query string style output;
        /// "key1=value1&key2=value2&key3=value3...&keyN=valueN"
        /// </summary>
        /// <param name="dictionary">Key,Value pairs of data to convert.</param>
        /// <returns>A "key1=value1&key2=value2&key3=value3...&keyN=valueN" style string.</returns>
        internal static string BuildQueryString(Dictionary<string, string> dictionary)
        {
            return string.Join('&', from d in dictionary
                                    select $"{d.Key}={d.Value}");
        }

        /// <summary>
        /// Prepares a full length Uri with a base Url and the endpoing suffix, for use in HttpClient.
        /// </summary>
        /// <param name="baseUrl">The Url start to use for the initial URL.</param>
        /// <param name="endpoint">The API endpoint for the Url.</param>
        /// <returns>A formatted Uri for using in HttpClient calls.</returns>
        internal static Uri BuildUri(string baseUrl, string endpoint)
        {
            return new($"{baseUrl}/{endpoint.Replace("/", "")}");
        }
    }
}
