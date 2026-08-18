namespace Insert.Domain.Entities;

public class ScriptVersion
{
    public Guid Id { get; set; }
    public Guid ScriptId { get; set; }
    public int VersionNumber { get; set; }
    public Guid AuthorId { get; set; }
    public string Content { get; set; } = string.Empty;
    public int WordCount { get; set; }
    public int EstimatedDurationSeconds { get; set; }
    public string? ChangeNote { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}