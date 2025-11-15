using System;
using System.Net.Http;

namespace SimpleTestFeature.Exceptions
{
    /// <summary>
    /// Exception object specific to HTTP status code 429, Too Many Request.
    /// </summary>
    public class TooManyRequestsException(string message,
                                    DateTime resetTime = default,
                                    HttpResponseMessage httpResponseMessage = null) : Exception(message)
    {

        /// <summary>
        /// The exception message.
        /// </summary>
        public new string Message { get; set; }
        /// <summary>
        /// The time when the request allowance is reset for another request.
        /// </summary>
        public DateTime ResetTime { get; set; } = resetTime;
        /// <summary>
        /// The full http response message.
        /// </summary>
        public HttpResponseMessage HttpResponseMessage { get; set; } = httpResponseMessage;
    }
}
