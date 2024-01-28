using Microsoft.EntityFrameworkCore;

using StreamerBotLib.DataSQL.Models;

namespace StreamerBotLib.DataSQL
{
    public class SQLDBContext : DbContext
    {
        private readonly string DatabaseFileName = "TwineStreamerBot.db";

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
        public DbSet<ModeratorApprove> ModeratorApprove { get; set; }
        #endregion

        #region MultiLive Data
        public DbSet<MultiMsgEndPoints> MultiMsgEndPoints { get; set; }
        public DbSet<MultiSummaryLiveStream> MultiSummaryLiveStreams { get; set; }
        public DbSet<MultiChannels> MultiChannels { get; set; }
        public DbSet<MultiLiveStream> MultiLiveStreams { get; set; }
        #endregion

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Data Source={DatabaseFileName}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<BanRules>()
                .HasOne(b => b.BanReasons)
                .WithOne(r => r.BanRules)
                .HasForeignKey<BanReasons>(b => b.MsgType);

            modelBuilder.Entity<CategoryList>()
                .HasOne(g => g.GameDeadCounter)
                .WithOne(c => c.CategoryList)
                .HasForeignKey<GameDeadCounter>(c => new { c.CategoryId, c.Category });

            modelBuilder.Entity<Users>()
                .HasOne(f => f.Followers)
                .WithOne(u => u.Users)
                .HasForeignKey<Followers>(f => new { f.UserId, f.UserName, f.Platform });

            modelBuilder.Entity<Users>()
                .HasOne(w => w.CustomWelcome)
                .WithOne(u => u.Users)
                .HasForeignKey<CustomWelcome>(w => new { w.UserId, w.UserName, w.Platform });

            modelBuilder.Entity<Users>()
                .HasOne(m => m.MultiChannels)
                .WithOne(u => u.Users)
                .HasForeignKey<MultiChannels>(m => new { m.UserId, m.UserName, m.Platform });

            modelBuilder.Entity<Users>()
                .HasOne(s => s.MultiSummaryLiveStream)
                .WithOne(u => u.Users)
                .HasForeignKey<MultiSummaryLiveStream>(s => new { s.UserId, s.UserName, s.Platform });

            modelBuilder.Entity<Users>()
                .HasOne(o => o.ShoutOuts)
                .WithOne(u => u.Users)
                .HasForeignKey<ShoutOuts>(o => new { o.UserId, o.UserName, o.Platform });

            modelBuilder.Entity<Users>()
                .HasOne(s => s.UserStats)
                .WithOne(u => u.Users)
                .HasForeignKey<UserStats>(s => new { s.UserId, s.UserName, s.Platform });
        }

        public SQLDBContext()
        {
            ChangeTracker.LazyLoadingEnabled = false;
        }
    }
}
