using StreamerBotLib.Models.BotManagement;
using StreamerBotLib.Models.Enums;

namespace StreamerBotLib.BotIOController
{
    /// <summary>
    /// Defines the Bot status for the GUI, and build the API objects
    /// </summary>
    public partial class BotController
    {
        private List<BotOperationBase> BotOperationStatus = [];

        public bool GetBotOperationStatus(Platform platform, BotOperationTypes botOperationType)
        {
            var botOperation = BotOperationStatus.FirstOrDefault(x => x.Platform == platform);
            if (botOperation == null)
            {
                return false;
            }
            return botOperation[botOperationType];
        }
    }
}
