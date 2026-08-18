using Insert.Domain.Entities;

namespace Insert.Application.Stories;

public class CreateStoryRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string? Program { get; set; }
    public StoryPriority Priority { get; set; } = StoryPriority.Normal;
    public DateTime? Deadline { get; set; }
    public string? Notes { get; set; }
}