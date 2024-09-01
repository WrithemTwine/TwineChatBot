#if RELEASE_KNET
using MASES.EntityFrameworkCore.KNet;
#endif

#define DEBUG_LOG // rename to DEBUG_LOG to enable the debug log

using Microsoft.EntityFrameworkCore;

#if DEBUG_LOG
using Microsoft.Extensions.Logging;
#endif

using StreamerBotLib.DataSQL.Models;
using StreamerBotLib.Static;

using System.IO;

namespace StreamerBotLib.DataSQL
{
    public class SQLDBContext : DbContext
    {
        #region User Data
        public DbSet<Followers> Followers { get; set; }
        public DbSet<Currency> Currency { get; set; }
        public DbSet<CustomWelcome> CustomWelcome { get; set; }
        public DbSet<GiveawayUserData> GiveawayUserData { get; set; }
        public DbSet<InRaidData> InRaidData { get; set; }
        public DbSet<OutRaidData> OutRaidData { get; set; }
        public DbSet<ShoutOuts> ShoutOuts { get; set; }
        public DbSet<Users> Users { get; set; }
        public DbSet<UserStats> UserStats { get; set; }
        #endregion

        #region Machine Learning Data
        public DbSet<BanReasons> BanReasons { get; set; }
        public DbSet<BanRules> BanRules { get; set; }
        public DbSet<LearnMsgs> LearnMsgs { get; set; }
        #endregion

        #region Stream Data
        public DbSet<CategoryList> CategoryList { get; set; }
        public DbSet<Clips> Clips { get; set; }
        public DbSet<CurrencyType> CurrencyType { get; set; }
        public DbSet<GameDeadCounter> GameDeadCounter { get; set; }
        public DbSet<OverlayServices> OverlayServices { get; set; }
        public DbSet<OverlayTicker> OverlayTicker { get; set; }
        public DbSet<Quotes> Quotes { get; set; }
        public DbSet<StreamStats> StreamStats { get; set; }
        public DbSet<Webhooks> Webhooks { get; set; }
        #endregion

        #region Stream Action Data
        public DbSet<ChannelEvents> ChannelEvents { get; set; }
        public DbSet<Commands> Commands { get; set; }
        public DbSet<CommandsUser> CommandsUser { get; set; }
        public DbSet<ModeratorApprove> ModeratorApprove { get; set; }
        #endregion

        #region MultiLive Data
        public DbSet<MultiMsgEndPoints> MultiMsgEndPoints { get; set; }
        public DbSet<MultiSummaryLiveStreams> MultiSummaryLiveStreams { get; set; }
        public DbSet<MultiChannels> MultiChannels { get; set; }
        public DbSet<MultiLiveStreams> MultiLiveStreams { get; set; }
        #endregion

#if DEBUG_LOG
        StreamWriter DebugLog = new("DebugLog_efc_sqlite.txt") { AutoFlush = true };
#endif

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // these flags build the different connections to specific databases
            // for splitting code to each release build package


#if DEBUG || RELEASE_SQLITE
            optionsBuilder
                .UseSqlite(OptionFlags.EFCConnectStringSqlite)
#if DEBUG_LOG
                .LogTo(DebugLog.WriteLine, LogLevel.Information) // This line enables logging to a file
#endif  
                ;

#elif RELEASE_POSTGRE
            optionsBuilder.UseNpgsql(connectionString: OptionFlags.EFCConnectStringPostgreSQL);
#elif RELEASE_COSMOS
            optionsBuilder.UseCosmos(OptionFlags.EFCConnectStringCosmos, OptionFlags.EFCDbNameCosmos);
#elif RELEASE_KNET
            optionsBuilder.UseKafkaCluster(
                OptionFlags.EFCKNetApplicationId, 
                OptionFlags.EFCDbNameKNet, 
                OptionFlags.EFCKNetBootstrapServers
                );
#elif RELEASE_SQLSERVER
            optionsBuilder.UseSqlServer(OptionFlags.EFCConnectStringSqlServer);
#elif RELEASE_MYSQL
            optionsBuilder.UseMySql(
                OptionFlags.EFCConnectStringMySql, 
                ServerVersion.AutoDetect(OptionFlags.EFCConnectStringMySql));
#endif


        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<CategoryList>()
                .HasOne(c => c.GameDeadCounter)
                .WithOne(g => g.CategoryList)
                .HasForeignKey<GameDeadCounter>(c => new { c.CategoryId, c.Category })
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired(true);

            modelBuilder.Entity<CategoryList>()
                .HasMany(c => c.Followers)
                .WithOne(c => c.CategoryList)
                .HasPrincipalKey(c => c.Category)
                .HasForeignKey(c => c.Category);

            modelBuilder.Entity<Currency>()
                .HasOne(c => c.CurrencyType)
                .WithMany(c => c.Currency)
                .HasForeignKey(c => new { c.CurrencyName })
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Users>()
                .HasOne(u => u.Followers)
                .WithOne(f => f.Users)
                .HasForeignKey<Followers>(u => new { u.UserId, u.UserName, u.Platform });

            modelBuilder.Entity<Users>()
                .HasMany(u => u.Currency)
                .WithOne(c => c.User)
                .HasForeignKey(u => new { u.UserId, u.UserName, u.Platform })
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired(true);

            modelBuilder.Entity<Users>()
                .HasOne(u => u.CustomWelcome)
                .WithOne(w => w.Users)
                .HasForeignKey<CustomWelcome>(w => new { w.UserId, w.UserName, w.Platform })
                .IsRequired(false);

            modelBuilder.Entity<Users>()
                .HasOne(u => u.ShoutOuts)
                .WithOne(u => u.Users)
                .HasForeignKey<ShoutOuts>(u => new { u.UserId, u.UserName, u.Platform })
                .IsRequired(false);

            modelBuilder.Entity<Users>()
                .HasOne(s => s.UserStats)
                .WithOne(u => u.Users)
                .HasForeignKey<UserStats>(s => new { s.UserId, s.UserName, s.Platform })
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired(true);

            modelBuilder.Entity<MultiChannels>()
                .HasMany(m => m.MultiLiveStreams)
                .WithOne(m => m.MultiChannels)
                .HasForeignKey(m => new { m.UserId, m.UserName, m.Platform })
                .IsRequired(true);

            modelBuilder.Entity<MultiChannels>()
                .HasOne(m => m.MultiSummaryLiveStreams)
                .WithOne(m => m.MultiChannels)
                .HasForeignKey<MultiSummaryLiveStreams>(m => new { m.UserId, m.UserName, m.Platform })
                .IsRequired(true);

            modelBuilder.Entity<LearnMsgs>()
                .Property(i => i.Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<StreamStats>()
                .Property(p => p.Duration)
                .HasComputedColumnSql("[StreamEnd] - [StreamStart]", stored: true);
        }
        
        public SQLDBContext()
        {
            ChangeTracker.LazyLoadingEnabled = false;
        }
    }
}
