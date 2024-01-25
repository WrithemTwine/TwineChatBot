using Microsoft.EntityFrameworkCore;

using StreamerBotLib.DataSQL.Models;

namespace StreamerBotLib.DataSQL
{
    public class SQLDBContext : DbContext
    {
        private readonly string DatabaseFileName="TwineStreamerBot.db";

        public DbSet<BanReasons> BanReasons { get; set; }
        public DbSet<BanRules> BanRules { get; set; }
        public DbSet<CategoryList> CategoryList { get; set; }
        public DbSet<ChannelEvents> ChannelEvents { get; set; }
        public DbSet<Clips> Clips { get; set; }
        public DbSet<Commands> Commands { get; set; }
        public DbSet<Currency> Currency { get; set; }
        public DbSet<CurrencyType> CurrencyType { get; set; }
        public DbSet<CustomWelcome> CustomWelcome { get; set; }
        public DbSet<Followers> Followers { get; set; }
        public DbSet<GameDeadCounter> GameDeadCounter { get; set; }
        public DbSet<GiveawayUserData> GiveawayUserData { get; set; }
        public DbSet<InRaidData> InRaidData { get; set; }
        public DbSet<LearnMsgs> LearnMsgs { get; set; }
        public DbSet<ModeratorApprove> ModeratorApprove { get; set; }
        public DbSet<OutRaidData> OutRaidData { get; set; }
        public DbSet<OverlayServices> OverlayServices { get; set; }
        public DbSet<OverlayTicker> OverlayTicker { get; set; }
        public DbSet<Quotes> Quotes { get; set; }
        public DbSet<ShoutOuts> ShoutOuts { get; set; }
        public DbSet<StreamStats> StreamStats { get; set; }
        public DbSet<Users> Users { get; set; }
        public DbSet<UserStats> UserStats { get; set; }
        public DbSet<Webhooks> Webhooks { get; set; }

        public DbSet<MultiMsgEndPoints> MultiMsgEndPoints { get; set; }
        public DbSet<MultiSummaryLiveStream> MultiSummaryLiveStreams { get; set; }
        public DbSet<MultiChannels> MultiChannels { get; set; }
        public DbSet<MultiLiveStream> MultiLiveStreams { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Data Source={DatabaseFileName}");
        }

        
    }
}
