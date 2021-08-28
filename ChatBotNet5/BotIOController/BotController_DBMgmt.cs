using ChatBot_Net5.Static;

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
            // TODO: add fixes if user re-enables 'managing { users || followers || stats }' to restart functions without restarting the bot

            // if ManageUsers is False, then remove users!
            if(!OptionFlags.ManageUsers) { DataManage.RemoveAllUsers(); } else
            {
                Stats.ManageUsers();
            }

            // if ManageFollowers is False, then remove followers!, upstream code stops the follow bot
            if (!OptionFlags.ManageFollowers) { DataManage.RemoveAllFollowers(); }
            // when management resumes, code upstream enables the startbot process

            //  if ManageStreamStats is False, then remove all Stream Statistics!
            if (!OptionFlags.ManageStreamStats) { DataManage.RemoveAllStreamStats(); }
        }

        /// <summary>
        /// Clear all user watchtimes
        /// </summary>
        public void ClearWatchTime()
        {
            DataManage.ClearWatchTime();
        }

        public void ClearAllCurrenciesValues()
        {
            DataManage.ClearAllCurrencyValues();
        }

        /// <summary>
        /// Add currency accrual rows for every user when a new currency type is added to the database
        /// </summary>
        public void UpdateCurrencyTable()
        {
            DataManage.AddCurrencyRows();
        }

    }
}
