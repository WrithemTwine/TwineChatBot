using StreamerBotLib.DataSQL;
using StreamerBotLib.Models;

namespace StreamerBotLib.Interfaces
{
    public interface IDataManagerTestMethods : IDataManager
    {
        IEnumerable<LiveUser> TestGetRandomUsers(int count, SQLDBContext Refcontext = null);
        bool TestInRaidData(string user, DateTime time, int viewers, string gamename, SQLDBContext Refcontext = null);
        bool TestOutRaidData(string HostedChannel, DateTime dateTime, SQLDBContext Refcontext = null);

    }
}
