using System;
using System.Threading.Tasks;
using BabySiimDiscordBot.Models.Options;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BabySiimDiscordBot
{
    /// <summary>Handles messages from the discord API.</summary>
    public class DiscordCommandHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commandService;
        private readonly ILogger<DiscordCommandHandler> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly DiscordOptions _options;

        private bool _isReady;

        /// <summary>Construct a new instance of this object.</summary>
        public DiscordCommandHandler(DiscordSocketClient client, CommandService commandService, IServiceProvider serviceProvider,
            IOptions<DiscordOptions> options, ILogger<DiscordCommandHandler> logger)
        {
            _client = client;
            _commandService = commandService;
            _serviceProvider = serviceProvider;
            _options = options.Value;
            _logger = logger;

            _client.MessageReceived += OnMessageReceived;
            _client.Ready += OnClientReady;
            _client.Log += OnLog;

            _commandService.CommandExecuted += CommandExecutedAsync;

            StartClient(_options.AccessToken).Wait();
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
                _logger.LogDebug("Bot is not ready to process messages yet.");
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
            if (!(message.HasCharPrefix(_options.CommandPrefix, ref argPos) || message.HasMentionPrefix(_client.CurrentUser, ref argPos))
                || message.Author.IsBot)
            {
                return;
            }

            // Create a WebSocket-based command context based on the message
            var context = new SocketCommandContext(_client, message);

            // Execute the command with the command context we just
            // created, along with the service provider for precondition checks.
            await _commandService.ExecuteAsync(context, argPos, _serviceProvider);
        }

        private async Task CommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            if (!command.IsSpecified && result.IsSuccess)
            {
                await context.Message.AddReactionAsync(new Emoji("\uD83D\uDE10"));
                return;
            }

            await context.Message.AddReactionAsync(new Emoji("\uD83D\uDE0B"));

            var commandOrEmptyString = command.Value?.Name ?? string.Empty;

            switch (result.Error)
            {
                case CommandError.UnknownCommand:
                    _logger.LogDebug($"Unknown command '{commandOrEmptyString}'");
                    break;
                case CommandError.Exception:
                    _logger.LogError($"Exception: {result.ErrorReason}");
                    break;
                case CommandError.BadArgCount:
                case CommandError.ParseFailed:
                case CommandError.ObjectNotFound:
                case CommandError.MultipleMatches:
                case CommandError.UnmetPrecondition:
                    await context.Channel.SendMessageAsync($"An error occured: {result.ErrorReason}");
                    break;
                case CommandError.Unsuccessful:
                    _logger.LogInformation($"Processing command '{commandOrEmptyString}' failed: {result.ErrorReason}");
                    break;
                default:
                    _logger.LogError($"Unknown error occured: {result.ErrorReason}");
                    break;
            }
        }
    }
}
