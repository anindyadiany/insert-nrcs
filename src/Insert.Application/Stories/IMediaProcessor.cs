namespace Insert.Application.Stories;

public class MediaProbeResult
{
    public int? DurationSeconds { get; set; }
    public string? Format { get; set; }
    public string? Codec { get; set; }
    public string? Resolution { get; set; }
    public string? FrameRate { get; set; }
}

public interface IMediaProcessor
{
    Task<MediaProbeResult> ProbeAsync(string filePath);
    Task<string> GenerateThumbnailAsync(string filePath, string outputDir);
    Task<string> GenerateProxyAsync(string filePath, string outputDir);
}