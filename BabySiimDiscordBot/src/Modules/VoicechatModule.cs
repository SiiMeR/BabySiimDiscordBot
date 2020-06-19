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
    /// <inheritdoc />
    [Group("music")]
    [Alias("m", "sound")]
    public class VoicechatModule : ModuleBase<SocketCommandContext>
    {
        // TODO: Perhaps MediatR should be used to reduce coupling between modules and services?
        private readonly IYoutubeService _youtubeService;
        private readonly ILogger<VoicechatModule> _logger;
        private static IAudioClient _audioClient;

        /// <summary>Construct a new instance of this object.</summary>
        public VoicechatModule(IYoutubeService youtubeService, ILogger<VoicechatModule> logger)
        {
            _youtubeService = youtubeService;
            _logger = logger;
        }

        /// <summary>
        /// Join an audio channel.
        /// </summary>
        /// <param name="channel">The channel to join. If channel is not provided, try to join the channel the user is located in.</param>
        [Command("join", RunMode = RunMode.Async)]
        public async Task JoinChannel(IVoiceChannel channel = null)
        {
            // Get the audio channel
            channel ??= (Context.User as IGuildUser)?.VoiceChannel;
            if (channel == null)
            {
                await ReplyAsync("User must be in a voice channel, or a voice channel must be passed as an argument.");
                return;
            }

            await channel.DisconnectAsync();

            // For the next step with transmitting audio, you would want to pass this Audio Client in to a service.
            _audioClient = await channel.ConnectAsync();
        }

        /// <summary>Replies with a list of all songs that have been added locally.</summary>
        [Command("list", RunMode = RunMode.Async)]
        [Alias("l")]
        public async Task ListSongs()
        {
            var songsDirectory = Path.Join(AppDomain.CurrentDomain.BaseDirectory, "audio");

            _logger.LogInformation($"Listing audio files from {songsDirectory}");
            var filesInDirectory = Directory.GetFiles(songsDirectory);

            var chunkedSongList = filesInDirectory
                .Where(file => !file.EndsWith(".empty"))
                .Select(str => $" - {Path.GetFileName(str)}")
                .ToList()
                .ChunkBy(40);

            await ReplyAsync($"Sounds that I can play (using `!play <sound>`):");

            foreach (var song in chunkedSongList)
            {
                var msgs = string.Join(Environment.NewLine, song);
                await ReplyAsync($"```{msgs}```");
            }

        }

        /// <summary>
        /// Play a song.
        /// </summary>
        /// <param name="songName">The filename of the song to play.</param>
        [Command("play", RunMode = RunMode.Async)]
        public async Task PlaySong(string songName)
        {
            await JoinChannel();

            if (_audioClient == null)
            {
                await ReplyAsync("Bot is not in a voice channel.");
                return;
            }

            if (songName.ToLower().Contains("youtube.com")) {
                var result = await _youtubeService.DownloadSong(songName);

                var songData = await _youtubeService.GetSongInformation(songName);

                await ReplyAsync($"Now playing: {songData.Title}");
                await SendAudioAsync(_audioClient, result);
                return;
            }

            await SendAudioAsync(_audioClient, $"audio/{songName}");
        }

        /// <summary>Stop audio playback.</summary>
        [Command("stop", RunMode = RunMode.Async)]
        public async Task StopSong()
        {
            await _audioClient.StopAsync();
        }

        /// <summary>Leave the voice channel.</summary>
        [Command("leave", RunMode = RunMode.Async)]
        public async Task LeaveVoiceChannel()
        {
            // Get the audio channel
            var channel = (Context.User as IGuildUser)?.VoiceChannel;
            if (channel == null)
            {
                await ReplyAsync("Bot must be in a voice channel to leave from.");
                return;
            }

            await channel.DisconnectAsync();
        }

        private async Task SendAudioAsync(IAudioClient client, string path)
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

        // TODO: Create a ProcessService or similar and move this logic there (+ some things from SendAsync).
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
