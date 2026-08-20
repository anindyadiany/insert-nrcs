namespace Insert.Domain.Entities;

public class IngestJob
{
    public Guid Id { get; set; }
    public string SourceType { get; set; } = "upload";
    public string SourceFilePath { get; set; } = string.Empty;
    public string OriginalFilename { get; set; } = string.Empty;
    public IngestJobStatus Status { get; set; } = IngestJobStatus.Queued;
    public int Progress { get; set; }
    public string? ErrorMessage { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
}

public enum IngestJobStatus
{
    Queued,
    Processing,
    Completed,
    Failed
}