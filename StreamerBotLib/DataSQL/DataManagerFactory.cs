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
