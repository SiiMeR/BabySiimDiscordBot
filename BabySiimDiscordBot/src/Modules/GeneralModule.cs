using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.Logging;
using Timer = System.Timers.Timer;

namespace BabySiimDiscordBot.Modules
{
    /// <inheritdoc />
    public class GeneralModule : ModuleBase<SocketCommandContext>
    {
        private readonly CommandService _commandService;
        private readonly ILogger<GeneralModule> _logger;

        /// <summary>Construct a new instance of this object.</summary>
        public GeneralModule(CommandService commandService, ILogger<GeneralModule> logger)
        {
            _commandService = commandService;
            _logger = logger;
        }

        /// <summary>Clear as much of the bot's commands from the calling channel as possible.</summary>
        [Command("clear", RunMode = RunMode.Async)]
        public async Task ClearCommands()
        {
            var msgs = (await Context.Channel
                .GetMessagesAsync()
                .FlattenAsync())
                .Where(message => message.Author.IsBot && message.Author.Username == "Baby Siim");

            foreach (var message in msgs)
            {
                try
                {
                    await message.DeleteAsync();
                }
                catch(Exception e)
                {
                    _logger.LogTrace($"Exception while deleting message: {e.Message}");
                }
            }
        }

        [Command("edittime", RunMode = RunMode.Async)]
        public async Task ShowCurrentTimeViaEdit()
        {
            var currentTimeMessage = await ReplyAsync($"The current time is: {DateTime.Now}");

            var startTime = DateTime.Now;
            var timer = new Timer(1000) {AutoReset = true};

            timer.Elapsed += onTimerElapsed;
            timer.Start();
            async void onTimerElapsed(object state, ElapsedEventArgs elapsedEventArgs)
            {
                if (startTime.AddSeconds(30) < DateTime.Now)
                {
                    timer.Stop();
                    return;
                }

                // if(((DateTime) state))
                await currentTimeMessage.ModifyAsync((properties =>
                {
                    properties.Content = $"The current time is: {DateTime.Now}";
                }));
            }
        }

        [Command("progressbar")]
        public async Task ShowProgressBar()
        {
            var animationInterval = TimeSpan.FromSeconds(1.0 / 8);
            var animation = @"|/-\";
            var animationIndex = 0;

            var maxProgressBars = 10;
            var currentProgressBars = 0;
            var currentTimeMessage = await ReplyAsync($"```[{new string('-', maxProgressBars)}] 0% {animation[++animationIndex % animation.Length]}```");

            var startTime = DateTime.Now;
            var timer = new Timer(1000) {AutoReset = true};

            timer.Elapsed += onTimerElapsed;
            timer.Start();
            async void onTimerElapsed(object state, ElapsedEventArgs elapsedEventArgs)
            {
                if (currentProgressBars == maxProgressBars || startTime.AddSeconds(30) < DateTime.Now)
                {
                    timer.Stop();
                    return;
                }

                await currentTimeMessage.ModifyAsync((properties =>
                {
                    properties.Content = $"```[{repeatChar('#', ++currentProgressBars)}{repeatChar('-', maxProgressBars - currentProgressBars)}] {currentProgressBars * 10}% {animation[++animationIndex % animation.Length]}```";
                }));
            }

            string repeatChar(char c, int times)
            {
                return times <= 0
                    ? string.Empty
                    : string.Concat(Enumerable.Repeat(c, times));
            }
        }

        /// <summary>Shows the list of all command with their aliases as a discord embed.</summary>
        [Command("help")]
        public async Task ShowHelp()
        {
            var commands = _commandService.Commands.ToList();
            var embedBuilder = new EmbedBuilder();

            foreach (var command in commands)
            {
                // Get the command Summary attribute information
                var embedFieldText = command.Summary ?? "No description available\n";

                var aliases = string.Join('/', command.Aliases);
                var parameters = string.Join(" ", command.Parameters.Select(param => $"<{param}>"));

                embedBuilder.AddField($"[{aliases}] {parameters}", embedFieldText);
            }

            await ReplyAsync("Here's a list of all commands and their description: ", false, embedBuilder.Build());
        }
    }
}
