using System.Reflection;
using BabySiimDiscordBot.Models;
using Microsoft.EntityFrameworkCore;

namespace BabySiimDiscordBot.DbContexts
{
    public class DiscordBotDbContext : DbContext
    {
        public DbSet<FredyConstant> FredyConstants { get; set; }

        public DiscordBotDbContext(DbContextOptions options) : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=db/discordbot.db", options =>
            {
                options.MigrationsAssembly(Assembly.GetExecutingAssembly().FullName);
            });

            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // // Map table names
            // modelBuilder.Entity<FredyConstant>().ToTable("Blogs", "test");
            // modelBuilder.Entity<FredyConstant>(entity =>
            // {
            //     entity.HasKey(e => e.BlogId);
            //     entity.HasIndex(e => e.Title).IsUnique();
            //     entity.Property(e => e.DateTimeAdd).HasDefaultValueSql("CURRENT_TIMESTAMP");
            // });
            // base.OnModelCreating(modelBuilder);
        }
    }
}
