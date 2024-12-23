using Microsoft.EntityFrameworkCore;

namespace EFEntityEntryTesting.EF
{
    public class EFTestDataContext : DbContext
    {
        public DbSet<Users> Users { get; set; }
        public DbSet<UserStats> UserStats { get; set; }
        public DbSet<Currency> Currency { get; set; }
        public DbSet<CurrencyType> CurrencyType { get; set; }

        //private readonly StreamWriter _writer = new("DebugTest.txt");

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                .UseSqlite("Data Source=EFTest.db")
                //.LogTo(_writer.WriteLine, Microsoft.Extensions.Logging.LogLevel.Information)
                ;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

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
                .HasOne(s => s.UserStats)
                .WithOne(u => u.User)
                .HasForeignKey<UserStats>(s => new { s.UserId, s.Platform })
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired(true);
        }
    }
}
