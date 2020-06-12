using System;
using System.Reflection;
using System.Threading.Tasks;
using BabySiimDiscordBot.Models.Options;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BabySiimDiscordBot
{
    public class DiscordCommandHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commandService;
        private readonly ILogger<DiscordCommandHandler> _logger;
        private readonly IServiceProvider _serviceProvider;

        private bool _isReady;

        public DiscordCommandHandler(DiscordSocketClient client, CommandService commandService, IServiceProvider serviceProvider,
            IOptions<DiscordOptions> options, ILogger<DiscordCommandHandler> logger)
        {
            _client = client;
            _commandService = commandService;
            _serviceProvider = serviceProvider;
            _logger = logger;

            _client.MessageReceived += OnMessageReceived;
            _client.Ready += OnClientReady;
            _client.Log += OnLog;

            _commandService.CommandExecuted += CommandExecutedAsync;

            StartClient(options.Value.AccessToken).GetAwaiter().GetResult();
        }

        private async Task StartClient(string accessToken)
        {
            await _client.LoginAsync(TokenType.Bot, accessToken);
            await _client.StartAsync();
        }

        private Task OnClientReady()
        {
            _logger.LogInformation("Bot is connected");
            _isReady = true;
            return Task.CompletedTask;
        }

        private Task OnLog(LogMessage logMessage)
        {
            _logger.LogInformation(logMessage.Message);
            return Task.CompletedTask;
        }

        private async Task OnMessageReceived(SocketMessage messageParam)
        {
            if (!_isReady)
            {
                _logger.LogDebug("Bot is not yet ready to process messages yet.");
                return;
            }

            // Don't process the command if it was a system message
            if (!(messageParam is SocketUserMessage message))
            {
                return;
            }

            _logger.LogInformation($"Received message {messageParam.Content}");



            // Create a number to track where the prefix ends and the command begins
            var argPos = 0;

            // Determine if the message is a command based on the prefix and make sure no bots trigger commands
            if (!(message.HasCharPrefix('!', ref argPos) || message.HasMentionPrefix(_client.CurrentUser, ref argPos)) || message.Author.IsBot)
            {
                return;
            }

            // Create a WebSocket-based command context based on the message
            var context = new SocketCommandContext(_client, message);

            // Execute the command with the command context we just
            // created, along with the service provider for precondition checks.
            var result = await _commandService.ExecuteAsync(context, argPos, _serviceProvider);

            if (!message.HasCharPrefix('!', ref argPos))
            {
                return;
            }

            if (!result.IsSuccess && result is ExecuteResult executeResult)
            {
                await messageParam.AddReactionAsync(new Emoji("\uD83D\uDE10"));
            }
            else
            {
                await messageParam.AddReactionAsync(new Emoji("\uD83D\uDE0B"));
            }
        }

        public async Task CommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            if (!command.IsSpecified && result.IsSuccess)
            {
                return;
            }

            switch (result.Error)
            {
                case CommandError.UnknownCommand:
                    break;
                case CommandError.BadArgCount:
                case CommandError.ParseFailed:
                case CommandError.ObjectNotFound:
                case CommandError.MultipleMatches:
                case CommandError.UnmetPrecondition:
                case CommandError.Exception:
                    await context.Channel.SendMessageAsync(result.ErrorReason);
                    break;
                case CommandError.Unsuccessful:
                    break;
                case null:
                    break;
            }
        }
    }
}
