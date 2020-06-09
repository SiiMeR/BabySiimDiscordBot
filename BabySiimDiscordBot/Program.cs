using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Web;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace BabySiimDiscordBot
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // NLog: setup the logger first to catch all errors
            var logger = NLogBuilder.ConfigureNLog(Path.Combine("Configuration","nlog.config")).GetCurrentClassLogger();
            try
            {
                logger.Debug("Starting application...");
                CreateHostBuilder(args).Build().Run(); 
            }
            catch (Exception ex)
            {
                //NLog: catch setup errors
                logger.Error(ex, "Stopped program because of exception");
                throw;
            }
            finally
            {
                // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
                LogManager.Shutdown();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.Sources.Clear();

                    var env = hostingContext.HostingEnvironment;

                    config.AddJsonFile(Path.Combine("Configuration", "appsettings.json"), optional: true, reloadOnChange: true)
                        .AddJsonFile(Path.Combine("Configuration", $"appsettings.{env.EnvironmentName}.json"), optional: true, reloadOnChange: true);
                    
                    config.AddEnvironmentVariables();
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                        .UseStartup<Startup>()
                        .ConfigureLogging(builder => builder.ClearProviders().SetMinimumLevel(LogLevel.Trace));
                })
                .UseNLog();
    }
}