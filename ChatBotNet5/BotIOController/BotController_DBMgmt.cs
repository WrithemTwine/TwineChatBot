using ChatBot_Net5.Static;
using ChatBot_Net5.Systems;

namespace ChatBot_Net5.BotIOController
{
    public partial class BotController
    {
        /// <summary>
        /// This method checks the user settings and will delete any DB data if the user unchecks the setting. 
        /// Other methods to manage users & followers will adapt to if the user adjusted the setting
        /// </summary>
        public void ManageDatabase()
        {
            BotSystems.ManageDatabase();
            // TODO: add fixes if user re-enables 'managing { users || followers || stats }' to restart functions without restarting the bot

            // if ManageUsers is False, then remove users!
            if (OptionFlags.ManageUsers)
            {
                Stats.ManageUsers();
            }

            // if ManageFollowers is False, then remove followers!, upstream code stops the follow bot
            if (OptionFlags.ManageFollowers)
            {
                BeginAddFollowers();
            }
            // when management resumes, code upstream enables the startbot process

            //  if ManageStreamStats is False, then remove all Stream Statistics!
            if (!OptionFlags.ManageStreamStats)
            {
                Stats.EndPostingStreamUpdates();
            } // when the LiveStream Online event fires again, the datacollection will restart
        }

        /// <summary>
        /// Clear all user watchtimes
        /// </summary>
        public void ClearWatchTime()
        {
            BotSystems.ClearWatchTime();
        }

        /// <summary>
        /// Clear all accrued user currencies
        /// </summary>
        public void ClearAllCurrenciesValues()
        {
            BotSystems.ClearAllCurrenciesValues();
        }
    }
}
