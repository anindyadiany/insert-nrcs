namespace Insert.Domain.Entities;

public class MediaAsset
{
    public Guid Id { get; set; }
    public Guid? IngestJobId { get; set; }
    public string Filename { get; set; } = string.Empty;
    public string OriginalFilename { get; set; } = string.Empty;
    public string StoragePath { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public int? DurationSeconds { get; set; }
    public string? Format { get; set; }
    public string? Codec { get; set; }
    public string? Resolution { get; set; }
    public string? FrameRate { get; set; }
    public string? ThumbnailPath { get; set; }
    public string? ProxyPath { get; set; }
    public MediaIngestStatus IngestStatus { get; set; } = MediaIngestStatus.Pending;
    public MediaRetentionStatus RetentionStatus { get; set; } = MediaRetentionStatus.Active;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? Checksum { get; set; }
}

public enum MediaIngestStatus
{
    Pending,
    Processing,
    Ready,
    Failed
}

public enum MediaRetentionStatus
{
    Active,
    Archived,
    PendingDeletion,
    Deleted
}