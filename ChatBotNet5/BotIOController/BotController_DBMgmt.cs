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
            // if ManageUsers is False, then remove users!
            if(!OptionFlags.ManageUsers) { DataManage.RemoveAllUsers(); }

            // if ManageFollowers is False, then remove followers!
            if (!OptionFlags.ManageFollowers) { DataManage.RemoveAllFollowers(); }

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

    }
}
