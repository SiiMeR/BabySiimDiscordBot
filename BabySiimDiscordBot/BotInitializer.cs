using System;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace BabySiimDiscordBot
{
    public class BotInitializer
    {
        private readonly CommandService _commands;
        private readonly DiscordSocketClient _client;

        // Ask if there are existing CommandService and DiscordSocketClient
        // instance. If there are, we retrieve them and add them to the
        // DI container; if not, we create our own.
        public BotInitializer(CommandService commands = null, DiscordSocketClient client = null)
        {
            _commands = commands ?? new CommandService();
            _client = client ?? new DiscordSocketClient();
        }

        public IServiceProvider BuildServiceProvider() => new ServiceCollection()
            .AddSingleton(_client)
            .AddSingleton(_commands)
            // You can pass in an instance of the desired type
            // .AddSingleton(new NotificationService())
            // ...or by using the generic method.
            //
            // The benefit of using the generic method is that 
            // ASP.NET DI will attempt to inject the required
            // dependencies that are specified under the constructor 
            // for us.
            // .AddSingleton<DatabaseService>()
            .AddSingleton<DiscordCommandHandler>()
            .BuildServiceProvider();
    }
}