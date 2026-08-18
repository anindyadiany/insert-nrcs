namespace Insert.Domain.Entities;

public class Script
{
    public Guid Id { get; set; }
    public Guid StoryId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}