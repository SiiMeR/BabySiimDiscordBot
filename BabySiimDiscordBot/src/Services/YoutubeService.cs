using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using BabySiimDiscordBot.Models;
using Microsoft.Extensions.Logging;

namespace BabySiimDiscordBot.Services
{
    /// <summary>A service to download sounds from Youtube.</summary>
    public interface IYoutubeService
    {
        /// <summary>
        /// Download the Video from YouTube url and extract it.
        /// </summary>
        /// <param name="url">URL to the YouTube Video</param>
        /// <returns>The File Path to the downloaded mp3.</returns>
        Task<string> DownloadSong(string url);

        /// <summary>
        /// Get the name and duration of a song.
        /// </summary>
        /// <param name="url">The URL of the song.</param>
        Task<SongData> GetSongInformation(string url);
    }


    /// <inheritdoc />
    /// <exception cref="Exception">Download failed.</exception>
    public class YoutubeService : IYoutubeService
    {
        private readonly ILogger<YoutubeService> _logger;

        /// <summary>Construct a new instance of this object.</summary>
        public YoutubeService(ILogger<YoutubeService> logger)
        {
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<string> DownloadSong(string url) {

            var appBaseDirectory = AppDomain.CurrentDomain.BaseDirectory ?? string.Empty;
            var file = Path.Combine(appBaseDirectory, $"ytdl-{Guid.NewGuid()}.mp3");

            var result = await Task.Run((() =>
            {
                //Download Video
                var fileWithExtension = file.Replace(".mp3", ".%(ext)s");

                _logger.LogDebug($"Downloading video {url} to {fileWithExtension}");

                var youtubedlDownload = new ProcessStartInfo
                {
                    FileName = Path.Combine(appBaseDirectory, "youtube-dl"),
                    Arguments = $"-x --audio-format mp3 -o \"{fileWithExtension}\" {url}",
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                };

                var youtubedl = Process.Start(youtubedlDownload);

                //Wait until download is finished
                youtubedl?.WaitForExit();

                Task.Delay(500);

                return File.Exists(file) ? file : null;
            }));

            if (result == null)
            {
                throw new Exception("youtube-dl.exe failed to download!");
            }

            //Remove \n at end of Line
            result = result.Replace("\n", string.Empty).Replace(Environment.NewLine, string.Empty);

            return result;
        }

        /// <inheritdoc />
        public async Task<SongData> GetSongInformation(string url)
        {
            var appBaseDirectory = AppDomain.CurrentDomain.BaseDirectory ?? string.Empty;
            var processInfo = new ProcessStartInfo
            {
                FileName = Path.Combine(appBaseDirectory, "youtube-dl"),
                Arguments = $"-e {url}",
                RedirectStandardOutput = true,
                UseShellExecute = false
            };

            var process = Process.Start(processInfo);
            process?.WaitForExit();

            var title = await process?.StandardOutput?.ReadToEndAsync() ?? "Unknown Song";
            return new SongData
            {
                Title = title
            };
        }
    }
}
