#define USE_POOLED_DBCONTEXT4

#if !USE_POOLED_DBCONTEXT

using Microsoft.EntityFrameworkCore;

namespace StreamerBotLib.DataSQL
{
    internal class DataManagerFactory : IDbContextFactory<SQLDBContext>
    {
        public SQLDBContext CreateDbContext()
        {
            SQLDBContext dbContext = new();
            dbContext.Database.EnsureCreated();
            return dbContext;
        }
    }
}
#endif
