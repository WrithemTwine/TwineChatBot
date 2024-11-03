using Microsoft.EntityFrameworkCore;

namespace StreamerBotLib.DataSQL
{
    internal class DataManagerFactory : IDbContextFactory<SQLDBContext>
    {
        public SQLDBContext CreateDbContext()
        {
            SQLDBContext dbContext = new SQLDBContext();
            dbContext.Database.EnsureCreated();
            return dbContext;
        }
    }
}
