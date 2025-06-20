#define USE_POOLED_DBCONTEXT4

#if !USE_POOLED_DBCONTEXT

#if RELEASE_KNET
using MASES.EntityFrameworkCore.KNet;
#endif

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Infrastructure;

using StreamerBotLib.Static;

namespace StreamerBotLib.DataSQL
{
    public class DataManagerFactory : IDbContextFactory<SQLDBContext>, IDesignTimeDbContextFactory<SQLDBContext>
    {
        private PooledDbContextFactory<SQLDBContext> _pooledDbContextFactory;

        private void SetupDataManagerFactory()
        {
            var options = new DbContextOptionsBuilder<SQLDBContext>()

            // these flags build the different connections to specific databases
            // for splitting code to each release build package
#if DEBUG || DEBUG_VIEWXAML || RELEASE_SQLITE
                .UseSqlite(OptionFlags.EFCConnectStringSqlite)
#if DEBUG_LOG
                            .LogTo(DebugLog.WriteLine, LogLevel.Information) // This line enables logging to a file
#endif

#elif RELEASE_POSTGRE
                        .UseNpgsql(connectionString: OptionFlags.EFCConnectStringPostgreSQL)
#elif RELEASE_COSMOS
                        .UseCosmos(OptionFlags.EFCConnectStringCosmos, OptionFlags.EFCDbNameCosmos)
#elif RELEASE_KNET
                        .UseKafkaCluster(
                            OptionFlags.EFCKNetApplicationId, 
                            OptionFlags.EFCDbNameKNet, 
                            OptionFlags.EFCKNetBootstrapServers
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
