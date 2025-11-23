using System.Diagnostics;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

class Program
{
    static async Task Main(string[] args)
    {
        string url = "https://www.youtube.com/watch?v=XUvhjjR-dVI&list=RDXUvhjjR-dVI&start_radio=1"; // use only allowed content
        string outputMp3 = "For Tracy Hyde - Underwater Girl";

        var youtube = new YoutubeClient();

        var video = await youtube.Videos.GetAsync(url);
        Console.WriteLine($"Downloading: {video.Title}");

        var manifest = await youtube.Videos.Streams.GetManifestAsync(video.Id);
        var audioStream = manifest.GetAudioOnlyStreams().GetWithHighestBitrate();

        string tempFile = "audio_temp.m4a";

        await youtube.Videos.Streams.DownloadAsync(audioStream, tempFile);
        Console.WriteLine("Download complete.");

        var ffmpeg = new ProcessStartInfo
        {
            FileName = @"C:\ProgramData\chocolatey\bin\ffmpeg.exe", // choco path
            Arguments = $"-i \"{tempFile}\" -vn -b:a 320k \"{outputMp3}\" -y",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(ffmpeg);
        string stdout = process.StandardOutput.ReadToEnd();
        string stderr = process.StandardError.ReadToEnd();
        process.WaitForExit();

        Console.WriteLine(stdout);
        Console.WriteLine(stderr);

        File.Delete(tempFile);

        Console.WriteLine("MP3 created!");
    }
}
