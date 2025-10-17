namespace StreamerBotLib.Models.Events
{
    public class ExpiredTokenEventArgs : EventArgs
    {
        /// <summary>
        /// When a token expires during an action, this action can be set to re-attempt the action after the token is refreshed.
        /// </summary>
        public Action RePerformAction { get; set; }

        public ExpiredTokenEventArgs(Action rePerformAction)
        {
            RePerformAction = rePerformAction;
        }
    }
}
