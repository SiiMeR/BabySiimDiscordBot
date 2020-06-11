using System.Reflection;
using BabySiimDiscordBot.Models;
using Microsoft.EntityFrameworkCore;

namespace BabySiimDiscordBot.DbContexts
{
    public interface IDiscordBotDbContext
    {
        DbSet<FredyConstant> FredyConstants { get; set; }
    }

    public class DiscordBotDbContext : DbContext, IDiscordBotDbContext
    {
        public DbSet<FredyConstant> FredyConstants { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=db/discordbot.db");
            base.OnConfiguring(optionsBuilder);
        }
    }
}
