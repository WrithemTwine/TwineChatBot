using Microsoft.EntityFrameworkCore;

using System.IO;

namespace EFEntityEntryTesting.EF
{
    public class EFTestDataContext : DbContext
    {
        public DbSet<CategoryList> CategoryList { get; set; }
        public DbSet<Quotes> Quotes { get; set; }

        private readonly StreamWriter _writer = new("DebugTest.txt");

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                .UseSqlite("Data Source=EFTest.db")
                .LogTo(_writer.WriteLine, Microsoft.Extensions.Logging.LogLevel.Information);
        }
    }
}
