namespace Insert.Domain.Entities;

public class Story
{
    public Guid Id { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Headline { get; set; }
    public string? Category { get; set; }
    public string? Program { get; set; }
    public StoryPriority Priority { get; set; } = StoryPriority.Normal;
    public StoryStatus Status { get; set; } = StoryStatus.Draft;
    public Guid? ReporterId { get; set; }
    public Guid? ProducerId { get; set; }
    public DateTime? Deadline { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }
}

public enum StoryStatus
{
    Draft,
    Assigned,
    InProgress,
    InReview,
    Approved,
    Published,
    Killed
}

public enum StoryPriority
{
    Normal,
    High,
    Urgent,
    Breaking
}