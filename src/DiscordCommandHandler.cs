using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NLog;

namespace BabySiimDiscordBot
{
    public class DiscordCommandHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commandService;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DiscordCommandHandler> _logger;

        private bool _isReady;

        public DiscordCommandHandler(DiscordSocketClient client, CommandService commandService, IServiceProvider serviceProvider, IOptions<DiscordOptions> options, ILogger<DiscordCommandHandler> logger)
        {
            _client = client;
            _commandService = commandService;
            _serviceProvider = serviceProvider;
            _logger = logger;

            _client.MessageReceived += OnMessageReceived;
            _client.Ready += OnClientReady;
            _client.Log += OnLog;

            StartClient(options.Value.AccessToken).GetAwaiter().GetResult();
        }

        private async Task StartClient(string accessToken)
        {
            await _client.LoginAsync(TokenType.Bot, accessToken);
            await _client.StartAsync();

            await _commandService.AddModulesAsync(Assembly.GetEntryAssembly(), _serviceProvider);
        }

        private Task OnClientReady()
        {
            _logger.LogInformation($"Bot is connected");
            _isReady = true;
            return Task.CompletedTask;
                
            // _client.Guilds.ToList().ForEach(Console.WriteLine);

            // _client.Guilds.FirstOrDefault(guild => guild.Name == "XtraSpicyChats")
            //     ?.TextChannels.FirstOrDefault(channel => channel.Name == "lööps-rant")
            //     ?.SendMessageAsync("..markus lits")
            //     ;
        }
        
        private Task OnLog(LogMessage logMessage)
        {
            _logger.LogDebug(logMessage.Message);
            return Task.CompletedTask;
        }

        private async Task OnMessageReceived(SocketMessage messageParam)
        {
            if(!_isReady)
            {
                _logger.LogDebug($"Bot is not yet ready to process messages yet.");
                return;
            }
            
            _logger.LogInformation($"Received message {messageParam.Content}");
            //
            
            // Don't process the command if it was a system message
            if (!(messageParam is SocketUserMessage message)) return;

            // Create a number to track where the prefix ends and the command begins
            var argPos = 0;

            // Determine if the message is a command based on the prefix and make sure no bots trigger commands
            if (!(message.HasCharPrefix('!', ref argPos) || 
                  message.HasMentionPrefix(_client.CurrentUser, ref argPos)) ||
                message.Author.IsBot)
                return;

            // Create a WebSocket-based command context based on the message
            var context = new SocketCommandContext(_client, message);

            // Execute the command with the command context we just
            // created, along with the service provider for precondition checks.
            var result = await _commandService.ExecuteAsync(context, argPos, _serviceProvider);
            if (!result.IsSuccess && result is ExecuteResult executeResult)
            {
                _logger.LogError(executeResult.Exception, executeResult.ErrorReason);
                await messageParam.AddReactionAsync(new Emoji("\uD83D\uDE10"));
            }
            else
            {
                await messageParam.AddReactionAsync(new Emoji("\uD83D\uDE0B"));
            }
        }
    }
}