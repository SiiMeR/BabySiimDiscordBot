using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace BabySiimDiscordBot.Services
{
    public interface IYoutubeService
    {
        /// <summary>
        /// Download the Video from YouTube url and extract it.
        /// </summary>
        /// <param name="url">URL to the YouTube Video</param>
        /// <returns>The File Path to the downloaded mp3.</returns>
        Task<string> DownloadFromYoutube(string url);
    }


    /// <inheritdoc />
    /// <exception cref="Exception">Download failed.</exception>
    public class YoutubeService : IYoutubeService
    {
        private readonly ILogger<YoutubeService> _logger;

        public YoutubeService(ILogger<YoutubeService> logger)
        {
            _logger = logger;
        }

        public async Task<string> DownloadFromYoutube(string url) {

            var result = await Task.Run((() =>
            {
                string file;
                var count = 0;
                do {
                    file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "botsong" + ++count + ".mp3");
                } while (File.Exists(file));

                //Download Video
                var fileWithExtension = file.Replace(".mp3", ".%(ext)s");

                _logger.LogDebug($"Downloading video {url} to {fileWithExtension}");

                var youtubedlDownload = new ProcessStartInfo() {
                    FileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "youtube-dl"),
                    Arguments = $"-x --audio-format mp3 -o \"{fileWithExtension}\" {url}",
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                };

                var youtubedl = Process.Start(youtubedlDownload);
                //Wait until download is finished
                youtubedl.WaitForExit();
                Thread.Sleep(1000);

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
    }
}
