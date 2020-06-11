using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace BabySiimDiscordBot.Modules
{
    public class GeneralModule : ModuleBase<SocketCommandContext>
    {
        [Command("clear", RunMode = RunMode.Async)]
        public async Task ClearCommands()
        {
            var msgs = (await Context.Channel
                .GetMessagesAsync()
                .FlattenAsync());

            if (Context.Channel is ITextChannel channel)
            {
                await channel.DeleteMessagesAsync(msgs);
            }
        }
    }
}
