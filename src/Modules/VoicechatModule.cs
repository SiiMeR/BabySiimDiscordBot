using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BabySiimDiscordBot.Extensions;
using BabySiimDiscordBot.Services;
using Discord;
using Discord.Audio;
using Discord.Commands;

namespace BabySiimDiscordBot.Modules
{
    public class VoicechatModule : ModuleBase<SocketCommandContext>
    {
        private readonly IYoutubeService _youtubeService;
        private static IAudioClient _audioClient;

        public VoicechatModule(IYoutubeService youtubeService)
        {
            _youtubeService = youtubeService;
        }

        // The command's Run Mode MUST be set to RunMode.Async, otherwise, being connected to a voice channel will block the gateway thread.
        [Command("join", RunMode = RunMode.Async)]
        public async Task JoinChannel(IVoiceChannel channel = null)
        {
            // Get the audio channel
            channel ??= (Context.User as IGuildUser)?.VoiceChannel;
            if (channel == null)
            {
                await Context.Channel.SendMessageAsync("User must be in a voice channel, or a voice channel must be passed as an argument.");
                return;
            }

            await channel.DisconnectAsync();

            await Task.Delay(1);
            // For the next step with transmitting audio, you would want to pass this Audio Client in to a service.
            _audioClient = await channel.ConnectAsync();
        }

        [Command("list", RunMode = RunMode.Async)]
        public async Task ListSongs()
        {
            try
            {
                var filesInDirectory = Directory.GetFiles(Path.Join(AppDomain.CurrentDomain.BaseDirectory, "audio"));

                var enumerable = filesInDirectory
                    .Where(file => !file.EndsWith(".empty"))
                    .Select(str => $" - {Path.GetFileName(str)}")
                    .ToList()
                    .ChunkBy(35);

                await Context.Channel.SendMessageAsync(
                    $"Sounds that I can play (using `!play <sound>`):");

                foreach (var s in enumerable)
                {
                    var msgs = string.Join(Environment.NewLine, s);
                    await Context.Channel.SendMessageAsync($"```{msgs}```");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

        }

        [Command("play", RunMode = RunMode.Async)]
        public async Task PlaySong(string songName)
        {
            await JoinChannel();

            if (_audioClient == null)
            {
                await Context.Channel.SendMessageAsync("Bot is not in a voice channel.");
                return;
            }

            if (songName.ToLower().Contains("youtube.com")) {
                var result = await _youtubeService.DownloadFromYoutube(songName);

                await SendAsync(_audioClient, result);
                return;
            }

            await SendAsync(_audioClient, $"audio/{songName}");
        }

        private Process CreateStream(string path) =>
            Process.Start(new ProcessStartInfo
            {
                //.\ffmpeg.exe -hide_banner -loglevel panic -i .\hyun.mp3 -ac 2 -f s16le -ar 48000 pipe:1
                FileName = "lib/ffmpeg",
                Arguments = $"-hide_banner -i \"{path}\" -ac 2 -f s16le -ar 48000 pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true
            });

        private async Task SendAsync(IAudioClient client, string path)
        {

            await client.SetSpeakingAsync(true);

            // Create FFmpeg using the previous example
            using var ffmpeg = CreateStream(path);
            await using var audio = ffmpeg.StandardOutput.BaseStream;
            await using var discord = client.CreatePCMStream(AudioApplication.Mixed);
            try
            {
                await audio.CopyToAsync(discord);
            }
            finally
            {
                await discord.FlushAsync();
                await client.SetSpeakingAsync(false);
            }
        }
    }
}
