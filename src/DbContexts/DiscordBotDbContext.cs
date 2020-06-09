using System.Reflection;
using BabySiimDiscordBot.Models;
using Microsoft.EntityFrameworkCore;

namespace BabySiimDiscordBot.DbContexts
{
    public class DiscordBotDbContext : DbContext
    {
        public DbSet<FredyConstant> FredyConstants { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=db/discordbot.db");

            base.OnConfiguring(optionsBuilder);
        }
    }
}
