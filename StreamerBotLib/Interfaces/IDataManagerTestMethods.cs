namespace StreamerBotLib.Interfaces
{
    public interface IDataManagerTestMethods : IDataManager
    {
        bool TestInRaidData(string user, DateTime time, string viewers, string gamename);
        bool TestOutRaidData(string HostedChannel, DateTime dateTime);

    }
}
