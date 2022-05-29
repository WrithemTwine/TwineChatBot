namespace MediaOverlayServer.Communication
{
    internal static class ProcessHyperText
    {
        private static string RefreshToken(int Duration)
        {
            return $"<meta http-equiv=\"refresh\" content=\"{Duration}\">";
        }

        internal static string DefaultPage
        {
            get
            {
                return $"<html><head>{RefreshToken(2)}</head><body></body></html>";
            }
        }

    }
}
