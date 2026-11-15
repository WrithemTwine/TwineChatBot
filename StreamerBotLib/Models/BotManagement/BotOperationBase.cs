using StreamerBotLib.Models.Enums;

using System;
using System.Collections.Generic;
using System.Text;

namespace StreamerBotLib.Models.BotManagement
{
    public abstract class BotOperationBase(Platform platform)
    {
        public bool this[BotOperationTypes operationType]
        {
            get
            {
                return operationType switch
                {
                    BotOperationTypes.ReadyToAuthorize => ReadyToAuthorize,
                    BotOperationTypes.Authorize => Authorized,
                    BotOperationTypes.ApiReady => ApiReady,
                    BotOperationTypes.BotsActive => BotsActive,
                    BotOperationTypes.UseSecondAccount => UseSecondAccount,
                    _ => throw new ArgumentOutOfRangeException(nameof(operationType), operationType, null)
                };
            }
        }

        /// <summary>
        /// Specifies the platform for the current bot calculation
        /// </summary>
        public Platform Platform { get; protected set; } = platform;
        /// <summary>
        /// Specifies a flag for the credentials are ready to authorize</summary>
        /// <value><c>true</c> the user entered the necessary data to authorize <br />
        /// <c>false</c> the user has not entered all required data</value>
        public bool ReadyToAuthorize { get; protected set; } = false;
        /// <summary>
        /// Specifies whether the credentials are authorized</summary>
        /// <value><c>true</c> the credentials are authorized <br />
        /// <c>false</c> the credentials are not currently authorized</value>
        public bool Authorized { get; protected set; } = false;
        /// <summary>
        /// Specifies whether the API is built and ready</summary>
        /// <value><c>true</c> the API is ready <br />
        /// <c>false</c> the API is not ready</value>
        public bool ApiReady { get; protected set; } = false;
        /// <summary>
        /// Specifies whether the bots are active</summary>
        /// <value><c>true</c> the bots are active <br />
        /// <c>false</c> the bots are not active</value>
        public bool BotsActive { get; protected set; } = false;
        /// <summary>
        /// Specifies if the user is using another account to use alongside the account for the bot to process actions. Primarily to help the GUI determine what controls should be displayed to the user.
        /// </summary>
        public bool UseSecondAccount { get; protected set; } = false;

        /// <summary>
        /// Check the user has entered the data necessary to authorize application access to the platform accounts.
        /// </summary>
        /// <returns><c>true</c> - the user has entered all required data, <c>false</c> - the user has not entered all required data</returns>
        public abstract Task<bool> TryCheckReadyToAuthorizeAsync();
        /// <summary>
        /// Check the platform credentials are authorized
        /// </summary>
        /// <returns><c>true</c> - the credentials are authorized, <c>false</c> - the credentials are not authorized</returns>
        public abstract Task<bool> TryCheckAuthorizeAsync();
        /// <summary>
        /// Build the platform API components
        /// </summary>
        /// <returns><c>true</c> - the api built successfully, <c>false</c> - failed to build the api</returns>
        public abstract Task<bool> TryBuildApiAsync();
        /// <summary>
        /// Check whether the platform API is active and ready to use for accessing the platform.
        /// </summary>
        /// <returns><c>true</c> -  the platform API is ready to use, <c>false</c> - the platform API is not ready to use.</returns>
        public abstract Task<bool> TryCheckApiReadyAsync();
        /// <summary>
        /// Check if any bot is active, within the current platform
        /// </summary>
        /// <returns><c>true</c> - any bot is active, <c>false</c> - no bots are active</returns>
        public abstract Task<bool> TryCheckBotsActiveAsync();
        /// <summary>
        /// Check a specific bot if it is active
        /// </summary>
        /// <param name="bot">the name of the bot to check</param>
        /// <returns><c>true</c> - active bot, <c>false</c> - inactive bot</returns>
        public abstract Task<bool> TryCheckBotsActiveAsync(Bots bot);

        /// <summary>
        /// Determines if every string in the attached collection is null or whitespace.
        /// </summary>
        /// <param name="strings">The collection of strings to check.</param>
        /// <returns><c>true</c> if any string is null or whitespace; otherwise, <c>false</c>.</returns>
        protected bool CalculateEmptyString(IEnumerable<string> strings)
        {
            return strings.Any(s => string.IsNullOrWhiteSpace(s));
        }

    }
}
