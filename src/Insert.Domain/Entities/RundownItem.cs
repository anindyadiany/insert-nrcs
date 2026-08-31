namespace Insert.Domain.Entities;

public class RundownItem
{
    public Guid Id { get; set; }
    public Guid RundownId { get; set; }
    public int SortOrder { get; set; }
    public RundownItemType ItemType { get; set; } = RundownItemType.Story;

    public Guid? StoryId { get; set; }
    public string? SegmentLabel { get; set; }
    public int? SegmentDurationSeconds { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public enum RundownItemType
{
    Open,
    Story,
    Break,
    Ads,
    Close
}