#define USE_POOLED_DBCONTEXT4

//#define DEBUG_LOG

#if !USE_POOLED_DBCONTEXT

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;

using StreamerBotLib.Static;

#if RELEASE_KNET
using MASES.EntityFrameworkCore.KNet;
using MASES.EntityFrameworkCore.KNet.Extensions;
#endif

namespace StreamerBotLib.DataSQL
{
    public class DataManagerFactory : IDbContextFactory<SQLDBContext>, IDesignTimeDbContextFactory<SQLDBContext>
    {
        private PooledDbContextFactory<SQLDBContext> _pooledDbContextFactory;

        private void SetupDataManagerFactory()
        {
            var options = new DbContextOptionsBuilder<SQLDBContext>()

#if DEBUG || DEBUG_VIEWXAML || RELEASE_SQLITE
        .UseSqlite(OptionFlags.EFCConnectStringSqlite)
#if DEBUG_LOG
        .LogTo(LogWriter.WriteLog, LogLevel.Information)
#endif

#elif RELEASE_POSTGRE
        .UseNpgsql(connectionString: OptionFlags.EFCConnectStringPostgreSQL)

#elif RELEASE_COSMOS
        .UseCosmos(OptionFlags.EFCConnectStringCosmos, OptionFlags.EFCDbNameCosmos)

#elif RELEASE_KNET
                .UseKEFCore(
                    OptionFlags.EFCKNetApplicationId,
                    OptionFlags.EFCKNetBootstrapServers
                // add other singleton options here if needed, e.g.
                // .WithPersistentStorage()
                // .WithSecurityProtocol(...)
                )

#elif RELEASE_SQLSERVER
        .UseSqlServer(OptionFlags.EFCConnectStringSqlServer)

#elif RELEASE_MYSQL
        .UseMySQL(OptionFlags.EFCConnectStringMySql)

#elif RELEASE_POMELOMYSQL
        .UseMySql(
            OptionFlags.EFCConnectStringMySql,
            ServerVersion.AutoDetect(OptionFlags.EFCConnectStringMySql))
#endif
                .Options;

            _pooledDbContextFactory = new PooledDbContextFactory<SQLDBContext>(options, poolSize: 64);
        }

        public SQLDBContext CreateDbContext()
        {
#if RELEASE_KNET
            KEFCore.CreateGlobalInstance();   // or MASES.EntityFrameworkCore.KNet.Infrastructure.KEFCore.CreateGlobalInstance();
#endif

            SetupDataManagerFactory();
            return _pooledDbContextFactory.CreateDbContext();
        }

        public SQLDBContext CreateDbContext(string[] args)
        { // necessary for EFC migration tools
            SetupDataManagerFactory();
            return _pooledDbContextFactory.CreateDbContext();
        }
    }
}
#endif
