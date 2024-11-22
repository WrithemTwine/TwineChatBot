using StreamerBotLib.DataSQL;

namespace StreamerBotLib.Interfaces
{
    public interface IDataManagerTestMethods : IDataManager
    {
        bool TestInRaidData(string user, DateTime time, int viewers, string gamename, SQLDBContext Refcontext = null);
        bool TestOutRaidData(string HostedChannel, DateTime dateTime, SQLDBContext Refcontext = null);

    }
}
