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
using Microsoft.Extensions.Logging;

namespace BabySiimDiscordBot.Modules
{
    [Group("music")]
    [Alias("m", "sound")]
    public class VoicechatModule : ModuleBase<SocketCommandContext>
    {
        private readonly IYoutubeService _youtubeService;
        private readonly ILogger<VoicechatModule> _logger;
        private static IAudioClient _audioClient;

        public VoicechatModule(IYoutubeService youtubeService, ILogger<VoicechatModule> logger)
        {
            _youtubeService = youtubeService;
            _logger = logger;
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

            // For the next step with transmitting audio, you would want to pass this Audio Client in to a service.
            _audioClient = await channel.ConnectAsync();
        }

        [Command("list", RunMode = RunMode.Async)]
        [Alias("l")]
        public async Task ListSongs()
        {
            var songsDirectory = Path.Join(AppDomain.CurrentDomain.BaseDirectory, "audio");

            _logger.LogInformation($"Listing audio files from {songsDirectory}");
            var filesInDirectory = Directory.GetFiles(songsDirectory);

            var enumerable = filesInDirectory
                .Where(file => !file.EndsWith(".empty"))
                .Select(str => $" - {Path.GetFileName(str)}")
                .ToList()
                .ChunkBy(40);

            await Context.Channel.SendMessageAsync(
                $"Sounds that I can play (using `!play <sound>`):");

            foreach (var s in enumerable)
            {
                var msgs = string.Join(Environment.NewLine, s);
                await Context.Channel.SendMessageAsync($"```{msgs}```");
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

        [Command("stop", RunMode = RunMode.Async)]
        public async Task StopSong()
        {
            await JoinChannel();
        }

        [Command("leave", RunMode = RunMode.Async)]
        public async Task LeaveVoiceChannel()
        {
            // Get the audio channel
            var channel = (Context.User as IGuildUser)?.VoiceChannel;
            if (channel == null)
            {
                await Context.Channel.SendMessageAsync("Bot must be in a voice channel to leave from.");
                return;
            }

            await channel.DisconnectAsync();
        }

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

        private Process CreateStream(string path) =>
            Process.Start(new ProcessStartInfo
            {
                //.\ffmpeg.exe -hide_banner -loglevel panic -i .\hyun.mp3 -ac 2 -f s16le -ar 48000 pipe:1
                FileName = "lib/ffmpeg",
                Arguments = $"-hide_banner -i \"{path}\" -ac 2 -f s16le -ar 48000 pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true
            });

    }
}
