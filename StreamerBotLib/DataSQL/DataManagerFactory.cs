using Microsoft.EntityFrameworkCore;

namespace StreamerBotLib.DataSQL
{
    internal class DataManagerFactory : IDbContextFactory<SQLDBContext>
    {
        public SQLDBContext CreateDbContext()
        {
            return new SQLDBContext();
        }
    }
}
