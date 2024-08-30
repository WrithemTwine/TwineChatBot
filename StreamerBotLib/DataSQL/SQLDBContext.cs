#if RELEASE_KNET
using MASES.EntityFrameworkCore.KNet;
#endif

using Microsoft.EntityFrameworkCore;

using StreamerBotLib.DataSQL.Models;
using StreamerBotLib.Static;

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

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // these flags build the different connections to specific databases
            // for splitting code to each release build package

#if DEBUG || RELEASE_SQLITE
            optionsBuilder.UseSqlite(OptionFlags.EFCConnectStringSqlite);
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

            modelBuilder.Entity<GameDeadCounter>()
                .HasOne(c => c.CategoryList)
                .WithOne(g => g.GameDeadCounter)
                .HasForeignKey<CategoryList>(c => new { c.CategoryId, c.Category })
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Clips>()
                .HasOne(c => c.CategoryList)
                .WithMany(c => c.Clips)
                .HasPrincipalKey(c => new { c.CategoryId })
                .HasForeignKey(c => new { c.CategoryId });

            modelBuilder.Entity<Followers>()
                .HasOne(c => c.CategoryList)
                .WithMany(c => c.Followers)
                .HasPrincipalKey(c => new { c.Category })
                .HasForeignKey(c => new { c.Category });

            modelBuilder.Entity<InRaidData>()
                .HasOne(c => c.CategoryList)
                .WithMany(c => c.InRaidData)
                .HasPrincipalKey(c => new { c.Category })
                .HasForeignKey(c => new { c.Category });

            modelBuilder.Entity<CurrencyType>()
                .HasMany(c => c.Currency)
                .WithOne(c => c.CurrencyType)
                .HasForeignKey(c => new { c.CurrencyName })
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Followers>()
                .HasOne(u => u.Users)
                .WithOne(f => f.Followers)
                .HasForeignKey<Users>(u => new { u.UserId, u.UserName, u.Platform });

            modelBuilder.Entity<Users>()
                .HasMany(u => u.Currency)
                .WithOne(c => c.User)
                .HasPrincipalKey(u => new { u.UserName })
                .HasForeignKey(u => new { u.UserName })
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CustomWelcome>()
                .HasOne(u => u.Users)
                .WithOne(w => w.CustomWelcome)
                .HasForeignKey<Users>(w => new { w.UserId, w.UserName, w.Platform });

            modelBuilder.Entity<InRaidData>()
                .HasOne(u => u.User)
                .WithMany(r => r.InRaidData)
                .HasForeignKey(f => new { f.UserId, f.UserName, f.Platform });

            modelBuilder.Entity<MultiChannels>()
                .HasOne(u => u.Users)
                .WithOne(m => m.MultiChannels)
                .HasForeignKey<Users>(m => new { m.UserId, m.UserName, m.Platform });

            modelBuilder.Entity<MultiLiveStreams>()
                .HasOne(u => u.Users)
                .WithMany(m => m.MultiLiveStreams)
                .HasPrincipalKey(u => new { u.UserId, u.UserName, u.Platform })
                .HasForeignKey(u => new { u.UserId, u.UserName, u.Platform });

            modelBuilder.Entity<MultiSummaryLiveStreams>()
                .HasOne(u => u.Users)
                .WithOne(s => s.MultiSummaryLiveStreams)
                .HasForeignKey<Users>(s => new { s.UserId, s.UserName, s.Platform });

            modelBuilder.Entity<ShoutOuts>()
                .HasOne(u => u.Users)
                .WithOne(u => u.ShoutOuts)
                .HasForeignKey<Users>(u => new { u.UserId, u.UserName, u.Platform });

            modelBuilder.Entity<Users>()
                .HasOne(s => s.UserStats)
                .WithOne(u => u.Users)
                .HasForeignKey<UserStats>(s => new { s.UserId, s.UserName, s.Platform })
                .OnDelete(DeleteBehavior.Cascade);

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
