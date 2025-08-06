namespace StreamerBotLib.Models.Interfaces
{
    public interface IDataManagerTestMethods : IDataManager
    {
        IEnumerable<LiveUser> TestGetRandomUsers(int count);
        bool TestInRaidData(string user, DateTime time, int viewers, string gamename);
        bool TestOutRaidData(string HostedChannel, DateTime dateTime);

    }
}
