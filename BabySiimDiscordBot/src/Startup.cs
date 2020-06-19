using System.Reflection;
using BabySiimDiscordBot.DbContexts;
using BabySiimDiscordBot.Models.Options;
using BabySiimDiscordBot.Services;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BabySiimDiscordBot
{
    /// <summary>Logic that is executed on program startup.</summary>
    public class Startup
    {
        /// <summary>Constructor.</summary>
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        private IConfiguration Configuration { get; }

        /// <summary>This method gets called by the runtime. Use this method to add services to the container.</summary>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(new DiscordSocketClient(
                    new DiscordSocketConfig
                    {
                        LogLevel = LogSeverity.Verbose,
                        HandlerTimeout = 5000,
                        MessageCacheSize = 1000
                    }
                )
            );
            services.AddSingleton(provider =>
            {
                var cs = new CommandService(
                    new CommandServiceConfig
                    {
                        LogLevel = LogSeverity.Verbose, // Tell the logger to give Verbose amount of info
                        DefaultRunMode = RunMode.Async, // Force all commands to run async by default
                    });

                cs.AddModulesAsync(Assembly.GetEntryAssembly(), provider).GetAwaiter().GetResult();

                return cs;
            });

            services.AddSingleton<DiscordCommandHandler>();
            services.AddOptions<DiscordOptions>().Bind(Configuration);

            services.AddTransient<IYoutubeService, YoutubeService>();

            services.AddDbContext<DiscordBotDbContext>();
        }

        /// <summary>This method gets called by the runtime. Use this method to configure the HTTP request pipeline.</summary>
        public void Configure(IApplicationBuilder app)
        {
            using (var scope = app.ApplicationServices.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetService<DiscordBotDbContext>();
                dbContext.Database.Migrate();
            }

            app.ApplicationServices.GetRequiredService<DiscordCommandHandler>();
        }
    }
}
