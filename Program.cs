using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

class Program
{
    static async Task Main()
    {
        Console.WriteLine("Enter a YouTube video URL:");
        string? url = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(url))
        {
            Console.WriteLine("No URL provided. Exiting.");
            return;
        }

        try
        {
            var youtube = new YoutubeClient();

            var video = await youtube.Videos.GetAsync(url);
            string safeTitle = string.Join("_", video.Title.Split(Path.GetInvalidFileNameChars()));
            string outputMp3 = $"{safeTitle}.mp3";

            Console.WriteLine($"Downloading: {video.Title}");

            var manifest = await youtube.Videos.Streams.GetManifestAsync(video.Id);
            var audioStream = manifest.GetAudioOnlyStreams().GetWithHighestBitrate();

            string tempFile = "audio_temp.m4a";

            await youtube.Videos.Streams.DownloadAsync(audioStream, tempFile);
            Console.WriteLine("Download complete. Converting to MP3...");

            var ffmpeg = new ProcessStartInfo
            {
                FileName = @"C:\ProgramData\chocolatey\bin\ffmpeg.exe", // Update if ffmpeg is elsewhere
                Arguments = $"-i \"{tempFile}\" -vn -b:a 320k \"{outputMp3}\" -y",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(ffmpeg);
            string stderr = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if (process.ExitCode == 0)
                Console.WriteLine($"MP3 saved as: {outputMp3}");
            else
                Console.WriteLine($"Conversion error:\n{stderr}");

            File.Delete(tempFile);
            Console.WriteLine("Temporary file removed.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}
