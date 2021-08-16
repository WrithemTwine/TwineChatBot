namespace ChatBot_Net5.Data
{
    public partial class DataManager
    {
        /// <summary>
        /// Remove all Users from the database.
        /// </summary>
        internal void RemoveAllUsers()
        {
            lock (_DataSource.Users)
            {
                _DataSource.Users.Clear();
            }
            OnPropertyChanged(nameof(Users));

        }

        /// <summary>
        /// Remove all Followers from the database.
        /// </summary>
        internal void RemoveAllFollowers()
        {
            lock (_DataSource.Followers)
            {
                _DataSource.Followers.Clear();
            }
            OnPropertyChanged(nameof(Followers));
        }
    }
}
