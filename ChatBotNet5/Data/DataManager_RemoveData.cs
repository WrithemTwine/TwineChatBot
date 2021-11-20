namespace ChatBot_Net5.Data
{
    public partial class DataManager
    {
        /// <summary>
        /// Remove all Users from the database.
        /// </summary>
        public void RemoveAllUsers()
        {
            lock (_DataSource.Users)
            {
                _DataSource.Users.Clear();
            }

        }

        /// <summary>
        /// Remove all Followers from the database.
        /// </summary>
        public void RemoveAllFollowers()
        {
            lock (_DataSource.Followers)
            {
                _DataSource.Followers.Clear();
            }
        }
    }
}
