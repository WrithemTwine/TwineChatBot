#if DEBUG
#define DEBUG_LOG1 // rename to DEBUG_LOG to enable the debug log
#endif

#if RELEASE_KNET
using MASES.EntityFrameworkCore.KNet;
#endif

using Microsoft.EntityFrameworkCore;

using StreamerBotLib.DataSQL.DiscriminatorEnums;


#if DEBUG_LOG
using Microsoft.Extensions.Logging;
using System.IO;
#endif

using StreamerBotLib.DataSQL.Models;



namespace StreamerBotLib.DataSQL
{
    public class SQLDBContext(DbContextOptions<SQLDBContext> options) : DbContext(options)
    {
        #region User Data
        public DbSet<Followers> Followers { get; set; }
        public DbSet<OldFollowUsers> OldFollowUsers { get; set; }
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
        public DbSet<WebhooksBase> WebhooksBase { get; set; }
        #endregion

        #region Stream Action Data
        public DbSet<ChannelEvents> ChannelEvents { get; set; }
        public DbSet<CommandsBase> CommandsBase { get; set; }
        public DbSet<Commands> Commands { get; set; }
        public DbSet<CommandsUser> CommandsUser { get; set; }
        public DbSet<ModeratorApprove> ModeratorApprove { get; set; }
        #endregion

        #region MultiLive Data
        public DbSet<MultiWebhooks> MultiWebhooks { get; set; }
        public DbSet<MultiSummaryLiveStreams> MultiSummaryLiveStreams { get; set; }
        public DbSet<MultiChannels> MultiChannels { get; set; }
        public DbSet<MultiLiveStreams> MultiLiveStreams { get; set; }
        #endregion

#if DEBUG_LOG
        StreamWriter DebugLog = new("DebugLog_efc_sqlite.txt") { AutoFlush = true };
#endif

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {

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
                .HasMany(u => u.Currency)
                .WithOne(c => c.User)
                .HasForeignKey(u => new { u.UserId, u.Platform })
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired(true);

            modelBuilder.Entity<Users>()
                .HasOne(u => u.CustomWelcome)
                .WithOne(w => w.User)
                .HasForeignKey<CustomWelcome>(w => new { w.UserId, w.Platform })
                .IsRequired(false);

            modelBuilder.Entity<Users>()
                .HasOne(u => u.ShoutOuts)
                .WithOne(u => u.User)
                .HasForeignKey<ShoutOuts>(u => new { u.UserId, u.Platform })
                .IsRequired(false);

            modelBuilder.Entity<Users>()
                .HasMany(u => u.GiveawayUserData)
                .WithOne(g => g.Users);

            modelBuilder.Entity<Users>()
                .HasOne(s => s.UserStats)
                .WithOne(u => u.User)
                .HasForeignKey<UserStats>(s => new { s.UserId, s.Platform })
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired(true);

            modelBuilder.Entity<Users>()
                .HasOne(u => u.Follower)
                .WithOne(f => f.User)
                .HasForeignKey<Followers>(f => new { f.UserId, f.Platform })
                .IsRequired(false)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<InRaidData>()
                .HasOne(r => r.User)
                .WithMany(u => u.InRaidDataList);

            //modelBuilder.Entity<Followers>()
            //    .HasMany(f => f.OldFollowUsers)
            //    .WithOne(f => f.Followers)
            //    .HasForeignKey(f => new {f.UserId, f.Platform });

            modelBuilder.Entity<MultiChannels>()
                .HasMany(m => m.MultiLiveStreams)
                .WithOne(m => m.MultiChannels)
                .HasForeignKey(m => new { m.UserId, m.Platform })
                .IsRequired(true);

            modelBuilder.Entity<MultiChannels>()
                .HasOne(m => m.MultiSummaryLiveStreams)
                .WithOne(m => m.MultiChannels)
                .HasForeignKey<MultiSummaryLiveStreams>(m => new { m.UserId, m.Platform })
                .IsRequired(true);

            modelBuilder.Entity<LearnMsgs>()
                .Property(i => i.Id)
                .ValueGeneratedOnAdd();

            // Discriminators for Type-Per-Hierarchy -entity derived types will coalesce in base table
            // discriminators distinguish between base type and other derived types
            modelBuilder.Entity<CommandsBase>()
                .HasDiscriminator(c => c.Commandtype)
                .HasValue<CommandsBase>(CommandTypes.Base)
                .HasValue<Commands>(CommandTypes.BuiltIn)
                .HasValue<CommandsUser>(CommandTypes.User);

            modelBuilder.Entity<WebhooksBase>()
                .HasDiscriminator(w => w.DataSource)
                .HasValue<WebhooksBase>(WebhookDataSource.Base)
                .HasValue<Webhooks>(WebhookDataSource.Channel)
                .HasValue<MultiWebhooks>(WebhookDataSource.MultiLive);

            // eagerly load all of the navigation properties - is significant load time for a small 'User' database
            //modelBuilder.Entity<Users>().Navigation(u => u.Currency).AutoInclude();
            //modelBuilder.Entity<Users>().Navigation(u => u.CustomWelcome).AutoInclude();
            //modelBuilder.Entity<Users>().Navigation(u => u.ShoutOuts).AutoInclude();
            //modelBuilder.Entity<Users>().Navigation(u => u.UserStats).AutoInclude();
            modelBuilder.Entity<Users>().Navigation(u => u.Follower).AutoInclude();
            //modelBuilder.Entity<Users>().Navigation(u => u.GiveawayUserData).AutoInclude();
            //modelBuilder.Entity<Users>().Navigation(u => u.InRaidDataList).AutoInclude();

            //modelBuilder.Entity<Currency>().Navigation(c => c.User);
            //modelBuilder.Entity<CustomWelcome>().Navigation(c => c.User);
            //modelBuilder.Entity<ShoutOuts>().Navigation(s => s.User);
            //modelBuilder.Entity<UserStats>().Navigation(s => s.User);
            //modelBuilder.Entity<Followers>().Navigation(f => f.User);
            //modelBuilder.Entity<GiveawayUserData>().Navigation(g => g.Users);
            //modelBuilder.Entity<InRaidData>().Navigation(r => r.User);

            //modelBuilder.Entity<Currency>().Navigation(c => c.CurrencyType).AutoInclude();
            //modelBuilder.Entity<CurrencyType>().Navigation(c => c.Currency).AutoInclude();

            //modelBuilder.Entity<CategoryList>().Navigation(c => c.GameDeadCounter).AutoInclude();

            //modelBuilder.Entity<GameDeadCounter>().Navigation(g => g.CategoryList).AutoInclude();

            //modelBuilder.Entity<MultiChannels>().Navigation(m => m.MultiLiveStreams).AutoInclude();
            //modelBuilder.Entity<MultiChannels>().Navigation(m => m.MultiSummaryLiveStreams).AutoInclude();
            //modelBuilder.Entity<MultiLiveStreams>().Navigation(m => m.MultiChannels).AutoInclude();
            //modelBuilder.Entity<MultiSummaryLiveStreams>().Navigation(m => m.MultiChannels).AutoInclude();
        }
    }
}
