using System.Reflection;
using BabySiimDiscordBot.Models.Options;
using BabySiimDiscordBot.Services;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BabySiimDiscordBot
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        private IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddSingleton(new DiscordSocketClient(
                    new DiscordSocketConfig
                    {
                        LogLevel = LogSeverity.Verbose,
                        HandlerTimeout = 5000,
                        MessageCacheSize = 1000
                    }
                )
            );
            services.AddSingleton<CommandService>(provider =>
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
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            app.UseHttpsRedirection();

            app.UseRouting();

            app.ApplicationServices.GetService<DiscordCommandHandler>();
        }
    }
}
