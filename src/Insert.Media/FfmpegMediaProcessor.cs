using System.Diagnostics;
using System.Text.Json;
using Insert.Application.Stories;

namespace Insert.Media;

public class FfmpegMediaProcessor : IMediaProcessor
{
    public async Task<MediaProbeResult> ProbeAsync(string filePath)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "ffprobe",
            Arguments = $"-v quiet -print_format json -show_format -show_streams \"{filePath}\"",
            RedirectStandardOutput = true,
            UseShellExecute = false,
        };

        using var process = Process.Start(psi)!;
        var output = await process.StandardOutput.ReadToEndAsync();
        await process.WaitForExitAsync();

        var result = new MediaProbeResult();

        using var doc = JsonDocument.Parse(output);
        var root = doc.RootElement;

        if (root.TryGetProperty("format", out var format))
        {
            if (format.TryGetProperty("duration", out var dur) &&
                double.TryParse(dur.GetString(), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var durSec))
                result.DurationSeconds = (int)durSec;
            if (format.TryGetProperty("format_name", out var fmtName))
                result.Format = fmtName.GetString();
        }

        if (root.TryGetProperty("streams", out var streams))
        {
            foreach (var stream in streams.EnumerateArray())
            {
                if (stream.TryGetProperty("codec_type", out var codecType) && codecType.GetString() == "video")
                {
                    if (stream.TryGetProperty("codec_name", out var codecName))
                        result.Codec = codecName.GetString();
                    if (stream.TryGetProperty("width", out var w) && stream.TryGetProperty("height", out var h))
                        result.Resolution = $"{w.GetInt32()}x{h.GetInt32()}";
                    if (stream.TryGetProperty("r_frame_rate", out var fr))
                        result.FrameRate = fr.GetString();
                    break;
                }
            }
        }

        return result;
    }

    public async Task<string> GenerateThumbnailAsync(string filePath, string outputDir)
    {
        Directory.CreateDirectory(outputDir);
        var outputPath = Path.Combine(outputDir, $"{Guid.NewGuid()}.jpg");
        await RunFfmpegAsync($"-i \"{filePath}\" -ss 00:00:01 -vframes 1 \"{outputPath}\" -y");
        return outputPath;
    }

    public async Task<string> GenerateProxyAsync(string filePath, string outputDir)
    {
        Directory.CreateDirectory(outputDir);
        var outputPath = Path.Combine(outputDir, $"{Guid.NewGuid()}.mp4");
        await RunFfmpegAsync($"-i \"{filePath}\" -vf scale=640:-2 -c:v libx264 -crf 28 -preset fast \"{outputPath}\" -y");
        return outputPath;
    }

    private static async Task RunFfmpegAsync(string arguments)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "ffmpeg",
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };

        using var process = Process.Start(psi)!;
        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            var error = await process.StandardError.ReadToEndAsync();
            throw new InvalidOperationException($"ffmpeg failed: {error}");
        }
    }
}