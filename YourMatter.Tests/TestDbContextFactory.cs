using Microsoft.EntityFrameworkCore;
using YourMatter.Data.Data;

namespace YourMatter.Tests
{
    public static class TestDbContextFactory
    {
        public static YourMatterDbContext Create(string dbName)
        {
            var options = new DbContextOptionsBuilder<YourMatterDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            var context = new YourMatterDbContext(options);
            context.Database.EnsureCreated();
            return context;
        }
    }
}