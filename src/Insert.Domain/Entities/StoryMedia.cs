namespace Insert.Domain.Entities;

public class StoryMedia
{
    public Guid Id { get; set; }
    public Guid StoryId { get; set; }
    public Guid MediaAssetId { get; set; }
    public string? Role { get; set; }
    public int SortOrder { get; set; }
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
}